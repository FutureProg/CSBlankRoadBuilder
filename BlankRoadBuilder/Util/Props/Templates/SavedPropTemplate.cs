using System;
using System.Reflection;

namespace BlankRoadBuilder.Util.Props.Templates;
[Serializable]
public class SavedPropTemplate
{
	public Prop Prop { get; set; }
	public string PropName { get; set; }
	public string Type { get; set; }
	public bool IsTree { get; set; }
	public bool IsBuilding { get; set; }
	public string[] PropertyKeys { get; set; }
	public object[] PropertyValues { get; set; }


	public static implicit operator PropTemplate(SavedPropTemplate prop)
	{
		var type = typeof(SavedPropTemplate).Assembly.GetType($"{nameof(BlankRoadBuilder)}.Util.Props.Templates.{prop.Type}");

		if (type != null)
		{
			var template = (PropTemplate)Activator.CreateInstance(type, prop.PropName, prop.IsTree, prop.IsBuilding);

			if (template != null)
			{
				if (prop.PropertyKeys != null && prop.PropertyValues != null && prop.PropertyKeys.Length == prop.PropertyValues.Length)
				{
					for (var i = 0; i < prop.PropertyKeys.Length; i++)
					{
						type.GetProperty(prop.PropertyKeys[i], BindingFlags.Public | BindingFlags.Instance)
							.SetValue(template, prop.PropertyValues[i], null);
					}
				}

				return template;
			}
		}

		return new PropTemplate(string.Empty);
	}
}