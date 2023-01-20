using AdaptiveRoads.CustomScript;
using AdaptiveRoads.Manager;

using System.Linq;
using KianCommons;

using System.Globalization;

namespace StepForwardFlag
{
	public class StepForward : PredicateBase
	{
		public override bool Condition()
		{
			var node = (Segment.Has(NetSegment.Flags.Invert) ? Segment.VanillaSegment.GetTailNode() : Segment.VanillaSegment.GetHeadNode()).ToNodeExt();

			if (!node.Has(NetNodeExt.Flags.SamePrefab))
				return true;

			if (node.Has(NetNode.Flags.Middle))
				return false;

			if (node.Has(NetNode.Flags.Bend))
			{
				var SegmentA = node.SegmentIDs.First().ToSegment();
				var SegmentD = node.SegmentIDs.Last().ToSegment();

				if ((SegmentA.m_flags & NetSegment.Flags.Invert) != (SegmentD.m_flags & NetSegment.Flags.Invert))
					return true;

				if (SegmentA.GetHeadNode() == SegmentD.GetHeadNode() || SegmentA.GetTailNode() == SegmentD.GetTailNode())
					return true;

				return false;

			}

			return true;
		}
	}
}