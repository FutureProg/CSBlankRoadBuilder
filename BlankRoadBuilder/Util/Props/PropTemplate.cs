namespace BlankRoadBuilder.Util.Props;

public class PropTemplate
{
	public string PropName { get; set; }
	public bool IsTree { get; set; }
	public bool IsBuilding { get; set; }

	public PropTemplate(string propName, bool isTree = false, bool isBuilding = false)
	{
		PropName = propName;
		IsTree = isTree;
		IsBuilding = isBuilding;
	}

	public static implicit operator PropInfo(PropTemplate prop)
	{
		return string.IsNullOrEmpty(prop.PropName) ? null : prop.IsTree ? null : PrefabCollection<PropInfo>.FindLoaded(prop.PropName);
	}

	public static implicit operator TreeInfo(PropTemplate prop)
	{
		return string.IsNullOrEmpty(prop.PropName) ? null : prop.IsTree ? PrefabCollection<TreeInfo>.FindLoaded(prop.PropName) : null;
	}

	public static implicit operator BuildingInfo(PropTemplate prop)
	{
		return string.IsNullOrEmpty(prop.PropName) ? null : prop.IsBuilding ? PrefabCollection<BuildingInfo>.FindLoaded(prop.PropName) : null;
	}
}
