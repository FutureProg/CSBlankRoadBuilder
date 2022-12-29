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

namespace BlankRoadBuilder.Util.Markings;
public static class AdaptiveNetworksMarkings
{
	public static List<Track> Markings(RoadInfo roadInfo, NetInfo netInfo, MarkingsInfo markings)
	{
		var tracks = new List<Track>();

		foreach (var item in markings.Fillers)
		{
			if (item.Type == LaneDecoration.Filler)
			{
				var filler = GetLaneFiller(roadInfo, netInfo, item);

				if (filler != null)
					tracks.Add(filler);
			}
			else
			{
				var filler = GetFiller(roadInfo, netInfo, item);

				//if (item.Type == LaneDecoration.Grass || item.Lanes.All(x => x.Decorations.HasAnyFlag(LaneDecoration.Tree)))
				//{
				//	var props = new List<NetLaneProps.Prop>();

				//	if (item.Lanes.All(x => x.Decorations.HasAnyFlag(LaneDecoration.Tree)))
				//		props.AddRange(LanePropsUtil.GetTrees(item.Lanes.First()));

				//	if (item.Type == LaneDecoration.Grass)
				//		props.AddRange(LanePropsUtil.GetGrassProps(item.Lanes.First()));

				//	filler.Props = props.Select(PropToTransitionProp).ToArray();
				//}

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

					filler.VanillaSegmentFlags.Forbidden |= NetSegment.Flags.StopAll;
					pavementFiller.SegmentFlags.Required |= RoadUtils.S_AnyStop;

					tracks.Add(pavementFiller);
				}

				if (filler != null)
					tracks.Add(filler);
			}
		}

		foreach (var item in markings.Lines.Values)
		{
			var track = GetMarking(roadInfo, netInfo, item);

			if (track != null)
				tracks.Add(track);
		}

		foreach (var track in tracks)
		{
			if (ModOptions.MarkingsGenerated == MarkingsSource.HiddenANMarkingsOnly || ModOptions.MarkingsGenerated == MarkingsSource.IMTWithANHelpersAndHiddenANMarkings)
			{
				track.SegmentFlags.Required |= RoadUtils.S_RemoveMarkings;
			}
			else
			{
				track.SegmentFlags.Forbidden |= RoadUtils.S_RemoveMarkings;
			}
		}

		return tracks;
	}

	private static TransitionProp PropToTransitionProp(NetLaneProps.Prop arg)
	{
		var prop = Activator.CreateInstance<TransitionProp>();

		prop.SegmentUserData = new AdaptiveRoads.Data.UserDataInfo();
		prop.Curve = new Range();
		prop.m_angle = arg.m_angle;
		prop.m_cornerAngle = arg.m_cornerAngle;
		prop.m_finalProp = arg.m_finalProp;
		prop.m_finalTree = arg.m_finalTree;
		prop.m_minLength = arg.m_minLength;
		prop.m_position = arg.m_position;
		prop.m_prop = arg.m_prop;
		prop.m_probability = arg.m_probability;
		prop.m_repeatDistance = arg.m_repeatDistance;
		prop.m_segmentOffset = arg.m_segmentOffset;
		prop.m_tree = arg.m_tree;
		prop.m_upgradable = arg.m_upgradable;

		return prop;
	}

	private static Track GetFiller(RoadInfo roadInfo, NetInfo netInfo, FillerMarking fillerMarking)
	{
		var mesh = GenerateMesh(fillerMarking, null, $"{fillerMarking.Type} Median");

		GenerateTexture(true, mesh, fillerMarking.Type);

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

		var nlane = roadInfo.Lanes.First(x => x.Tags.HasFlag(LaneTag.Damage)).NetLanes[0];

		return new Track(netInfo)
		{
			m_mesh = model.m_mesh,
			m_material = model.m_material,
			m_lodMesh = model.m_lodMesh,
			m_lodMaterial = model.m_lodMaterial,
			RenderNode = true,
			TreatBendAsSegment = true,
			LaneTags = new LaneTagsT(new[] { "RoadBuilderLane" }),
			LaneIndeces = AdaptiveNetworksUtil.GetLaneIndeces(netInfo, nlane),
			Tiling = 2F,
			VerticalOffset = 0.01F + fillerMarking.Elevation - nlane.m_verticalOffset,
			LaneTransitionFlags = new LaneTransitionInfoFlags { Required = RoadUtils.T_Markings }
		};
	}

	private static Track? GetLaneFiller(RoadInfo roadInfo, NetInfo netInfo, FillerMarking fillerMarking)
	{
		var filler = fillerMarking?.AN_Info;

		if (fillerMarking == null || filler == null)
			return null;

		var mesh = GenerateMesh(fillerMarking, null, $"{fillerMarking.Type} Filler");

		GenerateTexture(false, mesh, default, filler.Color, filler.MarkingStyle == MarkingFillerType.Dashed ? MarkingLineType.Dashed : MarkingLineType.Solid, filler.DashLength, filler.DashSpace);

		var model = AssetUtil.ImportAsset(ShaderType.Bridge, MeshType.Markings, mesh + ".obj", filesReady: true);

		var nlane = roadInfo.Lanes.First(x => x.Tags.HasFlag(LaneTag.Damage)).NetLanes[0];

		return new Track(netInfo)
		{
			m_mesh = model.m_mesh,
			m_material = model.m_material,
			m_lodMesh = model.m_lodMesh,
			m_lodMaterial = model.m_lodMaterial,
			RenderNode = true,
			TreatBendAsSegment = true,
			LaneTags = new LaneTagsT(new[] { "RoadBuilderLane" }),
			Tiling = filler.MarkingStyle == MarkingFillerType.Dashed ? 20F / (filler.DashLength + filler.DashSpace) : 10F,
			LaneIndeces = AdaptiveNetworksUtil.GetLaneIndeces(netInfo, nlane),
			VerticalOffset = 0.0025F + fillerMarking.Elevation - nlane.m_verticalOffset,
			LaneTransitionFlags = new LaneTransitionInfoFlags { Required = RoadUtils.T_Markings }
		};
	}

	private static Track? GetMarking(RoadInfo roadInfo, NetInfo netInfo, LineMarking marking)
	{
		var lineInfo = marking.AN_Info;

		if (lineInfo == null || lineInfo.MarkingStyle == MarkingLineType.None)
		{
			return null;
		}

		var mesh = GenerateMesh(null, marking, $"{marking.Marking} Line");

		GenerateTexture(false, mesh, default, lineInfo.Color, lineInfo.MarkingStyle, lineInfo.DashLength, lineInfo.DashSpace);

		var model = AssetUtil.ImportAsset(ShaderType.Bridge, MeshType.Markings, mesh + ".obj", filesReady: true);

		var nlane = roadInfo.Lanes.First(x => x.Tags.HasFlag(LaneTag.Damage)).NetLanes[0];

		return new Track(netInfo)
		{
			m_mesh = model.m_mesh,
			m_material = model.m_material,
			m_lodMesh = model.m_lodMesh,
			m_lodMaterial = model.m_lodMaterial,
			RenderNode = true,
			LaneTags = new LaneTagsT(new[] { "RoadBuilderLane" }),
			TreatBendAsSegment = true,
			LaneIndeces = AdaptiveNetworksUtil.GetLaneIndeces(netInfo, nlane),
			Tiling = lineInfo.MarkingStyle == MarkingLineType.Solid ? 10F : 20F / (lineInfo.DashLength + lineInfo.DashSpace),
			LaneTransitionFlags = new LaneTransitionInfoFlags { Required = RoadUtils.T_Markings },
			VerticalOffset = 0.0075F + marking.Elevation - nlane.m_verticalOffset
		};
	}

	private static void GenerateTexture(bool filler, string name, LaneDecoration decoration, Color32 color = default, MarkingLineType style = default, float dashLength = 0F, float dashSpace = 0F)
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
				generateRoad(file);
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

		void generateRoad(string file)
		{
			var roadTexture = new Image(file).CreateTexture().Color(new Color32((byte)(255 - color.a), (byte)(255 - color.a), (byte)(255 - color.a), 255), false);

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

	private static string GenerateMesh(FillerMarking? fillerMarking, LineMarking? lineMarking, string name)
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

						if (fillerMarking != null)
						{
							if (xPos <= -0.05)
							{
								xPos = -fillerMarking.RightPoint.X + xPos + 0.5F;

								if (fillerMarking.Type != LaneDecoration.Filler && !(fillerMarking.LeftPoint.RightLane?.FillerPadding.HasFlag(FillerPadding.Right) ?? false))
									xPos += 0.2F;
							}
							else if (xPos >= 0.05)
							{
								xPos = -fillerMarking.LeftPoint.X + xPos - 0.5F;

								if (fillerMarking.Type != LaneDecoration.Filler && !(fillerMarking.RightPoint.LeftLane?.FillerPadding.HasFlag(FillerPadding.Left) ?? false))
									xPos -= 0.2F;
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
						}

						data[1] = xPos.ToString("0.00000000");

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

	internal static IEnumerable<Track> IMTHelpers(RoadInfo roadInfo, NetInfo netInfo, MarkingsInfo markings)
	{
		throw new NotImplementedException();
	}
}
