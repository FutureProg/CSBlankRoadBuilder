namespace BlankRoadBuilder.Util.Props;

public class PropTemplate
{
	public string PropName { get; set; }
	public bool IsTree { get; set; }

	public PropTemplate(string propName, bool isTree = false)
	{
		PropName = propName;
		IsTree = isTree;
	}

	public static implicit operator PropInfo(PropTemplate prop)
		=> string.IsNullOrEmpty(prop.PropName) ? null : prop.IsTree ? null : PrefabCollection<PropInfo>.FindLoaded(prop.PropName);

	public static implicit operator TreeInfo(PropTemplate prop)
		=> string.IsNullOrEmpty(prop.PropName) ? null : prop.IsTree ? PrefabCollection<TreeInfo>.FindLoaded(prop.PropName) : null;
}
