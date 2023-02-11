using BlankRoadBuilder.Util;

using ColossalFramework.UI;

using UnityEngine;

namespace BlankRoadBuilder.UI;
public class CustomSlider : UISlider, IMarginedComponent
{
	public CustomSlider()
	{
		atlas = ResourceUtil.GetAtlas("SliderBackground.png");
		backgroundSprite = "normal";
		height = 10F;

		var thumb = AddUIComponent<UISlicedSprite>();
		thumb.atlas = ResourceUtil.GetAtlas("I_ArrowDown.png");
		thumb.spriteName = "normal";
		thumb.size = new Vector2(16, 16);
		thumbObject = thumb;
	}

	public RectOffset Margin { get; } = new RectOffset(0, 0, 6, 4);
}
