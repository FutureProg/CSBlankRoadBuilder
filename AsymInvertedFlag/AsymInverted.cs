using AdaptiveRoads.CustomScript;
using AdaptiveRoads.Manager;

using System.Linq;
using KianCommons;

using System.Globalization;

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

				UnityEngine.Debug.LogError($"ASI: {SegmentA.GetID()} {SegmentD.GetID()} {SegmentA.GetTailNode()} {SegmentA.GetTailNode()}");
				return SegmentA.GetTailNode() == SegmentD.GetTailNode();
			}

			return false;
		}
	}
}