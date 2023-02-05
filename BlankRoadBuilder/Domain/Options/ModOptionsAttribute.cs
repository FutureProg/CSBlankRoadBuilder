using System;

namespace BlankRoadBuilder.Domain.Options;
public class ModOptionsAttribute : Attribute
{
	public ModOptionsAttribute(string category, string name, string description = "")
	{
		Category = category;
		Name = name;
		Description = description;
	}

	public ModOptionsAttribute(string category, string name, string description, float minValue, float maxValue, float step, string? measurementUnit) : this(category, name, description)
	{
		MinValue = minValue;
		MaxValue = maxValue;
		Step = step;
		MeasurementUnit = measurementUnit;
	}

	public string Category { get; }
	public string Name { get; }
	public string Description { get; }
	public float MinValue { get; set; }
	public float MaxValue { get; set; }
	public float Step { get; set; }
	public string? MeasurementUnit { get; set; }
}