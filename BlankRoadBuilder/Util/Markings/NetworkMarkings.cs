using BlankRoadBuilder.Domain;
using BlankRoadBuilder.Domain.Options;
using BlankRoadBuilder.ThumbnailMaker;

using ColossalFramework.Importers;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEngine;

using static AdaptiveRoads.Manager.NetInfoExtionsion;
using static BlankRoadBuilder.Util.Markings.MarkingStyleUtil;

namespace BlankRoadBuilder.Util.Markings;
public static class NetworkMarkings
{
	public static IEnumerable<NetInfo.Segment> Markings(MarkingsInfo markingInfo)
	{
		var fillers = new List<MeshInfo<NetInfo.Segment, Segment>>();
		var markings = new List<MeshInfo<NetInfo.Segment, Segment>>();

		foreach (var item in markingInfo.Fillers)
		{
			if (item.Type == LaneDecoration.Filler)
			{
				if (ModOptions.MarkingsGenerated.HasAnyFlag(MarkingsSource.VanillaMarkings, MarkingsSource.HiddenVanillaMarkings))
				{
					foreach (var filler in GetFillers(item))
					{
						markings.Add(filler);
					}
				}
			}
			else
			{
				var filler = GetFillers(item).ToList();
				var transitStop = item.Lanes.Any(x => x.Decorations.HasFlag(LaneDecoration.TransitStop));
				
				if (item.Type != LaneDecoration.Pavement && (item.Lanes.Any(x => !x.Tags.HasFlag(LaneTag.Sidewalk)) || item.Elevation != item.SurfaceElevation))
				{
					var pavementFiller = GetFillers(new FillerMarking
					{
						Type = LaneDecoration.Pavement,
						Elevation = item.Elevation,
						LeftPoint = item.LeftPoint,
						RightPoint = item.RightPoint
					}, transitStop ? true : null).ToList();

					foreach (var p in pavementFiller)
					{
						p.Mesh.m_forwardForbidden |= NetSegment.Flags.Bend;
						p.Mesh.m_backwardForbidden |= NetSegment.Flags.Bend;

						if (transitStop)
						{
							p.MetaData.Forward.Required |= RoadUtils.Flags.S_AnyStop;
							p.MetaData.Backward.Required |= RoadUtils.Flags.S_AnyStop;
						}
						else
						{
							p.MetaData.Forward.Required |= RoadUtils.Flags.S_ToggleGrassMedian;
							p.MetaData.Backward.Required |= RoadUtils.Flags.S_ToggleGrassMedian;
						}
					}

					fillers.AddRange(pavementFiller);
				}

				foreach (var f in filler)
				{
					if (transitStop)
					{
						f.Mesh.m_forwardForbidden |= NetSegment.Flags.StopAll;
						f.Mesh.m_backwardForbidden |= NetSegment.Flags.StopAll;
					}
					else
					{
						f.MetaData.Forward.Forbidden |= RoadUtils.Flags.S_ToggleGrassMedian;
						f.MetaData.Backward.Forbidden |= RoadUtils.Flags.S_ToggleGrassMedian;
					}
				}

				fillers.AddRange(filler);
			}
		}

		if (ModOptions.MarkingsGenerated.HasAnyFlag(MarkingsSource.VanillaMarkings, MarkingsSource.HiddenVanillaMarkings))
		{
			foreach (var item in markingInfo.Lines.Values)
			{
				if (GetMarking(item) is MeshInfo<NetInfo.Segment, Segment> line)
					markings.Add(line);
			}
		}

		if (!ModOptions.MarkingsGenerated.HasAnyFlag(MarkingsSource.MeshFillers, MarkingsSource.IMTMarkings))
		{
			foreach (var filler in fillers)
			{
				if (ModOptions.MarkingsGenerated.HasFlag(MarkingsSource.HiddenVanillaMarkings))
				{
					filler.MetaData.Forward.Required |= RoadUtils.Flags.S_RemoveMarkings;
					filler.MetaData.Backward.Required |= RoadUtils.Flags.S_RemoveMarkings;
				}
				else
				{
					filler.MetaData.Forward.Forbidden |= RoadUtils.Flags.S_RemoveMarkings;
					filler.MetaData.Backward.Forbidden |= RoadUtils.Flags.S_RemoveMarkings;
				}
			}
		}

		foreach (var filler in markings)
		{
			if (ModOptions.MarkingsGenerated.HasFlag(MarkingsSource.HiddenVanillaMarkings))
			{
				filler.MetaData.Forward.Required |= RoadUtils.Flags.S_RemoveMarkings;
				filler.MetaData.Backward.Required |= RoadUtils.Flags.S_RemoveMarkings;
			}
			else
			{
				filler.MetaData.Forward.Forbidden |= RoadUtils.Flags.S_RemoveMarkings;
				filler.MetaData.Backward.Forbidden |= RoadUtils.Flags.S_RemoveMarkings;
			}
		}

		markings.AddRange(fillers);

		foreach (var item in markings)
		{
			item.Mesh.m_forwardForbidden |= NetSegment.Flags.Invert | NetSegment.Flags.AsymForward | NetSegment.Flags.AsymBackward;
			item.Mesh.m_backwardRequired |= NetSegment.Flags.Invert;
			item.Mesh.m_backwardForbidden |= NetSegment.Flags.AsymForward | NetSegment.Flags.AsymBackward;

			yield return item;
		}
	}

	public static IEnumerable<NetInfo.Segment> IMTHelpers(RoadInfo roadInfo)
	{
		foreach (var lane in roadInfo.Lanes)
		{
			if (lane.Elevation == null || lane.Tags.HasFlag(LaneTag.StackedLane) || lane.Decorations.HasAnyFlag(LaneDecoration.Grass, LaneDecoration.Gravel, LaneDecoration.Pavement))
				continue;

			if (lane.Elevation == (!lane.Tags.HasFlag(LaneTag.Sidewalk) && roadInfo.RoadType == RoadType.Road ? -0.3F : 0F))
				continue;

			var fillers = GetFillers(new FillerMarking
			{
				LeftPoint = new MarkingPoint(lane.LeftLane, lane),
				RightPoint = new MarkingPoint(lane, lane.RightLane),
				Elevation = lane.LaneElevation,
				Helper = true,
				Type = LaneDecoration.None
			});

			foreach (var helper in fillers)
			{
				if (lane.Decorations.HasFlag(LaneDecoration.Filler))
				{
					if (ModOptions.MarkingsGenerated.HasFlag(MarkingsSource.HiddenVanillaMarkings))
					{
						helper.MetaData.Forward.Forbidden |= RoadUtils.Flags.S_RemoveMarkings;
						helper.MetaData.Backward.Forbidden |= RoadUtils.Flags.S_RemoveMarkings;
					}
					else if (ModOptions.MarkingsGenerated.HasFlag(MarkingsSource.VanillaMarkings))
					{
						helper.MetaData.Forward.Required |= RoadUtils.Flags.S_RemoveMarkings;
						helper.MetaData.Backward.Required |= RoadUtils.Flags.S_RemoveMarkings;
					}
				}

				yield return helper;
			}
		}
	}

	public static IEnumerable<NetInfo.Node> GetCrosswalk(RoadInfo roadInfo)
	{
		var crosswalk = getNode();

		yield return crosswalk;

		if (roadInfo.LeftPavementWidth != roadInfo.RightPavementWidth)
		{
			var invertedCrosswalk = getNode(true);

			invertedCrosswalk.MetaData.SegmentEndFlags.Required |= RoadUtils.Flags.S_IsTailNode;
			crosswalk.MetaData.SegmentEndFlags.Forbidden |= RoadUtils.Flags.S_IsTailNode;

			yield return invertedCrosswalk;
		}

		MeshInfo<NetInfo.Node, Node> getNode(bool inverted = false)
		{
			var mesh = GenerateMesh(null, null, new CrosswalkMarking(roadInfo, inverted), $"Crosswalk");
			GenerateTexture(null, false, true, mesh, default, GetLineMarkingInfo(GenericMarkingType.End, MarkingType.AN)?.Color ?? new Color32(77, 77, 77, 255), dashSpace: roadInfo.AsphaltWidth);

			var model = AssetUtil.ImportAsset(ShaderType.Bridge, MeshType.Markings, mesh + ".obj", filesReady: true);
			var node = new MeshInfo<NetInfo.Node, Node>(model);

			node.MetaData.SegmentEndFlags.Required |= AdaptiveRoads.Manager.NetSegmentEnd.Flags.ZebraCrossing;
			node.MetaData.NodeFlags.Forbidden |= RoadUtils.Flags.N_RemoveLaneArrows;
			node.MetaData.SegmentFlags.Forbidden |= RoadUtils.Flags.S_RemoveMarkings;

			return node;
		}
	}

	public static IEnumerable<MeshInfo<NetInfo.Segment, Segment>> GetFillers(FillerMarking fillerMarking, bool? addTransition = null)
	{
		var filler = getFiller(false, false);

		if (addTransition == null)
		{
			var surface = fillerMarking.Lanes.Min(x => x.SurfaceElevation);
			var elevation = fillerMarking.Elevation + (fillerMarking.Helper ? 0 : fillerMarking.Type == LaneDecoration.Filler ? 0.0025F : 0.01F);

			addTransition = surface != elevation && fillerMarking.Lanes.Any(x => !x.Type.HasAnyFlag(LaneType.Curb, LaneType.Filler, LaneType.Tram));
		}

		if (!addTransition.Value || fillerMarking.Type == LaneDecoration.Filler)
		{
			if (filler != null)
				yield return filler.Value;
			yield break;
		}

		var transitionFillerForward = getFiller(true, false);
		var transitionFillerBackward = getFiller(false, true);
		var transitionBothFiller = getFiller(true, true);

		if (filler != null && transitionFillerForward != null && transitionFillerBackward != null && transitionBothFiller != null)
		{
			filler.Value.MetaData.Forward.Forbidden |= RoadUtils.Flags.S_StepBackward | RoadUtils.Flags.S_StepForward;
			filler.Value.MetaData.Backward.Forbidden |= RoadUtils.Flags.S_StepBackward | RoadUtils.Flags.S_StepForward;

			transitionFillerForward.Value.MetaData.Forward.Required |= RoadUtils.Flags.S_StepForward;
			transitionFillerForward.Value.MetaData.Forward.Forbidden |= RoadUtils.Flags.S_StepBackward;
			transitionFillerForward.Value.MetaData.Backward.Required |= RoadUtils.Flags.S_StepBackward;
			transitionFillerForward.Value.MetaData.Backward.Forbidden |= RoadUtils.Flags.S_StepForward;
			transitionFillerForward.Value.Mesh.m_forwardForbidden |= NetSegment.Flags.Bend;
			transitionFillerForward.Value.Mesh.m_backwardForbidden |= NetSegment.Flags.Bend;

			transitionFillerBackward.Value.MetaData.Forward.Required |= RoadUtils.Flags.S_StepBackward;
			transitionFillerBackward.Value.MetaData.Forward.Forbidden |= RoadUtils.Flags.S_StepForward;
			transitionFillerBackward.Value.MetaData.Backward.Required |= RoadUtils.Flags.S_StepForward;
			transitionFillerBackward.Value.MetaData.Backward.Forbidden |= RoadUtils.Flags.S_StepBackward;
			transitionFillerBackward.Value.Mesh.m_forwardForbidden |= NetSegment.Flags.Bend;
			transitionFillerBackward.Value.Mesh.m_backwardForbidden |= NetSegment.Flags.Bend;

			transitionBothFiller.Value.Mesh.m_forwardForbidden |= NetSegment.Flags.Invert;
			transitionBothFiller.Value.Mesh.m_backwardRequired |= NetSegment.Flags.Invert;
			transitionBothFiller.Value.MetaData.Forward.Required |= RoadUtils.Flags.S_StepBackward | RoadUtils.Flags.S_StepForward;
			transitionBothFiller.Value.MetaData.Backward.Required |= RoadUtils.Flags.S_StepBackward | RoadUtils.Flags.S_StepForward;
			transitionBothFiller.Value.Mesh.m_forwardForbidden |= NetSegment.Flags.Bend;
			transitionBothFiller.Value.Mesh.m_backwardForbidden |= NetSegment.Flags.Bend;

			yield return filler.Value;
			yield return transitionFillerForward.Value;
			yield return transitionFillerBackward.Value;
			yield return transitionBothFiller.Value;
		}

		MeshInfo<NetInfo.Segment, Segment>? getFiller(bool forward, bool backward)
		{
			fillerMarking.TransitionForward = forward;
			fillerMarking.TransitionBackward = backward;

			var filler = GenerateFiller(fillerMarking);

			if (filler == null)
				return null;

			filler.Value.Mesh.m_forwardForbidden |= NetSegment.Flags.Invert | NetSegment.Flags.AsymForward | NetSegment.Flags.AsymBackward;
			filler.Value.Mesh.m_backwardRequired |= NetSegment.Flags.Invert;
			filler.Value.Mesh.m_backwardForbidden |= NetSegment.Flags.AsymForward | NetSegment.Flags.AsymBackward;

			return filler;
		}
	}

	private static MeshInfo<NetInfo.Segment, Segment>? GenerateFiller(FillerMarking fillerMarking)
	{
		var isFiller = !fillerMarking.Helper && fillerMarking.Type != LaneDecoration.Filler;
		var filler = fillerMarking.AN_Info;

		if (!isFiller && filler == null)
			return null;

		var mesh = GenerateMesh(fillerMarking, null, null, GetName(fillerMarking));

		GenerateTexture(
			fillerMarking.Lanes.First(),
			isFiller,
			false,
			mesh,
			fillerMarking.Type,
			filler?.Color ?? default,
			filler?.MarkingStyle == MarkingFillerType.Dashed ? MarkingLineType.Dashed : MarkingLineType.Solid,
			filler?.DashLength ?? default,
			filler?.DashSpace ?? default);

		var shader = fillerMarking.Type switch
		{
			LaneDecoration.Grass => ShaderType.Basic,
			LaneDecoration.Gravel => ShaderType.Rail,
			_ => ShaderType.Bridge
		};

		var model = AssetUtil.ImportAsset(shader, MeshType.Filler, mesh + ".obj", filesReady: true);

		var segment = new MeshInfo<NetInfo.Segment, Segment>(model);

		if (filler == null || fillerMarking.Type != LaneDecoration.Filler || fillerMarking.Helper)
			segment.MetaData.Tiling = 2F;
		else
			segment.MetaData.Tiling = filler.MarkingStyle == MarkingFillerType.Dashed ? 20F / (filler.DashLength + filler.DashSpace) : 10F;

		return segment;
	}

	private static string GetName(FillerMarking fillerMarking)
	{
		if (fillerMarking.Helper)
			return "Elevated Step";

		return fillerMarking.Type != LaneDecoration.Filler ? $"{fillerMarking.Type} Median" : $"{fillerMarking.Lanes.FirstOrDefault()?.Type} Filler";
	}

	private static MeshInfo<NetInfo.Segment, Segment>? GetMarking(LineMarking marking)
	{
		var lineInfo = marking.AN_Info;

		if (lineInfo == null || lineInfo.MarkingStyle == MarkingLineType.None)
		{
			return null;
		}

		var mesh = GenerateMesh(null, marking, null, $"{marking.Marking} Line");

		GenerateTexture(null, false, false, mesh, default, lineInfo.Color, lineInfo.MarkingStyle, lineInfo.DashLength, lineInfo.DashSpace);

		var model = AssetUtil.ImportAsset(ShaderType.Bridge, MeshType.Markings, mesh + ".obj", filesReady: true);

		var segment = new MeshInfo<NetInfo.Segment, Segment>(model);

		segment.MetaData.Tiling = lineInfo.MarkingStyle == MarkingLineType.Solid ? 10F : 20F / (lineInfo.DashLength + lineInfo.DashSpace);

		return segment;
	}

	private static void GenerateTexture(LaneInfo? lane, bool filler, bool crosswalk, string name, LaneDecoration decoration, Color32 color = default, MarkingLineType style = default, float dashLength = 0F, float dashSpace = 0F)
	{
		var baseName = crosswalk ? "Crosswalk" : !filler ? "Marking" : decoration == LaneDecoration.Grass ? "GrassFiller" : "PavementFiller";

		foreach (var file in Directory.GetFiles(Path.Combine(BlankRoadBuilderMod.TexturesFolder, "Markings"), $"{baseName}*"))
		{
			if (filler)
			{
				File.Copy(file, Path.Combine(BlankRoadBuilderMod.ImportFolder, Path.GetFileName(file).Replace(baseName, name)), true);
			}
			else if (file.EndsWith("_d.png"))
			{
				generateDiffuse(file);
			}
			else if (file.EndsWith("_r.png"))
			{
				generateRoad(file, !(lane?.Tags.HasFlag(LaneTag.Asphalt) ?? false));
			}
			else if (file.EndsWith("_p.png"))
			{
				generateRoad(file, !(lane?.Tags.HasFlag(LaneTag.Sidewalk) ?? true));
			}
			else if (file.EndsWith("_a.png"))
			{
				if (crosswalk)
					generateCrosswalkAlpha(file);
				else
					generateAlpha(file);
			}
			else
			{
				File.Copy(file, Path.Combine(BlankRoadBuilderMod.ImportFolder, Path.GetFileName(file).Replace(baseName, name)), true);
			}
		}

		void generateDiffuse(string file)
		{
			var diffuseTexture = new Image(file).CreateTexture().Color(color, false);

			File.WriteAllBytes(Path.Combine(BlankRoadBuilderMod.ImportFolder, Path.GetFileName(file).Replace(baseName, name)), diffuseTexture.EncodeToPNG());
		}

		void generateRoad(string file, bool black)
		{
			var roadTexture = new Image(file).CreateTexture().Color(black ? Color.black : new Color32((byte)(255 - color.a), (byte)(255 - color.a), (byte)(255 - color.a), 255), false);

			File.WriteAllBytes(Path.Combine(BlankRoadBuilderMod.ImportFolder, Path.GetFileName(file).Replace(baseName, name)), roadTexture.EncodeToPNG());
		}

		void generateAlpha(string file)
		{
			var alphaTexture = new Image(file).CreateTexture();
			var width = alphaTexture.width;
			var pixels = alphaTexture.GetPixels32();
			var ratio = (int)(width * (dashLength / Math.Max(0.01, dashLength + dashSpace)));

			for (var i = 0; i < pixels.Length; i++)
			{
				switch (style)
				{
					case MarkingLineType.Solid:
						pixels[i] = Color.white;
						break;
					case MarkingLineType.SolidDouble:
						pixels[i] = i % width < width / 3 || i % width > width * 2 / 3 ? Color.white : Color.black;
						break;
					case MarkingLineType.Dashed:
						pixels[i] = i / width < ratio ? Color.white : Color.black;
						break;
					case MarkingLineType.DashedDouble:
						pixels[i] = i / width < ratio && (i % width < width / 3 || i % width > width * 2 / 3) ? Color.white : Color.black;
						break;
					case MarkingLineType.SolidDashed:
						pixels[i] = i % width < width / 3 || (i / width < ratio && i % width > width * 2 / 3) ? Color.white : Color.black;
						break;
					case MarkingLineType.DashedSolid:
						pixels[i] = (i / width < ratio && i % width < width / 3) || i % width > width * 2 / 3 ? Color.white : Color.black;
						break;
					case MarkingLineType.ZigZag:
						var y = i / width;

						if (i >= pixels.Length / 2)
							y = width - y;

						var pointIndex = Math.Max(2, Math.Min(width - 2, 2 * (y % width)));

						pixels[i] = i % width >= pointIndex - 2 && i % width <= pointIndex + 2 ? Color.white
							: i % width >= pointIndex - 3 && i % width <= pointIndex + 3 ? Color.gray : Color.black;
						break;
				}
			}

			alphaTexture.SetPixels32(pixels);
			alphaTexture.Apply(updateMipmaps: false);

			File.WriteAllBytes(Path.Combine(BlankRoadBuilderMod.ImportFolder, Path.GetFileName(file).Replace(baseName, name)), alphaTexture.EncodeToPNG());
		}

		void generateCrosswalkAlpha(string file)
		{
			var alphaTexture = new Image(file).CreateTexture();
			var width = alphaTexture.width;
			var pixels = alphaTexture.GetPixels32();
			var ratio = width / Math.Floor(dashSpace / 1F);
			var bottomY = width * 1000 / 1024;
			var topY = width * 880 / 1024;
			var line = width * 10 / 1024;

			for (var i = 0; i < pixels.Length; i++)
			{
				var y = width - (i / width);

				if (y < topY || y > bottomY)
					continue;

				if (ModOptions.VanillaCrosswalkStyle.HasFlag(CrosswalkStyle.Dashed))
				{
					var ratioX = i % width % ratio;

					if (y < topY + line || y > bottomY - line)
						pixels[i] = ratioX < ratio * 1 / 3 || ratioX > ratio * 2 / 3 ? Color.white : Color.black;
				}

				if (ModOptions.VanillaCrosswalkStyle.HasFlag(CrosswalkStyle.Zebra))
				{
					var ratioX = i % width % ratio;

					pixels[i] = ratioX < ratio * 1 / 3 || ratioX > ratio * 2 / 3 ? Color.black : Color.white;
				}

				if (ModOptions.VanillaCrosswalkStyle.HasFlag(CrosswalkStyle.DoubleSolid))
				{
					if (y < topY + line || y > bottomY - line)
						pixels[i] = Color.white;
				}
			}

			alphaTexture.SetPixels32(pixels);
			alphaTexture.Apply(updateMipmaps: false);

			File.WriteAllBytes(Path.Combine(BlankRoadBuilderMod.ImportFolder, Path.GetFileName(file).Replace(baseName, name)), alphaTexture.EncodeToPNG());
		}
	}

	private static string GenerateMesh(FillerMarking? fillerMarking, LineMarking? lineMarking, CrosswalkMarking? crosswalkMarking, string name, bool transition = false, bool inverted = false)
	{
		var type = lineMarking?.AN_Info;
		var lineWidth = type == null ? 0F : type.LineWidth;

		if (type != null && (type.MarkingStyle == MarkingLineType.SolidDouble || type.MarkingStyle == MarkingLineType.SolidDashed || type.MarkingStyle == MarkingLineType.DashedDouble || type.MarkingStyle == MarkingLineType.DashedSolid))
			lineWidth *= 3;

		var file = crosswalkMarking != null ? "Crosswalk" : fillerMarking == null ? "Marking" : (fillerMarking.Helper || fillerMarking.Type == LaneDecoration.Filler || (fillerMarking.Type != LaneDecoration.Grass && fillerMarking.Elevation == fillerMarking.SurfaceElevation)) ? "Platform" : fillerMarking.Type == LaneDecoration.Grass ? "GrassFiller" : "PavementFiller";

		var guid = Guid.NewGuid().ToString();

		resize(file + ".obj");
		resize(file + "_lod.obj");

		void resize(string originalFile)
		{
			originalFile = Path.Combine(Path.Combine(BlankRoadBuilderMod.MeshesFolder, "Markings"), originalFile);

			if (!File.Exists(originalFile))
			{
				return;
			}

			var lines = File.ReadAllLines(originalFile);

			for (var i = 0; i < lines.Length; i++)
			{
				if (lines[i].Length == 0)
				{
					continue;
				}

				var data = lines[i].Split(' ');

				switch (data[0])
				{
					case "g":
					case "G":
						if (!string.IsNullOrEmpty(name))
						{
							if (fillerMarking?.TransitionForward == true)
							{
								if (fillerMarking?.TransitionBackward == true)
									name += " Double Slope";
								else
									name += " Forward Slope";
							}
							else if (fillerMarking?.TransitionBackward == true)
							{
								name += " Backward Slope";
							}

							lines[i] = lines[i].Substring(0, 2) + name.Replace(",", "");
						}

						break;

					case "v":
					case "V":
						var xPos = float.Parse(data[1]);
						var yPos = float.Parse(data[2]);

						if (fillerMarking != null)
						{
							if (xPos <= -0.05)
							{
								xPos = inverted ? fillerMarking.LeftPoint.X :
									(-fillerMarking.RightPoint.X + (fillerMarking.Helper ? 0 : (xPos + 0.5F)));

								if (fillerMarking.Lanes.First().Type is LaneType.Curb)
									xPos -= 0.01F - fillerMarking.Lanes.Last().Road!.BufferWidth;
								else if (fillerMarking.Type != LaneDecoration.Filler && !fillerMarking.Helper && !(fillerMarking.LeftPoint.RightLane?.FillerPadding.HasFlag(FillerPadding.Right) ?? false))
									xPos += 0.2F;
							}
							else if (xPos >= 0.05)
							{
								xPos = inverted ? fillerMarking.RightPoint.X :
									(-fillerMarking.LeftPoint.X + (fillerMarking.Helper ? 0 : (xPos - 0.5F)));

								if (fillerMarking.Lanes.Last().Type is LaneType.Curb)
									xPos += 0.01F - fillerMarking.Lanes.Last().Road!.BufferWidth;
								else if (fillerMarking.Type != LaneDecoration.Filler && !fillerMarking.Helper && !(fillerMarking.RightPoint.LeftLane?.FillerPadding.HasFlag(FillerPadding.Left) ?? false))
									xPos -= 0.2F;
							}

							if ((fillerMarking.TransitionForward && (int)float.Parse(data[3]) >= (int)ModOptions.StepTransition)
								|| (fillerMarking.TransitionBackward && -(int)float.Parse(data[3]) >= (int)ModOptions.StepTransition))
							{
								var surface = fillerMarking.SurfaceElevation;
								var elevation = fillerMarking.Elevation + (fillerMarking.Helper ? 0 : fillerMarking.Type == LaneDecoration.Filler ? 0.0025F : 0.01F);

								if (yPos == -0.3F)
								{
									yPos = surface;
								}
								else
								{
									switch (ModOptions.StepTransition)
									{
										case StepSteepness.SteepSlope:
											yPos = surface;
											break;
										case StepSteepness.ModerateSlope:
											yPos = Math.Abs(float.Parse(data[3])) == 32 ? surface : (surface + ((elevation - surface) * 0.5F));
											break;
										case StepSteepness.GentleSlope:
											yPos = Math.Abs(float.Parse(data[3])) == 32 ? surface : (surface + ((elevation - surface) * (Math.Abs(float.Parse(data[3])) < 28 ? 0.66F : 0.33F)));
											break;
									}
								}
							}
							else if (yPos == -0.3F)
								yPos = -0.02F + fillerMarking.SurfaceElevation;
							else if (!transition)
								yPos = fillerMarking.Elevation + (fillerMarking.Helper ? 0 : fillerMarking.Type == LaneDecoration.Filler ? 0.02F : 0.021F);
							else
							{
								var start = fillerMarking.Elevation;
								var end = fillerMarking.SurfaceElevation;

								yPos = Math.Max(end - 0.2F, Math.Abs(float.Parse(data[1])) == 0.5 ? -1 : (start + ((end - start) / 0.32F) + (float.Parse(data[3]) * (end - start) / 32F / 0.32F)));
							}

							if (fillerMarking.Type == LaneDecoration.Filler && originalFile.Contains("_lod.obj"))
								yPos = Math.Max(0.1F, yPos);
						}
						else if (lineMarking != null)
						{
							if (xPos <= -0.05)
							{
								xPos = -lineMarking.Point.X - (lineWidth / 2F);
							}
							else if (xPos >= 0.05)
							{
								xPos = -lineMarking.Point.X + (lineWidth / 2F);
							}

							yPos = 0.025F + lineMarking.Elevation;

							if (originalFile.Contains("_lod.obj"))
								yPos = Math.Max(0.15F, yPos);
						}
						else if (crosswalkMarking != null)
						{
							if ((xPos < 0) == crosswalkMarking.Inverted)
								xPos = crosswalkMarking.Road.Lanes.First(x => x.Type == LaneType.Curb).Position + (crosswalkMarking.Road.Lanes.First(x => x.Type == LaneType.Curb).LaneWidth / 2) - crosswalkMarking.Road.BufferWidth;
							else
								xPos = crosswalkMarking.Road.Lanes.Last(x => x.Type == LaneType.Curb).Position - (crosswalkMarking.Road.Lanes.Last(x => x.Type == LaneType.Curb).LaneWidth / 2) + crosswalkMarking.Road.BufferWidth;

							if (!crosswalkMarking.Inverted)
								xPos *= -1;

							yPos = (crosswalkMarking.Road.RoadType == RoadType.Road ? -0.3F : 0) + 0.015F;

							if (originalFile.Contains("_lod.obj"))
								yPos = Math.Max(0.15F, yPos);
						}

						data[1] = xPos.ToString("0.00000000");
						data[2] = yPos.ToString("0.00000000");

						lines[i] = string.Join(" ", data);
						break;
				}
			}

			var exportedFile = Path.Combine(BlankRoadBuilderMod.ImportFolder, Path.GetFileName(originalFile).Replace(file, guid));

			Directory.CreateDirectory(BlankRoadBuilderMod.ImportFolder);

			File.WriteAllLines(exportedFile, lines);
		}

		return guid;
	}
}
