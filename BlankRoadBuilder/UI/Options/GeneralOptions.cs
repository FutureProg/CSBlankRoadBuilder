using AdaptiveRoads.UI.VBSTool;

using AlgernonCommons.UI;

using BlankRoadBuilder.Domain.Options;
using BlankRoadBuilder.Util;

using ColossalFramework.UI;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

using UnityEngine;

namespace BlankRoadBuilder.UI.Options;
internal class GeneralOptions : OptionsPanelBase
{
	public override string TabName { get; } = "General";

	private struct Setting
	{
		public PropertyInfo Property { get; set; }
		public ModOptionsAttribute? Info { get; set; }
	}

	public GeneralOptions(UITabstrip tabStrip, int tabIndex) : base(tabStrip, tabIndex)
	{
		var settings = typeof(ModOptions).GetProperties(BindingFlags.Public | BindingFlags.Static).Select(x => new Setting { Property = x, Info = Attribute.GetCustomAttribute(x, typeof(ModOptionsAttribute)) as ModOptionsAttribute }).ToList();
		var first = true;

		foreach (var grp in settings.GroupBy(x => x.Info?.Category))
		{
			if (first)
			{
				first = false;
			}
			else
			{
				yPos += 7;
			}

			var icon = _panel.AddUIComponent<UISprite>();
			icon.atlas = ResourceUtil.GetAtlas(OptionCategory.GetIcon(grp.Key));
			icon.spriteName = "normal";
			icon.size = new Vector2(32, 32);
			icon.relativePosition = new Vector2(Margin, yPos + (30 - 32) / 2);

			var title = _panel.AddUIComponent<UILabel>();
			title.text = grp.Key?.ToUpper();
			title.textScale = 1.4F;
			title.font = UIFonts.SemiBold;
			title.autoSize = true;
			title.relativePosition = new Vector2(32 + 2 * Margin, yPos + (30 - title.height) / 2);

			yPos += 50;

			foreach (var setting in grp)
			{
				if (GenerateSettingComponent(setting) is UIComponent component)
				{
					if (!string.IsNullOrEmpty(setting.Info?.Description))
					{
						component.tooltip = setting.Info?.Description;
					}
				}
			}
		}
	}

	private object? GenerateSettingComponent(Setting setting)
	{
		if (setting.Info == null)
		{
			return null;
		}

		if (setting.Property.PropertyType == typeof(string))
		{
			var textfield = AddTextField(setting.Info.Name, getDefaultValue<string>(setting.Property));

			textfield.eventTextChanged += (s, v) => setSettingValue(setting.Property, v);

			textfield.parent.relativePosition = new Vector2(textfield.parent.relativePosition.x + 40, textfield.parent.relativePosition.y);

			return textfield;
		}

		if (setting.Property.PropertyType.IsEnum)
		{
			var enumVales = getEnumValues(setting.Property.PropertyType);

			var dropDown = AddDropdown(setting.Info.Name, enumVales.Values.ToArray(), enumVales.Keys.ToList().IndexOf(getDefaultValue<int>(setting.Property)));

			dropDown.eventSelectedIndexChanged += (s, v) => setSettingValue(setting.Property, enumVales.Keys.ToList()[v]);

			dropDown.parent.relativePosition = new Vector2(dropDown.parent.relativePosition.x + 40, dropDown.parent.relativePosition.y);

			return dropDown;
		}

		if (setting.Property.PropertyType == typeof(bool))
		{
			var checkbox = AddCheckbox(setting.Info.Name, getDefaultValue<bool>(setting.Property));

			checkbox.eventCheckChanged += (s, v) => setSettingValue(setting.Property, v);

			checkbox.relativePosition = new Vector2(checkbox.relativePosition.x + 40, checkbox.relativePosition.y);

			yPos += 2;

			return checkbox;
		}

		if (setting.Property.PropertyType == typeof(float) || setting.Property.PropertyType == typeof(int))
		{
			yPos += 3;
			var slider = AddSlider(setting.Info.Name, setting.Info.MinValue, setting.Info.MaxValue, setting.Info.Step, getDefaultValue<float>(setting.Property), setting.Info.MeasurementUnit);

			slider.eventValueChanged += (s, v) => setSettingValue(setting.Property, v);

			slider.parent.relativePosition = new Vector2(slider.parent.relativePosition.x + 40, slider.parent.relativePosition.y);

			yPos += 5;

			return slider;
		}
		return null;
		static t? getDefaultValue<t>(PropertyInfo property) => (t)property.GetValue(null, null);

		static void setSettingValue(PropertyInfo property, object value) => property.SetValue(null, property.PropertyType.IsEnum ? Enum.ToObject(property.PropertyType, value) : Convert.ChangeType(value, property.PropertyType), null);

		static Dictionary<int, string> getEnumValues(Type enumType) => Enum.GetValues(enumType).Cast<object>().ToDictionary(x => (int)x, x => x.ToString().FormatWords());
	}
}