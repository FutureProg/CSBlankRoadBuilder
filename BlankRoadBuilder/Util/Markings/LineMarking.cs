using BlankRoadBuilder.Domain;

namespace BlankRoadBuilder.Util.Markings;

public class LineMarking
{
	public MarkingPoint Point { get; set; }
	public GenericMarkingType Marking { get; set; }
	public float Elevation { get; set; }

	public MarkingStyleUtil.LineInfo? AN_Info => MarkingStyleUtil.GetLineMarkingInfo(Marking, MarkingType.AN);
	public MarkingStyleUtil.LineInfo? IMT_Info => MarkingStyleUtil.GetLineMarkingInfo(Marking, MarkingType.IMT);
}