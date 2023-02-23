using BlankRoadBuilder.Util;

using ColossalFramework.UI;

using UnityEngine;

namespace BlankRoadBuilder.UI;
internal class CustomScrollBar : UIScrollbar
{
	private static CustomScrollBar AddScrollbar(UIComponent parent)
	{
		var uIScrollbar = parent.AddUIComponent<CustomScrollBar>();
		uIScrollbar.orientation = UIOrientation.Vertical;
		uIScrollbar.pivot = UIPivotPoint.TopLeft;
		uIScrollbar.minValue = 0f;
		uIScrollbar.value = 0f;
		uIScrollbar.incrementAmount = 50f;
		uIScrollbar.autoHide = true;
		uIScrollbar.width = 10f;
		var uISlicedSprite = uIScrollbar.AddUIComponent<UISlicedSprite>();
		uISlicedSprite.relativePosition = Vector2.zero;
		uISlicedSprite.autoSize = true;
		uISlicedSprite.anchor = UIAnchorStyle.All;
		uISlicedSprite.size = uISlicedSprite.parent.size;
		uISlicedSprite.fillDirection = UIFillDirection.Vertical;
		uIScrollbar.trackObject = uISlicedSprite;
		var uISlicedSprite2 = uISlicedSprite.AddUIComponent<UISlicedSprite>();
		uISlicedSprite2.relativePosition = Vector2.zero;
		uISlicedSprite2.fillDirection = UIFillDirection.Vertical;
		uISlicedSprite2.autoSize = true;
		uISlicedSprite2.width = uISlicedSprite2.parent.width;
		uIScrollbar.thumbObject = uISlicedSprite2;
		(uIScrollbar.thumbObject as UISlicedSprite)!.atlas =
		(uIScrollbar.trackObject as UISlicedSprite)!.atlas = ResourceUtil.GetAtlas("Scrollbar.png", new UITextureAtlas.SpriteInfo[]
		{
			new()
			{
				name = "bar",
				region = new Rect(0F, 0F, 0.5F, 1F)
			},
			new()
			{
				name = "thumb",
				region = new Rect(0.5F, 0F, 0.5F, 1F)
			},
		});
		(uIScrollbar.thumbObject as UISlicedSprite)!.spriteName = "thumb";
		(uIScrollbar.trackObject as UISlicedSprite)!.spriteName = "bar";
		uIScrollbar.thumbObject.eventMouseDown += (s, _) => (s as UISlicedSprite)!.color = new Color32(39, 130, 224, 255);
		uIScrollbar.thumbObject.eventMouseUp += (s, _) => (s as UISlicedSprite)!.color = Color.white;
		return uIScrollbar;
	}

	public static CustomScrollBar AddScrollbar(UIComponent parent, UIScrollablePanel scrollPanel)
	{
		var newScrollbar = AddScrollbar(parent);
		newScrollbar.relativePosition = new Vector2(scrollPanel.relativePosition.x + scrollPanel.width, scrollPanel.relativePosition.y);
		newScrollbar.height = scrollPanel.height;
		scrollPanel.eventSizeChanged += delegate
		{
			newScrollbar.relativePosition = new Vector2(scrollPanel.relativePosition.x + scrollPanel.width, scrollPanel.relativePosition.y);
			newScrollbar.height = scrollPanel.height;
		};
		scrollPanel.verticalScrollbar = newScrollbar;
		return newScrollbar;
	}
}
