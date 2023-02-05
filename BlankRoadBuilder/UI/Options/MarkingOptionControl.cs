using BlankRoadBuilder.Domain.Options;
using BlankRoadBuilder.Domain;

using ColossalFramework.UI;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ModsCommon.UI;
using BlankRoadBuilder.ThumbnailMaker;

namespace BlankRoadBuilder.UI.Options;
internal class MarkingOptionControl : UIComponent
{
	private const float Margin = 5F;

	private UIDropDown? dropDown;
	private ByteUITextField? rTB;
	private ByteUITextField? gTB;
	private ByteUITextField? bTB;
	private ByteUITextField? aTB;
	private FloatUITextField? lineWidthTB;
	private FloatUITextField? dashWidthTB;
	private FloatUITextField? dashSpaceTB;

	private void Init<DropDown>(string title, string description, Color32 color) where DropDown : UIDropDown
	{
		var titleLabel = AddUIComponent<UILabel>();
		titleLabel.text = title;
		titleLabel.textScale = 1;
		titleLabel.textColor = new Color32(48, 133, 209, 255);
		titleLabel.autoSize = true;
		titleLabel.relativePosition = new Vector2(Margin, Margin);

		var descLabel = AddUIComponent<UILabel>();
		descLabel.text = description;
		descLabel.textScale = 0.65F;
		descLabel.textColor = new Color32(200, 200, 200, 255);
		descLabel.autoSize = false;
		descLabel.autoHeight = true;
		descLabel.relativePosition = new Vector2(titleLabel.width + 2 * Margin, Margin);
		descLabel.width = width - descLabel.relativePosition.x - Margin;

		dropDown = AddUIComponent<DropDown>();
		dropDown.relativePosition = new Vector2(Margin, titleLabel.height + 2 * Margin);

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
		lineWidthTB.relativePosition = new Vector2(width - 125 - Margin, 0);
		lineWidthTB.textScale = 0.8F;
		lineWidthTB.size = new Vector2(125, 20);

		dashWidthTB = AddUIComponent<FloatUITextField>();
		dashWidthTB.relativePosition = new Vector2(width - 125 - Margin, 0);
		dashWidthTB.textScale = 0.8F;
		dashWidthTB.size = new Vector2(125, 20);

		dashSpaceTB = AddUIComponent<FloatUITextField>();
		dashSpaceTB.relativePosition = new Vector2(width - 125 - Margin, 0);
		dashSpaceTB.textScale = 0.8F;
		dashSpaceTB.size = new Vector2(125, 20);
	}

	public void Init(GenericMarkingType markingType, SavedLineOption value)
	{
		Init<LineDropDown>(GetTitle(markingType), GetDescription(markingType), value.Color);
	}

	public void Init(LaneType laneType, SavedFillerOption value)
	{
		Init<LineDropDown>($"{laneType} Lane", string.Empty, value.Color);
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
			GenericMarkingType.End => "used between two different lane types",
			GenericMarkingType.Parking => "used between a parking and vehicle lane",
			GenericMarkingType.Normal | GenericMarkingType.Soft => "used between two vehicle lanes going in the same direction",
			GenericMarkingType.Normal | GenericMarkingType.Hard => "used between two different vehicle lanes types going in the same direction",
			GenericMarkingType.Flipped | GenericMarkingType.Hard => "used between two vehicle lanes going in the opposite direction",
			GenericMarkingType.Flipped | GenericMarkingType.End => "used between a vehicle lane and a filler on its left",
			GenericMarkingType.Normal | GenericMarkingType.Bike => "used between two bike lanes going in the same direction",
			GenericMarkingType.Flipped | GenericMarkingType.Bike => "used between two bike lanes going in the opposite direction",
			GenericMarkingType.Normal | GenericMarkingType.Tram => "used between two tram lanes going in the same direction",
			GenericMarkingType.Flipped | GenericMarkingType.Tram => "used between two tram lanes going in the opposite direction",
			_ => string.Empty,
		};
	}

	private class LineDropDown : EnumDropDown<MarkingLineType> { }
	private class FillerDropDown : EnumDropDown<MarkingFillerType> { }
}
