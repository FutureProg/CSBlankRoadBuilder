using AdaptiveRoads.CustomScript;
using AdaptiveRoads.Manager;

namespace IsTailNodeFlag
{
	public class IsTailNode : PredicateBase
	{
		public override bool Condition()
		{
			return Segment.Has(NetSegmentExt.Flags.LeftHandTraffic) != SegmentEnd.Has(NetSegmentEnd.Flags.IsTailNode);
		}
	}
}