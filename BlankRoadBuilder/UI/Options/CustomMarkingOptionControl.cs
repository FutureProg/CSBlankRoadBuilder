using BlankRoadBuilder.Util;

using ColossalFramework.UI;

using ModsCommon.UI;

using System;

using UnityEngine;

namespace BlankRoadBuilder.UI.Options;
internal abstract class CustomMarkingOptionControl<DropDown, EnumType> : UISprite where DropDown : EnumDropDown<EnumType> where EnumType : Enum
{
	private const float Margin = 5F;

	protected DropDown? dropDown;
	private UISprite? colorPreview;
	protected ByteUITextField? rTB;
	protected ByteUITextField? gTB;
	protected ByteUITextField? bTB;
	protected ByteUITextField? aTB;
	protected FloatUITextField? lineWidthTB;
	protected FloatUITextField? dashWidthTB;
	protected FloatUITextField? dashSpaceTB;
	private UILabel? titleLabel;
	private UILabel? descLabel;

	protected void Init(string title, string description, Color32 color, EnumType value, float lineWidth, float dashWidth, float dashSpace)
	{
		size = new Vector2(355, 155);
		atlas = ResourceUtil.GetAtlas("MarkingOptionBack.png");
		spriteName = "normal";

		titleLabel = AddUIComponent<UILabel>();
		titleLabel.text = title;
		titleLabel.textScale = 1;
		titleLabel.textColor = new Color32(92, 182, 239, 255);
		titleLabel.autoSize = true;
		titleLabel.font = UIFonts.SemiBold;
		titleLabel.relativePosition = new Vector2(2 * Margin, 2 * Margin);

		descLabel = AddUIComponent<UILabel>();
		descLabel.text = description;
		descLabel.textScale = 0.65F;
		descLabel.textColor = new Color32(210, 210, 210, 255);
		descLabel.wordWrap = true;
		descLabel.autoSize = false;
		descLabel.autoHeight = false;
		descLabel.textAlignment = UIHorizontalAlignment.Right;
		descLabel.relativePosition = new Vector2(width - (3 * Margin) - 140, 27 + (3 * Margin));

		var undoButton = AddUIComponent<SlickButton>();
		undoButton.size = new Vector2(22, 22);
		undoButton.SetIcon("I_Undo.png");
		undoButton.text = " ";
		undoButton.tooltip = "Reset this marking option";
		undoButton.relativePosition = new Vector2(width - 22 - 6, 6);
		undoButton.eventClick += UndoButton_eventClick;

		dropDown = AddUIComponent<DropDown>();
		dropDown.size = new Vector2(140, 22);
		dropDown.relativePosition = new Vector2(Margin * 2, titleLabel.height + (3 * Margin) + 2);
		dropDown.SelectedObject = value;
		dropDown.UseWhiteStyle();

		colorPreview = AddUIComponent<UISprite>();
		colorPreview.size = new Vector2(Margin + (18 * 2), Margin + (18 * 2));
		colorPreview.spriteName = "normal";

		rTB = AddUIComponent<ByteUITextField>();
		gTB = AddUIComponent<ByteUITextField>();
		bTB = AddUIComponent<ByteUITextField>();
		aTB = AddUIComponent<ByteUITextField>();

		rTB.size = gTB.size = bTB.size = aTB.size = new Vector2(50, 18);
		rTB.textScale = gTB.textScale = bTB.textScale = aTB.textScale = 0.7F;

		rTB.relativePosition = new Vector2(Margin + 22, dropDown.relativePosition.y + dropDown.height + Margin);
		gTB.relativePosition = new Vector2(Margin + 22, rTB.relativePosition.y + rTB.height + Margin);
		bTB.relativePosition = new Vector2(Margin + 22, gTB.relativePosition.y + gTB.height + Margin);
		aTB.relativePosition = new Vector2(Margin + 22, bTB.relativePosition.y + bTB.height + Margin);

		colorPreview.relativePosition = new Vector2(Margin + 82, gTB.relativePosition.y);

		rTB.AddLabel("R", SpriteAlignment.LeftCenter);
		gTB.AddLabel("G", SpriteAlignment.LeftCenter);
		bTB.AddLabel("B", SpriteAlignment.LeftCenter);
		aTB.AddLabel("A", SpriteAlignment.LeftCenter);

		rTB.Value = color.r;
		gTB.Value = color.g;
		bTB.Value = color.b;
		aTB.Value = color.a;

		RefreshColor(0);

		lineWidthTB = AddUIComponent<FloatUITextField>();
		lineWidthTB.Value = lineWidth;
		lineWidthTB.relativePosition = new Vector2(width - 125 - Margin, 0);
		lineWidthTB.textScale = 0.8F;
		lineWidthTB.size = new Vector2(100, 18);

		dashWidthTB = AddUIComponent<FloatUITextField>();
		dashWidthTB.Value = dashWidth;
		dashWidthTB.relativePosition = new Vector2(width - 125 - Margin, 0);
		dashWidthTB.textScale = 0.8F;
		dashWidthTB.size = new Vector2(100, 18);

		dashSpaceTB = AddUIComponent<FloatUITextField>();
		dashSpaceTB.Value = dashSpace;
		dashSpaceTB.relativePosition = new Vector2(width - 125 - Margin, 0);
		dashSpaceTB.textScale = 0.8F;
		dashSpaceTB.size = new Vector2(100, 18);

		lineWidthTB.AddLabel("Line Width", SpriteAlignment.LeftCenter);
		dashWidthTB.AddLabel("Dash Width", SpriteAlignment.LeftCenter);
		dashSpaceTB.AddLabel("Dash Space", SpriteAlignment.LeftCenter);
		lineWidthTB.AddLabel("m", SpriteAlignment.RightCenter);
		dashWidthTB.AddLabel("m", SpriteAlignment.RightCenter);
		dashSpaceTB.AddLabel("m", SpriteAlignment.RightCenter);

		rTB.SetDefaultStyle();
		gTB.SetDefaultStyle();
		bTB.SetDefaultStyle();
		aTB.SetDefaultStyle();
		lineWidthTB.SetDefaultStyle();
		dashWidthTB.SetDefaultStyle();
		dashSpaceTB.SetDefaultStyle();

		rTB.OnValueChanged += SetColorR;
		gTB.OnValueChanged += SetColorG;
		bTB.OnValueChanged += SetColorB;
		aTB.OnValueChanged += SetColorA;
		rTB.OnValueChanged += RefreshColor;
		gTB.OnValueChanged += RefreshColor;
		bTB.OnValueChanged += RefreshColor;
		aTB.OnValueChanged += RefreshColor;
		dropDown.OnSelectObjectChanged += SetMarkingStyle;
		lineWidthTB.OnValueChanged += SetLineWidth;
		dashWidthTB.OnValueChanged += SetDashWidth;
		dashSpaceTB.OnValueChanged += SetDashSpace;

		SetVisibilityOfControls();
	}

	private void UndoButton_eventClick(UIComponent component, UIMouseEventParameter eventParam)
	{
		ResetMarking();
		RefreshColor(0);
	}

	private void RefreshColor(byte obj)
	{
		if (colorPreview == null)
			return;

		var color = new Color32(rTB!.Value, gTB!.Value, bTB!.Value, aTB!.Value);
		var texture = ResourceUtil.GetImage("White.png")!.CreateTexture();
		var pixels = texture.GetPixels32();

		for (var i = 0; i < pixels.Length; i++)
		{
			pixels[i] = new Color32(color.r, color.g, color.b, (byte)(pixels[i].a * color.a / 255F));
		}

		texture.SetPixels32(pixels);
		texture.Apply(updateMipmaps: false);

		colorPreview.atlas = ResourceUtil.GetAtlas(texture);
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

	public abstract void ResetMarking();
	protected abstract void SetMarkingStyle(EnumType val);
	protected abstract void SetColorR(byte val);
	protected abstract void SetColorG(byte val);
	protected abstract void SetColorB(byte val);
	protected abstract void SetColorA(byte val);
	protected abstract void SetLineWidth(float val);
	protected abstract void SetDashWidth(float val);
	protected abstract void SetDashSpace(float val);
}
