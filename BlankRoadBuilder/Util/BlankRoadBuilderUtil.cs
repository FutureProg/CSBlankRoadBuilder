namespace BlankRoadBuilder.Util;

using AdaptiveRoads.Manager;

using BlankRoadBuilder.Domain;
using BlankRoadBuilder.Domain.Options;
using BlankRoadBuilder.Patches;
using BlankRoadBuilder.ThumbnailMaker;
using BlankRoadBuilder.Util.Props;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

public static class BlankRoadBuilderUtil
{
    public static IEnumerable<StateInfo> Build(RoadInfo? roadInfo)
    {
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

        Exception? exception = null;

        var info = (NetInfo)ToolsModifierControl.toolController.m_editPrefabInfo;
        var netElelvations = info.GetElevations();

        try
        { GenerateLaneWidthsAndPositions(roadInfo); }
        catch (Exception ex)
        { exception = ex; }

        foreach (var elevation in netElelvations)
        {
            if (exception != null)
			{
				yield return new StateInfo(exception);
				yield break;
			}

			yield return new StateInfo($"Generating the {elevation.Key} elevation..");

            try
            {
                var netInfo = elevation.Value;

                if (elevation.Key == ElevationType.Elevated || elevation.Key == ElevationType.Bridge)
                    AddBridgePillar(netInfo, roadInfo);

                netInfo.m_lanes = roadInfo.Lanes.SelectMany(x =>
                {
                    x.NetLanes = GenerateLanes(x, roadInfo, elevation.Key).ToList();

					return x.NetLanes;
                }).ToArray();

                FillNetInfo(roadInfo, elevation, netInfo);

			    try
			    {
                    MeshUtil.UpdateMeshes(roadInfo, elevation.Value, elevation.Key);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to update mesh for {elevation.Key} elevation: \r\n{ex}");
				}
			}
			catch (Exception ex)
			{
                exception = ex; 
            }
		}

		yield return new StateInfo($"Road generation completed, applying changes..");

		SavePanelPatch.LastLoadedRoad = roadInfo;

        ToolsModifierControl.toolController.m_editPrefabInfo = info;
        AdaptiveNetworksUtil.Refresh();
	}

    private static void FillNetInfo(RoadInfo roadInfo, KeyValuePair<ElevationType, NetInfo> elevation, NetInfo netInfo)
    {
        netInfo.m_surfaceLevel = roadInfo.RoadType == RoadType.Road ? -0.3F : 0F;
        netInfo.m_clipTerrain = true;
        netInfo.m_pavementWidth = 3F;
		netInfo.m_halfWidth = (float)Math.Round(roadInfo.Width / 2D + 3F, 2);
        netInfo.m_maxBuildAngle = roadInfo.RoadType == RoadType.Highway ? 45F : 90F;
		netInfo.m_createPavement = roadInfo.RoadType != RoadType.Highway && elevation.Key == ElevationType.Basic;
        netInfo.m_createGravel = roadInfo.RoadType == RoadType.Highway && elevation.Key == ElevationType.Basic;
		netInfo.m_createRuining = false;
        
        RoadUtils.SetNetAi(netInfo, "m_outsideConnection", null);
        RoadUtils.SetNetAi(netInfo, "m_constructionCost", GetCost(roadInfo, elevation.Key, false));
        RoadUtils.SetNetAi(netInfo, "m_maintenanceCost", GetCost(roadInfo, elevation.Key, true));
        RoadUtils.SetNetAi(netInfo, "m_noiseAccumulation", (int)(netInfo.m_halfWidth / 3));
        RoadUtils.SetNetAi(netInfo, "m_noiseRadius", (int)(netInfo.m_halfWidth * 2.5F));
        RoadUtils.SetNetAi(netInfo, "m_trafficLights", netInfo.m_halfWidth >= 12F);
        RoadUtils.SetNetAi(netInfo, "m_highwayRules", roadInfo.RoadType == RoadType.Highway);
        RoadUtils.SetNetAi(netInfo, "m_enableZoning", roadInfo.RoadType != RoadType.Highway && elevation.Key == ElevationType.Basic);
        
		var data = netInfo.GetOrCreateMetaData();

        data.PavementWidthRight = 3F;
        data.ParkingAngleDegrees = roadInfo.HorizontalParking ? 90F : roadInfo.DiagonalParking ? 45F : roadInfo.InvertedDiagonalParking ? -45 : 0F;

        data.RenameCustomFlag(RoadUtils.S_LowCurbOnTheRight, "Low curb on the right");
        data.RenameCustomFlag(RoadUtils.S_LowCurbOnTheLeft, "Low curb on the left");
        data.RenameCustomFlag(RoadUtils.S_AddRoadDamage, "Add road damage");
        data.RenameCustomFlag(RoadUtils.S_RemoveRoadClutter, "Remove road clutter");
		data.RenameCustomFlag(RoadUtils.S_RemoveTramSupports, "Remove tram/trolley wires & supports");
		data.RenameCustomFlag(RoadUtils.S_RemoveMarkings, ModOptions.KeepMarkingsHiddenByDefault ? "Show markings & fillers" : "Remove markings & fillers");

		data.RenameCustomFlag(RoadUtils.N_FullLowCurb, "Full low curb");
        data.RenameCustomFlag(RoadUtils.N_ForceHighCurb, "Force high curb");
        data.RenameCustomFlag(RoadUtils.N_RemoveLaneArrows, "Remove lane arrows");
        data.RenameCustomFlag(RoadUtils.N_RemoveTramWires, "Remove tram/trolley wires");
		data.RenameCustomFlag(RoadUtils.N_RemoveTramTracks, "Remove tram tracks");
		data.RenameCustomFlag(RoadUtils.N_ShowTreesCloseToIntersection, "Show trees that are close to the intersection");

		for (var i = 0; i < netInfo.m_lanes.Length; i++)
        {
			data.RenameCustomFlag(i, RoadUtils.L_RemoveTramTracks, "Remove tram tracks");
			data.RenameCustomFlag(i, RoadUtils.L_TramTracks_1, ModOptions.TramTracks != TramTracks.Rev0 ? "Use Rev0's tram tracks" : "Use Vanilla tram tracks");
			data.RenameCustomFlag(i, RoadUtils.L_TramTracks_2, ModOptions.TramTracks == TramTracks.Clus ? "Use Vanilla tram tracks" : "Use Clus's LRT tram tracks");
			data.RenameCustomFlag(i, RoadUtils.L_RemoveStreetLights, "Remove street lights");
			data.RenameCustomFlag(i, RoadUtils.L_RemoveTrees, "Remove trees");
			data.RenameCustomFlag(i, RoadUtils.L_RemoveFiller, "Remove filler");
		}
	}

	private static int GetCost(RoadInfo roadInfo, ElevationType elevation, bool maintenance)
    {
        var elevationCost = elevation == ElevationType.Basic ? 1F : elevation == ElevationType.Elevated ? 2.5F : elevation == ElevationType.Bridge ? 3F : 6F;
		var pavementCost = 6 * (maintenance ? ThumbnailMakerUtil.GetLaneMaintenanceCost(LaneType.Pavement) : ThumbnailMakerUtil.GetLaneCost(LaneType.Pavement));
        var asphaltCost = roadInfo.Lanes.Sum(x => LaneInfo.GetLaneTypes(x.Type).Max(x => maintenance ? ThumbnailMakerUtil.GetLaneMaintenanceCost(x) : ThumbnailMakerUtil.GetLaneCost(x)));

		return maintenance 
            ? (int)Math.Ceiling((pavementCost + asphaltCost * elevationCost) * 625)
            : (int)Math.Ceiling(pavementCost + asphaltCost * elevationCost) * 100;
	}

    private static void AddBridgePillar(NetInfo netInfo, RoadInfo roadInfo)
    {
        if (!(netInfo.m_netAI is RoadBridgeAI bridgeAI))
            return;

        foreach (var p in _pillars)
        {
            if (roadInfo.Width + 1.5F < p.Key)
                continue;

            var pillar = PrefabCollection<BuildingInfo>.FindLoaded(p.Value);

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

    private static readonly Dictionary<float, string> _pillars = new()
    {
        { 38F, "760278365.R69 Over 4c_Data"   },
        { 30F, "760277420.R69 Over 3c_Data"   },
        { 24F, "760276468.R69 Middle 3c_Data" },
        { 16F, "760276148.R69 Middle 2c_Data" },
        {  0F, "760289402.R69 Middle 1c_Data" },
    };

    private static void GenerateLaneWidthsAndPositions(RoadInfo roadInfo)
    {
        // calculate non-filler widths
        foreach (var lane in roadInfo.Lanes.Where(x => !x.IsFiller() || roadInfo.Width <= 0))
        {
            lane.Width = r(lane.CustomWidth > 0F
                ? lane.CustomWidth
                : LaneInfo.GetLaneTypes(lane.Type).Max(x => ThumbnailMakerUtil.GetLaneWidth(x, lane)));
		}

		var lanesWidth = roadInfo.Lanes
			.Where(x => x.Tags.HasFlag(LaneTag.Asphalt))
			.Sum(x => x.Width);

		// calculate the fillers' width based on the provided road width
		if (roadInfo.Width > 0 && roadInfo.Lanes.Any(x => x.IsFiller()))
        {
			var totalFillersSize = (float)roadInfo.Lanes.Where(x => x.IsFiller()).Sum(x => x.FillerSize);
			var remainingWidth = roadInfo.Width - lanesWidth - roadInfo.BufferSize;

			foreach (var lane in roadInfo.Lanes.Where(x => x.IsFiller()))
			{
				lane.Width = r(remainingWidth * (lane.FillerSize / totalFillersSize));
			}

			lanesWidth = roadInfo.Lanes
			    .Where(x => x.Tags.HasFlag(LaneTag.Asphalt))
			    .Sum(x => x.Width);
		}
        else if (roadInfo.Width <= 0) // calculate default road width if not provided
        {
            roadInfo.Width = roadInfo.BufferSize + lanesWidth;
        }

        var index = r(lanesWidth / -2F);

        foreach (var lane in roadInfo.Lanes)
        {
            if (lane.Tags.HasFlag(LaneTag.StackedLane))
                continue;

            if (lane.Tags.HasFlag(LaneTag.Asphalt))
            {
                lane.Position = r(index + lane.Width / 2F);
                index = r(index + lane.Width);
            }
            else if (lane.Tags.HasFlag(LaneTag.Buffer))
            {
                lane.Position = r((index < 0 ? -1F : 1F) * (roadInfo.Width / 2F + 3F - 0.25F));
            }
            else if (lane.Tags.HasFlag(LaneTag.Sidewalk))
			{
				lane.Position = r((index < 0 ? -1F : 1F) * (roadInfo.Width / 2F + 3F - 1.25F));
			}
        }

        foreach (var lane in roadInfo.Lanes)
		{
			lane.LeftDrivableArea = GetDrivableArea(lane, roadInfo, true, false);
            lane.RightDrivableArea = GetDrivableArea(lane, roadInfo, false, false);

			lane.LeftInvertedDrivableArea = GetDrivableArea(lane, roadInfo, true, true);
			lane.RightInvertedDrivableArea = GetDrivableArea(lane, roadInfo, false, true);
		}

		if (roadInfo.Lanes.Count(x => x.Tags.HasFlag(LaneTag.CenterMedian)) > 1)
		{
			foreach (var item in roadInfo.Lanes
				.Where(x => x.Tags.HasFlag(LaneTag.CenterMedian))
				.OrderBy(x => Math.Abs(x.Position))
				.Skip(1))
			{
                item.Tags |= LaneTag.SecondaryCenterMedian;
			}
		}

        if (roadInfo.ContainsWiredLanes)
        {
            var leftPole = roadInfo.Lanes.FirstOrDefault(x => x.Tags.HasFlag(LaneTag.WirePoleLane));
            var rightPole = roadInfo.Lanes.LastOrDefault(x => x.Tags.HasFlag(LaneTag.WirePoleLane));

			var leftPos = leftPole.Position + (leftPole.Tags.HasFlag(LaneTag.Sidewalk) ? 1F : 0F);
            var rightPos = rightPole.Position - (rightPole.Tags.HasFlag(LaneTag.Sidewalk) ? 1F : 0F);

			roadInfo.Lanes[0].Position = r(leftPos + (rightPos - leftPos) / 2F);
			roadInfo.Lanes[0].Width = r(rightPos - leftPos);
		}

		static float r(float f) => (float)Math.Round(f, 3);
    }

    private static readonly LaneType _drivingLaneTypes = LaneType.Car | LaneType.Parking | LaneType.Bike | LaneType.Bus | LaneType.Emergency | LaneType.Highway | LaneType.Tram | LaneType.Trolley;

    private static float GetDrivableArea(LaneInfo lane, RoadInfo road, bool left, bool invert)
    {
        var drivableArea = 0F;

        for (var i = road.Lanes.IndexOf(lane) + (left ? 1 : -1); i < road.Lanes.Count - 1 && i > 0; i += left ? 1 : -1)
        {
            if (!road.Lanes[i].Tags.HasFlag(LaneTag.Asphalt))
                break;

            if ((road.Lanes[i].Type & _drivingLaneTypes) == 0)
                break;

            if (road.Lanes[i].Direction == LaneDirection.Backwards && (invert == left))
                break;

            if (road.Lanes[i].Direction == LaneDirection.Forward && (invert != left))
                break;

            drivableArea += road.Lanes[i].Width;
        }

        return drivableArea;
    }

    private static IEnumerable<NetInfo.Lane> GenerateLanes(LaneInfo lane, RoadInfo road, ElevationType elevation)
	{
        foreach (var laneType in LaneInfo.GetLaneTypes(lane.Type))
            yield return getLane(laneType, lane, road, elevation);

        if (lane.AddStopToFiller && lane.IsFiller())
        {
            var leftPed = lane.Duplicate(LaneType.Pedestrian, (lane.Width - 2.1F) / -2F);
            var rightPed = lane.Duplicate(LaneType.Pedestrian, (lane.Width - 2.1F) / 2F);

            leftPed.Width = 2F;
			leftPed.Tags &= ~LaneTag.StoppableVehicleOnRight;
			leftPed.RightLane = null;

			rightPed.Width = 2F;
			rightPed.Tags &= ~LaneTag.StoppableVehicleOnLeft;
			rightPed.LeftLane = null;

            if (leftPed.Tags.HasFlag(LaneTag.StoppableVehicleOnLeft))
                yield return getLane(LaneType.Pedestrian, leftPed, road, elevation);

            if (rightPed.Tags.HasFlag(LaneTag.StoppableVehicleOnRight))
                yield return getLane(LaneType.Pedestrian, rightPed, road, elevation);

            lane.Elevation = null;
        }

        if (lane.Type == LaneType.Pedestrian && lane.Tags.HasFlag(LaneTag.Asphalt))
        {
            var fillerLane = lane.Duplicate(LaneType.Empty);

            fillerLane.Elevation = null;

			yield return getLane(LaneType.Empty, fillerLane, road, elevation);
        }

        if (lane.Type.HasFlag(LaneType.Trolley))
		{
			var leftTrolley = lane.Duplicate(LaneType.Empty, -0.6F);
			var rightTrolley = lane.Duplicate(LaneType.Empty, 0.6F);

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

		static NetInfo.Lane getLane(LaneType type, LaneInfo lane, RoadInfo road, ElevationType elevation) => new()
        {
			m_position = ThumbnailMakerUtil.GetLanePosition(type, lane, road),
			m_width = lane.Tags.HasFlag(LaneTag.Sidewalk) && elevation != ElevationType.Basic ? 1.25F : lane.Width,
			m_verticalOffset = ThumbnailMakerUtil.GetLaneVerticalOffset(lane, road),
			m_speedLimit = ThumbnailMakerUtil.GetLaneSpeedLimit(type, lane, road),
			m_laneType = ThumbnailMakerUtil.GetLaneType(type, lane, road),
			m_vehicleType = ThumbnailMakerUtil.GetVehicleType(type, lane),
			m_vehicleCategoryPart1 = ThumbnailMakerUtil.GetVehicleCategory1(type),
			m_vehicleCategoryPart2 = ThumbnailMakerUtil.GetVehicleCategory2(type),
			m_stopType = ThumbnailMakerUtil.GetStopType(type, lane, road, out var stopForward),
			m_direction = ThumbnailMakerUtil.GetLaneDirection(lane),
			m_finalDirection = ThumbnailMakerUtil.GetLaneDirection(lane),
			m_laneProps = GetLaneProps(type, lane, road),
			m_stopOffset = ThumbnailMakerUtil.GetStopOffset(type, lane),
			m_elevated = false,
			m_useTerrainHeight = false,
			m_centerPlatform = lane.Type == LaneType.Pedestrian && lane.Tags.HasFlag(LaneTag.Asphalt) && stopForward != null,
			m_allowConnect = elevation == ElevationType.Basic,
			m_similarLaneCount = 1,
			m_similarLaneIndex = 0,
		};
	}

    private static NetLaneProps GetLaneProps(LaneType type, LaneInfo lane, RoadInfo road)
    {
        var laneProps = ScriptableObject.CreateInstance<NetLaneProps>();

        laneProps.m_props = LanePropsUtil.GetLaneProps(type, lane, road) ?? new NetLaneProps.Prop[0];

        return laneProps;
    }
}
