using BlankRoadBuilder.Domain;
using BlankRoadBuilder.Domain.Options;
using BlankRoadBuilder.ThumbnailMaker;
using BlankRoadBuilder.Util.Props.Templates;

using System;

namespace BlankRoadBuilder.Util;
public class TrafficLightsUtil
{
	public static void GetTrafficLights(RoadInfo roadInfo)
	{
		foreach (var lane in roadInfo.Lanes)
		{
			if (lane.Type != LaneType.Curb && !lane.Tags.HasFlag(LaneTag.Asphalt))
				continue;

			var minimumSpace = Math.Min(lane.Type == LaneType.Curb ? 0.24F : 24, ModOptions.MinimumDistanceForPedLight);

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
			tl.LeftForward = tl.LeftForwardSpace >= 6F ? Prop.TrafficLight02 : Prop.TrafficLight01Mirror;
		}
		else if (tl.LeftForwardSpace + tl.LeftBackwardSpace > minimumSpace)
		{
			tl.LeftForward = Prop.TrafficLightPedestrian;
		}

		if (tl.LeftBackwardSpace > 6F)
		{
			tl.LeftBackward = tl.LeftBackwardSpace >= 12F ? Prop.TrafficLight02Mirror : Prop.TrafficLight01;
		}
		else if (tl.LeftForwardSpace + tl.LeftBackwardSpace > minimumSpace)
		{
			tl.LeftBackward = Prop.TrafficLightPedestrian;
		}

		if (tl.RightForwardSpace > 6F)
		{
			tl.RightForward = tl.RightForwardSpace >= 12F ? Prop.TrafficLight02Mirror : Prop.TrafficLight01;
		}
		else if (tl.RightBackwardSpace + tl.RightForwardSpace > minimumSpace)
		{
			tl.RightForward = Prop.TrafficLightPedestrian;
		}

		if (tl.RightBackwardSpace > 0F)
		{
			tl.RightBackward = tl.RightBackwardSpace >= 6F ? Prop.TrafficLight02 : Prop.TrafficLight01Mirror;
		}
		else if (tl.RightBackwardSpace + tl.RightForwardSpace > minimumSpace)
		{
			tl.RightBackward = Prop.TrafficLightPedestrian;
		}

		return tl;
	}

	private static TrafficLight GenerateTrafficLightsFlipped(float minimumSpace, TrafficLight tl)
	{
		if (tl.LeftForwardSpace > 0F)
		{
			tl.LeftForward = Prop.TrafficLight01Mirror;
		}
		else if (tl.LeftForwardSpace + tl.LeftBackwardSpace > minimumSpace)
		{
			tl.LeftForward = Prop.TrafficLightPedestrian;
		}

		if (tl.LeftBackwardSpace > 6F)
		{
			tl.LeftBackward = Prop.TrafficLight01Mirror;
		}
		else if (tl.LeftForwardSpace > 0F)
		{
			tl.LeftBackward = tl.LeftForwardSpace >= 6F ? Prop.TrafficLight02 : Prop.TrafficLight01Mirror;
		}
		else if (tl.LeftBackwardSpace > minimumSpace)
		{
			tl.LeftBackward = Prop.TrafficLightPedestrian;
		}

		if (tl.RightForwardSpace > 6F)
		{
			tl.RightForward = Prop.TrafficLight01;
		}
		else if (tl.RightBackwardSpace > 0F)
		{
			tl.RightForward = tl.RightBackwardSpace >= 6F ? Prop.TrafficLight02 : Prop.TrafficLight01Mirror;
		}
		else if (tl.RightForwardSpace > minimumSpace)
		{
			tl.RightForward = Prop.TrafficLightPedestrian;
		}

		if (tl.RightBackwardSpace > 0F)
		{
			tl.RightBackward = Prop.TrafficLight01Mirror;
		}
		else if (tl.RightBackwardSpace + tl.RightForwardSpace > minimumSpace)
		{
			tl.RightBackward = Prop.TrafficLightPedestrian;
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
