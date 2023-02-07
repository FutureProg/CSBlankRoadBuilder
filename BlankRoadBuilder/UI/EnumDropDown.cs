using ModsCommon.UI;

using System;
using System.Linq;

using UnityEngine;

namespace BlankRoadBuilder.UI;

public class EnumDropDown<EnumType> : UIDropDown<EnumType> where EnumType : Enum
{
	public EnumDropDown()
	{
		ComponentStyle.CustomSettingsStyle(this, new Vector2(180, 22));

		foreach (EnumType item in Enum.GetValues(typeof(EnumType)))
		{
			AddItem(item, item.ToString().FormatWords());
		}

		selectedIndex = 0;

		size = new Vector2(180, 22);
		listWidth = 180;
		listHeight = 200;
		itemPadding = new RectOffset(14, 14, 0, 0);
		textScale = 0.7f;
		clampListToScreen = true;

		itemPadding = new RectOffset(6, 6, 6, 6);
		color = Color.white;
		hoveredBgColor = new Color32(172, 172, 172, 255);
		normalFgColor = Color.black;
		hoveredFgColor = new Color32(214, 214, 214, 255);
		popupColor = Color.white;
		popupTextColor = Color.black;
		textColor = Color.black;
	}
}
