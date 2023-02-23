using ColossalFramework.UI;

using System.Linq;

using UnityEngine;

namespace BlankRoadBuilder.UI;

public class AutoTabstrip : UITabstrip
{
	public float TabHeight { get; private set; }

	public static TTabstrip AddTabstrip<TTabstrip>(UIComponent parent, float posX, float posY, float width, float height, out UITabContainer container, float tabHeight) where TTabstrip : UITabstrip
	{
		var val = parent.AddUIComponent<TTabstrip>();
		val.relativePosition = new Vector2(posX, posY);
		val.width = width;
		val.height = height;
		val.clipChildren = false;
		var uITabContainer = parent.AddUIComponent<UITabContainer>();
		uITabContainer.name = "TabContainer";
		uITabContainer.relativePosition = new Vector2(posX, posY + tabHeight + 5f);
		uITabContainer.width = width;
		uITabContainer.height = height - tabHeight - 5f;
		uITabContainer.clipChildren = false;
		val.tabPages = uITabContainer;
		container = uITabContainer;
		return val;
	}

	public static AutoTabstrip AddTabstrip(UIComponent parent, float posX, float posY, float width, float height, out UITabContainer container, float tabHeight = 25f)
	{
		var autoTabstrip = AddTabstrip<AutoTabstrip>(parent, posX, posY, width, height, out container, tabHeight);
		autoTabstrip.TabHeight = tabHeight;
		return autoTabstrip;
	}

	public void EqualizeTabs()
	{
		var num = Mathf.Floor(base.width / tabCount);
		foreach (var item in m_ChildComponents.OfType<UIButton>())
		{
			item.width = num;
		}
	}

	protected override void OnComponentAdded(UIComponent child)
	{
		base.OnComponentAdded(child);
		if (child is not UIButton uIButton)
		{
			return;
		}

		uIButton.height = TabHeight;
		uIButton.wordWrap = true;
		var num = Mathf.Floor(base.width / tabCount);
		foreach (var item in m_ChildComponents.OfType<UIButton>())
		{
			item.width = num;
		}
	}
}