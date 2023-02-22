using AdaptiveRoads.Manager;

using BlankRoadBuilder.Domain.Options;
using BlankRoadBuilder.ThumbnailMaker;
using BlankRoadBuilder.Util.Props.Templates;

using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace BlankRoadBuilder.Util.Props;

public partial class LanePropsUtil
{
	private IEnumerable<NetLaneProps.Prop> GetLaneDecalProps()
	{
		if (!ModOptions.AddLaneDecals)
		{
			yield break;
		}

		var validProps = Lane.Type.GetValues()
			.OrderByDescending(ThumbnailMakerUtil.LaneTypeImportance)
			.Select(x => x switch
			{
				LaneType.Tram => Prop.TramLaneDecal,
				LaneType.Trolley => Prop.TrolleyLaneDecal,
				LaneType.Bus => Prop.BusLaneDecal,
				LaneType.Bike => Lane.LaneWidth < 2F ? Prop.BicycleLaneDecalSmall : Prop.BicycleLaneDecal,
				_ => Prop.None
			}).Select(GetProp);

		var index = 0;

		foreach (var prop in validProps)
		{
			if (prop is LaneDecalProp laneDecal && laneDecal.HideOnSharedLanes && Lane.Type.GetValues().Count() > 1)
			{
				continue;
			}

			if ((PropInfo?)prop == null)
			{
				continue;
			}

			yield return new NetLaneProps.Prop()
			{
				m_prop = prop,
				m_tree = prop,
				m_startFlagsRequired = prop is LaneDecalProp laneDecalProp && !laneDecalProp.OnlyShowAtIntersections ? NetNode.Flags.None : NetNode.Flags.Junction,
				m_angle = prop.Angle,
				m_minLength = 10,
				m_segmentOffset = prop.SegmentOffset,
				m_repeatDistance = prop.RepeatInterval,
				m_probability = 100,
				m_position = prop.Position + new Vector3(0, 0, index),
			};

			index += 6;
		}
	}

	private IEnumerable<NetLaneProps.Prop> GetLaneArrowProps()
	{
		if (!ModOptions.AddLaneArrows || !Lane.Type.HasAnyFlag(LaneType.Car, LaneType.Bus, LaneType.Trolley, LaneType.Emergency))
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

		NetLaneProps.Prop arrow(PropTemplate propInfo, NetLane.Flags flagsRequired, NetLane.Flags flagsForbidden) => new NetLaneProps.Prop
		{
			m_prop = propInfo,
			m_tree = propInfo,
			m_flagsRequired = flagsRequired,
			m_flagsForbidden = flagsForbidden,
			m_endFlagsRequired = NetNode.Flags.Junction,
			m_endFlagsForbidden = NetNode.Flags.LevelCrossing | NetNode.Flags.Bend,
			m_angle = propInfo.Angle,
			m_minLength = 10,
			m_segmentOffset = 1F,
			m_probability = 100,
			m_position = propInfo.Position,
		}.Extend(prop => new NetInfoExtionsion.LaneProp(prop)
		{
			EndNodeFlags = new NetInfoExtionsion.NodeInfoFlags
			{ Forbidden = flagsRequired == NetLane.Flags.Forward ? (NetNodeExt.Flags.TwoSegments | RoadUtils.Flags.N_RemoveLaneArrows) : RoadUtils.Flags.N_RemoveLaneArrows }
		});
	}
}
