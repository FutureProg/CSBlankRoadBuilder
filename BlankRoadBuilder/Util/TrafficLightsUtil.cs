using BlankRoadBuilder.Domain;
using BlankRoadBuilder.Domain.Options;
using BlankRoadBuilder.ThumbnailMaker;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace BlankRoadBuilder.Util;
public class TrafficLightsUtil
{
	public static void GetTrafficLights(RoadInfo roadInfo)
	{
		foreach (var lane in roadInfo.Lanes)
		{
			if (lane.Type != LaneType.Curb && !lane.Tags.HasFlag(LaneTag.Asphalt))
				continue;

			var minimumSpace = lane.Type == LaneType.Curb ? 0.24F : 3F;
			var tl = new TrafficLight
			{
				LeftForwardSpace = GetDrivableArea(lane, true, true),
				RightForwardSpace = GetDrivableArea(lane, false, true),
				LeftBackwardSpace = GetDrivableArea(lane, true, false),
				RightBackwardSpace = GetDrivableArea(lane, false, false),
			};

			tl = ModOptions.FlipTrafficLights 
				? GenerateTrafficLightsFlipped(minimumSpace, tl)
				: GenerateTrafficLights(minimumSpace, tl);

			lane.TrafficLight = tl;
		}
	}

	private static TrafficLight GenerateTrafficLights(float minimumSpace, TrafficLight tl)
	{
		if (tl.LeftForwardSpace > 0F)
		{
			tl.LeftForward = tl.LeftForwardSpace >= 6F ? Props.Prop.TrafficLight02 : Props.Prop.TrafficLight01Mirror;
		}
		else if (tl.LeftForwardSpace > minimumSpace || tl.LeftBackwardSpace > minimumSpace)
		{
			tl.LeftForward = Props.Prop.TrafficLightPedestrian;
		}

		if (tl.LeftBackwardSpace > 6F)
		{
			tl.LeftBackward = tl.LeftBackwardSpace >= 12F ? Props.Prop.TrafficLight02Mirror : Props.Prop.TrafficLight01;
		}
		else if (tl.LeftForwardSpace > minimumSpace || tl.LeftBackwardSpace > minimumSpace)
		{
			tl.LeftBackward = Props.Prop.TrafficLightPedestrian;
		}

		if (tl.RightForwardSpace > 6F)
		{
			tl.RightForward = tl.RightForwardSpace >= 12F ? Props.Prop.TrafficLight02Mirror : Props.Prop.TrafficLight01;
		}
		else if (tl.RightBackwardSpace > minimumSpace || tl.RightForwardSpace > minimumSpace)
		{
			tl.RightForward = Props.Prop.TrafficLightPedestrian;
		}

		if (tl.RightBackwardSpace > 0F)
		{
			tl.RightBackward = tl.RightBackwardSpace >= 6F ? Props.Prop.TrafficLight02 : Props.Prop.TrafficLight01Mirror;
		}
		else if (tl.RightBackwardSpace > minimumSpace || tl.RightForwardSpace > minimumSpace)
		{
			tl.RightBackward = Props.Prop.TrafficLightPedestrian;
		}

		return tl;
	}

	private static TrafficLight GenerateTrafficLightsFlipped(float minimumSpace, TrafficLight tl)
	{
		if (tl.LeftForwardSpace > 0F)
		{
			tl.LeftForward = Props.Prop.TrafficLight01Mirror;
		}
		else if (tl.LeftForwardSpace > minimumSpace || tl.LeftBackwardSpace > minimumSpace)
		{
			tl.LeftForward = Props.Prop.TrafficLightPedestrian;
		}

		if (tl.LeftBackwardSpace > 6F)
		{
			tl.LeftBackward = Props.Prop.TrafficLight01Mirror;
		}
		else if (tl.LeftForwardSpace > 0F)
		{
			tl.LeftBackward = tl.LeftForwardSpace >= 6F ? Props.Prop.TrafficLight02 : Props.Prop.TrafficLight01Mirror;
		}
		else if (tl.LeftBackwardSpace > minimumSpace)
		{
			tl.LeftBackward = Props.Prop.TrafficLightPedestrian;
		}

		if (tl.RightForwardSpace > 6F)
		{
			tl.RightForward = Props.Prop.TrafficLight01;
		}
		else if (tl.RightBackwardSpace > 0F)
		{
			tl.RightForward = tl.RightBackwardSpace >= 6F ? Props.Prop.TrafficLight02 : Props.Prop.TrafficLight01Mirror;
		}
		else if (tl.RightForwardSpace > minimumSpace)
		{
			tl.RightForward = Props.Prop.TrafficLightPedestrian;
		}

		if (tl.RightBackwardSpace > 0F)
		{
			tl.RightBackward = Props.Prop.TrafficLight01Mirror;
		}
		else if (tl.RightBackwardSpace > minimumSpace || tl.RightForwardSpace > minimumSpace)
		{
			tl.RightBackward = Props.Prop.TrafficLightPedestrian;
		}

		return tl;
	}

	private static float GetDrivableArea(LaneInfo? lane, bool left, bool forward)
	{
		var drivableArea = 0F;

		while (lane != null)
		{
			lane = left ? lane.LeftLane : lane.RightLane;

			if (lane == null || lane.Direction == (forward ? LaneDirection.Backwards : LaneDirection.Forward))
				break;

			if (lane.Type == LaneType.Empty)
				continue;

			if (!lane.Tags.HasFlag(LaneTag.Asphalt))
				break;

			if (!lane.Type.HasAnyFlag(LaneType.Car, LaneType.Parking, LaneType.Bike, LaneType.Bus, LaneType.Emergency, LaneType.Tram, LaneType.Trolley))
				break;

			drivableArea += lane.LaneWidth;
		}

		return drivableArea;
	}
}
