using BlankRoadBuilder.Domain;
using BlankRoadBuilder.Domain.Options;
using BlankRoadBuilder.Util.Markings;

namespace BlankRoadBuilder.UI.Options;

internal class LineDropDown : EnumDropDown<MarkingLineType> { }
internal class LineMarkingOptionControl : CustomMarkingOptionControl<LineDropDown, MarkingLineType>
{
	public MarkingType MarkingType { get; private set; }

	private SavedLineOption Value = new((GenericMarkingType)(-1), Util.Markings.MarkingType.IMT);

	public void Init(MarkingType markingType, GenericMarkingType genericMarkingType, SavedLineOption value)
	{
		MarkingType = markingType;
		Value = value;

		Init(GetTitle(genericMarkingType), GetDescription(genericMarkingType), value.Color, value.MarkingType, value.LineWidth, value.DashLength, value.DashSpace);
	}

	protected override void SetVisibilityOfControls()
	{
		SetColorVisibility(Value.MarkingType != MarkingLineType.None);

		if (lineWidthTB != null)
			lineWidthTB.isVisible = Value.MarkingType != MarkingLineType.None;

		if (dashWidthTB != null)
			dashWidthTB.isVisible = Value.MarkingType is MarkingLineType.ZigZag or MarkingLineType.Dashed or MarkingLineType.DashedDouble or MarkingLineType.DashedSolid or MarkingLineType.SolidDashed;

		if (dashSpaceTB != null)
			dashSpaceTB.isVisible = Value.MarkingType is MarkingLineType.Dashed or MarkingLineType.DashedDouble or MarkingLineType.DashedSolid or MarkingLineType.SolidDashed;

		base.SetVisibilityOfControls();
	}

	protected override void SetMarkingStyle(MarkingLineType val)
	{
		Value.MarkingType = val;

		SetVisibilityOfControls();
	}

	public override void ResetMarking()
	{
		var vanilla = MarkingStyleUtil._markings(ModOptions.MarkingsStyle == MarkingStyle.Custom ? MarkingStyle.Vanilla : ModOptions.MarkingsStyle, MarkingType);

		if (vanilla != null && vanilla.ContainsKey(Value.GenericMarking))
			Value.Set(vanilla[Value.GenericMarking]);
		else
			Value.Set(new MarkingStyleUtil.LineInfo());

		dropDown!.SelectedObject = Value.MarkingType;
		lineWidthTB!.Value = Value.LineWidth;
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

	protected override void SetLineWidth(float val)
	{
		Value.LineWidth = val;
	}

	protected override void SetDashWidth(float val)
	{
		Value.DashLength = val;
	}

	protected override void SetDashSpace(float val)
	{
		Value.DashSpace = val;
	}

	private string GetTitle(GenericMarkingType key)
	{
		return key switch
		{
			GenericMarkingType.End => "Border",
			GenericMarkingType.Parking => "Parking",
			GenericMarkingType.Normal | GenericMarkingType.Soft => "Separator",
			GenericMarkingType.Normal | GenericMarkingType.Hard => "Strict Separator",
			GenericMarkingType.Flipped | GenericMarkingType.Hard => "Divider",
			GenericMarkingType.Flipped | GenericMarkingType.End => "Center",
			GenericMarkingType.Normal | GenericMarkingType.Bike => "Bike Separator",
			GenericMarkingType.Flipped | GenericMarkingType.Bike => "Bike Divider",
			GenericMarkingType.Normal | GenericMarkingType.Tram => "Tram Separator",
			GenericMarkingType.Flipped | GenericMarkingType.Tram => "Tram Divider",
			_ => key.ToString(),
		};
	}

	private string GetDescription(GenericMarkingType key)
	{
		return key switch
		{
			GenericMarkingType.End => "Used between two different lane types",
			GenericMarkingType.Parking => "Used between a parking and vehicle lane",
			GenericMarkingType.Normal | GenericMarkingType.Soft => "Used between two vehicle lanes going in the same direction",
			GenericMarkingType.Normal | GenericMarkingType.Hard => "Used between two different vehicle lanes types going in the same direction",
			GenericMarkingType.Flipped | GenericMarkingType.Hard => "Used between two vehicle lanes going in the opposite direction",
			GenericMarkingType.Flipped | GenericMarkingType.End => "Used between a vehicle lane and a filler on its left",
			GenericMarkingType.Normal | GenericMarkingType.Bike => "Used between two bike lanes going in the same direction",
			GenericMarkingType.Flipped | GenericMarkingType.Bike => "Used between two bike lanes going in the opposite direction",
			GenericMarkingType.Normal | GenericMarkingType.Tram => "Used between two tram lanes going in the same direction",
			GenericMarkingType.Flipped | GenericMarkingType.Tram => "Used between two tram lanes going in the opposite direction",
			_ => string.Empty,
		};
	}
}
