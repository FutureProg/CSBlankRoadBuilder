using BlankRoadBuilder.ThumbnailMaker;

using System.Collections.Generic;

namespace BlankRoadBuilder.Util.Markings;

public class FillerMarking
{
	public MarkingPoint LeftPoint { get; set; }
	public MarkingPoint RightPoint { get; set; }
    public LaneDecoration Type { get; set; }
	public IEnumerable<LaneInfo> Lanes 
	{
		get
		{
			var lane = LeftPoint.RightLane;

			while (lane != null)
			{
				yield return lane;

				if (lane == RightPoint.LeftLane)
					yield break;
			}
		}
	}
}