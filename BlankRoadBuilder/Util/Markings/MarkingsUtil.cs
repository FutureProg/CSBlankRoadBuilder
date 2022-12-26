using BlankRoadBuilder.Domain;
using BlankRoadBuilder.Domain.Options;
using BlankRoadBuilder.ThumbnailMaker;

using ColossalFramework.Importers;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEngine;

using static AdaptiveRoads.DTO.NetInfoDTO;
using static AdaptiveRoads.Manager.NetInfoExtionsion;
using static BlankRoadBuilder.Util.Markings.MarkingStyleUtil;

namespace BlankRoadBuilder.Util.Markings;
public static class MarkingsUtil
{
    public static MarkingsInfo GenerateMarkings(RoadInfo roadInfo)
    {
        var markingInfo = new MarkingsInfo();
        var currentFiller = (FillerMarking?)null;

        foreach (var lane in roadInfo.Lanes.Where(x => !x.Tags.HasAnyFlag(LaneTag.Ghost, LaneTag.StackedLane)))
        {
            var laneFiller = lane.Decorations & (LaneDecoration.Filler | LaneDecoration.Grass | LaneDecoration.Gravel | LaneDecoration.Pavement);

            if (laneFiller != 0)
            {
                if (laneFiller == LaneDecoration.Filler
                    || currentFiller?.LeftPoint.LeftLane == null
                    || currentFiller.Type != laneFiller
                    || ThumbnailMakerUtil.GetLaneVerticalOffset(currentFiller.LeftPoint.LeftLane, roadInfo) != ThumbnailMakerUtil.GetLaneVerticalOffset(lane, roadInfo))
                {
                    if (currentFiller != null)
                    {
                        markingInfo.Fillers.Add(currentFiller);
                    }

                    currentFiller = new FillerMarking
                    {
                        Type = laneFiller,
                        LeftPoint = new MarkingPoint(lane.LeftLane, lane),
                        RightPoint = new MarkingPoint(lane, lane.RightLane)
                    };
                }

                currentFiller.RightPoint = new MarkingPoint(lane, lane.RightLane);

                continue;
            }

            if (currentFiller != null)
            {
                markingInfo.Fillers.Add(currentFiller);
                currentFiller = null;
            } 
        }

        foreach (var lane in roadInfo.Lanes.Where(x => !x.Tags.HasAnyFlag(LaneTag.Ghost, LaneTag.StackedLane)))
        {
            var currentCategory = lane.GetVehicleCategory();

            foreach (var item in GenerateLaneMarkings(roadInfo, lane, currentCategory))
            {
                if (!markingInfo.Lines.ContainsKey(item.Point) || markingInfo.Lines[item.Point].Marking < item.Marking)
                {
                    markingInfo.Lines[item.Point] = item;
                }
            }
        }

		return markingInfo;
    }

    private static IEnumerable<LineMarking> GenerateLaneMarkings(RoadInfo road, LaneInfo lane, LaneVehicleCategory currentCategory)
    {
        var leftLane = lane.Direction == LaneDirection.Backwards ? lane.RightLane : lane.LeftLane;
        var rightLane = lane.Direction == LaneDirection.Backwards ? lane.LeftLane : lane.RightLane;

        var leftCategory = leftLane?.GetVehicleCategory() ?? LaneVehicleCategory.Unknown;
        var rightCategory = rightLane?.GetVehicleCategory() ?? LaneVehicleCategory.Unknown;

        if ((currentCategory & (LaneVehicleCategory.Vehicle | LaneVehicleCategory.Bike | LaneVehicleCategory.Tram)) != 0)
        {
            if (rightCategory != currentCategory && (rightCategory <= LaneVehicleCategory.Pedestrian || rightLane?.Direction != leftLane?.Direction))
            {
                yield return marking(true, GenericMarkingType.End);
            }

            if (leftCategory != currentCategory && leftCategory != LaneVehicleCategory.Parking)
            {
                yield return marking(false,
                    currentCategory != LaneVehicleCategory.Vehicle ? GenericMarkingType.End
                    : leftLane?.Direction == lane.Direction ? GenericMarkingType.Normal | GenericMarkingType.Hard : GenericMarkingType.Flipped | GenericMarkingType.End);
            }
        }

        if (currentCategory == LaneVehicleCategory.Parking)
        {
            if (leftCategory == LaneVehicleCategory.Vehicle)
            {
                yield return marking(false, GenericMarkingType.Parking);
            }

            if (rightCategory == LaneVehicleCategory.Vehicle)
            {
                yield return marking(true, GenericMarkingType.Parking);
            }
        }
        else
        {
            if (currentCategory == leftCategory && leftLane?.Direction != lane.Direction)
            {
                if (lane.Direction != LaneDirection.Backwards)
                    yield return marking(false, GenericMarkingType.Flipped | (currentCategory == LaneVehicleCategory.Vehicle ? GenericMarkingType.Hard : (GenericMarkingType)(int)currentCategory));
            }

            if (currentCategory == leftCategory && leftLane?.Direction == lane.Direction)
            {
                yield return marking(false, GenericMarkingType.Normal | (currentCategory == LaneVehicleCategory.Vehicle ? leftLane.Type.HasFlag(lane.Type) || lane.Type.HasFlag(leftLane.Type) ? GenericMarkingType.Soft : GenericMarkingType.Hard : (GenericMarkingType)(int)currentCategory));
            }
        }

        LineMarking marking(bool r, GenericMarkingType t) => new LineMarking
        {
            Marking = road.BufferWidth == 0F && (r ? rightLane : leftLane)?.Type == LaneType.Curb ? GenericMarkingType.None : t,
            Point = r ? new MarkingPoint(lane, lane.RightLane) : new MarkingPoint(lane.LeftLane, lane)
        };
    }
}
