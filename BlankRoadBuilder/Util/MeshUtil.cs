using AdaptiveRoads.Manager;

using BlankRoadBuilder.Domain;
using BlankRoadBuilder.Domain.Options;
using BlankRoadBuilder.ThumbnailMaker;

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

		if (roadInfo.Options?.DoNotChangeTheRoadMeshes ?? false)
			return;

		var segments = GetSegments(elevation, roadInfo.RoadType);
		var nodes = GetNodes(roadInfo.RoadType);

		ApplyModel(segments[0], model(CurbType.HC, RoadAssetType.Segment));

		if (segments.Length > 1)
		{
			ApplyModel(segments[1], model(CurbType.LCS, RoadAssetType.Segment));
			ApplyModel(segments[2], model(CurbType.LCF, RoadAssetType.Segment));
		}

		ApplyModel(nodes[0], model(CurbType.HC, RoadAssetType.Node));

		if (nodes.Length > 1)
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

		netInfo.m_segments = segments;
		netInfo.m_nodes = nodes;

		var data = netInfo.GetMetaData();

		var tracks = MarkingsUtil.GenerateMarkings(netInfo, roadInfo);

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

		if (!road.WiredLanesAreNextToMedians || road.AsphaltWidth <= 10F)
		{
			var supportModel = AssetUtil.ImportAsset(ShaderType.Wire, MeshType.Tram, "TramSupport.fbx");

			yield return new Track(netInfo)
			{
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

	private static NetInfo.Segment[] GetSegments(ElevationType elevation, RoadType roadType)
	{
		var arr = new NetInfo.Segment[elevation == ElevationType.Basic && roadType == RoadType.Road? 3 : 1];

		for (var i = 0; i < arr.Length; i++)
		{
			arr[i] = new NetInfo.Segment().Extend().Base;
		}

		if (arr.Length == 1)
		{
			return arr;
		}
		
		(arr[0] as IInfoExtended)?.SetMetaData(new Segment(arr[0])
		{
			Forward = new SegmentInfoFlags
			{
				Forbidden = RoadUtils.S_LowCurbOnTheRight | RoadUtils.S_LowCurbOnTheLeft
			},
			Backward = new SegmentInfoFlags
			{
				Forbidden = RoadUtils.S_LowCurbOnTheRight | RoadUtils.S_LowCurbOnTheLeft
			}
		});

		arr[1].m_forwardForbidden = NetSegment.Flags.Bend;
		arr[1].m_backwardForbidden = NetSegment.Flags.Bend;
		(arr[1] as IInfoExtended)?.SetMetaData(new Segment(arr[1])
		{
			Forward = new SegmentInfoFlags
			{
				Required = RoadUtils.S_LowCurbOnTheRight,
				Forbidden = RoadUtils.S_LowCurbOnTheLeft
			},
			Backward = new SegmentInfoFlags
			{
				Required = RoadUtils.S_LowCurbOnTheLeft,
				Forbidden = RoadUtils.S_LowCurbOnTheRight
			}
		});

		arr[2].m_forwardForbidden = NetSegment.Flags.Bend;
		arr[2].m_backwardForbidden = NetSegment.Flags.Bend;
		(arr[2] as IInfoExtended)?.SetMetaData(new Segment(arr[2])
		{
			Forward = new SegmentInfoFlags
			{
				Required = RoadUtils.S_LowCurbOnTheRight | RoadUtils.S_LowCurbOnTheLeft
			},
			Backward = new SegmentInfoFlags
			{
				Required = RoadUtils.S_LowCurbOnTheRight | RoadUtils.S_LowCurbOnTheLeft
			}
		});

		return arr;
	}

	private static NetInfo.Node[] GetNodes(RoadType roadType)
	{
		var arr = new NetInfo.Node[roadType == RoadType.Road ? 5 : 2];

		for (var i = 0; i < arr.Length; i++)
		{
			arr[i] = new NetInfo.Node().Extend().Base;
			arr[i].m_tagsForbidden = new string[0];
			arr[i].m_tagsRequired = new string[0];
		}

		if (roadType != RoadType.Road)
		{
			arr[0].flagsForbidden = NetNode.FlagsLong.Transition;
			arr[1].flagsRequired = NetNode.FlagsLong.Transition;

			return arr;
		}

		(arr[0] as IInfoExtended)?.SetMetaData(new Node(arr[0])
		{
			NodeFlags = new NodeInfoFlags
			{
				Forbidden = RoadUtils.N_FullLowCurb | RoadUtils.N_ForceHighCurb
			},
			SegmentEndFlags = new SegmentEndInfoFlags
			{
				Forbidden = NetSegmentEnd.Flags.ZebraCrossing
			}
		});

		arr[1].flagsRequired = NetNode.FlagsLong.End;
		(arr[1] as IInfoExtended)?.SetMetaData(new Node(arr[1])
		{
			NodeFlags = new NodeInfoFlags
			{
				Forbidden = RoadUtils.N_FullLowCurb | RoadUtils.N_ForceHighCurb
			}
		});

		(arr[2] as IInfoExtended)?.SetMetaData(new Node(arr[2])
		{
			NodeFlags = new NodeInfoFlags
			{
				Required = RoadUtils.N_ForceHighCurb
			}
		});

		arr[3].flagsForbidden = NetNode.FlagsLong.End;
		(arr[3] as IInfoExtended)?.SetMetaData(new Node(arr[3])
		{
			NodeFlags = new NodeInfoFlags
			{
				Forbidden = RoadUtils.N_FullLowCurb | RoadUtils.N_ForceHighCurb
			},
			SegmentEndFlags = new SegmentEndInfoFlags
			{
				Required = NetSegmentEnd.Flags.ZebraCrossing
			}
		});

		(arr[4] as IInfoExtended)?.SetMetaData(new Node(arr[4])
		{
			NodeFlags = new NodeInfoFlags
			{
				Required = RoadUtils.N_FullLowCurb,
				Forbidden = RoadUtils.N_ForceHighCurb
			}
		});

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