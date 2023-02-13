using AdaptiveRoads.CustomScript;
using AdaptiveRoads.Manager;

namespace HighCurbFlag
{
	public class HighCurb : PredicateBase
	{
		public override bool Condition()
		{
			if (Node.Has(NetNodeExt.Flags.Custom1))
				return true;

			if (Node.Has(NetNodeExt.Flags.Custom0))
				return false;

			if (Node.Has(NetNode.Flags.End))
				return true;

			if (SegmentEnd.Has(NetSegmentEnd.Flags.ZebraCrossing))
				return false;

			return true;
		}
	}
}