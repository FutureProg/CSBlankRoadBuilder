using AdaptiveRoads.UI;
using AdaptiveRoads.UI.VBSTool;

using AlgernonCommons.Keybinding;
using AlgernonCommons.Translation;
using AlgernonCommons.UI;

using ColossalFramework.UI;

using System;

using UnityEngine;

namespace BlankRoadBuilder.UI.Options;
public abstract class OptionsPanelBase
{
    protected float yPos;

    protected const float Margin = 12f;
    protected const float LeftMargin = 24f;
    protected const float GroupMargin = 40f;

    protected readonly UIScrollablePanel _panel;

    public abstract string TabName { get; }

    public OptionsPanelBase(UITabstrip tabStrip, int tabIndex)
    {
        var panel = UITabstrips.AddTextTab(tabStrip, TabName, tabIndex, out var _, autoLayout: false);

		_panel = panel.AddUIComponent<UIScrollablePanel>();
		_panel.relativePosition = new Vector2(0, Margin);
		_panel.autoSize = false;
		_panel.autoLayout = false;
		_panel.width = panel.width - 30F;
        _panel.height = panel.height - Margin;
		_panel.clipChildren = true;
		_panel.builtinKeyNavigation = true;
		_panel.scrollWheelDirection = UIOrientation.Vertical;

		UIScrollbars.AddScrollbar(panel, _panel);
	}

	protected virtual UIDropDown AddDropdown(string labelKey, string[] items, int selectedIndex)
    {
        var newDropDown = UIDropDowns.AddPlainDropDown(_panel, Margin, yPos, labelKey, items, selectedIndex);

        yPos += newDropDown.parent.height + Margin;

        return newDropDown;
    }

	protected virtual UICheckBox AddCheckbox(string labelKey, bool initialValue)
    {
        var newCheckbox = UICheckBoxes.AddPlainCheckBox(_panel, Margin, yPos, labelKey);

        newCheckbox.isChecked = initialValue;

        yPos += newCheckbox.height + Margin;

        return newCheckbox;
    }

	protected virtual UITextField AddTextField(string labelKey, string? initialValue)
    {
        var newTextField = UITextFields.AddPlainTextfield(_panel, labelKey);

        newTextField.parent.relativePosition = new Vector2(Margin, yPos);
        newTextField.text = initialValue;

        yPos += newTextField.parent.height + Margin;

        return newTextField;
    }

    protected virtual UISlider AddSlider(string labelKey, float minValue, float maxValue, float step, float initialValue, string? measurementUnit = null, float width = 600, float? xPos = null)
    {
        var newSlider = UISliders.AddPlainSlider(_panel, xPos ?? Margin, yPos, $"{labelKey} ( {initialValue:0.#####}{measurementUnit} )", minValue, maxValue, step, initialValue, width);

        newSlider.eventValueChanged += (s, v) =>
        {
			s.parent.Find<UILabel>("Label").text = $"{labelKey} ( {v:0.#####}{measurementUnit} )";
		};

        yPos += newSlider.parent.height;

        return newSlider;
    }

	protected virtual UILabel AddLabel(string text)
    {
        var label = UILabels.AddLabel(_panel, Margin, yPos, text);

        yPos += label.height + Margin;

        return label;
    }
}