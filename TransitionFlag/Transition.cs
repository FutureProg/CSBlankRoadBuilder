using AdaptiveRoads.CustomScript;
using AdaptiveRoads.Manager;

using KianCommons;

namespace MarkingTransitionFlag
{
	public class Transition : PredicateBase
	{
		public override bool Condition()
		{
			foreach (var segmentId in Node.SegmentIDs)
			{
				var segment = segmentId.ToSegment();

				if (segment.Info.m_surfaceLevel != 0)
					return true;
			}

			return false;
		}
	}
}