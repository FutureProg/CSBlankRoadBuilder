using AdaptiveRoads.Manager;

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

		var propPosition = -Math.Max(0, (lane.Width / 2) - 0.5F);

		foreach (var prop in GetSignsAndTrafficLights(lane.RightDrivableArea, lane.LeftInvertedDrivableArea, propPosition, lane, road, GetSideLanes(lane, false), GetSideLanes(lane, true)))
		{
			yield return prop;
		}

		foreach (var prop in GetSignsAndTrafficLights(lane.LeftInvertedDrivableArea, lane.RightDrivableArea, propPosition, lane, road, GetSideLanes(lane, true), GetSideLanes(lane, false), true))
		{
			yield return prop;
		}

		foreach (var prop in GetSignsAndTrafficLights(lane.LeftDrivableArea, lane.RightInvertedDrivableArea, propPosition, lane, road, GetSideLanes(lane, true), GetSideLanes(lane, false), false, true))
		{
			yield return prop.ToggleForwardBackward();
		}

		foreach (var prop in GetSignsAndTrafficLights(lane.RightInvertedDrivableArea, lane.LeftDrivableArea, propPosition, lane, road, GetSideLanes(lane, false), GetSideLanes(lane, true), true, true))
		{
			yield return prop.ToggleForwardBackward();
		}

		foreach (var prop in GetBikeSignProps(lane))
		{
			yield return prop;
		}
	}

	private static IEnumerable<NetLaneProps.Prop> GetSignsAndTrafficLights(float drivableArea, float invertedDrivableArea, float propPosition, LaneInfo lane, RoadInfo road, IEnumerable<LaneInfo> sideLanes, IEnumerable<LaneInfo> invertedSideLanes, bool swapped = false, bool flipped = false)
	{
		if (swapped)
		{
			propPosition *= -1F;
		}

		if (drivableArea > 0F)
		{
			if (!swapped)
			{
				yield return StopSign(propPosition);
			}

			if (drivableArea >= 2F)
			{
				yield return RailwayCrossing(road, drivableArea, propPosition);
			}

			if (drivableArea >= 3)
			{
				if (sideLanes.Any(x => (x.Type & (LaneType.Parking | LaneType.Bike)) == 0))
				{
					foreach (var prop in GetWarningSigns(lane))
					{
						yield return prop;
					}
				}

				if (sideLanes.Any(x => (x.Type & (LaneType.Parking | LaneType.Bike | LaneType.Tram)) == 0))
				{
					yield return SpeedSign(road, lane);
				}
			}

			if (drivableArea >= (swapped ? 9F : 6F))
			{
				yield return TrafficLight(road, "Traffic Light 02" + (swapped ? " Mirror" : ""), propPosition);
			}
			else if (!swapped || !sideLanes.Take(2).Any(x => x.Type.HasAnyFlag(LaneType.Pedestrian, LaneType.Filler, LaneType.Curb)))
			{
				yield return TrafficLight(road, "Traffic Light 01" + (!swapped ? " Mirror" : ""), propPosition);
			}
			else
			{
				goto ped;
			}

			yield break;
		}

	ped:
		if (lane.Width < 1.5F && lane.Type != LaneType.Curb)
		{
			yield break;
		}

		var sidewalk = lane.Type == LaneType.Curb;

		if (sidewalk && ((lane.Position < 0) == (swapped == flipped)))
		{
			yield break;
		}

		if (!sidewalk && invertedSideLanes.Take(lane.Width >= 2.5F ? 1 : 2).Any(x => x.Type.HasAnyFlag(LaneType.Pedestrian, LaneType.Filler, LaneType.Curb)))
		{
			yield break;
		}

		yield return TrafficLight(road, "Traffic Light Pedestrian", propPosition, !swapped);
	}

	private static IEnumerable<LaneInfo> GetSideLanes(LaneInfo? lane, bool left)
	{
		while (lane != null)
		{
			lane = left ? lane.LeftLane : lane.RightLane;

			if (lane != null)
			{
				yield return lane;
			}
		}
	}

	private static IEnumerable<NetLaneProps.Prop> GetBikeSignProps(LaneInfo lane)
	{
		var bikeProp = Prop("1779509676.R2 WF11-1 Bicycle Sign_Data");

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
				m_position = new Vector3((lane.Width / 2F) - 0.3F, 0, 0),
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
				m_position = new Vector3((-lane.Width / 2F) + 0.3F, 0, 0),
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
		var railroadCrossingProp = Prop("1779509676.R2 W10-1 Railroad Crossing Sign_Data");
		var signalAheadProp = Prop("1779509676.R2 W3-3 Signal Ahead Sign_Data");

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
			m_position = new Vector3((-lane.Width / 2F) + 0.4F, 0, 2F),
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
			m_position = new Vector3((-lane.Width / 2F) + 0.4F, 0, 2F),
		}.Extend(prop => new NetInfoExtionsion.LaneProp(prop)
		{
			SegmentFlags = new NetInfoExtionsion.SegmentInfoFlags
			{ Forbidden = RoadUtils.S_RemoveRoadClutter }
		});
	}

	private static NetLaneProps.Prop TrafficLight(RoadInfo road, string propName, float propPosition, bool flipAngle = false)
	{
		var prop = Prop(propName);

		return new NetLaneProps.Prop
		{
			m_prop = prop,
			m_finalProp = prop,
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
		};
	}

	private static NetLaneProps.Prop StopSign(float propPosition)
	{
		var stopSign = Prop("Stop Sign");

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

	private static readonly int[] _speedSigns = new[] { 30, 40, 50, 60, 100 };

	private static NetLaneProps.Prop SpeedSign(RoadInfo road, LaneInfo lane)
	{
		PropInfo? sign = null;

		foreach (var item in _speedSigns)
		{
			if ((int)road.SpeedLimit <= item)
			{
				sign = Prop($"{item} Speed Limit");
				break;
			}
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
			m_position = new Vector3((-lane.Width / 2F) + 0.4F, 0, -1F),
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
				return Prop("Railway Crossing Very Long");
			}

			if (drivableArea >= 8F)
			{
				return Prop("Railway Crossing Long");
			}

			return drivableArea >= 6F ? Prop("Railway Crossing Medium") : Prop("Railway Crossing Short");
		}
	}
}
