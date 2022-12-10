using AdaptiveRoads.Manager;

using BlankRoadBuilder.ThumbnailMaker;

using PrefabMetadata.Helpers;

using System.Collections.Generic;

using UnityEngine;

namespace BlankRoadBuilder.Util.Props;

public static partial class LanePropsUtil
{
	private static IEnumerable<NetLaneProps.Prop> GetBikeLaneProps()
	{
		var bikeLane = Prop("Bike Lane");
		var prop = new NetLaneProps.Prop()
		{
			m_prop = bikeLane,
			m_finalProp = bikeLane,
			m_startFlagsRequired = NetNode.Flags.Junction,
			m_angle = 180,
			m_minLength = 10,
			m_segmentOffset = -1,
			m_probability = 100,
			m_position = new Vector3(0, 0, 5),
		};

		return new[] { prop };
	}

	private static IEnumerable<NetLaneProps.Prop> GetBusLaneProps(LaneInfo lane)
	{
		if ((lane.Type & (LaneType.Car | LaneType.Highway)) == 0)
		{
			foreach (var prop in GetLaneArrowProps())
			{
				yield return prop;
			}
		}

		var busLane = Prop("Bus Lane");

		yield return new NetLaneProps.Prop()
		{
			m_prop = busLane,
			m_finalProp = busLane,
			m_startFlagsRequired = NetNode.Flags.Junction,
			m_angle = 180,
			m_minLength = 10,
			m_segmentOffset = -1,
			m_probability = 100,
			m_position = new Vector3(0, 0, (lane.Type & LaneType.Bike) == LaneType.Bike ? 15 : 5),
		};
	}

	private static IEnumerable<NetLaneProps.Prop> GetLaneArrowProps()
	{
		var arrowF = Prop("Road Arrow F");
		var arrowFR = Prop("Road Arrow FR");
		var arrowL = Prop("Road Arrow L");
		var arrowLF = Prop("Road Arrow LF");
		var arrowLFR = Prop("Road Arrow LFR");
		var arrowLR = Prop("Road Arrow LR");
		var arrowR = Prop("Road Arrow R");

		yield return arrow(arrowF, NetLane.Flags.Forward, NetLane.Flags.LeftRight);

		yield return arrow(arrowFR, NetLane.Flags.ForwardRight, NetLane.Flags.Left);

		yield return arrow(arrowL, NetLane.Flags.Left, NetLane.Flags.ForwardRight);

		yield return arrow(arrowLF, NetLane.Flags.LeftForward, NetLane.Flags.Right);

		yield return arrow(arrowLFR, NetLane.Flags.LeftForwardRight, NetLane.Flags.None);

		yield return arrow(arrowLR, NetLane.Flags.LeftRight, NetLane.Flags.Forward);

		yield return arrow(arrowR, NetLane.Flags.Right, NetLane.Flags.LeftForward);

		static NetLaneProps.Prop arrow(PropInfo? propInfo, NetLane.Flags flagsRequired, NetLane.Flags flagsForbidden)
		{
			return new NetLaneProps.Prop
			{
				m_prop = propInfo,
				m_finalProp = propInfo,
				m_flagsRequired = flagsRequired,
				m_flagsForbidden = flagsForbidden,
				m_endFlagsRequired = NetNode.Flags.Junction,
				m_endFlagsForbidden = NetNode.Flags.LevelCrossing | NetNode.Flags.Bend,
				m_angle = 180,
				m_minLength = 10,
				m_segmentOffset = 0.8F,
				m_probability = 100,
				m_position = new Vector3(0, 0, -4),
			}.Extend(prop => new NetInfoExtionsion.LaneProp(prop)
			{
				EndNodeFlags = new NetInfoExtionsion.NodeInfoFlags
				{ Forbidden = flagsRequired == NetLane.Flags.Forward ? (NetNodeExt.Flags.TwoSegments | RoadUtils.N_RemoveLaneArrows) : RoadUtils.N_RemoveLaneArrows }
			});
		}
	}
}
