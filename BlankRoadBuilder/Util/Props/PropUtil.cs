﻿namespace BlankRoadBuilder.Util.Props;
public class PropUtil
{
	public static PropTemplate GetProp(Prop prop)
	{
		return GetDefaultProp(prop);
	}

	private static PropTemplate GetDefaultProp(Prop prop)
	{
		return prop switch
		{
			Prop.TrafficLight01 => new PropTemplate("Traffic Light 01"),
			Prop.TrafficLight01Mirror => new PropTemplate("Traffic Light 01 Mirror"),
			Prop.TrafficLight02 => new PropTemplate("Traffic Light 02"),
			Prop.TrafficLight02Mirror => new PropTemplate("Traffic Light 02 Mirror"),
			Prop.TrafficLightPedestrian => new PropTemplate("Traffic Light Pedestrian"),
			Prop.BicycleSign => new PropTemplate("1779509676.R2 WF11-1 Bicycle Sign_Data"),
			Prop.RailwayCrossingAheadSign => new PropTemplate("1779509676.R2 W10-1 Railroad Crossing Sign_Data"),
			Prop.TrafficLightAheadSign => new PropTemplate("1779509676.R2 W3-3 Signal Ahead Sign_Data"),
			Prop.StopSign => new PropTemplate("Stop Sign"),
			Prop.RailwayCrossingVeryLong => new PropTemplate("Railway Crossing Very Long"),
			Prop.RailwayCrossingLong => new PropTemplate("Railway Crossing Long"),
			Prop.RailwayCrossingMedium => new PropTemplate("Railway Crossing Medium"),
			Prop.RailwayCrossingShort => new PropTemplate("Railway Crossing Short"),
			Prop.BicycleLaneDecal => new PropTemplate("Bike Lane"),
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
			Prop.Tree => new PropTemplate("mp9-YoungLinden", true),
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
			_ => new PropTemplate(string.Empty),
		};
	}
}