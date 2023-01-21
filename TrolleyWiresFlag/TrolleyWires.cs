using AdaptiveRoads.CustomScript;
using AdaptiveRoads.Manager;

using KianCommons;
using System.Linq;

namespace TrolleyWiresFlag
{
	public class TrolleyWires : PredicateBase
	{
		public override bool Condition()
		{
			var indexA = GetIndex(InfoA, LaneInfoA);
			var indexD = GetIndex(InfoD, LaneInfoD);

			if (indexA == -1 || indexD == -1)
				return false;

			return InfoA.m_lanes[indexA].m_direction == InfoD.m_lanes[indexD].m_direction;
		}

		private int GetIndex(NetInfo road, NetInfo.Lane lane)
		{
			var diff = lane.m_vehicleType == VehicleInfo.VehicleType.TrolleybusRightPole ? -2 : -1;

			for (var i = 0; i < road.m_lanes.Length; i++)
			{
				if (road.m_lanes[i] == lane)
					return i + diff;
			}

			return -1;
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