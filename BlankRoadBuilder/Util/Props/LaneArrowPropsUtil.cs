using AdaptiveRoads.Manager;

using BlankRoadBuilder.Domain.Options;
using BlankRoadBuilder.ThumbnailMaker;

using System.Collections.Generic;

using UnityEngine;

namespace BlankRoadBuilder.Util.Props;

public partial class LanePropsUtil
{
	private IEnumerable<NetLaneProps.Prop> GetBikeLaneProps()
	{
		if (!ModOptions.AddLaneDecals)
		{
			yield break;
		}

		var bikeLane = GetProp(Lane.LaneWidth < 2F ? Prop.BicycleLaneDecalSmall : Prop.BicycleLaneDecal);
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

		yield return prop;
	}

	private IEnumerable<NetLaneProps.Prop> GetBusLaneProps()
	{
		if (!Lane.Type.HasFlag(LaneType.Car))
		{
			foreach (var prop in GetLaneArrowProps())
			{
				yield return prop;
			}
		}

		if (!ModOptions.AddLaneDecals)
		{
			yield break;
		}

		var busLane = GetProp(Prop.BusLaneDecal);

		yield return new NetLaneProps.Prop()
		{
			m_prop = busLane,
			m_finalProp = busLane,
			m_startFlagsRequired = NetNode.Flags.Junction,
			m_angle = 180,
			m_minLength = 10,
			m_segmentOffset = -1,
			m_probability = 100,
			m_position = new Vector3(0, 0, (Lane.Type & LaneType.Bike) == LaneType.Bike ? 15 : 5),
		};
	}

	private IEnumerable<NetLaneProps.Prop> GetLaneArrowProps()
	{
		if (!ModOptions.AddLaneArrows)
		{
			yield break;
		}

		var arrowF = GetProp(Prop.ArrowForward);
		var arrowFR = GetProp(Prop.ArrowForwardRight);
		var arrowL = GetProp(Prop.ArrowLeft);
		var arrowLF = GetProp(Prop.ArrowForwardLeft);
		var arrowLFR = GetProp(Prop.ArrowForwardLeftRight);
		var arrowLR = GetProp(Prop.ArrowLeftRight);
		var arrowR = GetProp(Prop.ArrowRight);

		yield return arrow(arrowF, NetLane.Flags.Forward, NetLane.Flags.LeftRight);

		yield return arrow(arrowFR, NetLane.Flags.ForwardRight, NetLane.Flags.Left);

		yield return arrow(arrowL, NetLane.Flags.Left, NetLane.Flags.ForwardRight);

		yield return arrow(arrowLF, NetLane.Flags.LeftForward, NetLane.Flags.Right);

		yield return arrow(arrowLFR, NetLane.Flags.LeftForwardRight, NetLane.Flags.None);

		yield return arrow(arrowLR, NetLane.Flags.LeftRight, NetLane.Flags.Forward);

		yield return arrow(arrowR, NetLane.Flags.Right, NetLane.Flags.LeftForward);

		NetLaneProps.Prop arrow(PropInfo? propInfo, NetLane.Flags flagsRequired, NetLane.Flags flagsForbidden) => new NetLaneProps.Prop
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
			{ Forbidden = flagsRequired == NetLane.Flags.Forward ? (NetNodeExt.Flags.TwoSegments | RoadUtils.Flags.N_RemoveLaneArrows) : RoadUtils.Flags.N_RemoveLaneArrows }
		});
	}
}
