using BlankRoadBuilder.Domain.Options;
using BlankRoadBuilder.Domain;
using BlankRoadBuilder.ThumbnailMaker;

namespace BlankRoadBuilder.UI.Options;

internal class FillerDropDown : EnumDropDown<MarkingFillerType> { }
internal class FillerMarkingOptionControl : MarkingOptionControl<FillerDropDown, MarkingFillerType>
{
	private SavedFillerOption Value = new(default, Util.Markings.MarkingType.IMT);

	public void Init(LaneType laneType, SavedFillerOption value)
	{
		Value = value;

		Init($"{laneType} Lane", string.Empty, value.Color, value.MarkingType, 0F, value.DashLength, value.DashSpace);

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

	protected override void SetColorR(byte val) => Value.R = val;
	protected override void SetColorG(byte val) => Value.G = val;
	protected override void SetColorB(byte val) => Value.B = val;
	protected override void SetColorA(byte val) => Value.A = val;
	protected override void SetLineWidth(float val) { }
	protected override void SetDashWidth(float val) => Value.DashLength = val;
	protected override void SetDashSpace(float val) => Value.DashSpace = val;
}
