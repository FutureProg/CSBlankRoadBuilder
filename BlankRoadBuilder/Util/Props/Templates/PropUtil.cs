using BlankRoadBuilder.Domain.Options;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;

using UnityEngine;

namespace BlankRoadBuilder.Util.Props.Templates;
public class PropUtil
{
	private static Dictionary<Prop, PropTemplate>? cachedTemplates;

	public static PropTemplate GetProp(Prop prop)
	{
		cachedTemplates ??= LoadTemplates();

		return cachedTemplates.ContainsKey(prop) ? cachedTemplates[prop] : GetDefaultProp(prop);
	}

	public static void SaveTemplate(Prop prop, PropTemplate? template)
	{
		cachedTemplates ??= LoadTemplates();

		cachedTemplates[prop] = template ?? GetDefaultProp(prop);

		SaveTemplates();
	}

	public static void ResetSettings()
	{
		cachedTemplates = new();

		SaveTemplates();
	}

	private static Dictionary<Prop, PropTemplate> LoadTemplates()
	{
		try
		{
			var path = Path.Combine(BlankRoadBuilderMod.BuilderFolder, "CustomProps.xml");

			if (!File.Exists(path))
				return new();

			var serializer = new XmlSerializer(typeof(SavedPropTemplate[]));

			using TextReader reader = new StreamReader(path);

			var templates = (SavedPropTemplate[])serializer.Deserialize(reader);

			return templates.ToDictionary(x => x.Prop, x => (PropTemplate)x);
		}
		catch (Exception ex) { Debug.LogException(ex); }

		return new();
	}

	private static void SaveTemplates()
	{
		var templates = GetTemplates();
		var path = Path.Combine(BlankRoadBuilderMod.BuilderFolder, "CustomProps.xml");
		var serializer = new XmlSerializer(typeof(SavedPropTemplate[]));

		using TextWriter writer = new StreamWriter(path);

		serializer.Serialize(writer, templates);
	}

	public static SavedPropTemplate[] GetTemplates()
	{
		var props = Enum.GetValues(typeof(Prop));
		var array = new SavedPropTemplate[props.Length];
		var ind = 0;

		foreach (Prop prop in props)
		{
			var template = GetProp(prop);
			var type = template.GetType();
			var properties = type
				.GetProperties(BindingFlags.Public | BindingFlags.Static)
				.Where(x => (Attribute.GetCustomAttribute(x, typeof(PropOptionAttribute)) as PropOptionAttribute) != null)
				.ToDictionary(x => x.Name, x => x.GetValue(template, null));

			array[ind++] = new SavedPropTemplate
			{
				Prop = prop,
				Type = type.Name,
				PropName = template.PropName,
				IsTree = template.IsTree,
				IsBuilding = template.IsBuilding,
				PropertyKeys = properties.Keys.ToArray(),
				PropertyValues = properties.Values.ToArray(),
			};
		}

		return array;
	}

	private static PropTemplate GetDefaultProp(Prop prop)
	{
		return prop switch
		{
			Prop.TrafficLight01 => new TrafficLightProp("Traffic Light 01"),
			Prop.TrafficLight01Mirror => new TrafficLightProp("Traffic Light 01 Mirror"),
			Prop.TrafficLight02 => new TrafficLightProp("Traffic Light 02"),
			Prop.TrafficLight02Mirror => new TrafficLightProp("Traffic Light 02 Mirror"),
			Prop.TrafficLightPedestrian => new TrafficLightProp("Traffic Light Pedestrian"),
			Prop.BicycleSign => new PropTemplate("1779509676.R2 WF11-1 Bicycle Sign_Data"),
			Prop.RailwayCrossingAheadSign => new PropTemplate("1779509676.R2 W10-1 Railroad Crossing Sign_Data"),
			Prop.TrafficLightAheadSign => new PropTemplate("1779509676.R2 W3-3 Signal Ahead Sign_Data"),
			Prop.YieldSign => new PropTemplate("1779508928.R2 R1-2 Yield Sign_Data"),
			Prop.StopSign => new PropTemplate("Stop Sign"),
			Prop.HighwaySign => new PropTemplate("Motorway Sign"),
			Prop.HighwayEndSign => new PropTemplate("1779509676.R2 W19-3 Fwy Ends Sign_Data"),
			Prop.RailwayCrossingVeryLong => new PropTemplate("Railway Crossing Very Long"),
			Prop.RailwayCrossingLong => new PropTemplate("Railway Crossing Long"),
			Prop.RailwayCrossingMedium => new PropTemplate("Railway Crossing Medium"),
			Prop.RailwayCrossingShort => new PropTemplate("Railway Crossing Short"),
			Prop.BicycleLaneDecal => new PropTemplate("Bike Lane"),
			Prop.BicycleLaneDecalSmall => new PropTemplate("Bike Lane Narrow"),
			Prop.FireHydrant => new PropTemplate("Fire Hydrant"),
			Prop.BusLaneDecal => new PropTemplate("Bus Lane"),
			Prop.ArrowForward => new PropTemplate("Road Arrow F"),
			Prop.ArrowLeft => new PropTemplate("Road Arrow L"),
			Prop.ArrowRight => new PropTemplate("Road Arrow R"),
			Prop.ArrowLeftRight => new PropTemplate("Road Arrow LR"),
			Prop.ArrowForwardLeft => new PropTemplate("Road Arrow LF"),
			Prop.ArrowForwardRight => new PropTemplate("Road Arrow FR"),
			Prop.ArrowForwardLeftRight => new PropTemplate("Road Arrow LFR"),
			Prop.BicycleParking => new PropTemplate("bicycle_stand"),
			Prop.TrashBin => new PropTemplate("Park Trashbin 01"),
			Prop.StreetAd => new PropTemplate("904031558.Street Ads 01_Data"),
			Prop.SingleStreetLight => new PropTemplate("Toll Road Light Single"),
			Prop.DoubleStreetLight => new PropTemplate("Toll Road Light Double"),
			Prop.Bench => new PropTemplate("Bench 01"),
			Prop.Hedge => new PropTemplate("Plant Pot 06"),
			Prop.FlowerPot => new PropTemplate("Flowerpot 04"),
			Prop.Bollard => new PropTemplate("1650964670.Bollard A 05_Data"),
			Prop.Grass => new PropTemplate("Roof Vegetation 01"),
			Prop.Tree => new PropTemplate("mp9-YoungLinden", isTree: true),
			Prop.TreePlanter => new PropTemplate("2086553476.Tree Planter 03 1m_Data"),
			Prop.TramPole => new PropTemplate("Tram Pole Side"),
			Prop.TramSidePole => new PropTemplate("Tram Pole Wide Side"),
			Prop.TramCenterPole => new PropTemplate("Tram Pole Center"),
			Prop.ParkingMeter => new PropTemplate("Parking Meter"),
			Prop.BusStopLarge => new PropTemplate("Bus Stop Large"),
			Prop.BusStopSmall => new PropTemplate("Bus Stop Small"),
			Prop.TramStopLarge => new PropTemplate("Tram Stop"),
			Prop.TramStopSmall => new PropTemplate("Tram Stop Sign"),
			Prop.TrolleyStopLarge => new PropTemplate("Trolleybus Stop"),
			Prop.TrolleyStopSmall => new PropTemplate("Sightseeing Bus Stop Small"),
			Prop.SpeedSign10 => new PropTemplate($"30 Speed Limit"),
			Prop.SpeedSign20 => new PropTemplate($"30 Speed Limit"),
			Prop.SpeedSign30 => new PropTemplate($"30 Speed Limit"),
			Prop.SpeedSign40 => new PropTemplate($"40 Speed Limit"),
			Prop.SpeedSign50 => new PropTemplate($"50 Speed Limit"),
			Prop.SpeedSign60 => new PropTemplate($"60 Speed Limit"),
			Prop.SpeedSign70 => new PropTemplate($"60 Speed Limit"),
			Prop.SpeedSign80 => new PropTemplate($"60 Speed Limit"),
			Prop.SpeedSign90 => new PropTemplate($"60 Speed Limit"),
			Prop.SpeedSign100 => new PropTemplate($"100 Speed Limit"),
			Prop.SpeedSign110 => new PropTemplate($"100 Speed Limit"),
			Prop.SpeedSign120 => new PropTemplate($"100 Speed Limit"),
			Prop.SpeedSign130 => new PropTemplate($"100 Speed Limit"),
			Prop.SpeedSign140 => new PropTemplate($"100 Speed Limit"),
			Prop.PillarBase => new PropTemplate($"760289402.R69 Middle 1c_Data", isBuilding: true),
			Prop.Pillar16 => new PropTemplate($"760276148.R69 Middle 2c_Data", isBuilding: true),
			Prop.Pillar24 => new PropTemplate($"760276468.R69 Middle 3c_Data", isBuilding: true),
			Prop.Pillar30 => new PropTemplate($"760277420.R69 Over 3c_Data", isBuilding: true),
			Prop.Pillar38 => new PropTemplate($"760278365.R69 Over 4c_Data", isBuilding: true),
			Prop.LampPost => new PropTemplate($"StreetLamp02"),
			Prop.Flowers => new PropTemplate($"2355034951.Wildflower_Yellow_Data", isTree: true),
			_ => new(""),
		};
	}
}