using AdaptiveRoads.Manager;

using BlankRoadBuilder.Domain;
using BlankRoadBuilder.Domain.Options;
using BlankRoadBuilder.ThumbnailMaker;
using BlankRoadBuilder.Util.Markings;

using PrefabMetadata.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using static AdaptiveRoads.Manager.NetInfoExtionsion;

namespace BlankRoadBuilder.Util;

public static class MeshUtil
{
	private static AssetModel GetModel(CurbType id, RoadAssetType type, RoadInfo roadInfo, ElevationType elevation, string name, bool inverted = false, bool sidewalkTransition = false)
		=> AssetUtil.ImportAsset(roadInfo, MeshType.Road, elevation, type, id, name + (inverted ? " Inverted" : ""), inverted, sidewalkTransition);

	public static void UpdateMeshes(RoadInfo roadInfo, NetInfo netInfo, ElevationType elevation)
	{
		if (elevation == ElevationType.Bridge) elevation = ElevationType.Elevated;

		if (elevation > ElevationType.Elevated) return;

		var segments = GetSegments(elevation, roadInfo);
		var nodes = GetNodes(elevation, roadInfo);

		if (ModOptions.MarkingsGenerated.HasAnyFlag(MarkingsSource.VanillaMarkings, MarkingsSource.HiddenVanillaMarkings, MarkingsSource.MeshFillers))
		{
			var markings = MarkingsUtil.GenerateMarkings(roadInfo);

			segments.AddRange(NetworkMarkings.Markings(roadInfo, netInfo, markings));

			segments.AddRange(NetworkMarkings.IMTHelpers(roadInfo, netInfo, markings));

			nodes.AddRange(NetworkMarkings.IMTNodeHelpers(roadInfo, netInfo, markings));
		}

		netInfo.m_segments = segments.ToArray();
		netInfo.m_nodes = nodes.ToArray();

		var data = netInfo.GetMetaData();
		var tracks = new List<Track>();

		tracks.AddRange(GenerateBarriers(netInfo, roadInfo));

		if (roadInfo.ContainsWiredLanes)
		{
			tracks.AddRange(GenerateTracksAndWires(netInfo, roadInfo));
		}

		data.Tracks = tracks.ToArray();
		data.TrackLaneCount = tracks.Count;
	}

	private static List<NetInfo.Segment> GetSegments(ElevationType elevation, RoadInfo roadInfo)
	{
		var list = new List<NetInfo.Segment>();

		MeshInfo<NetInfo.Segment, Segment> highCurb, lowCurb, fullLowCurb, curbless;

		highCurb = new(GetModel(CurbType.HC, RoadAssetType.Segment, roadInfo, elevation, "High Curb"));
		curbless = new(GetModel(CurbType.Curbless, RoadAssetType.Segment, roadInfo, elevation, "Curbless"));

		highCurb.Mesh.m_forwardForbidden |= NetSegment.Flags.Invert;
		highCurb.Mesh.m_backwardRequired |= NetSegment.Flags.Invert;

		curbless.MetaData.Forward.Required |= RoadUtils.S_Curbless;
		curbless.MetaData.Backward.Required |= RoadUtils.S_Curbless;
		curbless.MetaData.Forward.Forbidden = NetSegmentExt.Flags.None;
		curbless.MetaData.Backward.Forbidden = NetSegmentExt.Flags.None;
		curbless.Mesh.m_forwardForbidden |= NetSegment.Flags.Bend | NetSegment.Flags.Invert;
		curbless.Mesh.m_backwardForbidden |= NetSegment.Flags.Bend;
		highCurb.Mesh.m_backwardRequired |= NetSegment.Flags.Invert;

		list.Add(highCurb);
		list.Add(curbless);

		if (elevation == ElevationType.Basic && roadInfo.RoadType == RoadType.Road)
		{
			lowCurb = new(GetModel(CurbType.LCS, RoadAssetType.Segment, roadInfo, elevation, "Low Curb"));
			fullLowCurb = new(GetModel(CurbType.LCF, RoadAssetType.Segment, roadInfo, elevation, "Full Low Curb"));

			highCurb.MetaData.Forward.Forbidden |= RoadUtils.S_LowCurbOnTheRight | RoadUtils.S_LowCurbOnTheLeft;
			highCurb.MetaData.Backward.Forbidden |= RoadUtils.S_LowCurbOnTheRight | RoadUtils.S_LowCurbOnTheLeft;

			lowCurb.Mesh.m_forwardForbidden |= NetSegment.Flags.Bend;
			lowCurb.Mesh.m_backwardForbidden |= NetSegment.Flags.Bend;
			lowCurb.MetaData.Forward.Required |= RoadUtils.S_LowCurbOnTheRight;
			lowCurb.MetaData.Backward.Required |= RoadUtils.S_LowCurbOnTheLeft;
			lowCurb.MetaData.Forward.Forbidden |= RoadUtils.S_LowCurbOnTheLeft;
			lowCurb.MetaData.Backward.Forbidden |= RoadUtils.S_LowCurbOnTheRight;

			fullLowCurb.Mesh.m_forwardForbidden |= NetSegment.Flags.Bend;
			fullLowCurb.Mesh.m_backwardForbidden |= NetSegment.Flags.Bend;
			fullLowCurb.MetaData.Forward.Required |= RoadUtils.S_LowCurbOnTheRight | RoadUtils.S_LowCurbOnTheLeft;
			fullLowCurb.MetaData.Backward.Required |= RoadUtils.S_LowCurbOnTheRight | RoadUtils.S_LowCurbOnTheLeft;

			list.Add(lowCurb);
			list.Add(fullLowCurb);
		}

		//if (roadInfo.LeftPavementWidth != roadInfo.RightPavementWidth)
		//{
		//	highCurb.MetaData.Forward.Forbidden |= RoadUtils.S_Asym | RoadUtils.S_AsymInverted;
		//	highCurb.MetaData.Forward.Forbidden |= RoadUtils.S_Asym | RoadUtils.S_AsymInverted;
		//}

		return list;
	}

	private static List<NetInfo.Node> GetNodes(ElevationType elevation, RoadInfo roadInfo)
	{
		var list = new List<NetInfo.Node>();

		generateNodes(false);

		if (roadInfo.LeftPavementWidth != roadInfo.RightPavementWidth)
		{
			generateNodes(true);

			for (var i = 0; i < list.Count / 2; i++)
			{
				list[i].GetMetaData().SegmentEndFlags.Forbidden |= NetSegmentEnd.Flags.IsTailNode;
				list[i + list.Count / 2].GetMetaData().SegmentEndFlags.Required |= NetSegmentEnd.Flags.IsTailNode;
			}
		}

		void generateNodes(bool inverted)
		{
			MeshInfo<NetInfo.Node, Node> highCurb, lowCurb, fullLowCurb, transition, asymBendForward, asymBendBackward;

			highCurb = new(GetModel(CurbType.HC, RoadAssetType.Node, roadInfo, elevation, "High Curb", inverted));

			list.Add(highCurb);

			if (roadInfo.RoadType != RoadType.Road)
			{
				transition = new(GetModel(CurbType.TR, RoadAssetType.Node, roadInfo, elevation, "Transition", inverted));

				highCurb.MetaData.NodeFlags.Forbidden |= RoadUtils.N_FlatTransition;
				transition.MetaData.NodeFlags.Required |= RoadUtils.N_FlatTransition;

				list.Add(transition);

				return;
			}

			lowCurb = new(GetModel(CurbType.LCS, RoadAssetType.Node, roadInfo, elevation, "Low Curb", inverted));
			fullLowCurb = new(GetModel(CurbType.LCF, RoadAssetType.Node, roadInfo, elevation, "Full Low Curb", inverted));

			highCurb.MetaData.SegmentEndFlags.Required |= RoadUtils.S_HighCurb;

			lowCurb.Mesh.flagsForbidden = NetNode.FlagsLong.End;
			lowCurb.MetaData.NodeFlags.Forbidden |= RoadUtils.N_FullLowCurb | RoadUtils.N_ForceHighCurb;
			lowCurb.MetaData.SegmentEndFlags.Required |= NetSegmentEnd.Flags.ZebraCrossing;

			fullLowCurb.MetaData.NodeFlags.Required |= RoadUtils.N_FullLowCurb;
			fullLowCurb.MetaData.NodeFlags.Forbidden |= RoadUtils.N_ForceHighCurb;

			list.Add(lowCurb);
			list.Add(fullLowCurb);

			//if (roadInfo.LeftPavementWidth != roadInfo.RightPavementWidth)
			//{
			//	asymBendForward = new(GetModel(CurbType.HC, RoadAssetType.Node, roadInfo, elevation, "Pavement Transition", false, true));
			//	asymBendBackward = new(GetModel(CurbType.HC, RoadAssetType.Node, roadInfo, elevation, "Pavement Transition", true, true));

			//	asymBendForward.Mesh.m_directConnect = true;
			//	asymBendBackward.Mesh.m_directConnect = true;

			//	asymBendForward.MetaData.SegmentEndFlags.Required |= RoadUtils.N_Asym;
			//	asymBendBackward.MetaData.SegmentEndFlags.Required |= RoadUtils.N_AsymInverted;

			//	list.Add(asymBendForward);
			//	list.Add(asymBendBackward);
			//}
		}

		return list;
	}

	private static IEnumerable<Track> GenerateBarriers(NetInfo netInfo, RoadInfo road)
	{
		if (!road.Lanes.Any(x => x.Decorations.HasFlag(LaneDecoration.Barrier)))
			yield break;

		var lanes = road.Lanes.Where(x => x.Decorations.HasFlag(LaneDecoration.Barrier)).Select(x => x.NetLanes[0]).ToArray();

		var barriers = new AssetModel[]
		{
			AssetUtil.ImportAsset(ShaderType.Bridge, MeshType.Barriers, "Concrete Barrier.obj"),
			AssetUtil.ImportAsset(ShaderType.Bridge, MeshType.Barriers, "Soundwall.obj"),
			AssetUtil.ImportAsset(ShaderType.Bridge, MeshType.Barriers, "Left Steel Barrier.fbx"),
			AssetUtil.ImportAsset(ShaderType.Bridge, MeshType.Barriers, "Right Steel Barrier.fbx"),
			AssetUtil.ImportAsset(ShaderType.Bridge, MeshType.Barriers, "Double Steel Barrier.fbx", scale: 100F),
		};

		var tracks = barriers.Select(x => new Track(netInfo)
		{
			m_mesh = x.m_mesh,
			m_material = x.m_material,
			m_lodMesh = x.m_lodMesh,
			m_lodMaterial = x.m_lodMaterial,
			TreatBendAsSegment = true,
			RenderNode = true,
			LaneIndeces = AdaptiveNetworksUtil.GetLaneIndeces(netInfo, lanes),
			LaneFlags = new LaneInfoFlags { Forbidden = RoadUtils.L_RemoveBarrier },
			LaneTags = new LaneTagsT(new[] { "RoadBuilderBarrierLane" }),
			LaneTransitionFlags = new LaneTransitionInfoFlags { Required = RoadUtils.T_Markings }
		}).ToList();

		for (var i = 0; i < tracks.Count; i++)
		{
			tracks[i].LaneFlags.Required |= i == 1 ? RoadUtils.L_Barrier_1 : i == 2 ? RoadUtils.L_Barrier_2 : i == 3 ? RoadUtils.L_Barrier_3 : i == 4 ? RoadUtils.L_Barrier_4 : NetLaneExt.Flags.None;
			tracks[i].LaneFlags.Forbidden |= (RoadUtils.L_Barrier_1 | RoadUtils.L_Barrier_2 | RoadUtils.L_Barrier_3 | RoadUtils.L_Barrier_4) & ~tracks[i].LaneFlags.Required;

			yield return tracks[i];
		}
	}
	private static IEnumerable<Track> GenerateTracksAndWires(NetInfo netInfo, RoadInfo road)
	{
		if (road.Lanes.Any(x => x.Type.HasFlag(LaneType.Tram)))
		{
			var rev0TrackModel = AssetUtil.ImportAsset(ShaderType.Rail, MeshType.Tram, "TramTrack.obj");
			var vanillaTrackModel = AssetUtil.ImportAsset(ShaderType.Rail, MeshType.Tram, "VanillaTramTrack.obj");
			var cllusTrackModel = AssetUtil.ImportAsset(ShaderType.Rail, MeshType.Tram, "LRThlfcnrt.fbx");

			yield return new Track(netInfo)
			{
				m_mesh = rev0TrackModel.m_mesh,
				m_material = rev0TrackModel.m_material,
				m_lodMesh = rev0TrackModel.m_lodMesh,
				m_lodMaterial = rev0TrackModel.m_lodMaterial,
				NodeFlags = new NodeInfoFlags { Forbidden = RoadUtils.N_RemoveTramTracks },
				LaneFlags = new LaneInfoFlags { Required = ModOptions.TramTracks != TramTracks.Rev0 ? RoadUtils.L_TramTracks_1 : NetLaneExt.Flags.None, Forbidden = (ModOptions.TramTracks == TramTracks.Rev0 ? RoadUtils.L_TramTracks_1 : NetLaneExt.Flags.None) | RoadUtils.L_TramTracks_2 | RoadUtils.L_RemoveTramTracks },
				VerticalOffset = 0.33F
			};

			yield return new Track(netInfo)
			{
				m_mesh = vanillaTrackModel.m_mesh,
				m_material = vanillaTrackModel.m_material,
				m_lodMesh = vanillaTrackModel.m_lodMesh,
				m_lodMaterial = vanillaTrackModel.m_lodMaterial,
				NodeFlags = new NodeInfoFlags { Forbidden = RoadUtils.N_RemoveTramTracks },
				LaneFlags = new LaneInfoFlags { Required = ModOptions.TramTracks == TramTracks.Vanilla ? NetLaneExt.Flags.None : ModOptions.TramTracks == TramTracks.Clus ? RoadUtils.L_TramTracks_2 : RoadUtils.L_TramTracks_1, Forbidden = (ModOptions.TramTracks == TramTracks.Vanilla ? (RoadUtils.L_TramTracks_1 | RoadUtils.L_TramTracks_2) : ModOptions.TramTracks == TramTracks.Clus ? RoadUtils.L_TramTracks_1 : RoadUtils.L_TramTracks_2) | RoadUtils.L_RemoveTramTracks },
				VerticalOffset = 0.3F
			};

			yield return new Track(netInfo)
			{
				Title = "Clus's Tracks",
				m_mesh = cllusTrackModel.m_mesh,
				m_material = cllusTrackModel.m_material,
				m_lodMesh = cllusTrackModel.m_lodMesh,
				m_lodMaterial = cllusTrackModel.m_lodMaterial,
				NodeFlags = new NodeInfoFlags { Forbidden = RoadUtils.N_RemoveTramTracks },
				LaneFlags = new LaneInfoFlags { Required = ModOptions.TramTracks != TramTracks.Clus ? RoadUtils.L_TramTracks_2 : NetLaneExt.Flags.None, Forbidden = (ModOptions.TramTracks == TramTracks.Clus ? RoadUtils.L_TramTracks_2 : NetLaneExt.Flags.None) | RoadUtils.L_TramTracks_1 | RoadUtils.L_RemoveTramTracks },
				VerticalOffset = 0.3F
			};

			var railModel = AssetUtil.ImportAsset(ShaderType.Wire, MeshType.Tram, "TramRail.fbx");

			yield return new Track(netInfo)
			{
				Title = "Tram Wire",
				m_mesh = railModel.m_mesh,
				m_material = railModel.m_material,
				m_lodMesh = railModel.m_mesh,
				m_lodMaterial = railModel.m_material,
				NodeFlags = new NodeInfoFlags { Forbidden = RoadUtils.N_RemoveTramWires },
				SegmentFlags = new SegmentInfoFlags { Forbidden = RoadUtils.S_RemoveTramSupports },
				VerticalOffset = 0.3F,
			};
		}

		if (road.Lanes.Any(x => x.Type.HasFlag(LaneType.Trolley)))
		{
			var railModelLeft = AssetUtil.ImportAsset(ShaderType.Wire, MeshType.Tram, "TramRail.fbx");
			var railModelRight = AssetUtil.ImportAsset(ShaderType.Wire, MeshType.Tram, "TramRail.fbx");

			yield return new Track(netInfo)
			{
				Title = "Trolley Left Wire",
				m_mesh = railModelLeft.m_mesh,
				m_material = railModelLeft.m_material,
				m_lodMesh = railModelLeft.m_mesh,
				m_lodMaterial = railModelLeft.m_material,
				NodeFlags = new NodeInfoFlags { Forbidden = RoadUtils.N_RemoveTramWires },
				SegmentFlags = new SegmentInfoFlags { Forbidden = RoadUtils.S_RemoveTramSupports },
				VerticalOffset = -4.55F,
				LaneTags = new LaneTagsT(new[] { "RoadBuilderLeftTrolleyWire" }),
				LaneIndeces = AdaptiveNetworksUtil.GetLaneIndeces(netInfo, VehicleInfo.VehicleType.TrolleybusLeftPole)
			};

			yield return new Track(netInfo)
			{
				Title = "Trolley Right Wire",
				m_mesh = railModelRight.m_mesh,
				m_material = railModelRight.m_material,
				m_lodMesh = railModelRight.m_mesh,
				m_lodMaterial = railModelRight.m_material,
				NodeFlags = new NodeInfoFlags { Forbidden = RoadUtils.N_RemoveTramWires },
				SegmentFlags = new SegmentInfoFlags { Forbidden = RoadUtils.S_RemoveTramSupports },
				VerticalOffset = -4.55F,
				LaneTags = new LaneTagsT(new[] { "RoadBuilderRightTrolleyWire" }),
				LaneIndeces = AdaptiveNetworksUtil.GetLaneIndeces(netInfo, VehicleInfo.VehicleType.TrolleybusRightPole)
			};
		}

		if (!road.WiredLanesAreNextToMedians /*|| road.AsphaltWidth <= 10F*/)
		{
			var supportModel = AssetUtil.ImportAsset(ShaderType.Wire, MeshType.Tram, "TramSupport.fbx");

			yield return new Track(netInfo)
			{
				Title = "Support Wires",
				m_mesh = supportModel.m_mesh,
				m_material = supportModel.m_material,
				m_lodMesh = supportModel.m_mesh,
				m_lodMaterial = supportModel.m_material,
				SegmentFlags = new SegmentInfoFlags { Forbidden = RoadUtils.S_RemoveTramSupports },
				VerticalOffset = -0.175F,
				RenderNode = false,
				ScaleToLaneWidth = true,
				LaneCount = 1,
				LaneIndeces = 1UL
			};
		}
	}
}