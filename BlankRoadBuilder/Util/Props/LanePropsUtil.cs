using BlankRoadBuilder.Domain;
using BlankRoadBuilder.ThumbnailMaker;
using BlankRoadBuilder.Util.Props.Templates;

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace BlankRoadBuilder.Util.Props;

public partial class LanePropsUtil
{
	public LanePropsUtil(int index, LaneType type, LaneInfo lane, RoadInfo road, ElevationType elevation)
	{
		Index = index;
		Type = type;
		Lane = lane;
		Road = road;
		Elevation = elevation;
	}

	public int Index { get; }
	public LaneType Type { get; }
	public LaneInfo Lane { get; }
	public RoadInfo Road { get; }
	public ElevationType Elevation { get; }

	private class RhtProp : NetLaneProps.Prop { }

	public NetLaneProps.Prop[] GetLaneProps()
	{
		var result = new List<NetLaneProps.Prop>();

		foreach (var prop in getLaneProps().Where(x => x != null && (x.m_prop != null || x.m_tree != null)))
		{
			if (Type == LaneType.Curb)
			{
				prop.m_position.x += Road.BufferWidth / (Lane.Direction == LaneDirection.Forward ? 2 : -2);
			}

			result.Add(prop);

			if (prop.m_flagsForbidden.HasFlag(NetLane.Flags.Inverted) && prop is not RhtProp)
			{
				var lhtProp = prop.Clone()
					.ToggleRHT_LHT(Lane.Direction is LaneDirection.Forward or LaneDirection.Backwards && Lane.Type is not LaneType.Curb);

				result.Add(lhtProp);
			}
		}

		return result.ToArray();

		IEnumerable<NetLaneProps.Prop> getLaneProps()
		{
			if (Lane.Tags.HasFlag(LaneTag.Damage))
			{
				foreach (var prop in GetRoadDamageProps())
				{
					yield return prop;
				}

				yield break;
			}

			if (Index == 0)
			{
				foreach (var prop in GetDecorationProps())
				{
					yield return prop;
				}
			}

			switch (Type)
			{
				case LaneType.Pedestrian:
					foreach (var prop in GetPedestrianLaneProps())
						yield return prop;

					break;
				case LaneType.Bike:
					foreach (var prop in GetBikeLaneProps())
						yield return prop;

					break;
				case LaneType.Bus:
				case LaneType.Trolley:
					foreach (var prop in GetBusLaneProps())
						yield return prop;

					break;
				case LaneType.Emergency:
					if (Lane.Type.HasFlag(LaneType.Car))
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
					if (Lane.LaneWidth - Road.BufferWidth < 0.25F)
					{
						foreach (var prop in GetLights())
						{
							yield return prop;
						}

						yield break;
					}

					foreach (var prop in GetMedianProps())
						yield return prop;

					break;
			}
		}
	}

	private IEnumerable<NetLaneProps.Prop> GetMedianProps()
	{
		if (Lane.Tags.HasFlag(LaneTag.StackedLane))
		{
			yield break;
		}

		foreach (var prop in GetParkingProps())
		{
			yield return prop;
		}

		foreach (var prop in GetSignsAndTrafficLights())
		{
			yield return prop;
		}

		foreach (var prop in GetWirePoleProps())
		{
			yield return prop;
		}

		foreach (var prop in GetLights())
		{
			yield return prop;
		}
	}

	private IEnumerable<NetLaneProps.Prop> GetPedestrianLaneProps()
	{
		foreach (var prop in GetMedianProps())
			yield return prop;

		var stopType = ThumbnailMakerUtil.GetStopType(Lane.Type, Lane, Road, out var forward);

		if (forward == null)
			yield break;

		if (stopType.HasFlag(VehicleInfo.VehicleType.Car))
		{
			var busStopLarge = GetProp(Lane.Tags.HasFlag(LaneTag.Sidewalk) ? Prop.BusStopLarge : Prop.BusStopSmall);
			var stopDiff = Lane.Tags.HasFlag(LaneTag.Sidewalk) ? 0.5F
				: (float)Math.Round(Math.Abs(Lane.Position - ThumbnailMakerUtil.GetLanePosition(Lane.Type, Lane, Road)) - 0.2F, 3);

			yield return new NetLaneProps.Prop
			{
				m_prop = busStopLarge,
				m_finalProp = busStopLarge,
				m_flagsRequired = NetLane.Flags.Stop,
				m_angle = 90,
				m_probability = 100,
				m_position = new Vector3(stopDiff, 0, Lane.Tags.HasFlag(LaneTag.Sidewalk) || Lane.Tags.HasFlag(LaneTag.CenterMedian) ? 5F : 3F),
			}.ToggleForwardBackward((bool)forward && Lane.Direction != LaneDirection.Backwards);
		}

		if (stopType.HasFlag(VehicleInfo.VehicleType.Tram))
		{
			var tramStopLarge = GetProp(Lane.Tags.HasFlag(LaneTag.Sidewalk) ? Prop.TramStopLarge : Prop.TramStopSmall);

			yield return new NetLaneProps.Prop
			{
				m_prop = tramStopLarge,
				m_finalProp = tramStopLarge,
				m_flagsRequired = NetLane.Flags.Stop2,
				m_angle = 90,
				m_probability = 100,
				m_position = new Vector3(Lane.Tags.HasFlag(LaneTag.Sidewalk) ? 0.5F : 0.1F, 0, Lane.Tags.HasFlag(LaneTag.Sidewalk) || Lane.Tags.HasFlag(LaneTag.CenterMedian) ? -5F : -3F)
			}.ToggleForwardBackward((bool)forward && Lane.Direction != LaneDirection.Backwards);
		}

		if (stopType.HasFlag(VehicleInfo.VehicleType.Trolleybus))
		{
			var sightSeeingProp = GetProp(Prop.TrolleyStopSmall);
			var trolleyStop = GetProp(Prop.TrolleyStopLarge);

			yield return new NetLaneProps.Prop
			{
				m_prop = sightSeeingProp,
				m_finalProp = sightSeeingProp,
				m_flagsRequired = NetLane.Flags.Stops,
				m_angle = 90,
				m_probability = 100,
				m_position = new Vector3(Lane.Tags.HasFlag(LaneTag.Sidewalk) ? -0.75F : -0.5F, 0, -2F)
			}.ToggleForwardBackward((bool)forward && Lane.Direction != LaneDirection.Backwards);

			yield return new NetLaneProps.Prop
			{
				m_prop = trolleyStop,
				m_finalProp = trolleyStop,
				m_flagsRequired = NetLane.Flags.Stop2,
				m_flagsForbidden = NetLane.Flags.Stop,
				m_angle = 90,
				m_probability = 100,
				m_position = new Vector3(Lane.Tags.HasFlag(LaneTag.Sidewalk) ? -0.75F : -0.5F, 0, -2F)
			}.ToggleForwardBackward((bool)forward && Lane.Direction != LaneDirection.Backwards);
		}
	}

	private PropTemplate GetProp(Prop prop)
	{
		return PropUtil.GetProp(prop);
	}
}