using BlankRoadBuilder.Domain.Options;

using ColossalFramework.UI;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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

		foreach (var setting in settings)
		{
			if (GenerateSettingComponent(setting) is UIComponent component && !string.IsNullOrEmpty(setting.Info?.Description))
			{
				component.tooltip = setting.Info?.Description;
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

			return textfield;
		}

		if (setting.Property.PropertyType.IsEnum)
		{
			var enumVales = getEnumValues(setting.Property.PropertyType);

			var dropDown = AddDropdown(setting.Info.Name, enumVales.Values.ToArray(), enumVales.Keys.ToList().IndexOf(getDefaultValue<int>(setting.Property)));

			dropDown.eventSelectedIndexChanged += (s, v) => setSettingValue(setting.Property, enumVales.Keys.ToList()[v]);

			return dropDown;
		}

		if (setting.Property.PropertyType == typeof(bool))
		{
			var checkbox = AddCheckbox(setting.Info.Name, getDefaultValue<bool>(setting.Property));

			checkbox.eventCheckChanged += (s, v) => setSettingValue(setting.Property, v);

			return checkbox;
		}

		if (setting.Property.PropertyType == typeof(float) || setting.Property.PropertyType == typeof(int))
		{
			var slider = AddSlider(setting.Info.Name, setting.Info.MinValue, setting.Info.MaxValue, setting.Info.Step, getDefaultValue<float>(setting.Property), setting.Info.MeasurementUnit);

			slider.eventValueChanged += (s, v) => setSettingValue(setting.Property, v);

			return slider;
		}

		return null;

		static t? getDefaultValue<t>(PropertyInfo property) => (t)property.GetValue(null, null);

		static void setSettingValue(PropertyInfo property, object value) => property.SetValue(null, property.PropertyType.IsEnum ? Enum.ToObject(property.PropertyType, value) : Convert.ChangeType(value, property.PropertyType), null);

		static Dictionary<int, string> getEnumValues(Type enumType) => Enum.GetValues(enumType).Cast<object>().ToDictionary(x => (int)x, x => x.ToString().FormatWords());
	}
}