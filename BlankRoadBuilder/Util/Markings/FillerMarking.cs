using BlankRoadBuilder.ThumbnailMaker;

using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace BlankRoadBuilder.Util.Markings;

public class FillerMarking
{
	public MarkingPoint LeftPoint { get; set; }
	public MarkingPoint RightPoint { get; set; }
	public LaneDecoration Type { get; set; }
	public float Elevation { get; set; }
	public bool Helper { get; set; }
	public bool TransitionForward { get; set; }
	public bool TransitionBackward { get; set; }
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

			while (lane != null)
			{
				yield return lane;

				if (lane == RightPoint.LeftLane)
				{
					yield break;
				}

				lane = lane.RightLane;
			}
		}
	}
	public float SurfaceElevation => Lanes.Min(x => x.SurfaceElevation);

	private MarkingStyleUtil.FillerInfo? GetHelperInfo()
	{
		return new MarkingStyleUtil.FillerInfo
		{
			Color = new UnityEngine.Color32(0, 0, 0, 0),
			MarkingStyle = Domain.MarkingFillerType.Filled
		};
	}

	public MarkingStyleUtil.FillerInfo? AN_Info => Helper ? GetHelperInfo() : MarkingStyleUtil.GetFillerMarkingInfo(GetFillerLaneType(), MarkingType.AN);
	public MarkingStyleUtil.FillerInfo? IMT_Info => MarkingStyleUtil.GetFillerMarkingInfo(GetFillerLaneType(), MarkingType.IMT);

	private LaneType GetFillerLaneType()
	{
		return Lanes
			.First()
			.Type
			.GetValues()
			.OrderBy(LaneTypeImportance)
			.FirstOrDefault();
	}

	private static int LaneTypeImportance(LaneType type)
	{
		return type switch
		{
			LaneType.Train => 21,
			LaneType.Tram => 20,
			LaneType.Emergency => 19,
			LaneType.Trolley => 18,
			LaneType.Bus => 17,
			LaneType.Car => 16,
			LaneType.Bike => 15,
			LaneType.Parking => 14,
			LaneType.Pedestrian => 13,
			_ => 0,
		};
	}
}