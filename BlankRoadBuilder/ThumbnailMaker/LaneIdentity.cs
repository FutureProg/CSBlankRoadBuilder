using System;

using UnityEngine;

namespace BlankRoadBuilder.ThumbnailMaker;

public class LaneIdentityAttribute : Attribute
{
	public LaneIdentityAttribute(int id)
	{
		Id = id;
	}

	public LaneIdentityAttribute(int id, string name, int r, int g, int b)
	{
		Id = id;
		Name = name;
		DefaultColor = new Color(r, g, b); //FromArgb(r, g, b);
	}

	public int Id { get; }
	public string? Name { get; }
	public Color DefaultColor { get; }
}
