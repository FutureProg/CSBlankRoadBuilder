
using AdaptiveRoads.CustomScript;
using AdaptiveRoads.LifeCycle;
using AdaptiveRoads.Manager;

using BlankRoadBuilder.Domain;
using BlankRoadBuilder.Domain.Options;
using BlankRoadBuilder.ThumbnailMaker;
using BlankRoadBuilder.Util.Props;
using BlankRoadBuilder.Util.Props.Templates;

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
	private static Dictionary<ElevationType, NetInfo>? netElelvations;
	private static Dictionary<NetInfo?, RoadInfo> currentRoadDictionary = new();

	public static bool IsBuilding { get; private set; }
	public static RoadInfo? GetRoad(ElevationType elevation)
	{
		return netElelvations?.TryGetValue(elevation, out var net) ?? false ? GetRoad(net) : null;
	}

	public static RoadInfo? GetRoad(NetInfo? net)
	{
		return currentRoadDictionary.TryGetValue(net, out var road) ? road : null;
	}

	public static IEnumerable<StateInfo> Build(RoadInfo? roadInfo)
	{
		IsBuilding = true;

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

		yield return new StateInfo($"Preparing the road..", ElevationType.Basic);

		Exception? exception = null;
		NetInfo? info = null;

		try
		{
			info = GetNetInfo(roadInfo);
		}
		catch (Exception ex)
		{
			exception = ex;
		}

		if (exception != null)
		{
			yield return new StateInfo(exception);
			yield break;
		}

		if (info == null)
		{
			yield break;
		}

		netElelvations = info.GetElevations();
		currentRoadDictionary = new();

		foreach (var elevation in netElelvations.Keys)
		{
			if (exception != null)
			{
				break;
			}

			yield return new StateInfo($"Generating the {(elevation == ElevationType.Basic ? "Ground" : elevation.ToString())} level..", elevation);

			var generatedInfo = GenerateRoadElevation(roadInfo, netElelvations[elevation], elevation, out exception);

			currentRoadDictionary[netElelvations[elevation]] = generatedInfo;
		}

		if (exception != null)
		{
			yield return new StateInfo(exception);
			yield break;
		}

		yield return new StateInfo($"Road generation completed, applying changes..");

		try
		{
			ToolsModifierControl.toolController.m_editPrefabInfo = info;
			AdaptiveNetworksUtil.Refresh();
			IsBuilding = false;

			SegmentUtil.GenerateTemplateSegments(info);
		}
		catch (Exception ex)
		{
			exception = ex;
		}

		if (exception != null)
		{
			yield return new StateInfo(exception);
			yield break;
		}
	}

	private static RoadInfo GenerateRoadElevation(RoadInfo roadInfo, NetInfo netInfo, ElevationType elevation, out Exception? exception)
	{
		try
		{
			exception = null;

			roadInfo = roadInfo.DeepCopy()!;

			if (elevation != ElevationType.Basic)
			{
				if (ModOptions.GroundOnlyGrass)
					roadInfo.Lanes.Where(x => x.Decorations.HasFlag(LaneDecoration.Grass)).ForEach(x => x.Decorations = LaneDecoration.Pavement | (x.Decorations & ~LaneDecoration.Grass));

				if (ModOptions.GroundOnlyParking)
					roadInfo.Lanes.RemoveAll(x => x.Type == LaneType.Parking);
			}

			ThumbnailMakerUtil.ProcessRoadInfo(roadInfo);

			GenerateLaneWidthsAndPositions(roadInfo);

			if (elevation == ElevationType.Basic || !ModOptions.GroundOnlyStops)
				StopsUtil.ProcessStopsInfo(roadInfo);

			if (elevation is ElevationType.Elevated or ElevationType.Bridge || !roadInfo.Lanes.Any(x => x.Decorations.HasFlag(LaneDecoration.Barrier)))
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
				exception = ex;

				Debug.LogError($"Failed to update mesh for {elevation} elevation: \r\n{ex}");
			}
		}
		catch (Exception ex)
		{
			exception = ex;
		}

		return roadInfo;
	}

	private static NetInfo GetNetInfo(RoadInfo roadInfo)
	{
		if (AssetDataExtension.WasLastLoaded != false)
			return (NetInfo)ToolsModifierControl.toolController.m_editPrefabInfo;

		NetInfo template;

		if (roadInfo.RoadType is RoadType.Highway or RoadType.Flat)
		{
			template = PrefabCollection<NetInfo>.FindLoaded("Highway");
		}
		else if (roadInfo.RoadType is RoadType.Pedestrian)
		{
			template = PrefabCollection<NetInfo>.FindLoaded("Small Pedestrian Street 01");
		}
		else
		{
			template = PrefabCollection<NetInfo>.FindLoaded((int)Math.Min(3, Math.Ceiling(roadInfo.TotalRoadWidth / 8) - 1) switch
			{
				(int)RoadClass.LargeRoad => "Large Road",
				(int)RoadClass.MediumRoad => "Medium Road",
				_ => "Basic Road"
			});
		}

		if (roadInfo.DisabledElevations is not null && template.m_netAI is RoadAI roadAI)
		{
			if (roadInfo.DisabledElevations.Contains(RoadElevation.Elevated))
			{
				roadAI.m_elevatedInfo = null;
			}
			
			if (roadInfo.DisabledElevations.Contains(RoadElevation.Bridge))
			{
				roadAI.m_bridgeInfo = null;
			}
			
			if (roadInfo.DisabledElevations.Contains(RoadElevation.Tunnel))
			{
				roadAI.m_slopeInfo = null;
				roadAI.m_tunnelInfo = null;
			}
		}

		var info = (NetInfo)(ToolsModifierControl.toolController.m_editPrefabInfo = AssetEditorRoadUtils.InstantiatePrefab(template, true));
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
		netInfo.m_tags = GetTags(roadInfo, elevation).ToArray();
		netInfo.m_canCrossLanes = roadInfo.CanCrossLanes;

		if (roadInfo.RoadType == RoadType.Road)
		{
			var roadClass = roadInfo.TotalWidth <= 16 ? RoadClass.SmallRoad : roadInfo.TotalWidth <= 32 ? RoadClass.MediumRoad : RoadClass.LargeRoad;
			var itemClass = ScriptableObject.CreateInstance<ItemClass>();
			{
				itemClass.m_layer = ItemClass.Layer.Default;
				itemClass.m_service = ItemClass.Service.Road;
				itemClass.m_subService = ItemClass.SubService.None;
				itemClass.m_level = (ItemClass.Level)(int)roadClass;
				itemClass.name = roadClass.ToString().FormatWords();
			}

			netInfo.m_class = itemClass;
		}

		RoadUtils.SetNetAi(netInfo, "m_constructionCost", GetCost(roadInfo, elevation, false));
		RoadUtils.SetNetAi(netInfo, "m_maintenanceCost", GetCost(roadInfo, elevation, true));
		RoadUtils.SetNetAi(netInfo, "m_noiseAccumulation", (int)(netInfo.m_halfWidth / 3));
		RoadUtils.SetNetAi(netInfo, "m_noiseRadius", (int)(netInfo.m_halfWidth * 2.5F));
		RoadUtils.SetNetAi(netInfo, "m_trafficLights", netInfo.m_halfWidth >= 12F);
		RoadUtils.SetNetAi(netInfo, "m_highwayRules", roadInfo.HighwayRules);
		RoadUtils.SetNetAi(netInfo, "m_enableZoning", roadInfo.RoadType != RoadType.Highway && elevation == ElevationType.Basic);

		var metadata = netInfo.GetOrCreateMetaData();

		metadata.PavementWidthRight = roadInfo.RightPavementWidth;
		metadata.ParkingAngleDegrees = roadInfo.ParkingAngle switch { ParkingAngle.Horizontal => 90F, ParkingAngle.Diagonal => 60F, ParkingAngle.InvertedDiagonal => -60F, _ => 0F };

		if (elevation is ElevationType.Slope or ElevationType.Tunnel)
		{
			metadata.PavementWidthRight += 1.5F;
			netInfo.m_pavementWidth += 1.5F;
			netInfo.m_halfWidth += 1.5F;
		}

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
		metadata.ScriptedFlags[RoadUtils.Flags.S_HighCurb] = new ExpressionWrapper(GetExpression("HighCurbFlag"), "HighCurbFlag");
		metadata.ScriptedFlags[RoadUtils.Flags.S_StepBackward] = new ExpressionWrapper(GetExpression("StepBackwardFlag"), "StepBackwardFlag");
		metadata.ScriptedFlags[RoadUtils.Flags.S_StepForward] = new ExpressionWrapper(GetExpression("StepForwardFlag"), "StepForwardFlag");
		metadata.ScriptedFlags[RoadUtils.Flags.S_IsTailNode] = new ExpressionWrapper(GetExpression("IsTailNodeFlag"), "IsTailNodeFlag");

		if (roadInfo.Lanes.Any(x => x.Type.HasFlag(LaneType.Trolley)))
			metadata.ScriptedFlags[RoadUtils.Flags.T_TrolleyWires] = new ExpressionWrapper(GetExpression("TrolleyWiresFlag"), "TrolleyWiresFlag");

		metadata.RenameCustomFlag(RoadUtils.Flags.S_LowCurbOnTheRight, "Low curb on the right");
		metadata.RenameCustomFlag(RoadUtils.Flags.S_LowCurbOnTheLeft, "Low curb on the left");
		metadata.RenameCustomFlag(RoadUtils.Flags.S_AddRoadDamage, (ModOptions.HideRoadDamage ? "Show" : "Remove") + " road damage");
		metadata.RenameCustomFlag(RoadUtils.Flags.S_RemoveRoadClutter, (ModOptions.HideRoadClutter ? "Show" : "Remove") + " road clutter");
		metadata.RenameCustomFlag(RoadUtils.Flags.S_RemoveTramSupports, "Remove tram/trolley wires & supports");
		metadata.RenameCustomFlag(RoadUtils.Flags.S_RemoveMarkings, ModOptions.MarkingsGenerated.HasFlag(MarkingsSource.HiddenVanillaMarkings) ? "Show AN markings & fillers" : "Remove AN markings & fillers");
		metadata.RenameCustomFlag(RoadUtils.Flags.S_Curbless, "Make curb-less");
		metadata.RenameCustomFlag(RoadUtils.Flags.S_ToggleGrassMedian, "Switch grass medians to pavement");

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
			yield return "RB_NoAsphalt_" + roadInfo.SideTexture;
	}

	private static readonly Dictionary<string, byte[]> _loadedAssemblies = new();

	private static byte[] GetExpression(string name)
	{
		if (_loadedAssemblies.ContainsKey(name))
		{
			return _loadedAssemblies[name];
		}

		var assembly = typeof(BlankRoadBuilderMod).Assembly;
		var resourceName = $"{nameof(BlankRoadBuilder)}.Expressions.{name}.dll";
		var stream = assembly.GetManifestResourceStream(resourceName);

		if (stream == null)
		{ throw new Exception("Could not load the expression: " + name); }

		using var ms = new MemoryStream();

		stream.CopyTo(ms);

		return _loadedAssemblies[name] = ms.ToArray();
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
			var pillar = (BuildingInfo?)template;

			if (pillar != null && template is BridgePillarProp pillarProp)
			{
				bridgeAI.m_bridgePillarInfo = pillar;
				bridgeAI.m_bridgePillarOffset = pillarProp.PillarOffset;
				bridgeAI.m_middlePillarInfo = null;
				bridgeAI.m_middlePillarOffset = 0F;
				break;
			}
		}
	}

	private static readonly Dictionary<float, Prop> _pillars = new()
	{
		{ 38F, Prop.Pillar38m   },
		{ 30F, Prop.Pillar30m   },
		{ 24F, Prop.Pillar24m   },
		{ 16F, Prop.Pillar16m   },
		{  0F, Prop.PillarSmall },
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

		var index = ((roadInfo.AsphaltWidth + roadInfo.LeftPavementWidth + roadInfo.RightPavementWidth) / -2) + (roadInfo.LeftPavementWidth - leftPavementWidth);

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
			var pole = PropUtil.GetProp(Prop.TramPole);
			var leftPole = roadInfo.Lanes.FirstOrDefault(x => x.Tags.HasFlag(LaneTag.WirePoleLane)) ?? leftCurb;
			var rightPole = roadInfo.Lanes.LastOrDefault(x => x.Tags.HasFlag(LaneTag.WirePoleLane)) ?? rightCurb;
			var leftPolePosition = leftPole.Position + PropAngle(leftPole) * pole.Position.x;
			var rightPolePosition = rightPole.Position + PropAngle(rightPole) * pole.Position.x;

			roadInfo.Lanes[0].Position = r(leftPolePosition + ((rightPolePosition - leftPolePosition) / 2F));
			roadInfo.Lanes[0].CustomWidth = r(rightPolePosition - leftPolePosition);

			static int PropAngle(LaneInfo Lane)
			{
				return (Lane.Position < 0 ? -1 : 1) * (Lane.PropAngle == ThumbnailMaker.PropAngle.Right == (Lane.Direction != LaneDirection.Backwards || Lane.Type == LaneType.Curb) ? 1 : -1);
			}
		}

		static float r(float f) => (float)Math.Round(f, 3);
	}

	private static IEnumerable<NetInfo.Lane> GenerateLanes(LaneInfo lane, RoadInfo road, ElevationType elevation)
	{
		foreach (var laneType in LaneInfo.GetLaneTypes(lane.Type))
		{
			yield return getLane(laneType, lane, road, elevation);
		}

		if (!lane.Type.HasFlag(LaneType.Pedestrian) && lane.Decorations.HasFlag(LaneDecoration.TransitStop))
		{
			if (lane.Stops.LeftStopType != VehicleInfo.VehicleType.None)
			{
				var leftPed = lane.Duplicate(LaneType.Pedestrian, Math.Max(0, lane.LaneWidth - 2.1F) / -2F);

				leftPed.CustomWidth = 2F;
				leftPed.RightLane = null;
				leftPed.Stops = lane.Stops.AsLeft();

				yield return getLane(LaneType.Pedestrian, leftPed, road, elevation);
			}

			if (lane.Stops.RightStopType != VehicleInfo.VehicleType.None)
			{
				var rightPed = lane.Duplicate(LaneType.Pedestrian, Math.Max(0, lane.LaneWidth - 2.1F) / 2F);

				rightPed.CustomWidth = 2F;
				rightPed.LeftLane = null;
				rightPed.Stops = lane.Stops.AsRight();

				yield return getLane(LaneType.Pedestrian, rightPed, road, elevation);
			}

			lane.Elevation = null;
		}

		if (lane.Type == LaneType.Pedestrian && (ModOptions.AlwaysAddGhostLanes || ModOptions.MarkingsGenerated.HasFlag(MarkingsSource.IMTMarkings)))
		{
			var fillerLane = lane.Duplicate(LaneType.Empty);

			fillerLane.Tags |= LaneTag.Placeholder;
			fillerLane.Elevation = null;
			fillerLane.CustomWidth = lane.LaneWidth;

			yield return getLane(LaneType.Empty, fillerLane, road, elevation);
		}

		if (lane.Type.HasFlag(LaneType.Trolley))
		{
			var leftTrolley = lane.Duplicate(LaneType.Empty, lane.Direction == LaneDirection.Backwards ? 0.6F : -0.6F);
			var rightTrolley = lane.Duplicate(LaneType.Empty, lane.Direction == LaneDirection.Backwards ? -0.6F : 0.6F);

			var leftTrolleyLane = getLane(LaneType.Empty, leftTrolley, road, elevation);
			var rightTrolleyLane = getLane(LaneType.Empty, rightTrolley, road, elevation);

			leftTrolleyLane.m_vehicleType = VehicleInfo.VehicleType.TrolleybusLeftPole;
			rightTrolleyLane.m_vehicleType = VehicleInfo.VehicleType.TrolleybusRightPole;
			leftTrolleyLane.m_verticalOffset = 4.55F;
			rightTrolleyLane.m_verticalOffset = 4.55F;
			leftTrolleyLane.m_width = 0.1F;
			rightTrolleyLane.m_width = 0.1F;

			yield return leftTrolleyLane;
			yield return rightTrolleyLane;
		}

		NetInfo.Lane getLane(LaneType _type, LaneInfo _lane, RoadInfo _road, ElevationType _elevation) => new()
		{
			m_position = _lane.Position,
			m_width = Math.Max(0.1F, _lane.LaneWidth),
			m_verticalOffset = ThumbnailMakerUtil.GetLaneVerticalOffset(_lane, _road),
			m_speedLimit = ThumbnailMakerUtil.GetLaneSpeedLimit(_type, _lane, _road),
			m_laneType = ThumbnailMakerUtil.GetLaneType(_type),
			m_vehicleType = ThumbnailMakerUtil.GetVehicleType(_type, _lane),
			m_vehicleCategoryPart1 = ThumbnailMakerUtil.GetVehicleCategory1(_type),
			m_vehicleCategoryPart2 = ThumbnailMakerUtil.GetVehicleCategory2(_type),
			m_stopType = StopsUtil.GetStopType(_lane, _type),
			m_direction = ThumbnailMakerUtil.GetLaneDirection(_lane),
			m_finalDirection = ThumbnailMakerUtil.GetLaneDirection(_lane),
			m_laneProps = GetLaneProps(lane, _type, _lane, _road, _elevation),
			m_stopOffset = StopsUtil.GetStopOffset(_type, _lane),
			m_elevated = false,
			m_useTerrainHeight = false,
			m_centerPlatform = _lane.Decorations.HasFlag(LaneDecoration.TransitStop),
			m_allowConnect = _elevation == ElevationType.Basic,
			m_similarLaneCount = 1,
			m_similarLaneIndex = 0,
		};
	}

	private static NetLaneProps GetLaneProps(LaneInfo mainLane, LaneType type, LaneInfo lane, RoadInfo road, ElevationType elevation)
	{
		var laneProps = ScriptableObject.CreateInstance<NetLaneProps>();

		laneProps.m_props = new LanePropsUtil(mainLane, type, lane, road, elevation).GetLaneProps() ?? new NetLaneProps.Prop[0];

		return laneProps;
	}
}
