using BlankRoadBuilder.Util;

using ColossalFramework.UI;

using UnityEngine;

namespace BlankRoadBuilder.UI;

public class ToggleCheckbox : UICheckBox
{
	private readonly UISprite _backgroundSprite;

	public ToggleCheckbox()
	{
		width = 48F;
		height = 24F;
		clipChildren = false;

		_backgroundSprite = AddUIComponent<UISprite>();
		checkedBoxObject = AddUIComponent<UISprite>();

		checkedBoxObject.size = _backgroundSprite.size = size;
		checkedBoxObject.relativePosition = _backgroundSprite.relativePosition = Vector2.zero;
		_backgroundSprite.spriteName = "check-unchecked";
		((UISprite)checkedBoxObject).spriteName = "check-checked";
		((UISprite)checkedBoxObject).atlas = _backgroundSprite.atlas = ResourceUtil.GetAtlas("I_Toggle.png", new UITextureAtlas.SpriteInfo[]
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

		eventCheckChanged += CustomCheckbox_eventCheckChanged;
	}

	private void CustomCheckbox_eventCheckChanged(UIComponent component, bool value)
	{
		_backgroundSprite.isVisible = !value;
	}

	protected override void OnMouseEnter(UIMouseEventParameter p)
	{
		base.OnMouseEnter(p);

		color = new Color32(220, 220, 220, 255);
	}

	protected override void OnMouseLeave(UIMouseEventParameter p)
	{
		base.OnMouseLeave(p);

		color = Color.white;
	}
}
