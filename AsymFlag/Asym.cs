using AdaptiveRoads.CustomScript;
using AdaptiveRoads.Manager;

using System.Linq;
using KianCommons;

using System.Globalization;

namespace AsymFlag
{
	public class Asym : PredicateBase
	{
		public override bool Condition()
		{
			if (Node.Has(NetNode.Flags.Bend))
			{
				var segments = new NodeSegmentIterator(NodeID).ToArray();
				var SegmentA = segments[0].ToSegment();
				var SegmentD = segments[1].ToSegment();

				return SegmentA.GetTailNode() == SegmentD.GetTailNode();
			}

			return false;
		}
	}
}