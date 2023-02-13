using ColossalFramework.UI;

using ModsCommon.UI;
using ModsCommon.Utilities;

using UnityEngine;

namespace BlankRoadBuilder.UI.Options;
public abstract class OptionsPanelBase
{
	protected float yPos;
	private readonly UIScrollbar scrollbar;
	protected const float Margin = 12f;
	protected const float LeftMargin = 24f;
	protected const float GroupMargin = 40f;

	protected readonly UIScrollablePanel _panel;

	public abstract string TabName { get; }

	public OptionsPanelBase(UITabstrip tabStrip, int tabIndex, int tabCount, float? margin = null)
	{
		var panel = AddTextTab(tabStrip, TabName, tabIndex, tabCount);

		_panel = panel.AddUIComponent<UIScrollablePanel>();
		_panel.relativePosition = new Vector2(0, Margin);
		_panel.autoSize = false;
		_panel.autoLayout = false;
		_panel.width = panel.width - (margin ?? 12F);
		_panel.height = panel.height - Margin;
		_panel.clipChildren = false;
		_panel.builtinKeyNavigation = true;
		_panel.scrollWheelDirection = UIOrientation.Vertical;
		_panel.scrollPadding = new RectOffset((int)(margin ?? 10), 0, 0, 20);

		scrollbar = CustomScrollBar.AddScrollbar(panel, _panel);

		panel.eventVisibilityChanged += (s, e) => scrollbar.value = 0;
	}

	protected virtual UIDropDown AddDropdown(string labelKey, string[] items, int selectedIndex)
	{
		var dropDown = _panel.AddUIComponent<GenericDropDown>();
		dropDown.relativePosition = new Vector2(Margin, yPos + 28);
		dropDown.items = items;
		dropDown.selectedIndex = selectedIndex;

		dropDown.AddLabel(labelKey, SpriteAlignment.TopLeft, 1);

		yPos += dropDown.height + 28 + Margin;

		return dropDown;
	}

	protected virtual UICheckBox AddCheckbox(string labelKey, bool initialValue)
	{
		var newCheckbox = _panel.AddUIComponent<CustomCheckbox>();
		newCheckbox.relativePosition = new Vector2(Margin, yPos);
		newCheckbox.text = labelKey;
		newCheckbox.isChecked = initialValue;

		yPos += newCheckbox.height + Margin;

		return newCheckbox;
	}

	protected virtual UITextField AddTextField(string labelKey, string? initialValue)
	{
		var newTextField = _panel.AddUIComponent<StringUITextField>();
		newTextField.size = new Vector2(140, 22);
		newTextField.textScale = 0.8F;
		newTextField.SetDefaultStyle();

		newTextField.AddLabel(labelKey, SpriteAlignment.TopLeft, 1);

		newTextField.parent.relativePosition = new Vector2(Margin, yPos);
		newTextField.text = initialValue;

		yPos += newTextField.height + Margin;

		return newTextField;
	}

	protected virtual UISlider AddSlider(string labelKey, float minValue, float maxValue, float step, float initialValue, string? measurementUnit = null, float width = 400, float? xPos = null)
	{
		var newSlider = _panel.AddUIComponent<CustomSlider>();
		newSlider.relativePosition = new Vector2(xPos ?? Margin, yPos + 30 + newSlider.Margin.top);
		newSlider.minValue = minValue;
		newSlider.maxValue = maxValue;
		newSlider.stepSize = step;
		newSlider.value = initialValue;
		newSlider.width = width;

		var label = newSlider.AddLabel($"{labelKey} ( {initialValue:0.#####}{measurementUnit} )", SpriteAlignment.TopLeft, 1);

		newSlider.eventValueChanged += (s, v) =>
		{
			label.text = $"{labelKey} ( {v:0.#####}{measurementUnit} )";
		};

		yPos += newSlider.height + newSlider.Margin.vertical + 30 + Margin;

		return newSlider;
	}

	public static UIPanel AddTextTab(UITabstrip tabstrip, string tabName, int tabIndex, int tabCount, float width = 170f, bool autoLayout = false)
	{
		var uIButton = tabstrip.AddTab(tabName);
		uIButton.atlas = CommonTextures.Atlas;
		var name = tabIndex == 0 ? "Left" : tabIndex == (tabCount - 1) ? "Right" : "Middle";
		uIButton.normalBgSprite = "FieldNormal" + name;
		uIButton.disabledBgSprite = "FieldDisabled" + name;
		uIButton.focusedBgSprite = "FieldFocused" + name;
		uIButton.hoveredBgSprite = "FieldHovered" + name;
		uIButton.pressedBgSprite = "FieldFocused" + name;
		uIButton.color = new Color32(119, 127, 136, 255);
		uIButton.hoveredColor = new Color32(119, 127, 136, 255);
		uIButton.focusedColor = new Color32(51, 116, 187, 255);
		uIButton.pressedColor = new Color32(51, 116, 187, 255);

		if (tabstrip is not AutoTabstrip)
		{
			uIButton.width = width;
		}

		var uIPanel = tabstrip.tabContainer.components[tabIndex] as UIPanel;
		uIPanel!.autoLayout = autoLayout;
		uIPanel.autoLayoutDirection = LayoutDirection.Vertical;
		uIPanel.autoLayoutPadding.top = 5;
		uIPanel.autoLayoutPadding.left = 10;

		return uIPanel;
	}
}