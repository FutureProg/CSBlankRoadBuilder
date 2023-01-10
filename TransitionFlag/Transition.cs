using AdaptiveRoads.CustomScript;
using AdaptiveRoads.Manager;

using KianCommons;

using System.Globalization;

namespace MarkingTransitionFlag
{
	public class Transition : PredicateBase
	{
		public override bool Condition()
		{
			//Segment.NetInfoExt.UserDataNamesSet.Node[].GetMetaData()

			//Node.se.sele.CheckFlags()
			//Segment.Has(NetSegment.Flags.Invert) == SegmentEnd.Has(NetSegmentEnd.Flags.IsStartNode);

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