using BlankRoadBuilder.Domain.Options;

using System;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;

using UnityEngine;

namespace BlankRoadBuilder.Util.Props.Templates;
[Serializable]
public class SavedPropTemplate
{
#nullable disable
	public Prop Prop { get; set; }
	public string PropName { get; set; }
	public string Type { get; set; }
	public bool IsTree { get; set; }
	public bool IsBuilding { get; set; }
	public string[] PropertyKeys { get; set; }
	public object[] PropertyValues { get; set; }
#nullable enable

	public static implicit operator PropTemplate(SavedPropTemplate prop)
	{
		var type = typeof(SavedPropTemplate).Assembly.GetType($"{nameof(BlankRoadBuilder)}.Util.Props.Templates.{prop.Type}");

		if (type != null)
		{
			var template = (PropTemplate)Activator.CreateInstance(type, prop.PropName);

			if (template != null)
			{
				if (prop.PropertyKeys != null && prop.PropertyValues != null && prop.PropertyKeys.Length == prop.PropertyValues.Length)
				{
					for (var i = 0; i < prop.PropertyKeys.Length; i++)
					{
						try
						{
							var property = type.GetProperty(prop.PropertyKeys[i], BindingFlags.Public | BindingFlags.Instance);

							if (property.PropertyType.IsEnum)
								property.SetValue(template, Enum.ToObject(property.PropertyType, prop.PropertyValues[i]), null);
							else if (property.PropertyType.GetInterface(nameof(ICustomPropProperty)) != null)
							{
								var customProp = Activator.CreateInstance(property.PropertyType) as ICustomPropProperty;

								customProp!.FromPrimitive(prop.PropertyValues[i]);

								property.SetValue(template, customProp, null);
							}
							else
								property.SetValue(template, prop.PropertyValues[i], null);
						}
						catch (Exception ex)
						{ Debug.LogError($"FAILED TO SET {type.Name}.{prop.PropertyKeys[i]} : {prop.PropertyValues[i]}\r\n{ex.Message}"); }
					}

					var defaultProp = PropUtil.GetDefaultProp(prop.Prop);
					var properties = type
						.GetProperties(BindingFlags.Public | BindingFlags.Instance)
						.Where(x => (Attribute.GetCustomAttribute(x, typeof(PropOptionAttribute)) as PropOptionAttribute) != null)
						.Where(x => (Attribute.GetCustomAttribute(x, typeof(XmlIgnoreAttribute)) as XmlIgnoreAttribute) == null);

					foreach (var item in properties)
					{
						if (!prop.PropertyKeys.Contains(item.Name))
						{
							item.SetValue(template, item.GetValue(defaultProp, null), null);
						}
					}
				}

				return template;
			}
		}

		return new PropTemplate(string.Empty);
	}
}