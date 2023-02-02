using BlankRoadBuilder.ThumbnailMaker;

namespace BlankRoadBuilder.Util.Markings;

public class CrosswalkMarking
{
	public RoadInfo Road { get; set; }
	public bool Inverted { get; }

	public CrosswalkMarking(RoadInfo road, bool inverted)
	{
		Road = road;
		Inverted = inverted;
	}
}