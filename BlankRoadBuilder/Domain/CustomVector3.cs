using BlankRoadBuilder.Util.Props;
using System;
using System.ComponentModel;
using System.Globalization;

using UnityEngine;

namespace BlankRoadBuilder.Domain;

[Serializable]
public struct CustomVector3 : ICustomPropProperty
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }

    public CustomVector3() : this(0, 0, 0)
    { }

    public CustomVector3(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
	}

	public static implicit operator Vector3(CustomVector3 vector)
	{
		return new(vector.X, vector.Y, vector.Z);
	}

	public static implicit operator CustomVector3(Vector3 vector)
	{
		return new(vector.x, vector.y, vector.z);
	}

	public static implicit operator string(CustomVector3 vector)
	{
		return $"{vector.X};{vector.Y};{vector.Z}";
	}

	public static implicit operator CustomVector3(string vector)
	{
		var splits = vector?.Split(';');

		if (splits?.Length == 3)
		{
			return new(float.Parse(splits[0]), float.Parse(splits[1]), float.Parse(splits[2]));
		}

		return new();
	}

	public object AsPrimitive() => $"{X};{Y};{Z}";
	public void FromPrimitive(object primiteValue)
	{
		var splits = primiteValue?.ToString()?.Split(';');

		if (splits?.Length == 3)
		{
			X = float.Parse(splits[0]);
			Y = float.Parse(splits[1]);
			Z = float.Parse(splits[2]);
		}
	}
}