using AdaptiveRoads.CustomScript;
using AdaptiveRoads.Manager;

using KianCommons;

using System.Globalization;
using System.Linq;

namespace TransitionFlag
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