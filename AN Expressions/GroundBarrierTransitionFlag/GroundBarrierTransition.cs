﻿using AdaptiveRoads.CustomScript;
using AdaptiveRoads.Manager;

using KianCommons;

namespace GroundBarrierTransitionFlag
{
	public class GroundBarrierTransition : PredicateBase
	{
		public override bool Condition()
		{
			var sameDirection = SegmentA.GetHeadNode() == SegmentD.GetTailNode();
			var laneInfoA = LaneInfoA;
			var laneInfoD = LaneInfoD;

			return sameDirection &&
				Node.SegmentIDs.Length == 2 && // twoSegments
				(SegmentA.m_flags & NetSegment.Flags.Invert) == (SegmentD.m_flags & NetSegment.Flags.Invert) &&
				(laneInfoA.m_position >= 0) == (laneInfoD.m_position >= 0) &&
				laneInfoA.m_finalDirection == laneInfoD.m_finalDirection &&
				laneInfoA.m_laneType == laneInfoD.m_laneType &&
				laneInfoA.m_vehicleType == laneInfoD.m_vehicleType &&
				((InfoA.m_netAI as RoadAI)?.m_elevatedInfo == InfoD
					|| (InfoD.m_netAI as RoadAI)?.m_elevatedInfo == InfoA);
		}

		private ref NetLane LaneA => ref LaneTransition.LaneIDSource.ToLane();
		private ref NetLane LaneD => ref LaneTransition.LaneIDTarget.ToLane();
		private ref NetLaneExt LaneExtA => ref LaneTransition.LaneIDSource.ToLaneExt();
		private ref NetLaneExt LaneExtD => ref LaneTransition.LaneIDTarget.ToLaneExt();
		private ref NetSegment SegmentA => ref LaneA.m_segment.ToSegment();
		private ref NetSegment SegmentD => ref LaneD.m_segment.ToSegment();
		private NetInfo InfoA => SegmentA.Info;
		private NetInfo.Lane LaneInfoA => LaneExtA.LaneData.LaneInfo;
		private NetInfo InfoD => SegmentD.Info;
		private NetInfo.Lane LaneInfoD => LaneExtD.LaneData.LaneInfo;
	}
}