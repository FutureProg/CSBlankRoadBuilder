using BlankRoadBuilder.ThumbnailMaker;

using System;
using System.Collections.Generic;
using System.Linq;

namespace BlankRoadBuilder.Util.Markings;

public class FillerMarking
{
	public MarkingPoint LeftPoint { get; set; }
	public MarkingPoint RightPoint { get; set; }
    public LaneDecoration Type { get; set; }
	public float Elevation { get; set; }
	public bool Helper { get; set; }
	public bool Transition { get; set; }
	public IEnumerable<LaneInfo> Lanes
	{
		get
		{
			var lane = LeftPoint.RightLane;

			if (lane != null && lane == RightPoint.LeftLane)
			{
				yield return lane;
				yield break;
			}

			while (lane != null && lane != RightPoint.LeftLane)
			{
				yield return lane;

				lane = lane.RightLane;
			}
		}
	}

	private MarkingStyleUtil.FillerInfo? GetHelperInfo()
	{
		return new MarkingStyleUtil.FillerInfo
		{
			Color = new UnityEngine.Color32(0, 0, 0, 0),
			MarkingStyle = Domain.MarkingFillerType.Filled
		};
	}

	public MarkingStyleUtil.FillerInfo? AN_Info => Helper ? GetHelperInfo() : MarkingStyleUtil.GetFillerMarkingInfo(Lanes.First().Type, MarkingType.AN);
	public MarkingStyleUtil.FillerInfo? IMT_Info => MarkingStyleUtil.GetFillerMarkingInfo(Lanes.First().Type, MarkingType.IMT);
}