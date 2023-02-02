
using AdaptiveRoads.CustomScript;
using AdaptiveRoads.LifeCycle;
using AdaptiveRoads.Manager;

using BlankRoadBuilder.Domain;
using BlankRoadBuilder.Domain.Options;
using BlankRoadBuilder.Patches;
using BlankRoadBuilder.ThumbnailMaker;
using BlankRoadBuilder.Util.Props;

using ColossalFramework.IO;
using ColossalFramework.UI;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using UnityEngine;

using static AdaptiveRoads.Manager.NetInfoExtionsion;

namespace BlankRoadBuilder.Util;
public static class RoadBuilderUtil
{
	public static RoadInfo? CurrentRoad { get; private set; }
	public static bool IsBuilding { get; private set; }

	public static IEnumerable<StateInfo> Build(RoadInfo? roadInfo)
	{
		IsBuilding = true;

		ThumbnailMakerUtil.ProcessRoadInfo(roadInfo);

		if (roadInfo == null)
		{
			Debug.Log("Road info provided is (null)");

			yield break;
		}

		var gameController = GameObject.FindGameObjectWithTag("GameController");

		if (gameController == null || ToolsModifierControl.toolController.m_editPrefabInfo is not NetInfo)
		{
			yield break;
		}

		SegmentUtil.ClearNodes();

		yield return new StateInfo($"Preparing the road..");

		Exception? exception = null;

		var info = GetNetInfo(roadInfo);

		try
		{
			GenerateLaneWidthsAndPositions(roadInfo);
		}
		catch (Exception ex)
		{
			exception = ex;
		}

		var netElelvations = info.GetElevations();
		var lanes = roadInfo.Lanes;

		foreach (var elevation in netElelvations.Keys)
		{
			if (exception != null)
			{
				break;
			}

			yield return new StateInfo($"Generating the {elevation} elevation..");

			try
			{
				var netInfo = netElelvations[elevation];

				roadInfo.Lanes = new List<LaneInfo>(lanes);

				if (elevation is ElevationType.Elevated or ElevationType.Bridge)
				{
					AddBridgeBarriersAndPillar(netInfo, roadInfo);
				}

				netInfo.m_lanes = roadInfo.Lanes.SelectMany(x =>
				{
					x.NetLanes = GenerateLanes(x, roadInfo, elevation).ToList();

					return x.NetLanes;
				}).ToArray();

				if (netInfo.m_lanes.Length > 250)
				{
					throw new Exception("This road has too many lanes and will not work in-game, then road generation can not continue.");
				}

				FillNetInfo(roadInfo, elevation, netInfo);

				try
				{
					MeshUtil.UpdateMeshes(roadInfo, netInfo, elevation);
				}
				catch (Exception ex)
				{
					Debug.LogError($"Failed to update mesh for {elevation} elevation: \r\n{ex}");
				}
			}
			catch (Exception ex)
			{
				exception = ex;
			}
		}

		if (exception != null)
		{
			yield return new StateInfo(exception);
			yield break;
		}

		yield return new StateInfo($"Road generation completed, applying changes..");

		CurrentRoad = roadInfo;

		ToolsModifierControl.toolController.m_editPrefabInfo = info;
		AdaptiveNetworksUtil.Refresh();
		IsBuilding = false;

		SegmentUtil.GenerateTemplateSegments(info);
	}

	private static NetInfo GetNetInfo(RoadInfo? roadInfo)
	{
		if (AssetDataExtension.WasLastLoaded != false)
			return (NetInfo)ToolsModifierControl.toolController.m_editPrefabInfo;

		var template = PrefabCollection<NetInfo>.FindLoaded(roadInfo?.RoadType switch { RoadType.Highway or RoadType.Flat => "Highway", RoadType.Pedestrian => "Small Pedestrian Street 01", _ => "Basic Road" });

		if (template.m_netAI is RoadAI roadAI)
		{
			roadAI.m_slopeInfo = null;
			roadAI.m_tunnelInfo = null;
		}

		var info = (NetInfo)(ToolsModifierControl.toolController.m_editPrefabInfo = AssetEditorRoadUtils.InstantiatePrefab(template));
		return info;
	}

	private static void CopyProperties(object target, object origin)
	{
		var fields = target.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
		var array = fields;
		foreach (var fieldInfo in array)
		{
			if (fieldInfo.FieldType.IsArray)
			{
				var array2 = (Array)fieldInfo.GetValue(origin);
				var array3 = Array.CreateInstance(array2.GetType().GetElementType(), array2.Length);
				Array.Copy(array2, array3, array2.Length);
				fieldInfo.SetValue(target, array3);
			}
			else
			{
				fieldInfo.SetValue(target, fieldInfo.GetValue(origin));
			}
		}
	}

	private static void FillNetInfo(RoadInfo roadInfo, ElevationType elevation, NetInfo netInfo)
	{
		netInfo.m_surfaceLevel = roadInfo.RoadType == RoadType.Road ? -0.3F : 0F;
		netInfo.m_pavementWidth = roadInfo.LeftPavementWidth;
		netInfo.m_halfWidth = (float)Math.Round(roadInfo.TotalWidth / 2D, 4);
		netInfo.m_maxBuildAngle = roadInfo.RoadType == RoadType.Highway ? 60F : 90F;
		netInfo.m_createPavement = elevation == ElevationType.Basic && TextureType.Pavement == roadInfo.SideTexture;
		netInfo.m_createGravel = elevation == ElevationType.Basic && TextureType.Gravel == roadInfo.SideTexture;
		netInfo.m_createRuining = elevation == ElevationType.Basic && TextureType.Ruined == roadInfo.SideTexture;
		netInfo.m_enableBendingNodes = roadInfo.LeftPavementWidth == roadInfo.RightPavementWidth;
		netInfo.m_tags = GetTags(roadInfo).ToArray();

		RoadUtils.SetNetAi(netInfo, "m_constructionCost", GetCost(roadInfo, elevation, false));
		RoadUtils.SetNetAi(netInfo, "m_maintenanceCost", GetCost(roadInfo, elevation, true));
		RoadUtils.SetNetAi(netInfo, "m_noiseAccumulation", (int)(netInfo.m_halfWidth / 3));
		RoadUtils.SetNetAi(netInfo, "m_noiseRadius", (int)(netInfo.m_halfWidth * 2.5F));
		RoadUtils.SetNetAi(netInfo, "m_trafficLights", netInfo.m_halfWidth >= 12F);
		RoadUtils.SetNetAi(netInfo, "m_highwayRules", roadInfo.RoadType == RoadType.Highway);
		RoadUtils.SetNetAi(netInfo, "m_enableZoning", roadInfo.RoadType != RoadType.Highway && elevation == ElevationType.Basic);

		var metadata = netInfo.GetOrCreateMetaData();

		metadata.PavementWidthRight = roadInfo.RightPavementWidth;
		metadata.ParkingAngleDegrees = roadInfo.ParkingAngle switch { ParkingAngle.Horizontal => 90F, ParkingAngle.Diagonal => 60F, ParkingAngle.InvertedDiagonal => -60F, _ => 0F };

		foreach (var item in roadInfo.Lanes.Where(x => x.Tags.HasFlag(LaneTag.Damage) || x.Decorations.HasFlag(LaneDecoration.Barrier)))
		{
			if (item.Decorations.HasFlag(LaneDecoration.Barrier))
			{
				metadata.Lanes.Add(item.NetLanes[0], new Lane(item.NetLanes[0])
				{
					LaneTags = new LaneTagsT(new[] { "RoadBuilderBarrierLane" }) { Selected = new[] { "RoadBuilderBarrierLane" } }
				});
			}
		}

		foreach (var left in netInfo.m_lanes.Where(x => x.m_vehicleType == VehicleInfo.VehicleType.TrolleybusLeftPole))
		{
			metadata.Lanes.Add(left, new Lane(left)
			{
				LaneTags = new LaneTagsT(new[] { "RoadBuilderLeftTrolleyWire" }) { Selected = new[] { "RoadBuilderLeftTrolleyWire" } }
			});
		}

		foreach (var right in netInfo.m_lanes.Where(x => x.m_vehicleType == VehicleInfo.VehicleType.TrolleybusRightPole))
		{
			metadata.Lanes.Add(right, new Lane(right)
			{
				LaneTags = new LaneTagsT(new[] { "RoadBuilderRightTrolleyWire" }) { Selected = new[] { "RoadBuilderRightTrolleyWire" } }
			});
		}

		metadata.ScriptedFlags[RoadUtils.Flags.S_AnyStop] = new ExpressionWrapper(GetExpression("AnyStopFlag"), "AnyStopFlag");
		metadata.ScriptedFlags[RoadUtils.Flags.T_Markings] = new ExpressionWrapper(GetExpression("MarkingTransitionFlag"), "MarkingTransitionFlag");
		//metadata.ScriptedFlags[RoadUtils.Flags.N_FlatTransition] = new ExpressionWrapper(GetExpression("TransitionFlag"), "TransitionFlag");
		metadata.ScriptedFlags[RoadUtils.Flags.S_HighCurb] = new ExpressionWrapper(GetExpression("HighCurbFlag"), "HighCurbFlag");
		//metadata.ScriptedFlags[RoadUtils.Flags.N_Asym] = new ExpressionWrapper(GetExpression("AsymFlag"), "AsymFlag");
		//metadata.ScriptedFlags[RoadUtils.Flags.N_AsymInverted] = new ExpressionWrapper(GetExpression("AsymInvertedFlag"), "AsymInvertedFlag");
		//metadata.ScriptedFlags[RoadUtils.Flags.S_Asym] = new ExpressionWrapper(GetExpression("AsymFlag"), "AsymFlag");
		//metadata.ScriptedFlags[RoadUtils.Flags.S_AsymInverted] = new ExpressionWrapper(GetExpression("AsymInvertedFlag"), "AsymInvertedFlag");
		metadata.ScriptedFlags[RoadUtils.Flags.S_StepBackward] = new ExpressionWrapper(GetExpression("StepBackwardFlag"), "StepBackwardFlag");
		metadata.ScriptedFlags[RoadUtils.Flags.S_StepForward] = new ExpressionWrapper(GetExpression("StepForwardFlag"), "StepForwardFlag");
		metadata.ScriptedFlags[RoadUtils.Flags.S_IsTailNode] = new ExpressionWrapper(GetExpression("IsTailNodeFlag"), "IsTailNodeFlag");

		if (roadInfo.Lanes.Any(x => x.Type.HasFlag(LaneType.Trolley)))
			metadata.ScriptedFlags[RoadUtils.Flags.T_TrolleyWires] = new ExpressionWrapper(GetExpression("TrolleyWiresFlag"), "TrolleyWiresFlag");

		metadata.RenameCustomFlag(RoadUtils.Flags.S_LowCurbOnTheRight, "Low curb on the right");
		metadata.RenameCustomFlag(RoadUtils.Flags.S_LowCurbOnTheLeft, "Low curb on the left");
		metadata.RenameCustomFlag(RoadUtils.Flags.S_AddRoadDamage, "Add road damage");
		metadata.RenameCustomFlag(RoadUtils.Flags.S_RemoveRoadClutter, (ModOptions.HideRoadClutter ? "Show" : "Remove") + " road clutter");
		metadata.RenameCustomFlag(RoadUtils.Flags.S_RemoveTramSupports, "Remove tram/trolley wires & supports");
		metadata.RenameCustomFlag(RoadUtils.Flags.S_RemoveMarkings, ModOptions.MarkingsGenerated.HasFlag(MarkingsSource.HiddenVanillaMarkings) ? "Show AN markings & fillers" : "Remove AN markings & fillers");
		metadata.RenameCustomFlag(RoadUtils.Flags.S_Curbless, "Make curb-less");

		metadata.RenameCustomFlag(RoadUtils.Flags.N_FullLowCurb, "Full low curb");
		metadata.RenameCustomFlag(RoadUtils.Flags.N_ForceHighCurb, "Force high curb");
		metadata.RenameCustomFlag(RoadUtils.Flags.N_RemoveLaneArrows, "Remove lane arrows");
		metadata.RenameCustomFlag(RoadUtils.Flags.N_RemoveTramWires, "Remove tram/trolley wires");
		metadata.RenameCustomFlag(RoadUtils.Flags.N_RemoveTramTracks, "Remove tram tracks");
		metadata.RenameCustomFlag(RoadUtils.Flags.N_HideTreesCloseToIntersection, "Hide trees that are close to the intersection");
		metadata.RenameCustomFlag(RoadUtils.Flags.N_Nodeless, "Remove node mesh");

		var netLanes = netInfo.m_lanes.ToList();

		foreach (var lane in roadInfo.Lanes)
		{
			foreach (var netLane in lane.NetLanes)
			{
				var i = netLanes.IndexOf(netLane);

				metadata.RenameCustomFlag(i, RoadUtils.Flags.L_RemoveTrees, "Remove trees");
				metadata.RenameCustomFlag(i, RoadUtils.Flags.L_RemoveStreetLights, "Remove street lights");

				if (netLane.m_vehicleType == VehicleInfo.VehicleType.Tram)
				{
					metadata.RenameCustomFlag(i, RoadUtils.Flags.L_RemoveTramTracks, "Remove tram tracks");
					metadata.RenameCustomFlag(i, RoadUtils.Flags.L_TramTracks_1, ModOptions.TramTracks != TramTracks.Rev0 ? "Use Rev0's tram tracks" : "Use Vanilla tram tracks");
					metadata.RenameCustomFlag(i, RoadUtils.Flags.L_TramTracks_2, ModOptions.TramTracks == TramTracks.Clus ? "Use Vanilla tram tracks" : "Use Clus's LRT tram tracks");
				}

				if (lane.Decorations.HasFlag(LaneDecoration.Barrier))
				{
					metadata.RenameCustomFlag(i, RoadUtils.Flags.L_RemoveBarrier, "Remove barrier");
					metadata.RenameCustomFlag(i, RoadUtils.Flags.L_Barrier_1, "Use sound barrier");
					metadata.RenameCustomFlag(i, RoadUtils.Flags.L_Barrier_2, "Use left single-sided metal barrier");
					metadata.RenameCustomFlag(i, RoadUtils.Flags.L_Barrier_3, "Use right single-sided metal barrier");
					metadata.RenameCustomFlag(i, RoadUtils.Flags.L_Barrier_4, "Use double-sided metal barrier");
				}
			}
		}
	}

	private static IEnumerable<string> GetTags(RoadInfo roadInfo, ElevationType elevation)
	{
		var noAsphaltTransition = roadInfo.AsphaltStyle == AsphaltStyle.None && !(elevation == ElevationType.Basic ? (roadInfo.SideTexture == TextureType.Asphalt) : elevation <= ElevationType.Bridge && (roadInfo.BridgeSideTexture == BridgeTextureType.Asphalt));

		if (noAsphaltTransition)
			yield return "RB_NoAsphalt";
	}

	private static readonly Dictionary<string, byte[]> _loadedAssemblies = new();

	private static byte[] GetExpression(string name)
	{
		if (_loadedAssemblies.ContainsKey(name))
		{
			return _loadedAssemblies[name];
		}

		var assembly = typeof(BlankRoadBuilderMod).Assembly;

		// Get the name of the embedded resource
		var resourceName = $"{nameof(BlankRoadBuilder)}.Expressions.{name}.dll";

		// Load the embedded resource as a stream
		var stream = assembly.GetManifestResourceStream(resourceName);

		if (stream == null)
		{ throw new Exception("Could not load the expression: " + name); }

		// Convert the stream to a byte array
		byte[] bytes;
		using (var ms = new MemoryStream())
		{
			stream.CopyTo(ms);
			bytes = ms.ToArray();
		}

		return _loadedAssemblies[name] = bytes;
	}

	private static int GetCost(RoadInfo roadInfo, ElevationType elevation, bool maintenance)
	{
		var elevationCost = elevation == ElevationType.Basic ? 1F : elevation == ElevationType.Elevated ? 2.5F : elevation == ElevationType.Bridge ? 3F : 6F;
		var pavementCost = 6 * (maintenance ? ThumbnailMakerUtil.GetLaneMaintenanceCost(LaneType.Filler) : ThumbnailMakerUtil.GetLaneCost(LaneType.Filler));
		var asphaltCost = roadInfo.Lanes.Sum(x => LaneInfo.GetLaneTypes(x.Type).Max(x => maintenance ? ThumbnailMakerUtil.GetLaneMaintenanceCost(x) : ThumbnailMakerUtil.GetLaneCost(x)));

		return maintenance
			? (int)Math.Ceiling((pavementCost + (asphaltCost * elevationCost)) * 625)
			: (int)Math.Ceiling(pavementCost + (asphaltCost * elevationCost)) * 100;
	}

	private static void AddBridgeBarriersAndPillar(NetInfo netInfo, RoadInfo roadInfo)
	{
		if (!(roadInfo.Lanes.Where(x => x.Tags.HasFlag(LaneTag.Sidewalk)).FirstOrDefault()?.Decorations.HasFlag(LaneDecoration.Barrier) ?? false))
		{
			roadInfo.Lanes.Add(new LaneInfo
			{
				Type = LaneType.Empty,
				CustomWidth = 0.1F,
				Elevation = 0F,
				Decorations = LaneDecoration.Barrier,
				Position = -(roadInfo.TotalWidth / 2) + 0.45F,
				Tags = LaneTag.StackedLane
			});
		}

		if (!(roadInfo.Lanes.Where(x => x.Tags.HasFlag(LaneTag.Sidewalk)).LastOrDefault()?.Decorations.HasFlag(LaneDecoration.Barrier) ?? false))
		{
			roadInfo.Lanes.Add(new LaneInfo
			{
				Type = LaneType.Empty,
				CustomWidth = 0.1F,
				Elevation = 0F,
				Decorations = LaneDecoration.Barrier,
				Position = (roadInfo.TotalWidth / 2) - 0.45F,
				Tags = LaneTag.StackedLane
			});
		}

		if (netInfo.m_netAI is not RoadBridgeAI bridgeAI)
		{
			return;
		}

		foreach (var p in _pillars)
		{
			if (roadInfo.TotalWidth - 4.5F < p.Key)
			{
				continue;
			}

			var template = PropUtil.GetProp(p.Value);
			var pillar = (BuildingInfo)template;

			if (pillar != null)
			{
				bridgeAI.m_bridgePillarInfo = pillar;
				bridgeAI.m_bridgePillarOffset = 0.7F;
				bridgeAI.m_middlePillarInfo = null;
				bridgeAI.m_middlePillarOffset = 0F;
				break;
			}
		}
	}

	private static readonly Dictionary<float, Prop> _pillars = new()
	{
		{ 38F, Prop.Pillar38   },
		{ 30F, Prop.Pillar30   },
		{ 24F, Prop.Pillar24   },
		{ 16F, Prop.Pillar16   },
		{  0F, Prop.PillarBase },
	};

	private static void GenerateLaneWidthsAndPositions(RoadInfo roadInfo)
	{
		ModOptions.LaneSizes.Update();

		var sizeLanes = roadInfo.Lanes.Where(x => !x.Tags.HasAnyFlag(LaneTag.StackedLane));
		var leftCurb = roadInfo.Lanes.FirstOrDefault(x => x.Type == LaneType.Curb);
		var rightCurb = roadInfo.Lanes.LastOrDefault(x => x.Type == LaneType.Curb);
		var leftPavementWidth = sizeLanes.Where(x => roadInfo.Lanes.IndexOf(x) <= roadInfo.Lanes.IndexOf(leftCurb) && x.Tags.HasFlag(LaneTag.Sidewalk)).Sum(x => x.LaneWidth) - roadInfo.BufferWidth;
		var rightPavementWidth = sizeLanes.Where(x => roadInfo.Lanes.IndexOf(x) >= roadInfo.Lanes.IndexOf(rightCurb) && x.Tags.HasFlag(LaneTag.Sidewalk)).Sum(x => x.LaneWidth) - roadInfo.BufferWidth;

		roadInfo.LeftPavementWidth = Math.Max(1.5F, leftPavementWidth);
		roadInfo.RightPavementWidth = Math.Max(1.5F, rightPavementWidth);
		roadInfo.AsphaltWidth = sizeLanes.Where(x => x.Tags.HasFlag(LaneTag.Asphalt)).Sum(x => x.LaneWidth) + (2 * roadInfo.BufferWidth);
		roadInfo.TotalWidth = roadInfo.LeftPavementWidth + roadInfo.RightPavementWidth + roadInfo.AsphaltWidth;

		var index = (roadInfo.AsphaltWidth + roadInfo.LeftPavementWidth + roadInfo.RightPavementWidth) / -2 + (roadInfo.LeftPavementWidth - leftPavementWidth);

		foreach (var lane in roadInfo.Lanes.Where(x => !x.Tags.HasFlag(LaneTag.StackedLane)))
		{
			lane.Position = r(index + (lane.LaneWidth / 2F));
			index = r(index + lane.LaneWidth);
		}

		if (roadInfo.VanillaWidth)
		{
			var newWidth = (float)(16 * Math.Ceiling((roadInfo.TotalWidth - 1F) / 16D));
			var diff = newWidth - roadInfo.TotalWidth;

			roadInfo.TotalWidth = newWidth;
			roadInfo.LeftPavementWidth += diff / 2;
			roadInfo.RightPavementWidth += diff / 2;
		}

		if (roadInfo.RoadWidth > roadInfo.TotalWidth)
		{
			var diff = roadInfo.RoadWidth - roadInfo.TotalWidth;

			roadInfo.TotalWidth = roadInfo.RoadWidth;
			roadInfo.LeftPavementWidth += diff / 2;
			roadInfo.RightPavementWidth += diff / 2;
		}

		TrafficLightsUtil.GetTrafficLights(roadInfo);

		if (roadInfo.Lanes.Count(x => x.Tags.HasFlag(LaneTag.CenterMedian)) > 1)
		{
			var first = true;

			foreach (var item in roadInfo.Lanes
				.Where(x => x.Tags.HasFlag(LaneTag.CenterMedian))
				.OrderBy(x => Math.Abs(x.Position)))
			{
				if (!first || Math.Abs(item.Position) > 3F)
				{
					item.Tags &= ~LaneTag.CenterMedian;
				}

				first = false;
			}
		}

		if (roadInfo.ContainsWiredLanes)
		{
			var leftPole = roadInfo.Lanes.FirstOrDefault(x => x.Tags.HasFlag(LaneTag.WirePoleLane)) ?? leftCurb;
			var rightPole = roadInfo.Lanes.LastOrDefault(x => x.Tags.HasFlag(LaneTag.WirePoleLane)) ?? rightCurb;

			roadInfo.Lanes[0].Position = r(leftPole.Position + ((rightPole.Position - leftPole.Position) / 2F));
			roadInfo.Lanes[0].CustomWidth = r(rightPole.Position - leftPole.Position);
		}

		static float r(float f) => (float)Math.Round(f, 3);
	}

	private static IEnumerable<NetInfo.Lane> GenerateLanes(LaneInfo lane, RoadInfo road, ElevationType elevation)
	{
		var index = 0;

		foreach (var laneType in LaneInfo.GetLaneTypes(lane.Type))
		{
			yield return getLane(index++, laneType, lane, road, elevation);
		}

		if (lane.Type != LaneType.Pedestrian && lane.Decorations.HasFlag(LaneDecoration.TransitStop))
		{
			var leftPed = lane.Duplicate(LaneType.Pedestrian, (lane.LaneWidth - 2.1F) / -2F);
			var rightPed = lane.Duplicate(LaneType.Pedestrian, (lane.LaneWidth - 2.1F) / 2F);

			leftPed.CustomWidth = 2F;
			leftPed.Tags &= ~LaneTag.StoppableVehicleOnRight;
			leftPed.RightLane = null;

			rightPed.CustomWidth = 2F;
			rightPed.Tags &= ~LaneTag.StoppableVehicleOnLeft;
			rightPed.LeftLane = null;

			if (leftPed.Tags.HasFlag(LaneTag.StoppableVehicleOnLeft))
			{
				yield return getLane(index++, LaneType.Pedestrian, leftPed, road, elevation);
			}

			if (rightPed.Tags.HasFlag(LaneTag.StoppableVehicleOnRight))
			{
				yield return getLane(index++, LaneType.Pedestrian, rightPed, road, elevation);
			}

			lane.Elevation = null;
		}

		if (lane.Type == LaneType.Pedestrian && (ModOptions.AlwaysAddGhostLanes || ModOptions.MarkingsGenerated.HasFlag(MarkingsSource.IMTMarkings)))
		{
			var fillerLane = lane.Duplicate(LaneType.Empty);

			fillerLane.Tags |= LaneTag.Placeholder;
			fillerLane.Elevation = null;
			fillerLane.CustomWidth = lane.LaneWidth;

			yield return getLane(index++, LaneType.Empty, fillerLane, road, elevation);
		}

		if (lane.Type.HasFlag(LaneType.Trolley))
		{
			var leftTrolley = lane.Duplicate(LaneType.Empty, lane.Direction == LaneDirection.Backwards ? 0.6F : -0.6F);
			var rightTrolley = lane.Duplicate(LaneType.Empty, lane.Direction == LaneDirection.Backwards ? -0.6F : 0.6F);

			var leftTrolleyLane = getLane(index++, LaneType.Empty, leftTrolley, road, elevation);
			var rightTrolleyLane = getLane(index++, LaneType.Empty, rightTrolley, road, elevation);

			leftTrolleyLane.m_vehicleType = VehicleInfo.VehicleType.TrolleybusLeftPole;
			rightTrolleyLane.m_vehicleType = VehicleInfo.VehicleType.TrolleybusRightPole;
			leftTrolleyLane.m_verticalOffset = 4.55F;
			rightTrolleyLane.m_verticalOffset = 4.55F;
			leftTrolleyLane.m_width = 0.1F;
			rightTrolleyLane.m_width = 0.1F;

			yield return leftTrolleyLane;
			yield return rightTrolleyLane;
		}

		static NetInfo.Lane getLane(int index, LaneType type, LaneInfo lane, RoadInfo road, ElevationType elevation) => new()
		{
			m_position = ThumbnailMakerUtil.GetLanePosition(type, lane, road),
			m_width = Math.Max(0.1F, lane.LaneWidth),
			m_verticalOffset = ThumbnailMakerUtil.GetLaneVerticalOffset(lane, road),
			m_speedLimit = ThumbnailMakerUtil.GetLaneSpeedLimit(type, lane, road),
			m_laneType = ThumbnailMakerUtil.GetLaneType(type),
			m_vehicleType = ThumbnailMakerUtil.GetVehicleType(type, lane),
			m_vehicleCategoryPart1 = ThumbnailMakerUtil.GetVehicleCategory1(type),
			m_vehicleCategoryPart2 = ThumbnailMakerUtil.GetVehicleCategory2(type),
			m_stopType = ThumbnailMakerUtil.GetStopType(type, lane, road, out _),
			m_direction = ThumbnailMakerUtil.GetLaneDirection(lane),
			m_finalDirection = ThumbnailMakerUtil.GetLaneDirection(lane),
			m_laneProps = GetLaneProps(index, type, lane, road, elevation),
			m_stopOffset = ThumbnailMakerUtil.GetStopOffset(type, lane),
			m_elevated = false,
			m_useTerrainHeight = false,
			m_centerPlatform = lane.Decorations.HasFlag(LaneDecoration.TransitStop),
			m_allowConnect = elevation == ElevationType.Basic,
			m_similarLaneCount = 1,
			m_similarLaneIndex = 0,
		};
	}

	private static NetLaneProps GetLaneProps(int index, LaneType type, LaneInfo lane, RoadInfo road, ElevationType elevation)
	{
		var laneProps = ScriptableObject.CreateInstance<NetLaneProps>();

		laneProps.m_props = new LanePropsUtil(index, type, lane, road, elevation).GetLaneProps() ?? new NetLaneProps.Prop[0];

		return laneProps;
	}
}
