using BlankRoadBuilder.Domain;
using BlankRoadBuilder.Domain.Options;
using BlankRoadBuilder.ThumbnailMaker;
using BlankRoadBuilder.Util.Markings;

namespace BlankRoadBuilder.UI.Options;

internal class FillerDropDown : EnumDropDown<MarkingFillerType> { }
internal class FillerMarkingOptionControl : CustomMarkingOptionControl<FillerDropDown, MarkingFillerType>
{
	public MarkingType MarkingType { get; private set; }

	private SavedFillerOption Value = new(default, Util.Markings.MarkingType.IMT);

	public void Init(MarkingType markingType, LaneType laneType, SavedFillerOption value)
	{
		MarkingType = markingType;
		Value = value;

		Init($"{laneType} Lane", "Determines the design of the added filler in Thumbnail Maker", value.Color, value.MarkingType, 0F, value.DashLength, value.DashSpace);

		SetVisibilityOfControls();
	}

	protected override void SetVisibilityOfControls()
	{
		if (lineWidthTB != null)
			lineWidthTB.isVisible = false;

		if (dashWidthTB != null)
			dashWidthTB.isVisible = Value.MarkingType is not MarkingFillerType.Filled;

		if (dashSpaceTB != null)
			dashSpaceTB.isVisible = Value.MarkingType is not MarkingFillerType.Filled;

		base.SetVisibilityOfControls();
	}

	protected override void SetMarkingStyle(MarkingFillerType val)
	{
		Value.MarkingType = val;

		SetVisibilityOfControls();
	}

	public override void ResetMarking()
	{
		var vanilla = MarkingStyleUtil._fillers(ModOptions.MarkingsStyle == MarkingStyle.Custom ? MarkingStyle.Vanilla : ModOptions.MarkingsStyle, MarkingType);

		if (vanilla != null && vanilla.ContainsKey(Value.LaneType))
			Value.Set(vanilla[Value.LaneType]);
		else
			Value.Set(new MarkingStyleUtil.FillerInfo());

		dropDown!.SelectedObject = Value.MarkingType;
		dashWidthTB!.Value = Value.DashLength;
		dashSpaceTB!.Value = Value.DashSpace;
		rTB!.Value = (byte)Value.R;
		gTB!.Value = (byte)Value.G;
		bTB!.Value = (byte)Value.B;
		aTB!.Value = (byte)Value.A;
	}

	protected override void SetColorR(byte val)
	{
		Value.R = val;
	}

	protected override void SetColorG(byte val)
	{
		Value.G = val;
	}

	protected override void SetColorB(byte val)
	{
		Value.B = val;
	}

	protected override void SetColorA(byte val)
	{
		Value.A = val;
	}

	protected override void SetLineWidth(float val) { }
	protected override void SetDashWidth(float val)
	{
		Value.DashLength = val;
	}

	protected override void SetDashSpace(float val)
	{
		Value.DashSpace = val;
	}
}
