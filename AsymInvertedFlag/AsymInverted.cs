using AdaptiveRoads.CustomScript;

using KianCommons;

using System.Linq;

namespace AsymInvertedFlag
{
	public class AsymInverted : PredicateBase
	{
		public override bool Condition()
		{
			if (Node.Has(NetNode.Flags.Bend))
			{
				var segments = new NodeSegmentIterator(NodeID).ToArray();
				var SegmentA = segments[0].ToSegment();
				var SegmentD = segments[1].ToSegment();

				return SegmentA.GetHeadNode() == NodeID && SegmentD.GetHeadNode() == NodeID;
			}

			return false;
		}
	}
}