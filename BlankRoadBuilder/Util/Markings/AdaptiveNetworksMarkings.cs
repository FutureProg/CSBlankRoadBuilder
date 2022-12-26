using BlankRoadBuilder.Domain;
using BlankRoadBuilder.Domain.Options;
using BlankRoadBuilder.ThumbnailMaker;

using ColossalFramework.Importers;

using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

using UnityEngine;

using static AdaptiveRoads.Manager.NetInfoExtionsion;

using static BlankRoadBuilder.Util.Markings.MarkingStyleUtil;

namespace BlankRoadBuilder.Util.Markings;
public class AdaptiveNetworksMarkings
{
	public static List<Track> Markings(NetInfo netInfo, MarkingsInfo markings)
	{
		var tracks = new List<Track>();

		foreach (var item in markings.Fillers)
		{
			if (item.Type == LaneDecoration.Filler && item.LeftPoint.LeftLane != null)
			{
				var filler = GetLaneFiller(netInfo, item.LeftPoint.LeftLane, GetFillerMarkingInfo(item.LeftPoint.LeftLane?.Type ?? LaneType.Empty));

				if (filler != null)
					tracks.Add(filler);
			}
			else if (item.LeftPoint.LeftLane != null)
			{
				var filler = GetFiller(netInfo, item);

				if (item.Type != LaneDecoration.Pavement && item.Lanes.Any(x => x.Decorations.HasFlag(LaneDecoration.TransitStop)))
				{
					item.Type = LaneDecoration.Pavement;

					var pavementFiller = GetFiller(netInfo, item);

					filler.VanillaSegmentFlags.Forbidden |= NetSegment.Flags.StopAll;
					pavementFiller.SegmentFlags.Required |= RoadUtils.S_AnyStop;

					tracks.Add(pavementFiller);
				}

				tracks.Add(filler);
			}
		}

		foreach (var item in markings.Lines.Values)
		{
			tracks.AddRange(GetMarkings(netInfo, item));
		}

		foreach (var track in tracks)
		{
			if (ModOptions.KeepMarkingsHiddenByDefault)
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

	private static Track GetFiller(NetInfo netInfo, FillerMarking fillerIndex)
	{
		var mesh = GenerateMesh(true, false, false, 0F, 0F, fillerIndex.LeftPoint.RightLane, fillerIndex, $"{fillerIndex.LeftPoint.RightLane?.Type} {fillerIndex.Type} Median");

		GenerateTexture(true, mesh, fillerIndex.Type);

		var shader = ShaderType.Basic;

		if (fillerIndex.Type == LaneDecoration.Grass)
		{
			shader = ShaderType.Basic;
		}
		else if (fillerIndex.Type == LaneDecoration.Gravel)
		{
			shader = ShaderType.Rail;
		}
		else if (fillerIndex.Type == LaneDecoration.Pavement)
		{
			shader = ShaderType.Bridge;
		}

		var model = AssetUtil.ImportAsset(shader, MeshType.Filler, mesh + ".obj", filesReady: true);

		return new Track(netInfo)
		{
			m_mesh = model.m_mesh,
			m_material = model.m_material,
			m_lodMesh = model.m_lodMesh,
			m_lodMaterial = model.m_lodMaterial,
			RenderNode = true,
			TreatBendAsSegment = true,
			LaneTags = new LaneTagsT(new[] { "RoadBuilderLane" }),
			LaneFlags = new LaneInfoFlags { Forbidden = RoadUtils.L_RemoveFiller },
			LaneIndeces = AdaptiveNetworksUtil.GetLaneIndeces(netInfo, fillerIndex.LeftPoint.RightLane.NetLanes[0]),
			Tiling = 2F,
			VerticalOffset = 0.1F,
			//VerticalOffset = (float)Math.Round(offset ?? (lane.Type == LaneType.Pedestrian ? -0.25F : (ModOptions.FillerHeight - 0.3F)), 6),
			LaneTransitionFlags = new LaneTransitionInfoFlags { Required = RoadUtils.T_Markings }
		};
	}

	private static Track? GetLaneFiller(NetInfo netInfo, LaneInfo lane, FillerInfo? filler)
	{
		if (filler == null)
			return null;

		var mesh = GenerateMesh(false, true, false, 0F, 0F, lane, null, $"{lane.Type} Filler");

		GenerateTexture(false, mesh, default, filler.Color, filler.MarkingStyle == MarkingFillerType.Dashed ? MarkingLineType.Dashed : MarkingLineType.Solid, filler.DashLength, filler.DashSpace);

		var model = AssetUtil.ImportAsset(ShaderType.Bridge, MeshType.Markings, mesh + ".obj", filesReady: true);

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
			LaneIndeces = AdaptiveNetworksUtil.GetLaneIndeces(netInfo, lane.NetLanes[0]),
			VerticalOffset = 0.0025F,
			LaneTransitionFlags = new LaneTransitionInfoFlags { Required = RoadUtils.T_Markings }
		};
	}

	private static IEnumerable<Track> GetMarkings(NetInfo netInfo, LineMarking marking)
	{
		if (marking.Marking == GenericMarkingType.None)
			yield break;

		var rht = GetMarking(netInfo, marking.Point.RightLane, true, marking.Marking);
		var lht = GetMarking(netInfo, marking.Point.LeftLane, false, marking.Marking);

		if (rht == null || lht == null)
		{
			yield break;
		}

		rht.SegmentFlags.Forbidden |= AdaptiveRoads.Manager.NetSegmentExt.Flags.LeftHandTraffic;
		lht.SegmentFlags.Required |= AdaptiveRoads.Manager.NetSegmentExt.Flags.LeftHandTraffic;

		yield return rht;
		yield return lht;
	}

	private static Track? GetMarking(NetInfo netInfo, LaneInfo lane, bool left, GenericMarkingType type)
	{
		var lineInfo = GetLineMarkingInfo(type);

		if (lineInfo == null || lineInfo.MarkingStyle == MarkingLineType.None)
		{
			return null;
		}

		var mesh = GenerateMesh(false, false, left, lineInfo.LineWidth, lineInfo.MarkingStyle, lane, null, $"{lane.Type} {(left ? "Left" : "Right")} {type} Line");

		GenerateTexture(false, mesh, default, lineInfo.Color, lineInfo.MarkingStyle, lineInfo.DashLength, lineInfo.DashSpace);

		var model = AssetUtil.ImportAsset(ShaderType.Bridge, MeshType.Markings, mesh + ".obj", filesReady: true);

		return new Track(netInfo)
		{
			m_mesh = model.m_mesh,
			m_material = model.m_material,
			m_lodMesh = model.m_lodMesh,
			m_lodMaterial = model.m_lodMaterial,
			RenderNode = true,
			LaneTags = new LaneTagsT(new[] { "RoadBuilderLane" }),
			TreatBendAsSegment = true,
			LaneIndeces = AdaptiveNetworksUtil.GetLaneIndeces(netInfo, lane.NetLanes[0]),
			Tiling = lineInfo.MarkingStyle == MarkingLineType.Solid ? 10F : 20F / (lineInfo.DashLength + lineInfo.DashSpace),
			LaneTransitionFlags = new LaneTransitionInfoFlags { Required = RoadUtils.T_Markings },
			VerticalOffset = 0.0075F
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

	private static string GenerateMesh(bool filler, bool laneFiller, bool right, float width, MarkingLineType type, LaneInfo lane, FillerMarking? fillerIndex, string name)
	{
		const float baseWidth = 1F;

		var diff = lane.Width / 2F - baseWidth / 2F;

		if (type == MarkingLineType.SolidDouble || type == MarkingLineType.SolidDashed || type == MarkingLineType.DashedDouble || type == MarkingLineType.DashedSolid)
			width *= 3;

		var file = !filler ? "Marking" : fillerIndex?.Type == LaneDecoration.Grass ? "GrassFiller" : "PavementFiller";

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

						if (filler && fillerIndex != null)
						{
							var minX = fillerIndex.LeftPoint.X;
							var maxX = fillerIndex.RightPoint.X;

							if (xPos <= -0.05)
							{
								xPos -= minX - baseWidth / 2F;

								if (!(fillerIndex.LeftPoint.RightLane?.FillerPadding.HasFlag(FillerPadding.Left) ?? false))
									xPos += 0.2F;
							}
							else if (xPos >= 0.05)
							{
								xPos += maxX - baseWidth / 2F;

								if (!(fillerIndex.RightPoint.LeftLane?.FillerPadding.HasFlag(FillerPadding.Right) ?? false))
									xPos -= 0.2F;
							}
						}
						else if (laneFiller)
						{
							if (xPos <= -0.05)
							{
								xPos -= diff;
							}
							else if (xPos >= 0.05)
							{
								xPos += diff;
							}
						}
						else
						{
							if (xPos <= -0.05)
							{
								xPos = (right ? lane.Width : -lane.Width) / 2F - width / 2F;
							}
							else if (xPos >= 0.05)
							{
								xPos = (right ? lane.Width : -lane.Width) / 2F + width / 2F;
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

}
