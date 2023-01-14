﻿using BlankRoadBuilder.Domain;
using BlankRoadBuilder.Domain.Options;
using BlankRoadBuilder.ThumbnailMaker;

using ColossalFramework.Importers;

using PrefabMetadata.API;
using PrefabMetadata.Helpers;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using UnityEngine;

using static AdaptiveRoads.Manager.NetInfoExtionsion;

namespace BlankRoadBuilder.Util.Markings;
public static class NetworkMarkings
{
	public static IEnumerable<NetInfo.Segment> Markings(RoadInfo roadInfo, NetInfo netInfo, MarkingsInfo markingInfo)
	{
		var fillers = new List<MeshInfo<NetInfo.Segment, Segment>>();
		var markings = new List<MeshInfo<NetInfo.Segment, Segment>>();

		foreach (var item in markingInfo.Fillers)
		{
			if (item.Type == LaneDecoration.Filler)
			{
				if (ModOptions.MarkingsGenerated.HasAnyFlag(MarkingsSource.VanillaMarkings, MarkingsSource.HiddenVanillaMarkings))
				{
					if (GetLaneFiller(roadInfo, netInfo, item) is MeshInfo<NetInfo.Segment, Segment> filler)
						markings.Add(filler);
				}
			}
			else
			{
				var filler = GetFiller(roadInfo, netInfo, item);

				if (item.Type != LaneDecoration.Pavement && item.Lanes.Any(x => x.Decorations.HasFlag(LaneDecoration.TransitStop)))
				{
					item.Type = LaneDecoration.Pavement;

					var pavementFiller = GetFiller(roadInfo, netInfo, new FillerMarking
					{
						Type = LaneDecoration.Pavement,
						Elevation = item.Elevation,
						LeftPoint = item.LeftPoint,
						RightPoint = item.RightPoint
					});

					filler.Mesh.m_forwardForbidden |= NetSegment.Flags.StopAll;
					filler.Mesh.m_backwardForbidden |= NetSegment.Flags.StopAll;

					pavementFiller.MetaData.Forward.Required |= RoadUtils.S_AnyStop;
					pavementFiller.MetaData.Backward.Required |= RoadUtils.S_AnyStop;

					fillers.Add(pavementFiller);
				}

				fillers.Add(filler);
			}
		}

		if (ModOptions.MarkingsGenerated.HasAnyFlag(MarkingsSource.VanillaMarkings, MarkingsSource.HiddenVanillaMarkings))
		{
			foreach (var item in markingInfo.Lines.Values)
			{
				if (GetMarking(roadInfo, netInfo, item) is MeshInfo<NetInfo.Segment, Segment> line)
					markings.Add(line);
			}
		}

		if (!ModOptions.MarkingsGenerated.HasAnyFlag(MarkingsSource.MeshFillers, MarkingsSource.IMTMarkings))
		{
			foreach (var filler in fillers)
			{
				if (ModOptions.MarkingsGenerated.HasFlag(MarkingsSource.HiddenVanillaMarkings))
				{
					filler.MetaData.Forward.Required |= RoadUtils.S_RemoveMarkings;
					filler.MetaData.Backward.Required |= RoadUtils.S_RemoveMarkings;
				}
				else
				{
					filler.MetaData.Forward.Forbidden |= RoadUtils.S_RemoveMarkings;
					filler.MetaData.Backward.Forbidden |= RoadUtils.S_RemoveMarkings;
				}
			}
		}

		foreach (var filler in markings)
		{
			if (ModOptions.MarkingsGenerated.HasFlag(MarkingsSource.HiddenVanillaMarkings))
			{
				filler.MetaData.Forward.Required |= RoadUtils.S_RemoveMarkings;
				filler.MetaData.Backward.Required |= RoadUtils.S_RemoveMarkings;
			}
			else
			{
				filler.MetaData.Forward.Forbidden |= RoadUtils.S_RemoveMarkings;
				filler.MetaData.Backward.Forbidden |= RoadUtils.S_RemoveMarkings;
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

	public static IEnumerable<NetInfo.Segment> IMTHelpers(RoadInfo roadInfo, NetInfo netInfo, MarkingsInfo markings)
	{
		foreach (var lane in roadInfo.Lanes)
		{
			if (lane.Elevation == null || lane.Decorations.HasAnyFlag(LaneDecoration.Grass, LaneDecoration.Gravel, LaneDecoration.Pavement))
				continue;

			if (lane.Elevation == (!lane.Tags.HasFlag(LaneTag.Sidewalk) && roadInfo.RoadType == RoadType.Road ? -0.3F : 0F))
				continue;

			var filler = GetLaneFiller(roadInfo, netInfo, new FillerMarking
			{
				LeftPoint = new MarkingPoint(lane.LeftLane, lane),
				RightPoint = new MarkingPoint(lane, lane.RightLane),
				Elevation = lane.LaneElevation,
				Helper = true,
				Type = LaneDecoration.None
			});

			if (filler == null)
				continue;

			if (lane.Decorations.HasFlag(LaneDecoration.Filler))
			{
				if (ModOptions.MarkingsGenerated.HasFlag(MarkingsSource.HiddenVanillaMarkings))
				{
					filler.Value.MetaData.Forward.Forbidden |= RoadUtils.S_RemoveMarkings;
					filler.Value.MetaData.Backward.Forbidden |= RoadUtils.S_RemoveMarkings;
				}
				else if (ModOptions.MarkingsGenerated.HasFlag(MarkingsSource.VanillaMarkings))
				{
					filler.Value.MetaData.Forward.Required |= RoadUtils.S_RemoveMarkings;
					filler.Value.MetaData.Backward.Required |= RoadUtils.S_RemoveMarkings;
				}
			}

			filler.Value.Mesh.m_forwardForbidden |= NetSegment.Flags.Invert | NetSegment.Flags.AsymForward | NetSegment.Flags.AsymBackward;
			filler.Value.Mesh.m_backwardRequired |= NetSegment.Flags.Invert;
			filler.Value.Mesh.m_backwardForbidden |= NetSegment.Flags.AsymForward | NetSegment.Flags.AsymBackward;

			yield return filler;
		}
	}

	public static IEnumerable<NetInfo.Node> IMTNodeHelpers(RoadInfo roadInfo, NetInfo netInfo, MarkingsInfo markings)
	{
		foreach (var lane in roadInfo.Lanes)
		{
			if (lane.Elevation == null || lane.Decorations.HasAnyFlag(LaneDecoration.Grass, LaneDecoration.Gravel, LaneDecoration.Pavement))
				continue;

			if (lane.Elevation == (!lane.Tags.HasFlag(LaneTag.Sidewalk) && roadInfo.RoadType == RoadType.Road ? -0.3F : 0F))
				continue;

			var fillerMarking = new FillerMarking
			{
				LeftPoint = new MarkingPoint(lane.LeftLane, lane),
				RightPoint = new MarkingPoint(lane, lane.RightLane),
				Elevation = lane.LaneElevation,
				Helper = true,
				Type = LaneDecoration.None
			};

			var filler = fillerMarking?.AN_Info;

			if (fillerMarking == null || filler == null)
				continue;

			yield return generateNode(lane, fillerMarking, filler, false, false);
			yield return generateNode(lane, fillerMarking, filler, true, false);
			yield return generateNode(lane, fillerMarking, filler, false, true);
			yield return generateNode(lane, fillerMarking, filler, true, true);
		}

		static NetInfo.Node generateNode(LaneInfo lane, FillerMarking fillerMarking, MarkingStyleUtil.FillerInfo filler, bool flipped, bool inverted)
		{
			var mesh = GenerateMesh(fillerMarking, null, $"{fillerMarking.Type} Step" + (flipped ? " Flipped" : ""), true, flipped != inverted);

			GenerateTexture(fillerMarking.Lanes.First(), false, mesh, default, filler.Color, filler.MarkingStyle == MarkingFillerType.Dashed ? MarkingLineType.Dashed : MarkingLineType.Solid, filler.DashLength, filler.DashSpace);

			var model = AssetUtil.ImportAsset(ShaderType.Bridge, MeshType.Markings, mesh + ".obj", filesReady: true);

			var node = new MeshInfo<NetInfo.Node, Node>(model);

			node.MetaData.Tiling = filler.MarkingStyle == MarkingFillerType.Dashed ? 20F / (filler.DashLength + filler.DashSpace) : 10F;
			node.MetaData.VanillaSegmentFlags.Required = inverted ? NetSegment.Flags.Invert : NetSegment.Flags.None;
			node.MetaData.SegmentEndFlags.Required = flipped ? AdaptiveRoads.Manager.NetSegmentEnd.Flags.IsStartNode : AdaptiveRoads.Manager.NetSegmentEnd.Flags.None;
			node.MetaData.SegmentEndFlags.Forbidden = flipped ? AdaptiveRoads.Manager.NetSegmentEnd.Flags.None : AdaptiveRoads.Manager.NetSegmentEnd.Flags.IsStartNode;

			if (lane.Decorations.HasFlag(LaneDecoration.Filler))
			{
				if (ModOptions.MarkingsGenerated.HasFlag(MarkingsSource.HiddenVanillaMarkings))
				{
					node.MetaData.SegmentFlags.Forbidden |= RoadUtils.S_RemoveMarkings;
					node.MetaData.SegmentFlags.Forbidden |= RoadUtils.S_RemoveMarkings;
				}
				else if (ModOptions.MarkingsGenerated.HasFlag(MarkingsSource.VanillaMarkings))
				{
					node.MetaData.SegmentFlags.Required |= RoadUtils.S_RemoveMarkings;
					node.MetaData.SegmentFlags.Required |= RoadUtils.S_RemoveMarkings;
				}
			}

			return node;
		}
	}

	private static MeshInfo<NetInfo.Segment, Segment> GetFiller(RoadInfo roadInfo, NetInfo netInfo, FillerMarking fillerMarking)
	{
		var nlane = roadInfo.Lanes.First(x => x.Tags.HasFlag(LaneTag.Damage)).NetLanes[0];

		var mesh = GenerateMesh(fillerMarking, null, $"{fillerMarking.Type} Median");

		GenerateTexture(null, true, mesh, fillerMarking.Type);

		var shader = ShaderType.Basic;

		if (fillerMarking.Type == LaneDecoration.Grass)
		{
			shader = ShaderType.Basic;
		}
		else if (fillerMarking.Type == LaneDecoration.Gravel)
		{
			shader = ShaderType.Rail;
		}
		else if (fillerMarking.Type == LaneDecoration.Pavement)
		{
			shader = ShaderType.Bridge;
		}

		var model = AssetUtil.ImportAsset(shader, MeshType.Filler, mesh + ".obj", filesReady: true);

		var segment = new MeshInfo<NetInfo.Segment, Segment>(model);

		segment.MetaData.Tiling = 2F;

		return segment;
	}

	private static MeshInfo<NetInfo.Segment, Segment>? GetLaneFiller(RoadInfo roadInfo, NetInfo netInfo, FillerMarking fillerMarking)
	{
		var filler = fillerMarking?.AN_Info;

		if (fillerMarking == null || filler == null)
			return null;

		var mesh = GenerateMesh(fillerMarking, null, $"{fillerMarking.Type} Filler");

		GenerateTexture(fillerMarking.Lanes.First(), false, mesh, default, filler.Color, filler.MarkingStyle == MarkingFillerType.Dashed ? MarkingLineType.Dashed : MarkingLineType.Solid, filler.DashLength, filler.DashSpace);

		var model = AssetUtil.ImportAsset(ShaderType.Bridge, MeshType.Markings, mesh + ".obj", filesReady: true);

		var segment = new MeshInfo<NetInfo.Segment, Segment>(model);

		segment.MetaData.Tiling = filler.MarkingStyle == MarkingFillerType.Dashed ? 20F / (filler.DashLength + filler.DashSpace) : 10F;
		
		return segment;
	}

	private static MeshInfo<NetInfo.Segment, Segment>? GetMarking(RoadInfo roadInfo, NetInfo netInfo, LineMarking marking)
	{
		var lineInfo = marking.AN_Info;

		if (lineInfo == null || lineInfo.MarkingStyle == MarkingLineType.None)
		{
			return null;
		}

		var mesh = GenerateMesh(null, marking, $"{marking.Marking} Line");

		GenerateTexture(null, false, mesh, default, lineInfo.Color, lineInfo.MarkingStyle, lineInfo.DashLength, lineInfo.DashSpace);

		var model = AssetUtil.ImportAsset(ShaderType.Bridge, MeshType.Markings, mesh + ".obj", filesReady: true);
	
		var segment = new MeshInfo<NetInfo.Segment, Segment>(model);

		segment.MetaData.Tiling = lineInfo.MarkingStyle == MarkingLineType.Solid ? 10F : 20F / (lineInfo.DashLength + lineInfo.DashSpace);

		return segment;
	}

	private static void GenerateTexture(LaneInfo? lane, bool filler, string name, LaneDecoration decoration, Color32 color = default, MarkingLineType style = default, float dashLength = 0F, float dashSpace = 0F)
	{
		var baseName = !filler ? "Marking" : decoration == LaneDecoration.Grass ? "GrassFiller" : "PavementFiller";

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
				generateRoad(file, !(lane?.Tags.HasFlag(LaneTag.Asphalt) ?? true));
			}
			else if (file.EndsWith("_p.png"))
			{
				generateRoad(file, !(lane?.Tags.HasFlag(LaneTag.Sidewalk) ?? true));
			}
			else if (file.EndsWith("_a.png"))
			{
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
						pixels[i] = i % width < width / 3 || i / width < ratio && i % width > width * 2 / 3 ? Color.white : Color.black;
						break;
					case MarkingLineType.DashedSolid:
						pixels[i] = i / width < ratio && i % width < width / 3 || i % width > width * 2 / 3 ? Color.white : Color.black;
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
	}

	private static string GenerateMesh(FillerMarking? fillerMarking, LineMarking? lineMarking, string name, bool transition = false, bool inverted = false)
	{
		var type = lineMarking?.AN_Info;
		var lineWidth = type == null ? 0F : type.LineWidth;

		if (type != null && (type.MarkingStyle == MarkingLineType.SolidDouble || type.MarkingStyle == MarkingLineType.SolidDashed || type.MarkingStyle == MarkingLineType.DashedDouble || type.MarkingStyle == MarkingLineType.DashedSolid))
			lineWidth *= 3;

		var file = fillerMarking == null || fillerMarking.Type == LaneDecoration.Filler ? "Marking" : fillerMarking.Type == LaneDecoration.Grass ? "GrassFiller" : "PavementFiller";

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
								xPos = inverted ? fillerMarking.LeftPoint.X  :
									(-fillerMarking.RightPoint.X + (fillerMarking.Helper ? 0 : (xPos + 0.5F)));

								if (fillerMarking.Type != LaneDecoration.Filler && !fillerMarking.Helper && !(fillerMarking.LeftPoint.RightLane?.FillerPadding.HasFlag(FillerPadding.Right) ?? false))
									xPos += fillerMarking.LeftPoint.RightLane?.Type == LaneType.Curb ? 0.26F : 0.2F;
							}
							else if (xPos >= 0.05)
							{
								xPos = inverted ? fillerMarking.RightPoint.X :
									(-fillerMarking.LeftPoint.X + (fillerMarking.Helper ? 0 : (xPos - 0.5F)));

								if (fillerMarking.Type != LaneDecoration.Filler && !fillerMarking.Helper && !(fillerMarking.RightPoint.LeftLane?.FillerPadding.HasFlag(FillerPadding.Left) ?? false))
									xPos -= fillerMarking.RightPoint.LeftLane?.Type == LaneType.Curb ? 0.26F : 0.2F;
							}

							if (fillerMarking.Type != LaneDecoration.Filler && yPos == -0.3F)
								yPos = -0.01F + fillerMarking.Lanes.Min(x => x.SurfaceElevation);
							else if (!transition)
								yPos = fillerMarking.Elevation + (fillerMarking.Helper ? 0 : fillerMarking.Type == LaneDecoration.Filler ? 0.0025F : 0.01F);
							else
							{
								var start = fillerMarking.Elevation;
								var end = fillerMarking.Lanes.Min(x => x.SurfaceElevation);

								yPos = Math.Max(end - 0.1F, Math.Abs(float.Parse(data[1])) == 0.5 ? -1 : (start + (end - start) / 0.32F + (float.Parse(data[3]) * (end - start) / 32F / 0.32F)));
							}
						}
						else if (lineMarking != null)
						{
							if (xPos <= -0.05)
							{
								xPos = -lineMarking.Point.X - lineWidth / 2F;
							}
							else if (xPos >= 0.05)
							{
								xPos = -lineMarking.Point.X + lineWidth / 2F;
							}

							yPos = 0.0075F + lineMarking.Elevation;
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