using System;

namespace BlankRoadBuilder.Domain.Options;
public class PropOptionAttribute : Attribute
{
	public PropOptionAttribute(object defaultValue, string name, string description = "")
	{
		DefaultValue = defaultValue;
		Name = name;
		Description = description;
	}

	public PropOptionAttribute(object defaultValue, string name, string description, float minValue, float maxValue, float step, string? measurementUnit) : this(defaultValue, name, description)
	{
		MinValue = minValue;
		MaxValue = maxValue;
		Step = step;
		MeasurementUnit = measurementUnit;
	}

	public object DefaultValue { get; }
	public string Name { get; }
	public string Description { get; }
	public float MinValue { get; set; }
	public float MaxValue { get; set; }
	public float Step { get; set; }
	public string? MeasurementUnit { get; set; }
}