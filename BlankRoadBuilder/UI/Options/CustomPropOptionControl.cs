using AlgernonCommons.UI;

using BlankRoadBuilder.Util;
using BlankRoadBuilder.Util.Props.Templates;

using ColossalFramework.UI;

using ModsCommon.UI;

using UnityEngine;

namespace BlankRoadBuilder.UI.Options;
internal class CustomPropOptionControl : UISprite
{
	private const float Margin = 5F;

	protected SelectPropProperty? dropDown;
	private readonly UISprite? colorPreview;
	protected ByteUITextField? rTB;
	protected ByteUITextField? gTB;
	protected ByteUITextField? bTB;
	protected ByteUITextField? aTB;
	protected FloatUITextField? lineWidthTB;
	protected FloatUITextField? dashWidthTB;
	protected FloatUITextField? dashSpaceTB;
	private UILabel? titleLabel;
	private readonly UILabel? descLabel;

	public Prop Prop { get; private set; }
	public PropTemplate? Value { get; private set; }

	internal void Init(Prop prop, PropTemplate value)
	{
		Prop = prop;
		size = new Vector2(345, 155);
		atlas = ResourceUtil.GetAtlas("MarkingOptionBack.png");
		spriteName = "normal";

		titleLabel = AddUIComponent<UILabel>();
		titleLabel.text = prop.ToString().FormatWords();
		titleLabel.textScale = 1;
		titleLabel.textColor = new Color32(92, 182, 239, 255);
		titleLabel.autoSize = true;
		titleLabel.font = UIFonts.SemiBold;
		titleLabel.relativePosition = new Vector2(2 * Margin, 2 * Margin);

		var undoButton = AddUIComponent<SlickButton>();
		undoButton.size = new Vector2(30, 30);
		undoButton.SetIcon("I_Undo.png");
		undoButton.text = " ";
		undoButton.tooltip = "Reset this marking option";
		undoButton.relativePosition = new Vector2(width - 30 - Margin, Margin);
		undoButton.eventClick += UndoButton_eventClick;

		dropDown = AddUIComponent<SelectPropProperty>();
		dropDown.size = new Vector2(200, 50);
		dropDown.relativePosition = new Vector2(Margin * 2, titleLabel.height + (3 * Margin) + 2);

		UpdateData(value);

		dropDown.OnValueChanged += DropDown_OnValueChanged;
	}

	private void DropDown_OnValueChanged(PropInfo obj)
	{
		Value!.PropName = obj.name;

		PropUtil.SaveTemplate(Prop, Value);
	}

	private void UndoButton_eventClick(UIComponent component, UIMouseEventParameter eventParam)
	{
		PropUtil.SaveTemplate(Prop, null);

		UpdateData(PropUtil.GetProp(Prop));
	}

	public void UpdateData(PropTemplate value)
	{
		Value = value;

		dropDown!.Prefab = Value!;
	}

	protected virtual void SetVisibilityOfControls()
	{
		var y = aTB!.relativePosition.y;

		if (dashSpaceTB != null && dashSpaceTB.isVisible)
		{
			dashSpaceTB.relativePosition = new Vector2(dashSpaceTB.relativePosition.x, y);

			y -= dashSpaceTB.height + Margin;
		}

		if (dashWidthTB != null && dashWidthTB.isVisible)
		{
			dashWidthTB.relativePosition = new Vector2(dashWidthTB.relativePosition.x, y);

			y -= dashWidthTB.height + Margin;
		}

		if (lineWidthTB != null && lineWidthTB.isVisible)
		{
			lineWidthTB.relativePosition = new Vector2(lineWidthTB.relativePosition.x, y);

			y -= lineWidthTB.height + Margin;
		}

		if (descLabel != null)
		{
			descLabel.size = new Vector2(width - descLabel.relativePosition.x - Margin, y - descLabel.relativePosition.y - Margin + 30);
		}
	}

	protected void SetColorVisibility(bool visible)
	{
		if (rTB == null || gTB == null || bTB == null || aTB == null || colorPreview == null)
		{
			return;
		}

		rTB.isVisible = visible;
		gTB.isVisible = visible;
		bTB.isVisible = visible;
		aTB.isVisible = visible;
		colorPreview.isVisible = visible;
	}
}