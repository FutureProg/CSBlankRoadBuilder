using BlankRoadBuilder.Util;

using ColossalFramework.UI;

using ModsCommon.UI;

using System;

using UnityEngine;

namespace BlankRoadBuilder.UI;

public class EnumDropDown<EnumType> : CustomUIDropDown<EnumType> where EnumType : Enum
{
	public EnumDropDown() : base(new Vector2(180, 22))
	{
		foreach (EnumType item in Enum.GetValues(typeof(EnumType)))
		{
			AddItem(item, item.ToString().FormatWords());
		}

		selectedIndex = 0;
	}
}

public class GenericDropDown : CustomUIDropDown<string>
{
	public GenericDropDown() : base(new Vector2(360, 28))
	{
		textScale = 0.9F;
	}
}

public class SmallGenericDropDown : CustomUIDropDown<string>
{
	public SmallGenericDropDown() : base(new Vector2(170, 22))
	{
		textScale = 0.7F;
		textFieldPadding = itemPadding = new RectOffset(10, 10, 4, 0);
	}

	protected override Vector2 GetForegroundRenderSize(UITextureAtlas.SpriteInfo spriteInfo)
	{
		return new Vector2(16, 16);
	}
}

public class CustomUIDropDown<ValueType> : UIDropDown<ValueType>
{
	public CustomUIDropDown(Vector2 size)
	{
		this.size = size;
		selectedIndex = 0;

		ComponentStyle.CustomSettingsStyle(this, size);
		textScale = 0.7f;
		listWidth = (int)width;
		listHeight = (int)height * 10;
		itemHeight = (int)height;
		clampListToScreen = true;
		textFieldPadding = itemPadding = new RectOffset(10, 10, 7, 0);
		color = Color.white;
		hoveredBgColor = new Color32(200, 200, 200, 255);
		normalFgColor = new Color32(60, 60, 60, 255);
		hoveredFgColor = new Color32(20, 20, 20, 255);
		focusedFgColor = new Color32(60, 60, 60, 255);
		pressedFgColor = new Color32(20, 20, 20, 255);
		pressedBgColor = new Color32(220, 240, 255, 255);
		focusedBgColor = Color.white;
		popupColor = Color.white;
		textColor = new Color32(20, 20, 20, 255);
		popupTextColor = new Color32(20, 20, 20, 255);

		atlasForeground = ResourceUtil.GetAtlas("I_DropArrow.png")!;
		normalFgSprite = hoveredFgSprite = focusedFgSprite = disabledFgSprite = "normal";
	}

	public void UseWhiteStyle()
	{
		normalFgColor = focusedFgColor = new Color32(178, 192, 202, 255);
		hoveredFgColor = pressedFgColor = new Color32(155, 164, 170, 255);
		textColor = Color.white;
		popupTextColor = Color.white;
		popupColor = color = new Color32(162, 168, 178, 255);
		focusedBgColor = new Color32(162, 168, 178, 255);
		hoveredBgColor = new Color32(179, 188, 203, 255);
	}
}
