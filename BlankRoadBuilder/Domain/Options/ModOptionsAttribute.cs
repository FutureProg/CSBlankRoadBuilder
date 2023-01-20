using System;

namespace BlankRoadBuilder.Domain.Options;
public class ModOptionsAttribute : Attribute
{
	public ModOptionsAttribute(string name, string description = "")
	{
		Name = name;
		Description = description;
	}

	public ModOptionsAttribute(string name, string description, float minValue, float maxValue, float step, string? measurementUnit) : this(name, description)
	{
		MinValue = minValue;
		MaxValue = maxValue;
		Step = step;
		MeasurementUnit = measurementUnit;
	}

	public string Name { get; }
	public string Description { get; }
	public float MinValue { get; set; }
	public float MaxValue { get; set; }
	public float Step { get; set; }
	public string? MeasurementUnit { get; internal set; }
}