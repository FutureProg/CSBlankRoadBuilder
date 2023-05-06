using BlankRoadBuilder.Domain.Options;
using BlankRoadBuilder.Util.Props.Templates;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;

using UnityEngine;

namespace BlankRoadBuilder.Util.Props;
public class PropUtil
{
	private static Dictionary<Prop, PropTemplate>? cachedTemplates;

	public static bool SavePaused { get; set; }

	public static PropTemplate GetProp(Prop prop)
	{
		cachedTemplates ??= LoadTemplates();

		return cachedTemplates.ContainsKey(prop) ? cachedTemplates[prop] : GetDefaultProp(prop);
	}

	public static void SaveTemplate(Prop prop, PropTemplate? template)
	{
		if (SavePaused)
			return;

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
		var list = new List<SavedPropTemplate>();

		foreach (Prop prop in props)
		{
			var template = GetProp(prop);

			if (template.Category == PropCategory.None)
				continue;

			var type = template.GetType();
			var properties = type
				.GetProperties(BindingFlags.Public | BindingFlags.Instance)
				.Where(x => (Attribute.GetCustomAttribute(x, typeof(PropOptionAttribute)) as PropOptionAttribute) != null)
				.Where(x => (Attribute.GetCustomAttribute(x, typeof(XmlIgnoreAttribute)) as XmlIgnoreAttribute) == null)
				.ToDictionary(x => x.Name, x => x.GetValue(template, null));

			list.Add(new SavedPropTemplate
			{
				Prop = prop,
				Type = type.Name,
				PropName = template.PropName,
				IsTree = template.IsTree,
				IsBuilding = template.IsBuilding,
				PropertyKeys = properties.Keys.ToArray(),
				PropertyValues = properties.Values.Select(TranslateProperty).ToArray(),
			});

			static object TranslateProperty(object value)
			{
				if (value is ICustomPropProperty customPropProperty)
					return customPropProperty.AsPrimitive();

				if (value is Enum)
					return Convert.ToInt32(value);

				return value;
			}
		}

		return list.ToArray();
	}

	public static PropTemplate GetDefaultProp(Prop prop)
	{
		return prop switch
		{
			Prop.TrafficLight01 => new TrafficLightProp("Traffic Light 01"),
			Prop.TrafficLight01Mirror => new TrafficLightProp("Traffic Light 01 Mirror"),
			Prop.TrafficLight02 => new TrafficLightProp("Traffic Light 02"),
			Prop.TrafficLight02Mirror => new TrafficLightProp("Traffic Light 02 Mirror"),
			Prop.TrafficLightPedestrian => new TrafficLightProp("Traffic Light Pedestrian"),
			Prop.BicycleSign => new RoadSignProp("1779509676.R2 WF11-1 Bicycle Sign_Data") { StartAngle = 70F, SegmentSnapping = PropSegmentSnapping.SnapToBack },
			Prop.RailwayCrossingAheadSign => new RoadSignProp("1779509676.R2 W10-1 Railroad Crossing Sign_Data") { StartAngle = 70F, SegmentSnapping = PropSegmentSnapping.SnapHalfwayToBack },
			Prop.TrafficLightAheadSign => new RoadSignProp("1779509676.R2 W3-3 Signal Ahead Sign_Data") { StartAngle = 70F, SegmentSnapping = PropSegmentSnapping.SnapHalfwayToBack },
			Prop.PrioritySign => new RoadSignProp("") { StartAngle = 90F, SegmentSnapping = PropSegmentSnapping.SnapToFront },
			Prop.YieldSign => new RoadSignProp("1779508928.R2 R1-2 Yield Sign_Data") { StartAngle = 90F, SegmentSnapping = PropSegmentSnapping.SnapToFront },
			Prop.StopSign => new RoadSignProp("Stop Sign") { SegmentSnapping = PropSegmentSnapping.SnapToFront },
			Prop.HighwaySign => new RoadSignProp("Motorway Sign") { StartAngle = -20, SegmentSnapping = PropSegmentSnapping.SnapToBack, RelativePosition = new Domain.CustomVector3(0, 0, 10) },
			Prop.HighwayEndSign => new RoadSignProp("1779509676.R2 W19-3 Fwy Ends Sign_Data") { StartAngle = 70, RelativePosition = new Domain.CustomVector3(0, 0, 10) },
			Prop.DoNotEnterSign => new RoadSignProp("1779508928.R2 R5-1 Do Not Enter Sign_Data") { StartAngle = 250, SegmentSnapping = PropSegmentSnapping.SnapToFront },
			Prop.EndOfOneWaySign => new RoadSignProp("1779508928.R2 R6-7 End One Way Sign_Data") { StartAngle = 70, SegmentSnapping = PropSegmentSnapping.SnapHalfwayToFront },
			Prop.PedestrianCrossingSign => new RoadSignProp("1779509676.R2 W11-2 Pedestrian Sign_Data") { StartAngle = 90, SegmentSnapping = PropSegmentSnapping.SnapToFront, RelativePosition = new(0,0,-5) },
			Prop.RailwayCrossingVeryLong => new LevelCrossingProp("Railway Crossing Very Long"),
			Prop.RailwayCrossingLong => new LevelCrossingProp("Railway Crossing Long"),
			Prop.RailwayCrossingMedium => new LevelCrossingProp("Railway Crossing Medium"),
			Prop.RailwayCrossingShort => new LevelCrossingProp("Railway Crossing Short"),
			Prop.BicycleLaneDecal => new LaneDecalProp("Bike Lane") { StartAngle = 180, SegmentSnapping = PropSegmentSnapping.SnapToBack, RelativePosition = new(0, 0, 5), OnlyShowAtIntersections = true },
			Prop.BicycleLaneDecalSmall => new LaneDecalProp("Bike Lane Narrow") { StartAngle = 180, SegmentSnapping = PropSegmentSnapping.SnapToBack, RelativePosition = new(0, 0, 5), OnlyShowAtIntersections = true },
			Prop.FireHydrant => new DecorationProp("Fire Hydrant") { StartAngle = 90, Chance = 70, SegmentSnapping = PropSegmentSnapping.SnapHalfwayToFront, RelativePosition = new(0, 0, 0.15F) },
			Prop.BusLaneDecal => new LaneDecalProp("Bus Lane") { StartAngle = 180, SegmentSnapping = PropSegmentSnapping.SnapToBack, RelativePosition = new(0, 0, 5), OnlyShowAtIntersections = true },
			Prop.TrolleyLaneDecal => new LaneDecalProp("Bus Only Lane") { HideOnSharedLanes = true, StartAngle = 180, SegmentSnapping = PropSegmentSnapping.SnapToBack, RelativePosition = new(0, 0, 5), OnlyShowAtIntersections = true },
			Prop.TramLaneDecal => new LaneDecalProp(string.Empty) { StartAngle = 180, SegmentSnapping = PropSegmentSnapping.SnapToBack, RelativePosition = new(0, 0, 5), OnlyShowAtIntersections = true },
			Prop.ArrowForward => new ArrowProp("Road Arrow F") { StartAngle = 180, RelativePosition = new(0, 0, -5) },
			Prop.ArrowLeft => new ArrowProp("Road Arrow L") { StartAngle = 180, RelativePosition = new(0, 0, -5) },
			Prop.ArrowRight => new ArrowProp("Road Arrow R") { StartAngle = 180, RelativePosition = new(0, 0, -5) },
			Prop.ArrowLeftRight => new ArrowProp("Road Arrow LR") { StartAngle = 180, RelativePosition = new(0, 0, -5) },
			Prop.ArrowForwardLeft => new ArrowProp("Road Arrow LF") { StartAngle = 180, RelativePosition = new(0, 0, -5) },
			Prop.ArrowForwardRight => new ArrowProp("Road Arrow FR") { StartAngle = 180, RelativePosition = new(0, 0, -5) },
			Prop.ArrowForwardLeftRight => new ArrowProp("Road Arrow LFR") { StartAngle = 180, RelativePosition = new(0, 0, -5) },
			Prop.BicycleParking => new DecorationProp("bicycle_stand") { StartAngle = 90, RepeatDistance = 30 },
			Prop.TrashBin => new DecorationProp("Park Trashbin 01") { StartAngle = 90, RepeatDistance = 15, RelativePosition = new(0, 0, 1.5F) },
			Prop.StreetAd => new DecorationProp("904031558.Street Ads 01_Data") { RepeatDistance = 25 },
			Prop.TunnelLight => new LightProp("Tunnel Light Small Road") { RepeatDistance = 12F, RelativePosition = new Vector3(0, 5, 0), StartAngle = 90 },
			Prop.TunnelExitProp => new DecorationProp("2244948378.Tunnel Emergency Doors_Data") { StartAngle = 180, Chance = 95, RepeatDistance = 32 },
			Prop.TunnelVent => new DecorationProp("2244948378.Tunnel Ventilation_Data") { RepeatDistance = 24F, RelativePosition = new Vector3(0, 4.5F, 0) },
			Prop.SingleStreetLight => new LightProp("Toll Road Light Single") { RepeatDistance = 24F },
			Prop.DoubleStreetLight => new LightProp("Toll Road Light Double") { RepeatDistance = 24F },
			Prop.Bench => new DecorationProp("Bench 01") { StartAngle = 90, RepeatDistance = 15 },
			Prop.Hedge => new DecorationProp("Plant Pot 06") { RepeatDistance = 3.25F },
			Prop.FlowerPot => new DecorationProp("Flowerpot 04") { StartAngle = 90, RepeatDistance = 5 },
			Prop.Bollard => new DecorationProp("1650964670.Bollard A 05_Data") { RepeatDistance = 2, RelativePosition = new(0, 0.01F, 0) },
			Prop.Grass => new DecorationProp("Roof Vegetation 01") { RepeatDistance = 1.25F, RelativePosition = new(0, -0.4F, 0) },
			Prop.Tree => new DecorationProp("mp9-YoungLinden") { UseTree = true, RepeatDistance = 12, OnlyOnGround = true },
			Prop.TreePlanter => new DecorationProp("2086553476.Tree Planter 03 1m_Data") { RepeatDistance = 12, OnlyOnGround = true },
			Prop.TramPole => new TramPoleProp("Tram Pole Side"),
			Prop.TramSidePole => new TramPoleProp("Tram Pole Wide Side"),
			Prop.TramCenterPole => new TramPoleProp("Tram Pole Center"),
			Prop.ParkingMeter => new DecorationProp("Parking Meter") { StartAngle = 90, SegmentSnapping = PropSegmentSnapping.SnapHalfwayToBack, RelativePosition = new(0, 0, 4) },
			Prop.BusStopLarge => new StopProp("Bus Stop Large") { StartAngle = 90, RelativePosition = new(0, 0, 3) },
			Prop.BusStopSmall => new StopProp("Bus Stop Small") { StartAngle = 90, RelativePosition = new(0, 0, 3) },
			Prop.TramStopLarge => new StopProp("Tram Stop") { StartAngle = 90, RelativePosition = new(0, 0, -3) },
			Prop.TramStopSmall => new StopProp("Tram Stop Sign") { StartAngle = 90, RelativePosition = new(0, 0, -3) },
			Prop.TrolleyStopLarge => new StopProp("Trolleybus Stop") { StartAngle = 90, RelativePosition = new(0, 0, -2) },
			Prop.TrolleyStopSmall => new StopProp("Sightseeing Bus Stop Small") { StartAngle = 90, RelativePosition = new(0, 0, -2) },
			Prop.SpeedSign10 => new SpeedSignProp($"30 Speed Limit") { StartAngle = 340, SegmentSnapping = PropSegmentSnapping.SnapHalfwayToBack, RelativePosition = new(0, 0, -1) },
			Prop.SpeedSign20 => new SpeedSignProp($"30 Speed Limit") { StartAngle = 340, SegmentSnapping = PropSegmentSnapping.SnapHalfwayToBack, RelativePosition = new(0, 0, -1) },
			Prop.SpeedSign30 => new SpeedSignProp($"30 Speed Limit") { StartAngle = 340, SegmentSnapping = PropSegmentSnapping.SnapHalfwayToBack, RelativePosition = new(0, 0, -1) },
			Prop.SpeedSign40 => new SpeedSignProp($"40 Speed Limit") { StartAngle = 340, SegmentSnapping = PropSegmentSnapping.SnapHalfwayToBack, RelativePosition = new(0, 0, -1) },
			Prop.SpeedSign50 => new SpeedSignProp($"50 Speed Limit") { StartAngle = 340, SegmentSnapping = PropSegmentSnapping.SnapHalfwayToBack, RelativePosition = new(0, 0, -1) },
			Prop.SpeedSign60 => new SpeedSignProp($"60 Speed Limit") { StartAngle = 340, SegmentSnapping = PropSegmentSnapping.SnapHalfwayToBack, RelativePosition = new(0, 0, -1) },
			Prop.SpeedSign70 => new SpeedSignProp($"60 Speed Limit") { StartAngle = 340, SegmentSnapping = PropSegmentSnapping.SnapHalfwayToBack, RelativePosition = new(0, 0, -1) },
			Prop.SpeedSign80 => new SpeedSignProp($"60 Speed Limit") { StartAngle = 340, SegmentSnapping = PropSegmentSnapping.SnapHalfwayToBack, RelativePosition = new(0, 0, -1) },
			Prop.SpeedSign90 => new SpeedSignProp($"60 Speed Limit") { StartAngle = 340, SegmentSnapping = PropSegmentSnapping.SnapHalfwayToBack, RelativePosition = new(0, 0, -1) },
			Prop.SpeedSign100 => new SpeedSignProp($"100 Speed Limit") { StartAngle = 340, SegmentSnapping = PropSegmentSnapping.SnapHalfwayToBack, RelativePosition = new(0, 0, -1) },
			Prop.SpeedSign110 => new SpeedSignProp($"100 Speed Limit") { StartAngle = 340, SegmentSnapping = PropSegmentSnapping.SnapHalfwayToBack, RelativePosition = new(0, 0, -1) },
			Prop.SpeedSign120 => new SpeedSignProp($"100 Speed Limit") { StartAngle = 340, SegmentSnapping = PropSegmentSnapping.SnapHalfwayToBack, RelativePosition = new(0, 0, -1) },
			Prop.SpeedSign130 => new SpeedSignProp($"100 Speed Limit") { StartAngle = 340, SegmentSnapping = PropSegmentSnapping.SnapHalfwayToBack, RelativePosition = new(0, 0, -1) },
			Prop.SpeedSign140 => new SpeedSignProp($"100 Speed Limit") { StartAngle = 340, SegmentSnapping = PropSegmentSnapping.SnapHalfwayToBack, RelativePosition = new(0, 0, -1) },
			Prop.PillarSmall => new BridgePillarProp($"760289402.R69 Middle 1c_Data") { PillarOffset = 0.7F },
			Prop.Pillar16m => new BridgePillarProp($"760276148.R69 Middle 2c_Data") { PillarOffset = 0.7F },
			Prop.Pillar24m => new BridgePillarProp($"760276468.R69 Middle 3c_Data") { PillarOffset = 0.7F },
			Prop.Pillar30m => new BridgePillarProp($"760277420.R69 Over 3c_Data") { PillarOffset = 0.7F },
			Prop.Pillar38m => new BridgePillarProp($"760278365.R69 Over 4c_Data") { PillarOffset = 0.7F },
			Prop.LampPost => new LightProp($"StreetLamp02") { RepeatDistance = 12F },
			Prop.Flowers => new DecorationProp($"2355034951.Wildflower_Yellow_Data") { UseTree = true, OnlyOnGround = true, Chance = 85, RepeatDistance = 1.25F },
			_ => new(""),
		};
	}
}