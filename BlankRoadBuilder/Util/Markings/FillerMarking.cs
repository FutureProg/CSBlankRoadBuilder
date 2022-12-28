using BlankRoadBuilder.ThumbnailMaker;

using System.Collections.Generic;
using System.Linq;

namespace BlankRoadBuilder.Util.Markings;

public class FillerMarking
{
	public MarkingPoint LeftPoint { get; set; }
	public MarkingPoint RightPoint { get; set; }
    public LaneDecoration Type { get; set; }
	public float Elevation { get; set; }
	public IEnumerable<LaneInfo> Lanes 
	{
		get
		{
			var lane = LeftPoint.RightLane;

			while (lane != null)
			{
				yield return lane;

				lane = lane.RightLane;

				if (lane == RightPoint.LeftLane)
					yield break;
			}
		}
	}

	public MarkingStyleUtil.FillerInfo? AN_Info => MarkingStyleUtil.GetFillerMarkingInfo(Lanes.First().Type, MarkingType.AN);
	public MarkingStyleUtil.FillerInfo? IMT_Info => MarkingStyleUtil.GetFillerMarkingInfo(Lanes.First().Type, MarkingType.IMT);
}