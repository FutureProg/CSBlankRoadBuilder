using BlankRoadBuilder.Domain.Options;
using BlankRoadBuilder.Util;
using BlankRoadBuilder.Util.Props;
using BlankRoadBuilder.Util.Props.Templates;

using ColossalFramework.UI;

using ModsCommon.UI;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

using UnityEngine;

namespace BlankRoadBuilder.UI.Options;
internal class CustomPropOptionControl : UISprite
{
	private const float Margin = 5F;

	private readonly List<Action> _setDataProperties=new();
	private SelectPropProperty? propSelectButton;
	private SelectTreeProperty? treeSelectButton;
	private SelectBuildingProperty? buildingSelectButton;
	private UILabel? titleLabel;
	private float yPos;

	public Prop Prop { get; private set; }
	public PropTemplate? Value { get; private set; }

	internal void Init(Prop prop, PropTemplate value)
	{
		Prop = prop;
		size = new Vector2(345, 155);
		atlas = ResourceUtil.GetAtlas("MarkingOptionBack.png");
		spriteName = "normal";

		titleLabel = AddUIComponent<UILabel>();
		titleLabel.text = prop.ToString().FormatWords();
		titleLabel.textScale = 1;
		titleLabel.textColor = new Color32(92, 182, 239, 255);
		titleLabel.autoSize = true;
		titleLabel.font = UIFonts.SemiBold;
		titleLabel.relativePosition = new Vector2(2 * Margin, 2 * Margin);

		var undoButton = AddUIComponent<SlickButton>();
		undoButton.size = new Vector2(22, 22);
		undoButton.SetIcon("I_Undo.png");
		undoButton.text = " ";
		undoButton.tooltip = "Reset this prop's settings to their default values";
		undoButton.relativePosition = new Vector2(width - 22 - 6, 6);
		undoButton.eventClick += UndoButton_eventClick;

		propSelectButton = AddUIComponent<SelectPropProperty>();
		propSelectButton.size = new Vector2(240, 50);
		propSelectButton.relativePosition = new Vector2(width - propSelectButton.width - Margin, 30 + (2 * Margin));

		treeSelectButton = AddUIComponent<SelectTreeProperty>();
		treeSelectButton.size = new Vector2(240, 50);
		treeSelectButton.relativePosition = new Vector2(width - treeSelectButton.width - Margin, 30 + (2 * Margin));

		buildingSelectButton = AddUIComponent<SelectBuildingProperty>();
		buildingSelectButton.size = new Vector2(240, 50);
		buildingSelectButton.relativePosition = new Vector2(width - buildingSelectButton.width - Margin, 30 + (2 * Margin));

		var label = propSelectButton.AddLabel("Prop", SpriteAlignment.LeftCenter, 0.8F);
		label.relativePosition = new Vector2(-propSelectButton.relativePosition.x + 2 * Margin, label.relativePosition.y);
		label = treeSelectButton.AddLabel("Tree", SpriteAlignment.LeftCenter, 0.8F);
		label.relativePosition = new Vector2(-treeSelectButton.relativePosition.x + 2 * Margin, label.relativePosition.y);
		label = buildingSelectButton.AddLabel("Pillar", SpriteAlignment.LeftCenter, 0.8F);
		label.relativePosition = new Vector2(-buildingSelectButton.relativePosition.x + 2 * Margin, label.relativePosition.y);

		yPos = propSelectButton.relativePosition.y + propSelectButton.height + Margin;

		var properties = value.GetType()
			.GetProperties(BindingFlags.Public | BindingFlags.Instance)
			.ToDictionary(x => x, x => Attribute.GetCustomAttribute(x, typeof(PropOptionAttribute)) as PropOptionAttribute);
		
		foreach (var item in properties.Where(x => x.Value != null && !x.Value.Serialization))
		{
			var seperator = AddUIComponent<UISprite>();
			seperator.size = new Vector2(width, 1);
			seperator.relativePosition = new Vector3(0, yPos);
			seperator.atlas = ResourceUtil.GetAtlas("I_Seperator.png");
			seperator.spriteName = "normal";
			seperator.color = new Color32(104, 109, 116, 255);

			GenerateSettingComponent(item.Key, item.Value);
		}

		height =  Margin + components.Max(x => x.height + x.relativePosition.y);

		UpdateData(value);

		propSelectButton.OnValueChanged += DropDown_OnValueChanged;
		treeSelectButton.OnValueChanged += DropDown_OnValueChanged;
		buildingSelectButton.OnValueChanged += DropDown_OnValueChanged;
	}

	private void DropDown_OnValueChanged(PropInfo obj)
	{
		Value!.PropName = obj.name;

		PropUtil.SaveTemplate(Prop, Value);
	}

	private void DropDown_OnValueChanged(TreeInfo obj)
	{
		Value!.PropName = obj.name;

		PropUtil.SaveTemplate(Prop, Value);
	}

	private void DropDown_OnValueChanged(BuildingInfo obj)
	{
		Value!.PropName = obj.name;

		PropUtil.SaveTemplate(Prop, Value);
	}

	private void UndoButton_eventClick(UIComponent component, UIMouseEventParameter eventParam)
	{
		PropUtil.SaveTemplate(Prop, null);

		UpdateData(PropUtil.GetProp(Prop));
	}

	public void UpdateData(PropTemplate value)
	{
		Value = value;

		if (Value.IsTree)
		{
			propSelectButton!.isVisible = buildingSelectButton!.isVisible = false;
			treeSelectButton!.isVisible = true;
			treeSelectButton!.Prefab = Value!;
		}
		else if (Value.IsBuilding)
		{
			propSelectButton!.isVisible = treeSelectButton!.isVisible = false;
			buildingSelectButton!.isVisible = true;
			buildingSelectButton!.Prefab = Value!;
		}
		else
		{
			treeSelectButton!.isVisible = buildingSelectButton!.isVisible = false;
			propSelectButton!.isVisible = true;
			propSelectButton!.Prefab = Value!;
		}

		foreach (var item in _setDataProperties)
		{
			item();
		}
	}

	private UIComponent? GenerateSettingComponent(PropertyInfo property, PropOptionAttribute? info)
	{
		if (info == null)
		{
			return null;
		}

		if (property.PropertyType == typeof(string))
		{
			var textfield = AddTextField(info.Name);

			textfield.OnValueChanged += (v) => setSettingValue(property, v);

			_setDataProperties.Add(() => textfield.Value = getValue<string>(property) ?? string.Empty);

			return textfield;
		}

		if (property.PropertyType.IsEnum)
		{
			var enumVales = getEnumValues(property.PropertyType);

			var dropDown = AddDropdown(info.Name, enumVales.Values.ToArray());

			dropDown.eventSelectedIndexChanged += (s, v) => setSettingValue(property, enumVales.Keys.ToList()[v]);

			_setDataProperties.Add(() => dropDown.selectedIndex = enumVales.Keys.ToList().IndexOf(getValue<int>(property)));

			return dropDown;
		}

		if (property.PropertyType == typeof(bool))
		{
			var checkbox = AddCheckbox(info.Name);

			checkbox.eventCheckChanged += (s, v) => setSettingValue(property, v);

			_setDataProperties.Add(() => checkbox.isChecked = getValue<bool>(property));

			yPos += 2;

			return checkbox;
		}

		if (property.PropertyType == typeof(int))
		{
			var slider = AddIntField(info.Name, info.MinValue, info.MaxValue, info.MeasurementUnit);

			slider.OnValueChanged += (v) => setSettingValue(property, v);

			_setDataProperties.Add(() => slider.Value = getValue<int>(property));

			yPos += 5;

			return slider;
		}

		if (property.PropertyType == typeof(float))
		{
			var slider = AddFloatField(info.Name, info.MinValue, info.MaxValue, info.MeasurementUnit);

			slider.OnValueChanged += (v) => setSettingValue(property, v);

			_setDataProperties.Add(() => slider.Value = getValue<float>(property));

			yPos += 5;

			return slider;
		}

		return null;

		t? getValue<t>(PropertyInfo property) => (t)property.GetValue(Value, null);

		Dictionary<int, string> getEnumValues(Type enumType) => Enum.GetValues(enumType).Cast<object>().ToDictionary(x => (int)x, x => x.ToString().FormatWords());

		void setSettingValue(PropertyInfo property, object value)
		{
			property.SetValue(Value, property.PropertyType.IsEnum ? Enum.ToObject(property.PropertyType, value) : Convert.ChangeType(value, property.PropertyType), null);
			
			PropUtil.SaveTemplate(Prop, Value);
		}
	}

	private GenericDropDown AddDropdown(string labelKey, string[] items)
	{
		var ctrl = AddUIComponent<GenericDropDown>();
		ctrl.size = new Vector2(140, 22);
		ctrl.textScale = 0.8F;
		ctrl.items = items;

		var label = ctrl.AddLabel(labelKey, SpriteAlignment.LeftCenter, 0.8F);

		ctrl.relativePosition = new Vector2(width - ctrl.width - Margin, yPos + Margin);
		label.relativePosition = new Vector2(-ctrl.relativePosition.x + 2 * Margin, label.relativePosition.y);

		yPos = ctrl.height + ctrl.relativePosition.y;

		return ctrl;
	}

	private IntUITextField AddIntField(string labelKey, float minValue, float maxValue, string? measurementUnit = null)
	{
		var ctrl = AddUIComponent<IntUITextField>();
		ctrl.size = new Vector2(140, 22);
		ctrl.textScale = 0.8F;
		ctrl.MinValue = (int)minValue;
		ctrl.MaxValue = (int)maxValue;
		ctrl.SetDefaultStyle();

		var label = ctrl.AddLabel(labelKey, SpriteAlignment.LeftCenter, 0.8F);

		ctrl.relativePosition = new Vector2(width - ctrl.width - Margin, yPos + Margin);
		label.relativePosition = new Vector2(-ctrl.relativePosition.x + 2 * Margin, label.relativePosition.y);

		if (!string.IsNullOrEmpty(measurementUnit))
		{
			label.text += $" ({measurementUnit})";
		}

		yPos = ctrl.height + ctrl.relativePosition.y;

		return ctrl;
	}

	private FloatUITextField AddFloatField(string labelKey, float minValue, float maxValue, string? measurementUnit = null)
	{
		var ctrl = AddUIComponent<FloatUITextField>();
		ctrl.size = new Vector2(140, 22);
		ctrl.textScale = 0.8F;
		ctrl.MinValue = minValue;
		ctrl.MaxValue = maxValue;
		ctrl.SetDefaultStyle();

		var label = ctrl.AddLabel(labelKey, SpriteAlignment.LeftCenter, 0.8F);

		ctrl.relativePosition = new Vector2(width - ctrl.width - Margin, yPos + Margin);
		label.relativePosition = new Vector2(-ctrl.relativePosition.x + 2 * Margin, label.relativePosition.y);

		if (!string.IsNullOrEmpty(measurementUnit))
		{
			label.text += $" ({measurementUnit})";
		}

		yPos = ctrl.height + ctrl.relativePosition.y;

		return ctrl;
	}

	private StringUITextField AddTextField(string labelKey)
	{
		var ctrl = AddUIComponent<StringUITextField>();
		ctrl.size = new Vector2(140, 22);
		ctrl.textScale = 0.8F;
		ctrl.SetDefaultStyle();

		var label = ctrl.AddLabel(labelKey, SpriteAlignment.LeftCenter, 0.8F);

		ctrl.relativePosition = new Vector2(width - ctrl.width - Margin, yPos + Margin);
		label.relativePosition = new Vector2(-ctrl.relativePosition.x + 2 * Margin, label.relativePosition.y);

		yPos = ctrl.height + ctrl.relativePosition.y;

		return ctrl;
	}

	private UICheckBox AddCheckbox(string labelKey)
	{
		var newCheckbox = AddUIComponent<CustomCheckbox>();
		newCheckbox.relativePosition = new Vector2(Margin, yPos);
		newCheckbox.text = labelKey;

		yPos += newCheckbox.height + Margin;

		return newCheckbox;
	}
}