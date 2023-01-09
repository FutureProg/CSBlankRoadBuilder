using AdaptiveRoads.Manager;

using BlankRoadBuilder.Domain;
using BlankRoadBuilder.Domain.Options;
using BlankRoadBuilder.ThumbnailMaker;
using BlankRoadBuilder.Util.Markings;
using PrefabMetadata.API;
using PrefabMetadata.Helpers;

using System.Collections.Generic;
using System.Linq;

using static AdaptiveRoads.Manager.NetInfoExtionsion;

namespace BlankRoadBuilder.Util;

public class MeshUtil
{
	public static void UpdateMeshes(RoadInfo roadInfo, NetInfo netInfo, ElevationType elevation)
	{
		if (elevation == ElevationType.Bridge)
			elevation = ElevationType.Elevated;

		if (elevation > ElevationType.Elevated)
			return;

		var segments = GetSegments(elevation, roadInfo.RoadType);
		var nodes = GetNodes(roadInfo.RoadType);

		ApplyModel(segments[0], model(CurbType.HC, RoadAssetType.Segment));

		if (segments.Count > 3)
		{
			ApplyModel(segments[1], model(CurbType.LCS, RoadAssetType.Segment));
			ApplyModel(segments[2], model(CurbType.LCF, RoadAssetType.Segment));
		}

		ApplyModel(segments[segments.Count - 1], model(CurbType.Curbless, RoadAssetType.Segment));

		ApplyModel(nodes[0], model(CurbType.HC, RoadAssetType.Node));

		if (nodes.Count > 1)
		{
			if (roadInfo.RoadType == RoadType.Road)
			{
				ApplyModel(nodes[1], model(CurbType.HC, RoadAssetType.Node));
				ApplyModel(nodes[2], model(CurbType.HC, RoadAssetType.Node));
				ApplyModel(nodes[3], model(CurbType.LCS, RoadAssetType.Node));
				ApplyModel(nodes[4], model(CurbType.LCF, RoadAssetType.Node));
			}
			else
				ApplyModel(nodes[1], model(CurbType.TR, RoadAssetType.Node));
		}

		if (ModOptions.MarkingsGenerated.HasAnyFlag(MarkingsSource.ANMarkings, MarkingsSource.HiddenANMarkings, MarkingsSource.ANFillers))
		{
			var markings = MarkingsUtil.GenerateMarkings(roadInfo);

			segments.AddRange(AdaptiveNetworksMarkings.Markings(roadInfo, netInfo, markings));

			segments.AddRange(AdaptiveNetworksMarkings.IMTHelpers(roadInfo, netInfo, markings));

			nodes.AddRange(AdaptiveNetworksMarkings.IMTNodeHelpers(roadInfo, netInfo, markings));
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

		AssetModel model(CurbType id, RoadAssetType type)
			=> AssetUtil.ImportAsset(roadInfo, MeshType.Road, elevation, type, id);
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
			var railModel = AssetUtil.ImportAsset(ShaderType.Wire, MeshType.Tram, "TramRail.fbx");

			yield return new Track(netInfo)
			{
				Title = "Trolley Wires",
				m_mesh = railModel.m_mesh,
				m_material = railModel.m_material,
				m_lodMesh = railModel.m_mesh,
				m_lodMaterial = railModel.m_material,
				NodeFlags = new NodeInfoFlags { Forbidden = RoadUtils.N_RemoveTramWires },
				SegmentFlags = new SegmentInfoFlags { Forbidden = RoadUtils.S_RemoveTramSupports },
				VerticalOffset = -4.55F,
				LaneIndeces = AdaptiveNetworksUtil.GetLaneIndeces(netInfo, VehicleInfo.VehicleType.TrolleybusLeftPole | VehicleInfo.VehicleType.TrolleybusRightPole)
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
			LaneTags = new LaneTagsT(new[] { "RoadBuilderLane" }),
			LaneTransitionFlags = new LaneTransitionInfoFlags { Required = RoadUtils.T_Markings }
		}).ToList();

		for (var i = 0; i < tracks.Count; i++)
		{
			tracks[i].LaneFlags.Required |= i == 1 ? RoadUtils.L_Barrier_1 : i == 2 ? RoadUtils.L_Barrier_2 : i == 3 ? RoadUtils.L_Barrier_3 : i == 4 ? RoadUtils.L_Barrier_4 : NetLaneExt.Flags.None;
			tracks[i].LaneFlags.Forbidden |= (RoadUtils.L_Barrier_1 | RoadUtils.L_Barrier_2 | RoadUtils.L_Barrier_3 | RoadUtils.L_Barrier_4) & ~tracks[i].LaneFlags.Required;

			yield return tracks[i];
		}
	}

	private static List<NetInfo.Segment> GetSegments(ElevationType elevation, RoadType roadType)
	{
		var arr = new List<NetInfo.Segment>();
		var meta = new List<Segment>();

		for (var i = 0; i < (elevation == ElevationType.Basic && roadType == RoadType.Road ? 4 : 2) + (elevation == ElevationType.Basic ? 0 : 0); i++)
		{
			arr.Add(new NetInfo.Segment().Extend().Base);
			meta.Add(new Segment(arr[i])
			{
				Forward = new SegmentInfoFlags { Forbidden = RoadUtils.S_Curbless },
				Backward = new SegmentInfoFlags { Forbidden = RoadUtils.S_Curbless }
			});

			(arr[i] as IInfoExtended)?.SetMetaData(meta[i]);				
		}

		meta[arr.Count - 1].Forward.Required = RoadUtils.S_Curbless;
		meta[arr.Count - 1].Backward.Required = RoadUtils.S_Curbless;
		meta[arr.Count - 1].Forward.Forbidden = NetSegmentExt.Flags.None;
		meta[arr.Count - 1].Backward.Forbidden = NetSegmentExt.Flags.None;
		
		if (arr.Count == 2)
		{
			return arr;
		}

		meta[0].Forward.Forbidden |= RoadUtils.S_LowCurbOnTheRight | RoadUtils.S_LowCurbOnTheLeft;
		meta[0].Backward.Forbidden |= RoadUtils.S_LowCurbOnTheRight | RoadUtils.S_LowCurbOnTheLeft;

		arr[1].m_forwardForbidden = NetSegment.Flags.Bend;
		arr[1].m_backwardForbidden = NetSegment.Flags.Bend;
		meta[1].Forward.Required |= RoadUtils.S_LowCurbOnTheRight;
		meta[1].Backward.Required |= RoadUtils.S_LowCurbOnTheLeft;
		meta[1].Forward.Forbidden |= RoadUtils.S_LowCurbOnTheLeft;
		meta[1].Backward.Forbidden |= RoadUtils.S_LowCurbOnTheRight;

		arr[2].m_forwardForbidden = NetSegment.Flags.Bend;
		arr[2].m_backwardForbidden = NetSegment.Flags.Bend;
		meta[2].Forward.Required |= RoadUtils.S_LowCurbOnTheRight | RoadUtils.S_LowCurbOnTheLeft;
		meta[2].Backward.Required |= RoadUtils.S_LowCurbOnTheRight | RoadUtils.S_LowCurbOnTheLeft;

		return arr;
	}

	private static List<NetInfo.Node> GetNodes(RoadType roadType)
	{
		var arr = new List<NetInfo.Node>();
		var meta = new List<Node>();

		for (var i = 0; i < (roadType == RoadType.Road ? 5 : 2); i++)
		{
			arr.Add(new NetInfo.Node().Extend().Base);
			arr[i].m_tagsForbidden = new string[0];
			arr[i].m_tagsRequired = new string[0];

			meta.Add(new Node(arr[i]) { NodeFlags = new NodeInfoFlags { Forbidden = RoadUtils.N_Nodeless } });

			(arr[i] as IInfoExtended)?.SetMetaData(meta[i]);
		}

		if (roadType != RoadType.Road)
		{
			meta[0].NodeFlags.Forbidden |= RoadUtils.N_FlatTransition;
			meta[1].NodeFlags.Required |= RoadUtils.N_FlatTransition;

			return arr;
		}

		meta[0].NodeFlags.Forbidden |= RoadUtils.N_FullLowCurb | RoadUtils.N_ForceHighCurb;
		meta[0].SegmentEndFlags.Forbidden |= NetSegmentEnd.Flags.ZebraCrossing;

		arr[1].flagsRequired = NetNode.FlagsLong.End;
		meta[1].NodeFlags.Forbidden |= RoadUtils.N_FullLowCurb | RoadUtils.N_ForceHighCurb;

		meta[2].NodeFlags.Required |= RoadUtils.N_ForceHighCurb;

		arr[3].flagsForbidden = NetNode.FlagsLong.End;
		meta[3].NodeFlags.Forbidden |= RoadUtils.N_FullLowCurb | RoadUtils.N_ForceHighCurb;
		meta[3].SegmentEndFlags.Required |= NetSegmentEnd.Flags.ZebraCrossing;

		meta[4].NodeFlags.Required |= RoadUtils.N_FullLowCurb;
		meta[4].NodeFlags.Forbidden |= RoadUtils.N_ForceHighCurb;

		return arr;
	}

	private static void ApplyModel(NetInfo.Segment segment, AssetModel model)
	{
		segment.m_mesh = model.m_mesh;
		segment.m_material = model.m_material;
		segment.m_lodMesh = model.m_lodMesh;
		segment.m_lodMaterial = model.m_lodMaterial;
	}

	private static void ApplyModel(NetInfo.Node node, AssetModel model)
	{
		node.m_mesh = model.m_mesh;
		node.m_material = model.m_material;
		node.m_lodMesh = model.m_lodMesh;
		node.m_lodMaterial = model.m_lodMaterial;
	}
}