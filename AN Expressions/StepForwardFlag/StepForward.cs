using AdaptiveRoads.CustomScript;
using AdaptiveRoads.Manager;

using KianCommons;

using System.Linq;

namespace StepForwardFlag
{
	public class StepForward : PredicateBase
	{
		public override bool Condition()
		{
			var node = (Segment.Has(NetSegment.Flags.Invert) == !Segment.Has(NetSegmentExt.Flags.LeftHandTraffic) ? Segment.VanillaSegment.GetTailNode() : Segment.VanillaSegment.GetHeadNode()).ToNodeExt();

			if (!SamePrefab(node))
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

		private bool SamePrefab(NetNodeExt node)
		{
			var InfoA = node.SegmentIDs.First().ToSegment().Info;
			var InfoD = node.SegmentIDs.Last().ToSegment().Info;

			return InfoA == InfoD // Same road
				|| (InfoA.m_netAI as RoadAI)?.m_elevatedInfo == InfoD
				|| (InfoD.m_netAI as RoadAI)?.m_elevatedInfo == InfoA
				|| (InfoA.m_netAI as RoadAI)?.m_slopeInfo == InfoD
				|| (InfoD.m_netAI as RoadAI)?.m_slopeInfo == InfoA;
		}
	}
}