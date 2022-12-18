﻿using AdaptiveRoads.Manager;
using BlankRoadBuilder.Domain.Options;
using BlankRoadBuilder.ThumbnailMaker;

using PrefabMetadata.API;
using PrefabMetadata.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace BlankRoadBuilder.Util.Props;

public static partial class LanePropsUtil
{
	private class RhtProp : NetLaneProps.Prop { }

	public static NetLaneProps.Prop[] GetLaneProps(int index, LaneType type, LaneInfo lane, RoadInfo road)
	{
		var result = new List<NetLaneProps.Prop>();

		foreach (var prop in getLaneProps().Where(x => x != null && (x.m_prop != null || x.m_tree != null)))
		{
			result.Add(prop);

			if (prop.m_flagsForbidden.HasFlag(NetLane.Flags.Inverted) && prop is not RhtProp)
			{
				var lhtProp = prop.Clone()
					.ToggleRHT_LHT(lane.Direction == LaneDirection.Forward || lane.Direction == LaneDirection.Backwards);

				result.Add(lhtProp);
			}
		}

		return result.ToArray();

		IEnumerable<NetLaneProps.Prop> getLaneProps()
		{
			if (lane.Tags.HasFlag(LaneTag.Damage))
			{
				foreach (var prop in GetRoadDamageProps(road))
				{
					yield return prop;
				}

				yield break;
			}

			if (index == 0)
			{
				foreach (var prop in GetDecorationProps(lane, road))
				{
					yield return prop;
				}
			}

			switch (type)
			{
				case LaneType.Pedestrian:
					foreach (var prop in GetPedestrianLaneProps(lane, road))
						yield return prop;

					break;
				case LaneType.Bike:
					foreach (var prop in GetBikeLaneProps())
						yield return prop;

					break;
				case LaneType.Bus:
					foreach (var prop in GetBusLaneProps(lane))
						yield return prop;

					break;
				case LaneType.Emergency:
					if (lane.Type.HasFlag(LaneType.Car))
						yield break;

					foreach (var prop in GetLaneArrowProps())
						yield return prop;

					break;
				case LaneType.Car:
					foreach (var prop in GetLaneArrowProps())
						yield return prop;

					break;
				case LaneType.Curb:
				case LaneType.Filler:
					if (lane.Width < 0.75F)
						yield break;

					foreach (var prop in GetMedianProps(lane, road))
						yield return prop;
					
					break;
			}
		}
	}

	private static IEnumerable<NetLaneProps.Prop> GetMedianProps(LaneInfo lane, RoadInfo road)
	{
		if (lane.Tags.HasFlag(LaneTag.StackedLane) || lane.Tags.HasFlag(LaneTag.Ghost))
		{
			yield break;
		}

		foreach (var prop in GetParkingProps(lane))
		{
			yield return prop;
		}

		foreach (var prop in GetSignsAndTrafficLights(lane, road))
		{
			yield return prop;
		}

		foreach (var prop in GetWirePoleProps(lane, road))
		{
			yield return prop;
		}

		foreach (var prop in GetLights(lane, road))
		{
			yield return prop;
		}
	}

	private static IEnumerable<NetLaneProps.Prop> GetPedestrianLaneProps(LaneInfo lane, RoadInfo road)
	{
		foreach (var prop in GetMedianProps(lane, road))
			yield return prop;

		var stopType = ThumbnailMakerUtil.GetStopType(lane.Type, lane, road, out var forward);

		if (forward == null)
			yield break;

		if (stopType.HasFlag(VehicleInfo.VehicleType.Car))
		{
			var busStopLarge = Prop(lane.Tags.HasFlag(LaneTag.Sidewalk) ? "Bus Stop Large" : "Bus Stop Small");
			var stopDiff = lane.Tags.HasFlag(LaneTag.Sidewalk) ? 0.5F 
				: (float)Math.Round(Math.Abs(lane.Position - ThumbnailMakerUtil.GetLanePosition(lane.Type, lane, road)) - 0.2F, 3);

			yield return new NetLaneProps.Prop
			{
				m_prop = busStopLarge,
				m_finalProp = busStopLarge,
				m_flagsRequired = NetLane.Flags.Stop,
				m_angle = 90,
				m_probability = 100,
				m_position = new Vector3(stopDiff, 0, lane.Tags.HasFlag(LaneTag.Sidewalk) || lane.Tags.HasFlag(LaneTag.CenterMedian) ? 5F : 3F),
			}.ToggleForwardBackward((bool)forward && lane.Direction != LaneDirection.Backwards);
		}

		if (stopType.HasFlag(VehicleInfo.VehicleType.Tram))
		{
			var tramStopLarge = Prop(lane.Tags.HasFlag(LaneTag.Sidewalk) ? "Tram Stop" : "Tram Stop Sign");

			yield return new NetLaneProps.Prop
			{
				m_prop = tramStopLarge,
				m_finalProp = tramStopLarge,
				m_flagsRequired = NetLane.Flags.Stop2,
				m_angle = 90,
				m_probability = 100,
				m_position = new Vector3(lane.Tags.HasFlag(LaneTag.Sidewalk) ? 0.5F : 0.1F, 0, lane.Tags.HasFlag(LaneTag.Sidewalk) || lane.Tags.HasFlag(LaneTag.CenterMedian) ? -5F : -3F)
			}.ToggleForwardBackward((bool)forward && lane.Direction != LaneDirection.Backwards);
		}

		if (stopType.HasFlag(VehicleInfo.VehicleType.Trolleybus))
		{
			var sightSeeingProp = Prop("Sightseeing Bus Stop Small");
			var trolleyStop = Prop("Trolleybus Stop");

			yield return new NetLaneProps.Prop
			{
				m_prop = sightSeeingProp,
				m_finalProp = sightSeeingProp,
				m_flagsRequired = NetLane.Flags.Stops,
				m_angle = 90,
				m_probability = 100,
				m_position = new Vector3(lane.Tags.HasFlag(LaneTag.Sidewalk) ? -0.75F : -0.5F, 0, -2F)
			}.ToggleForwardBackward((bool)forward && lane.Direction != LaneDirection.Backwards);

			yield return new NetLaneProps.Prop
			{
				m_prop = trolleyStop,
				m_finalProp = trolleyStop,
				m_flagsRequired = NetLane.Flags.Stop2,
				m_flagsForbidden = NetLane.Flags.Stop,
				m_angle = 90,
				m_probability = 100,
				m_position = new Vector3(lane.Tags.HasFlag(LaneTag.Sidewalk) ? -0.75F : -0.5F, 0, -2F)
			}.ToggleForwardBackward((bool)forward && lane.Direction != LaneDirection.Backwards);
		}
	}

	private static PropInfo Prop(string name)
	{
		return PrefabCollection<PropInfo>.FindLoaded(name);
	}
}	