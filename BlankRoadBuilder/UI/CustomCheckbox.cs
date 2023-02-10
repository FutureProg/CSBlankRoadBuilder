using AlgernonCommons.UI;

using BlankRoadBuilder.Util;

using ColossalFramework.UI;

using System.Drawing.Printing;

using UnityEngine;

namespace BlankRoadBuilder.UI;

public class CustomCheckbox : UICheckBox
{
	private readonly UISprite _backgroundSprite;

	public CustomCheckbox()
	{
		width = 24F;
		height = 24F;
		clipChildren = false;

		_backgroundSprite = AddUIComponent<UISprite>();
		checkedBoxObject = AddUIComponent<UISprite>();

		checkedBoxObject.size = _backgroundSprite.size = size;
		checkedBoxObject.relativePosition = _backgroundSprite.relativePosition = Vector2.zero;
		_backgroundSprite.spriteName = "check-unchecked";
		((UISprite)checkedBoxObject).spriteName = "check-checked";
		((UISprite)checkedBoxObject).atlas = _backgroundSprite.atlas = ResourceUtil.GetAtlas("I_Check.png", new UITextureAtlas.SpriteInfo[]
		{
			new()
			{
				name = "check-unchecked",
				region = new Rect(0,0,1,0.5F)
			},
			new()
			{
				name = "check-checked",
				region = new Rect(0,0.5F,1,0.5F)
			}
		});

		label = AddUIComponent<UILabel>();
		label.verticalAlignment = UIVerticalAlignment.Middle;
		label.textScale = 1;
		label.autoSize = true;
		label.relativePosition = new Vector2(width + 5, 5);
		width += label.width + 10;

		eventCheckChanged += CustomCheckbox_eventCheckChanged;
	}

	private void CustomCheckbox_eventCheckChanged(UIComponent component, bool value)
	{
		_backgroundSprite.isVisible = !value;
	}
}
