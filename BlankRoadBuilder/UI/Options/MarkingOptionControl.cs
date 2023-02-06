using BlankRoadBuilder.Domain;

using ColossalFramework.UI;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ModsCommon.UI;

namespace BlankRoadBuilder.UI.Options;
internal abstract class MarkingOptionControl<DropDown, EnumType> : UIComponent where DropDown : EnumDropDown<EnumType> where EnumType : Enum
{
	private const float Margin = 5F;

	protected DropDown? dropDown;
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
		titleLabel = AddUIComponent<UILabel>();
		titleLabel.text = title;
		titleLabel.textScale = 1;
		titleLabel.textColor = new Color32(48, 133, 209, 255);
		titleLabel.autoSize = true;
		titleLabel.relativePosition = new Vector2(Margin, Margin);

		descLabel = AddUIComponent<UILabel>();
		descLabel.text = description;
		descLabel.textScale = 0.65F;
		descLabel.textColor = new Color32(200, 200, 200, 255);
		descLabel.autoSize = false;
		descLabel.autoHeight = true;
		descLabel.relativePosition = new Vector2(titleLabel.width + 2 * Margin, Margin);
		descLabel.width = width - descLabel.relativePosition.x - Margin;

		dropDown = AddUIComponent<DropDown>();
		dropDown.relativePosition = new Vector2(Margin, titleLabel.height + 2 * Margin);
		dropDown.SelectedObject = value;

		rTB = AddUIComponent<ByteUITextField>();
		gTB = AddUIComponent<ByteUITextField>();
		bTB = AddUIComponent<ByteUITextField>();
		aTB = AddUIComponent<ByteUITextField>();

		rTB.size = gTB.size = bTB.size = aTB.size = new Vector2(60, 20);
		rTB.textScale = gTB.textScale = bTB.textScale = aTB.textScale = 0.7F;

		rTB.relativePosition = new Vector2(Margin + 25, dropDown.relativePosition.y + dropDown.height + Margin);
		gTB.relativePosition = new Vector2(Margin + 25, rTB.relativePosition.y + rTB.height + Margin);
		bTB.relativePosition = new Vector2(Margin + 25, gTB.relativePosition.y + gTB.height + Margin);
		aTB.relativePosition = new Vector2(Margin + 25, bTB.relativePosition.y + bTB.height + Margin);

		rTB.AddLabel("R", SpriteAlignment.LeftCenter);
		gTB.AddLabel("G", SpriteAlignment.LeftCenter);
		bTB.AddLabel("B", SpriteAlignment.LeftCenter);
		aTB.AddLabel("A", SpriteAlignment.LeftCenter);

		rTB.Value = color.r;
		gTB.Value = color.g;
		bTB.Value = color.b;
		aTB.Value = color.a;

		lineWidthTB = AddUIComponent<FloatUITextField>();
		lineWidthTB.Value = lineWidth;
		lineWidthTB.relativePosition = new Vector2(width - 125 - Margin, 0);
		lineWidthTB.textScale = 0.8F;
		lineWidthTB.size = new Vector2(125, 20);

		dashWidthTB = AddUIComponent<FloatUITextField>();
		dashWidthTB.Value = dashWidth;
		dashWidthTB.relativePosition = new Vector2(width - 125 - Margin, 0);
		dashWidthTB.textScale = 0.8F;
		dashWidthTB.size = new Vector2(125, 20);

		dashSpaceTB = AddUIComponent<FloatUITextField>();
		dashSpaceTB.Value = dashSpace;
		dashSpaceTB.relativePosition = new Vector2(width - 125 - Margin, 0);
		dashSpaceTB.textScale = 0.8F;
		dashSpaceTB.size = new Vector2(125, 20);

		rTB.OnValueChanged += SetColorR;
		gTB.OnValueChanged += SetColorG;
		bTB.OnValueChanged += SetColorB;
		aTB.OnValueChanged += SetColorA;
		dropDown.OnSelectObjectChanged += SetMarkingStyle;
		lineWidthTB.OnValueChanged += SetLineWidth;
		dashWidthTB.OnValueChanged += SetDashWidth;
		dashSpaceTB.OnValueChanged += SetDashSpace;

		SetVisibilityOfControls();
	}

	protected virtual void SetVisibilityOfControls()
	{
		var y = titleLabel?.height ?? 0 + 2 * Margin;

		if (lineWidthTB != null && lineWidthTB.isVisible)
		{
			lineWidthTB.relativePosition = new Vector2(lineWidthTB.relativePosition.x, y);

			y += lineWidthTB.height + Margin;
		}

		if (dashWidthTB != null && dashWidthTB.isVisible)
		{
			dashWidthTB.relativePosition = new Vector2(dashWidthTB.relativePosition.x, y);

			y += dashWidthTB.height + Margin;
		}

		if (dashSpaceTB != null && dashSpaceTB.isVisible)
		{
			dashSpaceTB.relativePosition = new Vector2(dashSpaceTB.relativePosition.x, y);
		}
	}

	protected void SetColorVisibility(bool visible)
	{
		if (rTB == null || gTB == null || bTB == null || aTB == null)
		{
			return;
		}

		rTB.isVisible = visible;
		gTB.isVisible = visible;
		bTB.isVisible = visible;
		aTB.isVisible = visible;
	}

	protected abstract void SetMarkingStyle(EnumType val);
	protected abstract void SetColorR(byte val);
	protected abstract void SetColorG(byte val);
	protected abstract void SetColorB(byte val);
	protected abstract void SetColorA(byte val);
	protected abstract void SetLineWidth(float val);
	protected abstract void SetDashWidth(float val);
	protected abstract void SetDashSpace(float val);
}

internal class LineDropDown : EnumDropDown<MarkingLineType> { }
internal class FillerDropDown : EnumDropDown<MarkingFillerType> { }