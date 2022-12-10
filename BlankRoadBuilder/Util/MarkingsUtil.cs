﻿using BlankRoadBuilder.Domain;
using BlankRoadBuilder.Domain.Options;
using BlankRoadBuilder.ThumbnailMaker;

using ColossalFramework.Importers;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEngine;

using static AdaptiveRoads.Manager.NetInfoExtionsion;
using static BlankRoadBuilder.Util.MarkingStyleUtil;

namespace BlankRoadBuilder.Util;
public static class MarkingsUtil
{
	public static List<Track> GenerateMarkings(NetInfo netInfo, RoadInfo roadInfo)
	{
		var tracks = new List<Track>();

		foreach (var lane in roadInfo.Lanes.Where(x => x.Tags.HasFlag(LaneTag.Asphalt)))
		{
			var currentCategory = lane.GetVehicleCategory();

			if (currentCategory == LaneVehicleCategory.Filler/* || currentCategory == LaneVehicleCategory.Pedestrian*/)
			{
				tracks.Add(GetFiller(netInfo, lane));
			}

			if (currentCategory <= LaneVehicleCategory.Pedestrian)
			{
				continue;
			}

			tracks.AddRange(GenerateLaneFillers(netInfo, lane));

			tracks.AddRange(GenerateLaneMarkings(netInfo, lane, currentCategory).SelectMany(x => x));
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

	private static IEnumerable<Track> GenerateLaneFillers(NetInfo netInfo, LaneInfo lane)
	{
		var filler = GetFillerMarkingInfo(lane.Type);

		if (filler == null || filler.MarkingStyle == MarkingFillerType.None)
		{
			yield break;
		}

		if (filler.MarkingStyle >= MarkingFillerType.Grass)
		{
			var fillerLane = lane.Duplicate(filler.MarkingStyle == MarkingFillerType.Grass ? LaneType.Grass : filler.MarkingStyle == MarkingFillerType.Pavement ? LaneType.Pavement : LaneType.Gravel);

			fillerLane.Width += 0.5125F;

			yield return GetFiller(netInfo, fillerLane, -0.285F - (lane.Position / 1000F));
		}
		else
		{
			yield return GetLaneFiller(netInfo, lane, filler);
		}
	}

	private static IEnumerable<IEnumerable<Track>> GenerateLaneMarkings(NetInfo netInfo, LaneInfo lane, LaneVehicleCategory currentCategory)
	{
		var leftLane = lane.Direction == LaneDirection.Backwards ? lane.RightLane : lane.LeftLane;
		var rightLane = lane.Direction == LaneDirection.Backwards ? lane.LeftLane : lane.RightLane;

		var leftCategory = leftLane?.GetVehicleCategory() ?? LaneVehicleCategory.Unknown;
		var rightCategory = rightLane?.GetVehicleCategory() ?? LaneVehicleCategory.Unknown;

		if ((currentCategory & (LaneVehicleCategory.Vehicle | LaneVehicleCategory.Bike | LaneVehicleCategory.Tram)) != 0)
		{
			if (rightCategory != currentCategory && rightCategory <= LaneVehicleCategory.Pedestrian)
			{
				yield return markings(true, GenericMarkingType.End);
			}

			if (leftCategory != currentCategory && leftCategory != LaneVehicleCategory.Parking)
			{
				yield return markings(false, 
					currentCategory != LaneVehicleCategory.Vehicle ? GenericMarkingType.End
					: (leftLane?.Direction == lane.Direction ? (GenericMarkingType.Normal | GenericMarkingType.Hard) : (GenericMarkingType.Flipped | GenericMarkingType.End)));
			}
		}

		if (currentCategory == LaneVehicleCategory.Parking)
		{
			if (leftCategory == LaneVehicleCategory.Vehicle)
			{
				yield return markings(false, GenericMarkingType.Parking);
			}

			if (rightCategory == LaneVehicleCategory.Vehicle)
			{
				yield return markings(true, GenericMarkingType.Parking);
			}
		}
		else
		{
			if (currentCategory == leftCategory && leftLane?.Direction != lane.Direction)
			{
				if (lane.Direction != LaneDirection.Backwards)
					yield return markings(false, GenericMarkingType.Flipped | (currentCategory == LaneVehicleCategory.Vehicle ? GenericMarkingType.Hard : (GenericMarkingType)(int)currentCategory));
			}

			if (currentCategory == leftCategory && leftLane?.Direction == lane.Direction)
			{
				yield return markings(false, GenericMarkingType.Normal | (currentCategory == LaneVehicleCategory.Vehicle ? (leftLane.Type.HasFlag(lane.Type) || lane.Type.HasFlag(leftLane.Type) ? GenericMarkingType.Soft : GenericMarkingType.Hard) : (GenericMarkingType)(int)currentCategory));
			}
		}

		IEnumerable<Track> markings(bool r, GenericMarkingType t) => GetMarkings(netInfo, lane, r, t);
	}

	private static Track GetFiller(NetInfo netInfo, LaneInfo lane, float? offset = null)
	{
		var mesh = GenerateMesh(true, false, false, 0F, 0F, lane, $"{lane.Type} Median");

		GenerateTexture(true, mesh, lane);

		var shader = ShaderType.Basic;

		if ((lane.Type & (LaneType.Grass | LaneType.Trees)) != 0)
		{
			shader = ShaderType.Basic;
		}
		else if (lane.Type.HasFlag(LaneType.Gravel))
		{
			shader = ShaderType.Rail;
		}
		else if (lane.Type.HasFlag(LaneType.Pavement))
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
			RenderNode = false,
			TreatBendAsSegment = false,
			LaneFlags = new LaneInfoFlags { Forbidden = RoadUtils.L_RemoveFiller },
			LaneIndeces = AdaptiveNetworksUtil.GetLaneIndeces(netInfo, lane.NetLanes[0]),
			Tiling = 2F,
			VerticalOffset = (float)Math.Round(offset ?? (lane.Type == LaneType.Pedestrian ? -0.25F : (ModOptions.FillerHeight - 0.3F)), 6)
		};
	}

	private static Track GetLaneFiller(NetInfo netInfo, LaneInfo lane, FillerInfo filler)
	{
		var mesh = GenerateMesh(false, true, false, 0F, 0F, lane, $"{lane.Type} Filler");

		GenerateTexture(false, mesh, lane, filler.Color, filler.MarkingStyle == MarkingFillerType.Dashed ? MarkingLineType.Dashed : MarkingLineType.Solid, filler.DashLength, filler.DashSpace);

		var model = AssetUtil.ImportAsset(ShaderType.Bridge, MeshType.Markings, mesh + ".obj", filesReady: true);

		return new Track(netInfo)
		{
			m_mesh = model.m_mesh,
			m_material = model.m_material,
			m_lodMesh = model.m_lodMesh,
			m_lodMaterial = model.m_lodMaterial,
			RenderNode = false,
			TreatBendAsSegment = false,
			Tiling = filler.MarkingStyle == MarkingFillerType.Filled ? 10F : 10F / (filler.DashLength + filler.DashSpace),
			LaneIndeces = AdaptiveNetworksUtil.GetLaneIndeces(netInfo, lane.NetLanes[0]),
			VerticalOffset = 0F
		};
	}

	private static IEnumerable<Track> GetMarkings(NetInfo netInfo, LaneInfo lane, bool right, GenericMarkingType type)
	{
		var rht = GetMarking(netInfo, lane, !right, type);
		var lht = GetMarking(netInfo, lane, right, type);

		if (rht == null || lht == null)
		{
			yield break;
		}

		rht.SegmentFlags.Forbidden |= AdaptiveRoads.Manager.NetSegmentExt.Flags.LeftHandTraffic;
		lht.SegmentFlags.Required |= AdaptiveRoads.Manager.NetSegmentExt.Flags.LeftHandTraffic;

		//if (type.HasFlag(GenericMarkingType.Soft))
		//{
		//	var rhtSolid = GetMarking(netInfo, lane, !right, type & ~GenericMarkingType.Soft | GenericMarkingType.Hard);
		//	var lhtSolid = GetMarking(netInfo, lane, right, type & ~GenericMarkingType.Soft | GenericMarkingType.Hard);

		//	if (rhtSolid == null || lhtSolid == null)
		//	{
		//		yield break;
		//	}

		//	rhtSolid.SegmentFlags = new SegmentInfoFlags { Forbidden = AdaptiveRoads.Manager.NetSegmentExt.Flags.LeftHandTraffic };
		//	lhtSolid.SegmentFlags = new SegmentInfoFlags { Required = AdaptiveRoads.Manager.NetSegmentExt.Flags.LeftHandTraffic };

		//	rhtSolid.VanillaNodeFlags = new VanillaNodeInfoFlagsLong { Required = NetNode.FlagsLong.Junction };
		//	lhtSolid.VanillaNodeFlags = new VanillaNodeInfoFlagsLong { Required = NetNode.FlagsLong.Junction };

		//	rhtSolid.VanillaSegmentFlags = new VanillaSegmentInfoFlags { Forbidden = NetSegment.Flags.Invert };
		//	lhtSolid.VanillaSegmentFlags = new VanillaSegmentInfoFlags { Forbidden = NetSegment.Flags.Invert };

		//	//yield return rhtSolid;
		//	//yield return lhtSolid;
		//}

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

		var mesh = GenerateMesh(false, false, left, lineInfo.LineWidth, lineInfo.MarkingStyle, lane, $"{lane.Type} {(left ? "Left" : "Right")} {type} Line");

		GenerateTexture(false, mesh, lane, lineInfo.Color, lineInfo.MarkingStyle, lineInfo.DashLength, lineInfo.DashSpace);

		var model = AssetUtil.ImportAsset(ShaderType.Bridge, MeshType.Markings, mesh + ".obj", filesReady: true);

		return new Track(netInfo)
		{
			m_mesh = model.m_mesh,
			m_material = model.m_material,
			m_lodMesh = model.m_lodMesh,
			m_lodMaterial = model.m_lodMaterial,
			RenderNode = false,
			TreatBendAsSegment = false,
			LaneIndeces = AdaptiveNetworksUtil.GetLaneIndeces(netInfo, lane.NetLanes[0]),
			Tiling = lineInfo.MarkingStyle == MarkingLineType.Solid ? 10F : 10F / (lineInfo.DashLength + lineInfo.DashSpace),
			VerticalOffset = 0.005F
		};
	}

	private static void GenerateTexture(bool filler, string name, LaneInfo lane, Color32 color = default, MarkingLineType style = default, float dashLength = 0F, float dashSpace = 0F)
	{
		var baseName = !filler ? "Marking" : ((lane.Type & (LaneType.Grass | LaneType.Trees)) != 0) ? "GrassFiller" : "PavementFiller";

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
			var ratio = (int)(width * (dashLength / Math.Max(0.01, (dashLength + dashSpace))));

			for (var i = 0; i < pixels.Length; i++)
			{
				switch (style)
				{
					case MarkingLineType.Solid:
						pixels[i] = Color.white;
						break;
					case MarkingLineType.SolidDouble:
						pixels[i] = (i % width < width / 3 || i % width > width * 2 / 3) ? Color.white : Color.black;
						break;
					case MarkingLineType.Dashed:
						pixels[i] = i / width < ratio ? Color.white : Color.black;
						break;
					case MarkingLineType.DashedDouble:
						pixels[i] = i / width < ratio && (i % width < width / 3 || i % width > width * 2 / 3) ? Color.white : Color.black;
						break;
					case MarkingLineType.SolidDashed:
						pixels[i] = (i % width < width / 3 || (i / width < ratio && i % width > width * 2 / 3)) ? Color.white : Color.black;
						break;
					case MarkingLineType.DashedSolid:
						pixels[i] = ((i / width < ratio && i % width < width / 3) || i % width > width * 2 / 3) ? Color.white : Color.black;
						break;
					case MarkingLineType.ZigZag:
						var y = i / width;

						if (i >= pixels.Length / 2)
							y = width - y;

						var pointIndex = Math.Max(2, Math.Min(width - 2, 2 * (y % width)));

						pixels[i] = (i % width) >= (pointIndex - 2) && (i % width) <= (pointIndex + 2) ? Color.white 
							: (i % width) >= (pointIndex - 3) && (i % width) <= (pointIndex + 3) ? Color.gray : Color.black;
						break;
				}
			}

			alphaTexture.SetPixels32(pixels);
			alphaTexture.Apply(updateMipmaps: false);

			File.WriteAllBytes(Path.Combine(BlankRoadBuilderMod.ImportFolder, Path.GetFileName(file).Replace(baseName, name)), alphaTexture.EncodeToPNG());
		}
	}

	private static string GenerateMesh(bool filler, bool laneFiller, bool right, float width, MarkingLineType type, LaneInfo lane, string name)
	{
		const float baseWidth = 1F;

		var diff = lane.Width / 2F - baseWidth / 2F;

		if (filler)
		{
			diff -= 0.175F;
		}

		if (type == MarkingLineType.SolidDouble || type == MarkingLineType.SolidDashed || type == MarkingLineType.DashedDouble || type == MarkingLineType.DashedSolid)
			width *= 3;

		var file = !filler ? "Marking" : ((lane.Type & (LaneType.Grass | LaneType.Trees)) != 0) ? "GrassFiller" : "PavementFiller";

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

						if (filler || laneFiller)
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