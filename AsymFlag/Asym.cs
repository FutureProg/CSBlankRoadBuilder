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
			//if (Node.Has(NetNode.Flags.Bend))
			//{
			//	var segments = new NodeSegmentIterator(NodeID).ToArray();
			//	foreach (var item in segments)
			//	{
			//		UnityEngine.Debug.LogWarning($"{segments.Length} - {item}");
			//	}
			//	var SegmentA = segments[0].ToSegment();
			//	var SegmentD = segments[1].ToSegment();

			//	UnityEngine.Debug.LogError($"AS: {SegmentA.GetID()} {SegmentD.GetID()} {SegmentA.GetHeadNode()} {SegmentA.GetHeadNode()}");
			//	return SegmentA.GetHeadNode() == SegmentD.GetHeadNode();
			//}

			return false;
		}
	}
}