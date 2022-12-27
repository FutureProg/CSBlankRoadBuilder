using BlankRoadBuilder.Domain;
using BlankRoadBuilder.ThumbnailMaker;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlankRoadBuilder.Util;
public class TrafficLightsUtil
{
	public static void GetTrafficLights(RoadInfo roadInfo)
	{
		foreach (var lane in roadInfo.Lanes)
		{
			if (lane.Type != LaneType.Curb && lane.Tags.HasFlag(LaneTag.Sidewalk))
				continue;

			var minimumSpace = lane.Type == LaneType.Curb ? 1F : 3F;
			var tl = new TrafficLight
			{
				LeftForwardSpace = GetDrivableArea(lane, roadInfo, true, true),
				RightForwardSpace = GetDrivableArea(lane, roadInfo, false, true),
				LeftBackwardSpace = GetDrivableArea(lane, roadInfo, true, false),
				RightBackwardSpace = GetDrivableArea(lane, roadInfo, false, false),
			};

			if (tl.LeftForwardSpace > 0)
			{
				tl.LeftForward = tl.LeftForwardSpace >= 6F ? Props.Prop.TrafficLight02 : Props.Prop.TrafficLight01Mirror;
			}
			else if (tl.LeftBackwardSpace > minimumSpace)
			{
				tl.LeftForward = Props.Prop.TrafficLightPedestrian;
			}

			if (tl.LeftBackwardSpace > 0)
			{
				tl.LeftBackward = tl.LeftForwardSpace >= 6F ? Props.Prop.TrafficLight02Mirror : Props.Prop.TrafficLight01;
			}
			else if (tl.LeftForwardSpace > minimumSpace)
			{
				tl.LeftBackward = Props.Prop.TrafficLightPedestrian;
			}

			if (tl.RightForwardSpace > 3F)
			{
				tl.RightForward = tl.LeftForwardSpace >= 9F ? Props.Prop.TrafficLight02Mirror : Props.Prop.TrafficLight01;
			}
			else if (tl.RightBackwardSpace > minimumSpace)
			{
				tl.RightForward = Props.Prop.TrafficLightPedestrian;
			}

			if (tl.RightBackwardSpace > 3F)
			{
				tl.RightBackward = tl.LeftForwardSpace >= 9F ? Props.Prop.TrafficLight02 : Props.Prop.TrafficLight01Mirror;
			}
			else if (tl.RightForwardSpace > minimumSpace)
			{
				tl.RightBackward = Props.Prop.TrafficLightPedestrian;
			}

			lane.TrafficLight = tl;
		}
	}

	private static float GetDrivableArea(LaneInfo lane, RoadInfo road, bool left, bool forward)
	{
		var drivableArea = 0F;

		for (var i = road.Lanes.IndexOf(lane) + (left ? 1 : -1); i < road.Lanes.Count - 1 && i > 0; i += left ? 1 : -1)
		{
			if (!road.Lanes[i].Tags.HasFlag(LaneTag.Asphalt))
				break;

			if (road.Lanes[i].Type.HasAnyFlag(LaneType.Car, LaneType.Parking , LaneType.Bike , LaneType.Bus, LaneType.Emergency , LaneType.Tram , LaneType.Trolley))
				break;

			if (road.Lanes[i].Direction == LaneDirection.Backwards && forward)
				break;

			if (road.Lanes[i].Direction == LaneDirection.Forward && !forward)
				break;

			drivableArea += road.Lanes[i].Width;
		}

		return drivableArea;
	}
}
