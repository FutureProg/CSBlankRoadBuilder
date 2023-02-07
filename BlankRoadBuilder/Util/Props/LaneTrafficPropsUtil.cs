using AdaptiveRoads.Manager;

using BlankRoadBuilder.Domain.Options;
using BlankRoadBuilder.ThumbnailMaker;

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

		foreach (var prop in GetBikeSignProps())
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

				foreach (var prop in GetWarningSigns())
				{
					yield return prop;
				}
			}

			if (!GetSideLanes(Lane, true, true).All(x => x.Type.HasAnyFlag(LaneType.Parking, LaneType.Bike, LaneType.Tram)))
			{
				yield return SpeedSign();
			}
		}

		if (Lane.TrafficLight.RightBackwardSpace >= 2F)
		{
			yield return RailwayCrossing(Lane.TrafficLight.LeftForwardSpace, propPosition).ToggleForwardBackward();

			if (!GetSideLanes(Lane, false, false).All(x => x.Type.HasAnyFlag(LaneType.Parking, LaneType.Bike)))
			{
				yield return StopSign(propPosition).ToggleForwardBackward();
				yield return YieldSign(propPosition).ToggleForwardBackward();

				foreach (var prop in GetWarningSigns())
				{
					yield return prop.ToggleForwardBackward();
				}
			}

			if (!GetSideLanes(Lane, false, false).All(x => x.Type.HasAnyFlag(LaneType.Parking, LaneType.Bike, LaneType.Tram)))
			{
				yield return SpeedSign().ToggleForwardBackward();
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

				yield return lane;
			}
			else
				break;
		}
	}

	private IEnumerable<NetLaneProps.Prop> GetBikeSignProps()
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
				m_position = new Vector3((Lane.LaneWidth / 2F) - 0.3F, 0, 0),
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
				m_position = new Vector3((-Lane.LaneWidth / 2F) + 0.3F, 0, 0),
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

	private IEnumerable<NetLaneProps.Prop> GetWarningSigns()
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
			m_position = new Vector3((-Lane.LaneWidth / 2F) + 0.4F, 0, 2F),
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
			m_position = new Vector3((-Lane.LaneWidth / 2F) + 0.4F, 0, 2F),
		}.Extend(prop => new NetInfoExtionsion.LaneProp(prop)
		{
			SegmentFlags = new NetInfoExtionsion.SegmentInfoFlags
			{
				Forbidden = ModOptions.HideRoadClutter ? NetSegmentExt.Flags.None : RoadUtils.Flags.S_RemoveRoadClutter,
				Required = ModOptions.HideRoadClutter ? RoadUtils.Flags.S_RemoveRoadClutter : NetSegmentExt.Flags.None
			}
		});
	}

	private NetLaneProps.Prop StopSign(float propPosition)
	{
		var stopSign = GetProp(Prop.StopSign);

		return new NetLaneProps.Prop
		{
			m_prop = stopSign,
			m_finalProp = stopSign,
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
		var stopSign = GetProp(Prop.YieldSign);

		return new NetLaneProps.Prop
		{
			m_prop = stopSign,
			m_finalProp = stopSign,
			m_flagsForbidden = NetLane.Flags.Inverted,
			m_startFlagsRequired = NetNode.Flags.None,
			m_startFlagsForbidden = NetNode.Flags.None,
			m_endFlagsRequired = NetNode.Flags.Junction,
			m_endFlagsForbidden = NetNode.Flags.TrafficLights | NetNode.Flags.OneWayIn,
			m_minLength = 0,
			m_repeatDistance = 0,
			m_segmentOffset = 1,
			m_angle = 90,
			m_probability = 100,
			m_position = new Vector3(propPosition, 0, 0),
		}.Extend(p => new NetInfoExtionsion.LaneProp(p)
		{
			SegmentEndFlags = new NetInfoExtionsion.SegmentEndInfoFlags
			{
				Required = NetSegmentEnd.Flags.Yield
			}
		});
	}

	private NetLaneProps.Prop HighwaySign(float propPosition)
	{
		var stopSign = GetProp(Prop.HighwaySign);

		return new NetLaneProps.Prop
		{
			m_prop = stopSign,
			m_finalProp = stopSign,
			m_flagsForbidden = NetLane.Flags.Inverted,
			m_startFlagsRequired = NetNode.Flags.Transition,
			m_startFlagsForbidden = NetNode.Flags.None,
			m_minLength = 20,
			m_repeatDistance = 0,
			m_segmentOffset = -1,
			m_angle = -20,
			m_probability = 100,
			m_position = new Vector3(propPosition, 0, 10),
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
		var stopSign = GetProp(Prop.HighwayEndSign);

		return new NetLaneProps.Prop
		{
			m_prop = stopSign,
			m_finalProp = stopSign,
			m_flagsForbidden = NetLane.Flags.Inverted,
			m_endFlagsRequired = NetNode.Flags.Transition,
			m_startFlagsForbidden = NetNode.Flags.None,
			m_minLength = 20,
			m_repeatDistance = 0,
			m_segmentOffset = 0,
			m_angle = 70,
			m_probability = 100,
			m_position = new Vector3(propPosition, 0, 10),
		}.Extend(prop => new NetInfoExtionsion.LaneProp(prop)
		{
			SegmentFlags = new NetInfoExtionsion.SegmentInfoFlags
			{
				Forbidden = ModOptions.HideRoadClutter ? NetSegmentExt.Flags.None : RoadUtils.Flags.S_RemoveRoadClutter,
				Required = ModOptions.HideRoadClutter ? RoadUtils.Flags.S_RemoveRoadClutter : NetSegmentExt.Flags.None
			}
		});
	}

	private NetLaneProps.Prop SpeedSign()
	{
		PropInfo? sign = ((int)Math.Round(Road.SpeedLimit * (Road.RegionType == RegionType.USA ? 1.609F : 1F) / 10D) * 10) switch
		{
			10 => GetProp(Prop.SpeedSign10),
			20 => GetProp(Prop.SpeedSign20),
			30 => GetProp(Prop.SpeedSign30),
			40 => GetProp(Prop.SpeedSign40),
			50 => GetProp(Prop.SpeedSign50),
			60 => GetProp(Prop.SpeedSign60),
			70 => GetProp(Prop.SpeedSign70),
			80 => GetProp(Prop.SpeedSign80),
			90 => GetProp(Prop.SpeedSign90),
			100 => GetProp(Prop.SpeedSign100),
			110 => GetProp(Prop.SpeedSign110),
			120 => GetProp(Prop.SpeedSign120),
			130 => GetProp(Prop.SpeedSign130),
			140 => GetProp(Prop.SpeedSign140),
			_ => new PropTemplate(string.Empty)
		};

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
			m_position = new Vector3((-Lane.LaneWidth / 2F) + 0.4F, 0, -1F),
		}.Extend(prop => new NetInfoExtionsion.LaneProp(prop)
		{
			SegmentFlags = new NetInfoExtionsion.SegmentInfoFlags
			{
				Forbidden = ModOptions.HideRoadClutter ? NetSegmentExt.Flags.None : RoadUtils.Flags.S_RemoveRoadClutter,
				Required = ModOptions.HideRoadClutter ? RoadUtils.Flags.S_RemoveRoadClutter : NetSegmentExt.Flags.None
			},
			StartNodeFlags = new NetInfoExtionsion.NodeInfoFlags
			{ Required = NetNodeExt.Flags.SpeedChange },
		});
	}

	private NetLaneProps.Prop RailwayCrossing(float drivableArea, float propPosition)
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
			m_position = new Vector3(propPosition, 0, Road.ContainsWiredLanes ? -2F : -1F),
		};

		PropInfo getCrossingProp(float drivableArea)
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
