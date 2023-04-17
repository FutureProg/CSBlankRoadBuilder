using AdaptiveRoads.Manager;

using BlankRoadBuilder.Domain.Options;
using BlankRoadBuilder.ThumbnailMaker;
using BlankRoadBuilder.Util.Props.Templates;

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace BlankRoadBuilder.Util.Props;

public partial class LanePropsUtil
{
	private IEnumerable<NetLaneProps.Prop> GetSignsAndTrafficLights()
	{
		if (Lane.Tags.HasFlag(LaneTag.Sidewalk) && Lane.Type != LaneType.Curb)
			yield break;

		var propPosition = -Math.Max(0, (Lane.LaneWidth / 2) - 0.5F);

		foreach (var prop in GetTrafficSigns(propPosition))
		{
			yield return prop;
		}

		foreach (var prop in GetBikeSignProps(propPosition))
		{
			yield return prop;
		}

		yield return TrafficLight(Lane.TrafficLight.LeftForward, propPosition, true && Lane.TrafficLight.LeftForward == Prop.TrafficLightPedestrian);
		yield return TrafficLight(Lane.TrafficLight.RightForward, -propPosition, ModOptions.FlipTrafficLights && Lane.TrafficLight.RightForward != Prop.TrafficLightPedestrian);
		yield return TrafficLight(Lane.TrafficLight.LeftBackward, -propPosition).ToggleForwardBackward(true, ModOptions.FlipTrafficLights && Lane.TrafficLight.LeftBackward != Prop.TrafficLightPedestrian);
		yield return TrafficLight(Lane.TrafficLight.RightBackward, propPosition, true && Lane.TrafficLight.RightBackward == Prop.TrafficLightPedestrian).ToggleForwardBackward(true, false);
	}

	private IEnumerable<NetLaneProps.Prop> GetTrafficSigns(float propPosition)
	{
		if (Lane.TrafficLight.LeftForwardSpace >= 2F)
		{
			yield return RailwayCrossing(Lane.TrafficLight.LeftForwardSpace, propPosition);

			if (!GetSideLanes(Lane, true, true).All(x => x.Type.HasAnyFlag(LaneType.Parking, LaneType.Bike)))
			{
				yield return StopSign(propPosition);
				yield return YieldSign(propPosition);
				yield return PrioritySign(propPosition);

				foreach (var prop in GetWarningSigns(propPosition))
				{
					yield return prop;
				}
			}

			if (!GetSideLanes(Lane, true, true).All(x => x.Type.HasAnyFlag(LaneType.Parking, LaneType.Bike, LaneType.Tram)))
			{
				foreach (var prop in SpeedSigns(propPosition))
				{
					yield return prop;
				}
			}
		}

		if (Road.RoadType != RoadType.Highway && ThumbnailMakerUtil.IsOneWay(Road.Lanes) == true)
		{
			if (Lane.TrafficLight.LeftForwardSpace >= 2.5F)
			{
				yield return OneWayEndsSign(propPosition);
			}

			if (Lane.TrafficLight.RightForwardSpace >= 2.5F)
			{
				yield return DoNotEnterSign(-propPosition);
			}
		}

		if (Lane.TrafficLight.RightBackwardSpace >= 2F)
		{
			yield return RailwayCrossing(Lane.TrafficLight.RightBackwardSpace, propPosition).ToggleForwardBackward();

			if (!GetSideLanes(Lane, false, false).All(x => x.Type.HasAnyFlag(LaneType.Parking, LaneType.Bike)))
			{
				yield return StopSign(propPosition).ToggleForwardBackward();
				yield return YieldSign(propPosition).ToggleForwardBackward();
				yield return PrioritySign(propPosition).ToggleForwardBackward();

				foreach (var prop in GetWarningSigns(propPosition))
				{
					yield return prop.ToggleForwardBackward();
				}
			}

			if (!GetSideLanes(Lane, false, false).All(x => x.Type.HasAnyFlag(LaneType.Parking, LaneType.Bike, LaneType.Tram)))
			{
				foreach (var prop in SpeedSigns(propPosition))
				{
					yield return prop.ToggleForwardBackward();
				}
			}
		}

		if (Lane.Type == LaneType.Curb)
		{
			if (Road.RoadType == RoadType.Highway && Road.Lanes.Any(x => x != Lane && x.Direction == Lane.Direction))
			{
				yield return HighwaySign(propPosition).ToggleForwardBackward(Lane.Direction == LaneDirection.Backwards);
				yield return HighwayEndSign(propPosition).ToggleForwardBackward(Lane.Direction == LaneDirection.Backwards);
			}
		}
	}

	private NetLaneProps.Prop TrafficLight(Prop prop, float propPosition, bool flipAngle = false)
	{
		var propTemplate = GetProp(prop);

		return new NetLaneProps.Prop
		{
			m_prop = propTemplate,
			m_tree = propTemplate,
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
			m_angle = (flipAngle ? 270 : 90) + propTemplate.Angle,
			m_colorMode = NetLaneProps.ColorMode.EndState,
			m_probability = 100,
			m_position = new Vector3(propPosition, 0, Road.ContainsWiredLanes ? -1.5F : -0.75F),
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

				if (lane.Type is LaneType.Filler or LaneType.Curb)
					break;

				yield return lane;
			}
			else
				break;
		}
	}

	private IEnumerable<NetLaneProps.Prop> GetBikeSignProps(float propPosition)
	{
		var bikeProp = GetProp(Prop.BicycleSign);

		if (bikeProp == null)
		{
			yield break;
		}

		if (Lane.RightLane?.Type == LaneType.Bike && Lane.RightLane.Direction == LaneDirection.Backwards)
		{
			yield return new NetLaneProps.Prop
			{
				m_prop = bikeProp,
				m_tree = bikeProp,
				m_flagsRequired = NetLane.Flags.None,
				m_flagsForbidden = NetLane.Flags.Inverted,
				m_startFlagsRequired = NetNode.Flags.None,
				m_startFlagsForbidden = NetNode.Flags.None,
				m_endFlagsRequired = NetNode.Flags.Junction,
				m_endFlagsForbidden = NetNode.Flags.LevelCrossing,
				m_cornerAngle = 0,
				m_minLength = 10F,
				m_repeatDistance = 0,
				m_segmentOffset = -bikeProp.SegmentOffset,
				m_angle = bikeProp.Angle + 180,
				m_probability = bikeProp.Probability,
				m_position = new Vector3(propPosition, 0, 0) + PropPosition(bikeProp, -1),
			}.Extend(prop => new NetInfoExtionsion.LaneProp(prop)
			{
				SegmentFlags = new NetInfoExtionsion.SegmentInfoFlags
				{
					Forbidden = ModOptions.HideRoadClutter ? NetSegmentExt.Flags.None : RoadUtils.Flags.S_RemoveRoadClutter,
					Required = ModOptions.HideRoadClutter ? RoadUtils.Flags.S_RemoveRoadClutter : NetSegmentExt.Flags.None
				},
				EndNodeFlags = new NetInfoExtionsion.NodeInfoFlags
				{ Forbidden = NetNodeExt.Flags.SamePrefab }
			});
		}

		if (Lane.LeftLane?.Type == LaneType.Bike && Lane.LeftLane.Direction == LaneDirection.Forward)
		{
			yield return new NetLaneProps.Prop
			{
				m_prop = bikeProp,
				m_tree = bikeProp,
				m_flagsRequired = NetLane.Flags.None,
				m_flagsForbidden = NetLane.Flags.Inverted,
				m_startFlagsRequired = NetNode.Flags.Junction,
				m_startFlagsForbidden = NetNode.Flags.LevelCrossing,
				m_endFlagsRequired = NetNode.Flags.None,
				m_endFlagsForbidden = NetNode.Flags.None,
				m_cornerAngle = 0,
				m_minLength = 10F,
				m_repeatDistance = 0,
				m_segmentOffset = bikeProp.SegmentOffset,
				m_angle = bikeProp.Angle,
				m_probability = bikeProp.Probability,
				m_position = new Vector3(-propPosition, 0, 0) + PropPosition(bikeProp, 1),
			}.Extend(prop => new NetInfoExtionsion.LaneProp(prop)
			{
				SegmentFlags = new NetInfoExtionsion.SegmentInfoFlags
				{
					Forbidden = ModOptions.HideRoadClutter ? NetSegmentExt.Flags.None : RoadUtils.Flags.S_RemoveRoadClutter,
					Required = ModOptions.HideRoadClutter ? RoadUtils.Flags.S_RemoveRoadClutter : NetSegmentExt.Flags.None
				},
				StartNodeFlags = new NetInfoExtionsion.NodeInfoFlags
				{ Forbidden = NetNodeExt.Flags.SamePrefab }
			});
		}
	}

	private IEnumerable<NetLaneProps.Prop> GetWarningSigns(float propPosition)
	{
		var railroadCrossingProp = GetProp(Prop.RailwayCrossingAheadSign);
		var signalAheadProp = GetProp(Prop.TrafficLightAheadSign);
		var pedCrossing = GetProp(Prop.PedestrianCrossingSign);

		yield return new NetLaneProps.Prop
		{
			m_prop = railroadCrossingProp,
			m_tree = railroadCrossingProp,
			m_flagsRequired = NetLane.Flags.None,
			m_flagsForbidden = NetLane.Flags.JoinedJunctionInverted,
			m_startFlagsRequired = NetNode.Flags.None,
			m_startFlagsForbidden = NetNode.Flags.LevelCrossing,
			m_endFlagsRequired = NetNode.Flags.LevelCrossing,
			m_endFlagsForbidden = NetNode.Flags.None,
			m_cornerAngle = 0,
			m_minLength = 20F,
			m_repeatDistance = 0,
			m_segmentOffset = railroadCrossingProp.SegmentOffset,
			m_angle = railroadCrossingProp.Angle,
			m_probability = railroadCrossingProp.Probability,
			m_position = new Vector3(propPosition, 0, 2F) + railroadCrossingProp.Position,
		}.Extend(prop => new NetInfoExtionsion.LaneProp(prop)
		{
			SegmentFlags = new NetInfoExtionsion.SegmentInfoFlags
			{
				Forbidden = ModOptions.HideRoadClutter ? NetSegmentExt.Flags.None : RoadUtils.Flags.S_RemoveRoadClutter,
				Required = ModOptions.HideRoadClutter ? RoadUtils.Flags.S_RemoveRoadClutter : NetSegmentExt.Flags.None
			}
		});

		yield return new NetLaneProps.Prop
		{
			m_prop = signalAheadProp,
			m_tree = signalAheadProp,
			m_flagsRequired = NetLane.Flags.None,
			m_flagsForbidden = NetLane.Flags.JoinedJunctionInverted,
			m_startFlagsRequired = NetNode.Flags.None,
			m_startFlagsForbidden = NetNode.Flags.TrafficLights,
			m_endFlagsRequired = NetNode.Flags.TrafficLights,
			m_endFlagsForbidden = NetNode.Flags.LevelCrossing,
			m_cornerAngle = 0,
			m_minLength = 20F,
			m_repeatDistance = 0,
			m_segmentOffset = signalAheadProp.SegmentOffset,
			m_angle = signalAheadProp.Angle,
			m_probability = signalAheadProp.Probability,
			m_position = new Vector3(propPosition, 0, 2F) + signalAheadProp.Position,
		}.Extend(prop => new NetInfoExtionsion.LaneProp(prop)
		{
			SegmentFlags = new NetInfoExtionsion.SegmentInfoFlags
			{
				Forbidden = ModOptions.HideRoadClutter ? NetSegmentExt.Flags.None : RoadUtils.Flags.S_RemoveRoadClutter,
				Required = ModOptions.HideRoadClutter ? RoadUtils.Flags.S_RemoveRoadClutter : NetSegmentExt.Flags.None
			}
		});

		yield return new NetLaneProps.Prop
		{
			m_prop = pedCrossing,
			m_tree = pedCrossing,
			m_flagsRequired = NetLane.Flags.None,
			m_flagsForbidden = NetLane.Flags.JoinedJunctionInverted,
			m_endFlagsForbidden = NetNode.Flags.LevelCrossing | NetNode.Flags.End | NetNode.Flags.TrafficLights | NetNode.Flags.Middle | NetNode.Flags.Bend | NetNode.Flags.Underground,
			m_cornerAngle = 0,
			m_minLength = 20F,
			m_repeatDistance = 0,
			m_segmentOffset = pedCrossing.SegmentOffset,
			m_angle = pedCrossing.Angle,
			m_probability = pedCrossing.Probability,
			m_position = new Vector3(propPosition, 0, 0) + pedCrossing.Position,
		}.Extend(prop => new NetInfoExtionsion.LaneProp(prop)
		{
			SegmentEndFlags = new NetInfoExtionsion.SegmentEndInfoFlags
			{
				Required = NetSegmentEnd.Flags.ZebraCrossing,
			},
			SegmentFlags = new NetInfoExtionsion.SegmentInfoFlags
			{
				Forbidden = ModOptions.HideRoadClutter ? NetSegmentExt.Flags.None : RoadUtils.Flags.S_RemoveRoadClutter,
				Required = ModOptions.HideRoadClutter ? RoadUtils.Flags.S_RemoveRoadClutter : NetSegmentExt.Flags.None
			}
		});
	}

	private NetLaneProps.Prop DoNotEnterSign(float propPosition)
	{
		var sign = GetProp(Prop.DoNotEnterSign);

		return new NetLaneProps.Prop
		{
			m_prop = sign,
			m_tree = sign,
			m_flagsForbidden = NetLane.Flags.Inverted,
			m_startFlagsRequired = NetNode.Flags.None,
			m_startFlagsForbidden = NetNode.Flags.None,
			m_endFlagsRequired = NetNode.Flags.OneWayOut | NetNode.Flags.Junction,
			m_endFlagsForbidden = NetNode.Flags.LevelCrossing,
			m_minLength = 0,
			m_repeatDistance = 0,
			m_segmentOffset = sign.SegmentOffset,
			m_angle = sign.Angle,
			m_probability = sign.Probability,
			m_position = new Vector3(propPosition, 0, 0) + sign.Position,
		}.Extend(prop => new(prop) { EndNodeFlags = new() { Forbidden = NetNodeExt.Flags.TwoSegments } });
	}

	private NetLaneProps.Prop OneWayEndsSign(float propPosition)
	{
		var sign = GetProp(Prop.EndOfOneWaySign);

		return new NetLaneProps.Prop
		{
			m_prop = sign,
			m_tree = sign,
			m_flagsForbidden = NetLane.Flags.Inverted,
			m_startFlagsRequired = NetNode.Flags.None,
			m_startFlagsForbidden = NetNode.Flags.None,
			m_endFlagsRequired = NetNode.Flags.OneWayIn | NetNode.Flags.Junction,
			m_endFlagsForbidden = NetNode.Flags.LevelCrossing,
			m_minLength = 0,
			m_repeatDistance = 0,
			m_segmentOffset = sign.SegmentOffset,
			m_angle = sign.Angle,
			m_probability = sign.Probability,
			m_position = new Vector3(propPosition, 0, 0) + sign.Position,
		}.Extend(prop => new(prop) { EndNodeFlags = new() { Forbidden = NetNodeExt.Flags.TwoSegments } });
	}

	private NetLaneProps.Prop StopSign(float propPosition)
	{
		var stopSign = GetProp(Prop.StopSign);

		return new NetLaneProps.Prop
		{
			m_prop = stopSign,
			m_tree = stopSign,
			m_flagsForbidden = NetLane.Flags.Inverted,
			m_startFlagsRequired = NetNode.Flags.None,
			m_startFlagsForbidden = NetNode.Flags.None,
			m_endFlagsRequired = NetNode.Flags.Junction,
			m_endFlagsForbidden = NetNode.Flags.TrafficLights | NetNode.Flags.OneWayIn,
			m_minLength = 0,
			m_repeatDistance = 0,
			m_segmentOffset = stopSign.SegmentOffset,
			m_angle = stopSign.Angle,
			m_probability = stopSign.Probability,
			m_position = new Vector3(propPosition, 0, 0) + stopSign.Position,
		}.Extend(p => new NetInfoExtionsion.LaneProp(p)
		{
			SegmentEndFlags = new NetInfoExtionsion.SegmentEndInfoFlags
			{
				Required = NetSegmentEnd.Flags.Stop
			}
		});
	}

	private NetLaneProps.Prop YieldSign(float propPosition)
	{
		var yieldSign = GetProp(Prop.YieldSign);

		return new NetLaneProps.Prop
		{
			m_prop = yieldSign,
			m_tree = yieldSign,
			m_flagsForbidden = NetLane.Flags.Inverted,
			m_startFlagsRequired = NetNode.Flags.None,
			m_startFlagsForbidden = NetNode.Flags.None,
			m_endFlagsRequired = NetNode.Flags.Junction,
			m_endFlagsForbidden = NetNode.Flags.TrafficLights | NetNode.Flags.OneWayIn,
			m_minLength = 0,
			m_repeatDistance = 0,
			m_segmentOffset = yieldSign.SegmentOffset,
			m_angle = yieldSign.Angle,
			m_probability = yieldSign.Probability,
			m_position = new Vector3(propPosition, 0, 0) + yieldSign.Position,
		}.Extend(p => new NetInfoExtionsion.LaneProp(p)
		{
			SegmentEndFlags = new NetInfoExtionsion.SegmentEndInfoFlags
			{
				Required = NetSegmentEnd.Flags.Yield
			}
		});
	}

	private NetLaneProps.Prop PrioritySign(float propPosition)
	{
		var prioritySign = GetProp(Prop.PrioritySign);

		return new NetLaneProps.Prop
		{
			m_prop = prioritySign,
			m_tree = prioritySign,
			m_flagsForbidden = NetLane.Flags.Inverted,
			m_startFlagsRequired = NetNode.Flags.None,
			m_startFlagsForbidden = NetNode.Flags.None,
			m_endFlagsRequired = NetNode.Flags.Junction,
			m_endFlagsForbidden = NetNode.Flags.TrafficLights | NetNode.Flags.OneWayIn,
			m_minLength = 0,
			m_repeatDistance = 0,
			m_segmentOffset = prioritySign.SegmentOffset,
			m_angle = prioritySign.Angle,
			m_probability = prioritySign.Probability,
			m_position = new Vector3(propPosition, 0, 0) + prioritySign.Position,
		}.Extend(p => new NetInfoExtionsion.LaneProp(p)
		{
			SegmentEndFlags = new NetInfoExtionsion.SegmentEndInfoFlags
			{
				Required = NetSegmentEnd.Flags.PriorityMain
			}
		});
	}

	private NetLaneProps.Prop HighwaySign(float propPosition)
	{
		var highwaySign = GetProp(Prop.HighwaySign);

		return new NetLaneProps.Prop
		{
			m_prop = highwaySign,
			m_tree = highwaySign,
			m_flagsForbidden = NetLane.Flags.Inverted,
			m_startFlagsRequired = NetNode.Flags.Transition,
			m_startFlagsForbidden = NetNode.Flags.None,
			m_minLength = 20,
			m_repeatDistance = 0,
			m_segmentOffset = highwaySign.SegmentOffset,
			m_angle = highwaySign.Angle,
			m_probability = highwaySign.Probability,
			m_position = new Vector3(propPosition, 0, 0) + highwaySign.Position,
		}.Extend(prop => new NetInfoExtionsion.LaneProp(prop)
		{
			SegmentFlags = new NetInfoExtionsion.SegmentInfoFlags
			{
				Forbidden = ModOptions.HideRoadClutter ? NetSegmentExt.Flags.None : RoadUtils.Flags.S_RemoveRoadClutter,
				Required = ModOptions.HideRoadClutter ? RoadUtils.Flags.S_RemoveRoadClutter : NetSegmentExt.Flags.None
			}
		});
	}

	private NetLaneProps.Prop HighwayEndSign(float propPosition)
	{
		var highwayEndSign = GetProp(Prop.HighwayEndSign);

		return new NetLaneProps.Prop
		{
			m_prop = highwayEndSign,
			m_tree = highwayEndSign,
			m_flagsForbidden = NetLane.Flags.Inverted,
			m_endFlagsRequired = NetNode.Flags.Transition,
			m_startFlagsForbidden = NetNode.Flags.None,
			m_minLength = 20,
			m_repeatDistance = 0,
			m_segmentOffset = highwayEndSign.SegmentOffset,
			m_angle = highwayEndSign.Angle,
			m_probability = highwayEndSign.Probability,
			m_position = new Vector3(propPosition, 0, 0) + highwayEndSign.Position,
		}.Extend(prop => new NetInfoExtionsion.LaneProp(prop)
		{
			SegmentFlags = new NetInfoExtionsion.SegmentInfoFlags
			{
				Forbidden = ModOptions.HideRoadClutter ? NetSegmentExt.Flags.None : RoadUtils.Flags.S_RemoveRoadClutter,
				Required = ModOptions.HideRoadClutter ? RoadUtils.Flags.S_RemoveRoadClutter : NetSegmentExt.Flags.None
			}
		});
	}

	private IEnumerable<NetLaneProps.Prop> SpeedSigns(float propPosition)
	{
		var signs = new Dictionary<float, PropTemplate>()
		{
			{ 10, GetProp(Prop.SpeedSign10) },
			{ 20, GetProp(Prop.SpeedSign20) },
			{ 30, GetProp(Prop.SpeedSign30) },
			{ 40, GetProp(Prop.SpeedSign40) },
			{ 50, GetProp(Prop.SpeedSign50) },
			{ 60, GetProp(Prop.SpeedSign60) },
			{ 70, GetProp(Prop.SpeedSign70) },
			{ 80, GetProp(Prop.SpeedSign80) },
			{ 90, GetProp(Prop.SpeedSign90) },
			{ 100, GetProp(Prop.SpeedSign100) },
			{ 110, GetProp(Prop.SpeedSign110) },
			{ 120, GetProp(Prop.SpeedSign120) },
			{ 130, GetProp(Prop.SpeedSign130) },
			{ 140, GetProp(Prop.SpeedSign140) },
		};

		foreach (var grp in signs.GroupBy(x => x.Value.PropName))
		{
			var sign = grp.First().Value;
			var minSpeed = (grp.Min(x => x.Key) - 5) * (Road.RegionType == RegionType.USA ? 1.609F : 1F) / 50;
			var maxSpeed = (grp.Max(x => x.Key) + 5) * (Road.RegionType == RegionType.USA ? 1.609F : 1F) / 50;

			yield return new NetLaneProps.Prop
			{
				m_prop = sign,
				m_tree = sign,
				m_flagsRequired = NetLane.Flags.None,
				m_flagsForbidden = NetLane.Flags.Inverted,
				m_minLength = 15F,
				m_repeatDistance = 0,
				m_segmentOffset = sign.SegmentOffset,
				m_angle = sign.Angle,
				m_probability = sign.Probability,
				m_position = new Vector3(propPosition, 0, 0) + sign.Position,
			}.Extend(prop => new NetInfoExtionsion.LaneProp(prop)
			{
				ForwardSpeedLimit = new()
				{
					Lower = minSpeed,
					Upper = maxSpeed,
				},
				SegmentFlags = new NetInfoExtionsion.SegmentInfoFlags
				{
					Forbidden = ModOptions.HideRoadClutter ? NetSegmentExt.Flags.None : RoadUtils.Flags.S_RemoveRoadClutter,
					Required = ModOptions.HideRoadClutter ? RoadUtils.Flags.S_RemoveRoadClutter : NetSegmentExt.Flags.None
				},
				StartNodeFlags = new NetInfoExtionsion.NodeInfoFlags
				{ Required = NetNodeExt.Flags.SpeedChange },
			}); 
		}
	}

	private NetLaneProps.Prop RailwayCrossing(float drivableArea, float propPosition)
	{
		var railwayCrossing = getCrossingProp(drivableArea);

		return new NetLaneProps.Prop
		{
			m_prop = railwayCrossing,
			m_tree = railwayCrossing,
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
			m_angle = railwayCrossing.Angle,
			m_colorMode = NetLaneProps.ColorMode.EndState,
			m_probability = 100,
			m_position = new Vector3(propPosition, 0, Road.ContainsWiredLanes ? -2F : -1F) + railwayCrossing.Position,
		};

		PropTemplate getCrossingProp(float drivableArea)
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
