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

				foreach (var prop in GetLaneDecalProps())
				{
					yield return prop;
				}

				foreach (var prop in GetLaneArrowProps())
				{
					yield return prop;
				}
			}

			switch (Type)
			{
				case LaneType.Pedestrian:
					foreach (var prop in GetPedestrianLaneProps())
					{
						yield return prop;
					}
					break;

				case LaneType.Curb:
				case LaneType.Filler:
					if (Lane.GetLaneWidth(true) < 0.25F)
					{
						foreach (var prop in GetLights())
						{
							yield return prop;
						}

						yield break;
					}

					foreach (var prop in GetMedianProps())
					{
						yield return prop;
					}

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
		{
			yield return prop;
		}

		var stopType = ThumbnailMakerUtil.GetStopType(Lane.Type, Lane, Road, Elevation, out var stopDirection);

		if (stopDirection == LaneDirection.None)
		{
			yield break;
		}

		var largeStop = Lane.Tags.HasFlag(LaneTag.Sidewalk) || Lane.LaneWidth >= 3;
		var stopDiff = (float)((largeStop ? Math.Min(0, -Lane.LaneWidth / 2 + 0.75F) : 0)
			+ Math.Round(Math.Abs(Lane.Position - ThumbnailMakerUtil.GetLanePosition(Lane.Type, Lane, Road, Elevation)), 3));

		if (stopType.HasFlag(VehicleInfo.VehicleType.Car))
		{
			var busStopLarge = GetProp(largeStop ? Prop.BusStopLarge : Prop.BusStopSmall);

			yield return new NetLaneProps.Prop
			{
				m_prop = busStopLarge,
				m_tree = busStopLarge,
				m_flagsRequired = NetLane.Flags.Stop,
				m_angle = busStopLarge.Angle,
				m_probability = 100,
				m_position = busStopLarge.Position + new Vector3(stopDiff, 0, Lane.Tags.HasAnyFlag(LaneTag.Sidewalk, LaneTag.CenterMedian) ? 2F : 0F),
			}.ToggleForwardBackward(stopDirection is LaneDirection.Forward && Lane.Direction != LaneDirection.Backwards);
		}

		if (stopType.HasFlag(VehicleInfo.VehicleType.Tram))
		{
			var tramStopLarge = GetProp(largeStop ? Prop.TramStopLarge : Prop.TramStopSmall);

			yield return new NetLaneProps.Prop
			{
				m_prop = tramStopLarge,
				m_tree = tramStopLarge,
				m_flagsRequired = NetLane.Flags.Stop2,
				m_angle = tramStopLarge.Angle,
				m_probability = 100,
				m_position = tramStopLarge.Position + new Vector3(stopDiff, 0, Lane.Tags.HasAnyFlag(LaneTag.Sidewalk, LaneTag.CenterMedian) ? -2F : 0F)
			}.ToggleForwardBackward(stopDirection is LaneDirection.Forward && Lane.Direction != LaneDirection.Backwards);
		}

		if (stopType.HasFlag(VehicleInfo.VehicleType.Trolleybus))
		{
			var sightSeeingProp = GetProp(Prop.TrolleyStopSmall);
			var trolleyStop = GetProp(Prop.TrolleyStopLarge);

			yield return new NetLaneProps.Prop
			{
				m_prop = sightSeeingProp,
				m_tree = sightSeeingProp,
				m_flagsRequired = NetLane.Flags.Stops,
				m_angle = sightSeeingProp.Angle,
				m_probability = 100,
				m_position = sightSeeingProp.Position + new Vector3(stopDiff, 0, 0)
			}.ToggleForwardBackward(stopDirection is LaneDirection.Forward && Lane.Direction != LaneDirection.Backwards);

			yield return new NetLaneProps.Prop
			{
				m_prop = trolleyStop,
				m_tree = trolleyStop,
				m_flagsRequired = NetLane.Flags.Stop2,
				m_flagsForbidden = NetLane.Flags.Stop,
				m_angle = trolleyStop.Angle,
				m_probability = 100,
				m_position = trolleyStop.Position + new Vector3(stopDiff, 0, 0)
			}.ToggleForwardBackward(stopDirection is LaneDirection.Forward && Lane.Direction != LaneDirection.Backwards);
		}
	}

	private PropTemplate GetProp(Prop prop)
	{
		return PropUtil.GetProp(prop);
	}
}