using UnityEngine;
namespace BlankRoadBuilder.Util.Props.Templates;

public class PropTemplate
{
	public virtual PropCategory Category { get; }
	public string PropName { get; set; }
	public bool IsTree { get; protected set; }
	public bool IsBuilding { get; protected set; }

	public float Angle { get; protected set; }
	public float RepeatInterval { get; protected set; }
	public float SegmentOffset { get; protected set; }
	public int Probability { get; protected set; } = 100;
	public Vector3 Position { get; protected set; }

	public PropTemplate(string propName)
	{
		PropName = propName;
	}

	public static implicit operator PropInfo?(PropTemplate prop)
	{
		return string.IsNullOrEmpty(prop.PropName) || prop.IsTree || prop.IsBuilding ? null : PrefabCollection<PropInfo>.FindLoaded(prop.PropName);
	}

	public static implicit operator TreeInfo?(PropTemplate prop)
	{
		return string.IsNullOrEmpty(prop.PropName) || !prop.IsTree ? null : PrefabCollection<TreeInfo>.FindLoaded(prop.PropName);
	}

	public static implicit operator BuildingInfo?(PropTemplate prop)
	{
		return string.IsNullOrEmpty(prop.PropName) || !prop.IsBuilding ? null : PrefabCollection<BuildingInfo>.FindLoaded(prop.PropName);
	}
}
