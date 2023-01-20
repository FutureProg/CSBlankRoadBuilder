using AdaptiveRoads.Manager;

using BlankRoadBuilder.Domain.Options;
using BlankRoadBuilder.ThumbnailMaker;

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace BlankRoadBuilder.Util.Props;

public static partial class LanePropsUtil
{
	private static IEnumerable<NetLaneProps.Prop> GetSignsAndTrafficLights(LaneInfo lane, RoadInfo road)
	{
		if (lane.Tags.HasFlag(LaneTag.Sidewalk) && lane.Type != LaneType.Curb)
			yield break;

		var propPosition = -Math.Max(0, (lane.LaneWidth / 2) - 0.5F);

		foreach (var prop in GetTrafficSigns(road, lane, propPosition))
		{
			yield return prop;
		}

		foreach (var prop in GetBikeSignProps(lane))
		{
			yield return prop;
		}

		yield return TrafficLight(road, lane.TrafficLight.LeftForward, propPosition, true && lane.TrafficLight.LeftForward == Prop.TrafficLightPedestrian);
		yield return TrafficLight(road, lane.TrafficLight.RightForward, -propPosition, ModOptions.FlipTrafficLights && lane.TrafficLight.RightForward != Prop.TrafficLightPedestrian);
		yield return TrafficLight(road, lane.TrafficLight.LeftBackward, -propPosition).ToggleForwardBackward(true, ModOptions.FlipTrafficLights && lane.TrafficLight.LeftBackward != Prop.TrafficLightPedestrian);
		yield return TrafficLight(road, lane.TrafficLight.RightBackward, propPosition, true && lane.TrafficLight.RightBackward == Prop.TrafficLightPedestrian).ToggleForwardBackward(true, false);
	}

	private static IEnumerable<NetLaneProps.Prop> GetTrafficSigns(RoadInfo road, LaneInfo lane, float propPosition)
	{
		if (lane.TrafficLight.LeftForwardSpace >= 2F)
		{
			yield return RailwayCrossing(road, lane.TrafficLight.LeftForwardSpace, propPosition);

			if (!GetSideLanes(lane, true, true).All(x => x.Type.HasAnyFlag(LaneType.Parking, LaneType.Bike)))
			{
				yield return StopSign(propPosition);

				foreach (var prop in GetWarningSigns(lane))
				{
					yield return prop;
				}
			}

			if (!GetSideLanes(lane, true, true).All(x => x.Type.HasAnyFlag(LaneType.Parking, LaneType.Bike, LaneType.Tram)))
			{
				yield return SpeedSign(road, lane);
			}
		}

		if (lane.TrafficLight.RightBackwardSpace >= 2F)
		{
			yield return RailwayCrossing(road, lane.TrafficLight.LeftForwardSpace, propPosition).ToggleForwardBackward();

			if (!GetSideLanes(lane, false, false).All(x => x.Type.HasAnyFlag(LaneType.Parking, LaneType.Bike)))
			{
				yield return StopSign(propPosition).ToggleForwardBackward();

				foreach (var prop in GetWarningSigns(lane))
				{
					yield return prop.ToggleForwardBackward();
				}
			}

			if (!GetSideLanes(lane, false, false).All(x => x.Type.HasAnyFlag(LaneType.Parking, LaneType.Bike, LaneType.Tram)))
			{
				yield return SpeedSign(road, lane).ToggleForwardBackward();
			}
		}
	}

	private static NetLaneProps.Prop TrafficLight(RoadInfo road, Prop prop, float propPosition, bool flipAngle = false)
	{
		var propTemplate = GetProp(prop);

		return new NetLaneProps.Prop
		{
			m_prop = propTemplate,
			m_finalProp = propTemplate,
			m_flagsRequired = NetLane.Flags.None,
			m_flagsForbidden = NetLane.Flags.JoinedJunction | NetLane.Flags.Inverted,
			m_startFlagsRequired = NetNode.Flags.None,
			m_startFlagsForbidden = NetNode.Flags.None,
			m_endFlagsRequired = NetNode.Flags.TrafficLights,
			m_endFlagsForbidden = NetNode.Flags.LevelCrossing,
			m_cornerAngle = 0.5F,
			m_minLength = 0,
			m_repeatDistance = 0,
			m_segmentOffset = 1,
			m_angle = flipAngle ? 270 : 90,
			m_colorMode = NetLaneProps.ColorMode.EndState,
			m_probability = 100,
			m_position = new Vector3(propPosition, 0, road.ContainsWiredLanes ? -1.5F : -0.75F),
		}.Extend(p => new NetInfoExtionsion.LaneProp(p)
		{
			SegmentEndFlags = new NetInfoExtionsion.SegmentEndInfoFlags { Required = prop == Prop.TrafficLightPedestrian ? NetSegmentEnd.Flags.ZebraCrossing : NetSegmentEnd.Flags.None }
		});
	}

	private static IEnumerable<LaneInfo> GetSideLanes(LaneInfo? lane, bool left, bool forward)
	{
		while (lane != null)
		{
			lane = left ? lane.LeftLane : lane.RightLane;

			if (lane != null)
			{
				if (lane.Direction == (forward ? LaneDirection.Backwards : LaneDirection.Forward))
					break;

				yield return lane;
			}
			else
				break;
		}
	}

	private static IEnumerable<NetLaneProps.Prop> GetBikeSignProps(LaneInfo lane)
	{
		var bikeProp = GetProp(Prop.BicycleSign);

		if (bikeProp == null)
		{
			yield break;
		}

		if (lane.RightLane?.Type == LaneType.Bike && lane.RightLane.Direction == LaneDirection.Backwards)
		{
			yield return new NetLaneProps.Prop
			{
				m_prop = bikeProp,
				m_finalProp = bikeProp,
				m_flagsRequired = NetLane.Flags.None,
				m_flagsForbidden = NetLane.Flags.Inverted,
				m_startFlagsRequired = NetNode.Flags.None,
				m_startFlagsForbidden = NetNode.Flags.None,
				m_endFlagsRequired = NetNode.Flags.Junction,
				m_endFlagsForbidden = NetNode.Flags.LevelCrossing,
				m_cornerAngle = 0,
				m_minLength = 10F,
				m_repeatDistance = 0,
				m_segmentOffset = 1F,
				m_angle = 250F,
				m_probability = 100,
				m_position = new Vector3((lane.LaneWidth / 2F) - 0.3F, 0, 0),
			}.Extend(prop => new NetInfoExtionsion.LaneProp(prop)
			{
				SegmentFlags = new NetInfoExtionsion.SegmentInfoFlags
				{ Forbidden = RoadUtils.S_RemoveRoadClutter },
				EndNodeFlags = new NetInfoExtionsion.NodeInfoFlags
				{ Forbidden = NetNodeExt.Flags.SamePrefab }
			});
		}

		if (lane.LeftLane?.Type == LaneType.Bike && lane.LeftLane.Direction == LaneDirection.Forward)
		{
			yield return new NetLaneProps.Prop
			{
				m_prop = bikeProp,
				m_finalProp = bikeProp,
				m_flagsRequired = NetLane.Flags.None,
				m_flagsForbidden = NetLane.Flags.Inverted,
				m_startFlagsRequired = NetNode.Flags.Junction,
				m_startFlagsForbidden = NetNode.Flags.LevelCrossing,
				m_endFlagsRequired = NetNode.Flags.None,
				m_endFlagsForbidden = NetNode.Flags.None,
				m_cornerAngle = 0,
				m_minLength = 10F,
				m_repeatDistance = 0,
				m_segmentOffset = -1F,
				m_angle = 70F,
				m_probability = 100,
				m_position = new Vector3((-lane.LaneWidth / 2F) + 0.3F, 0, 0),
			}.Extend(prop => new NetInfoExtionsion.LaneProp(prop)
			{
				SegmentFlags = new NetInfoExtionsion.SegmentInfoFlags
				{ Forbidden = RoadUtils.S_RemoveRoadClutter },
				StartNodeFlags = new NetInfoExtionsion.NodeInfoFlags
				{ Forbidden = NetNodeExt.Flags.SamePrefab }
			});
		}
	}

	private static IEnumerable<NetLaneProps.Prop> GetWarningSigns(LaneInfo lane)
	{
		var railroadCrossingProp = GetProp(Prop.RailwayCrossingAheadSign);
		var signalAheadProp = GetProp(Prop.TrafficLightAheadSign);

		yield return new NetLaneProps.Prop
		{
			m_prop = railroadCrossingProp,
			m_finalProp = railroadCrossingProp,
			m_flagsRequired = NetLane.Flags.None,
			m_flagsForbidden = NetLane.Flags.JoinedJunctionInverted,
			m_startFlagsRequired = NetNode.Flags.None,
			m_startFlagsForbidden = NetNode.Flags.None,
			m_endFlagsRequired = NetNode.Flags.LevelCrossing,
			m_endFlagsForbidden = NetNode.Flags.None,
			m_cornerAngle = 0,
			m_minLength = 20F,
			m_repeatDistance = 0,
			m_segmentOffset = -0.65F,
			m_angle = 70F,
			m_probability = 100,
			m_position = new Vector3((-lane.LaneWidth / 2F) + 0.4F, 0, 2F),
		}.Extend(prop => new NetInfoExtionsion.LaneProp(prop)
		{
			SegmentFlags = new NetInfoExtionsion.SegmentInfoFlags
			{ Forbidden = RoadUtils.S_RemoveRoadClutter }
		});

		yield return new NetLaneProps.Prop
		{
			m_prop = signalAheadProp,
			m_finalProp = signalAheadProp,
			m_flagsRequired = NetLane.Flags.None,
			m_flagsForbidden = NetLane.Flags.JoinedJunctionInverted,
			m_startFlagsRequired = NetNode.Flags.None,
			m_startFlagsForbidden = NetNode.Flags.None,
			m_endFlagsRequired = NetNode.Flags.TrafficLights,
			m_endFlagsForbidden = NetNode.Flags.LevelCrossing,
			m_cornerAngle = 0,
			m_minLength = 20F,
			m_repeatDistance = 0,
			m_segmentOffset = -0.65F,
			m_angle = 70F,
			m_probability = 100,
			m_position = new Vector3((-lane.LaneWidth / 2F) + 0.4F, 0, 2F),
		}.Extend(prop => new NetInfoExtionsion.LaneProp(prop)
		{
			SegmentFlags = new NetInfoExtionsion.SegmentInfoFlags
			{ Forbidden = RoadUtils.S_RemoveRoadClutter }
		});
	}

	private static NetLaneProps.Prop StopSign(float propPosition)
	{
		var stopSign = GetProp(Prop.StopSign);

		return new NetLaneProps.Prop
		{
			m_prop = stopSign,
			m_finalProp = stopSign,
			m_flagsRequired = NetLane.Flags.YieldEnd,
			m_flagsForbidden = NetLane.Flags.Inverted,
			m_startFlagsRequired = NetNode.Flags.None,
			m_startFlagsForbidden = NetNode.Flags.None,
			m_endFlagsRequired = NetNode.Flags.Junction,
			m_endFlagsForbidden = NetNode.Flags.TrafficLights | NetNode.Flags.OneWayIn,
			m_minLength = 0,
			m_repeatDistance = 0,
			m_segmentOffset = 1,
			m_angle = 0,
			m_probability = 100,
			m_position = new Vector3(propPosition, 0, 0),
		};
	}

	private static NetLaneProps.Prop SpeedSign(RoadInfo road, LaneInfo lane)
	{
		PropInfo? sign = null;

		switch ((int)Math.Round(road.SpeedLimit * (road.RegionType == RegionType.USA ? 1.609F : 1F) / 10D) * 10)
		{
			case 10: sign = GetProp(Prop.SpeedSign10); break;
			case 20: sign = GetProp(Prop.SpeedSign20); break;
			case 30: sign = GetProp(Prop.SpeedSign30); break;
			case 40: sign = GetProp(Prop.SpeedSign40); break;
			case 50: sign = GetProp(Prop.SpeedSign50); break;
			case 60: sign = GetProp(Prop.SpeedSign60); break;
			case 70: sign = GetProp(Prop.SpeedSign70); break;
			case 80: sign = GetProp(Prop.SpeedSign80); break;
			case 90: sign = GetProp(Prop.SpeedSign90); break;
			case 100: sign = GetProp(Prop.SpeedSign100); break;
			case 110: sign = GetProp(Prop.SpeedSign110); break;
			case 120: sign = GetProp(Prop.SpeedSign120); break;
			case 130: sign = GetProp(Prop.SpeedSign130); break;
			case 140: sign = GetProp(Prop.SpeedSign140); break;
		}

		return new NetLaneProps.Prop
		{
			m_prop = sign,
			m_finalProp = sign,
			m_flagsRequired = NetLane.Flags.None,
			m_flagsForbidden = NetLane.Flags.Inverted,
			m_minLength = 15F,
			m_repeatDistance = 0,
			m_segmentOffset = -0.65F,
			m_angle = 340F,
			m_probability = 100,
			m_position = new Vector3((-lane.LaneWidth / 2F) + 0.4F, 0, -1F),
		}.Extend(prop => new NetInfoExtionsion.LaneProp(prop)
		{
			SegmentFlags = new NetInfoExtionsion.SegmentInfoFlags
			{ Forbidden = RoadUtils.S_RemoveRoadClutter },
			StartNodeFlags = new NetInfoExtionsion.NodeInfoFlags
			{ Required = NetNodeExt.Flags.SpeedChange },
		});
	}

	private static NetLaneProps.Prop RailwayCrossing(RoadInfo road, float drivableArea, float propPosition)
	{
		var railwayCrossing = getCrossingProp(drivableArea);

		return new NetLaneProps.Prop
		{
			m_prop = railwayCrossing,
			m_finalProp = railwayCrossing,
			m_flagsRequired = NetLane.Flags.None,
			m_flagsForbidden = NetLane.Flags.Inverted,
			m_startFlagsRequired = NetNode.Flags.None,
			m_startFlagsForbidden = NetNode.Flags.None,
			m_endFlagsRequired = NetNode.Flags.LevelCrossing,
			m_endFlagsForbidden = NetNode.Flags.None,
			m_cornerAngle = 1F,
			m_minLength = 0,
			m_repeatDistance = 0,
			m_segmentOffset = 1,
			m_angle = 0,
			m_colorMode = NetLaneProps.ColorMode.EndState,
			m_probability = 100,
			m_position = new Vector3(propPosition, 0, road.ContainsWiredLanes ? -2F : -1F),
		};

		static PropInfo getCrossingProp(float drivableArea)
		{
			if (drivableArea >= 11F)
			{
				return GetProp(Prop.RailwayCrossingVeryLong);
			}

			if (drivableArea >= 8F)
			{
				return GetProp(Prop.RailwayCrossingLong);
			}

			return drivableArea >= 6F ? GetProp(Prop.RailwayCrossingMedium) : GetProp(Prop.RailwayCrossingShort);
		}
	}
}
