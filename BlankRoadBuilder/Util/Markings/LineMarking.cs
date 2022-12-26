using BlankRoadBuilder.Domain;

namespace BlankRoadBuilder.Util.Markings;

public class LineMarking
{
	public MarkingPoint Point { get; set; }
	public GenericMarkingType Marking { get; set; }
	public float Elevation { get; set; }

	public MarkingStyleUtil.LineInfo? LineInfo => MarkingStyleUtil.GetLineMarkingInfo(Marking);
}