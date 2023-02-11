using AdaptiveRoads.CustomScript;

namespace AnyStopFlag
{
	public class AnyStop : PredicateBase
	{
		public override bool Condition()
		{
			return Segment.Has(NetSegment.Flags.StopLeft) ||
			Segment.Has(NetSegment.Flags.StopRight) ||
			Segment.Has(NetSegment.Flags.StopLeft2) ||
			Segment.Has(NetSegment.Flags.StopRight2);
		}
	}
}