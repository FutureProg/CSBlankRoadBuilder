using ColossalFramework.UI;

using System.Linq;

using UnityEngine;

namespace BlankRoadBuilder.UI;

public class AutoTabstrip : UITabstrip
{
	private float _tabHeight;

	public float TabHeight => _tabHeight;

	public static TTabstrip AddTabstrip<TTabstrip>(UIComponent parent, float posX, float posY, float width, float height, out UITabContainer container, float tabHeight) where TTabstrip : UITabstrip
	{
		TTabstrip val = parent.AddUIComponent<TTabstrip>();
		val.relativePosition = new Vector2(posX, posY);
		val.width = width;
		val.height = height;
		val.clipChildren = false;
		UITabContainer uITabContainer = parent.AddUIComponent<UITabContainer>();
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
		AutoTabstrip autoTabstrip = AddTabstrip<AutoTabstrip>(parent, posX, posY, width, height, out container, tabHeight);
		autoTabstrip._tabHeight = tabHeight;
		return autoTabstrip;
	}

	public void EqualizeTabs()
	{
		float num = Mathf.Floor(base.width / (float)base.tabCount);
		foreach (UIButton item in m_ChildComponents.OfType<UIButton>())
		{
			item.width = num;
		}
	}

	protected override void OnComponentAdded(UIComponent child)
	{
		base.OnComponentAdded(child);
		UIButton uIButton = child as UIButton;
		if ((object)uIButton == null)
		{
			return;
		}

		uIButton.height = _tabHeight;
		uIButton.wordWrap = true;
		float num = Mathf.Floor(base.width / (float)base.tabCount);
		foreach (UIButton item in m_ChildComponents.OfType<UIButton>())
		{
			item.width = num;
		}
	}
}