using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlankRoadBuilder.Util.Props;
public class PropUtil
{
	public static PropTemplate GetProp(Prop prop)
	{
		return GetDefaultProp(prop);
	}

	private static PropTemplate GetDefaultProp(Prop prop)
	{
		switch (prop)
		{
			case Prop.TrafficLight01: return new PropTemplate("Traffic Light 01");
			case Prop.TrafficLight01Mirror: return new PropTemplate("Traffic Light 01 Mirror");
			case Prop.TrafficLight02: return new PropTemplate("Traffic Light 02");
			case Prop.TrafficLight02Mirror: return new PropTemplate("Traffic Light 02 Mirror");
			case Prop.TrafficLightPedestrian: return new PropTemplate("Traffic Light Pedestrian");
			case Prop.BicycleSign: return new PropTemplate("1779509676.R2 WF11-1 Bicycle Sign_Data");
			case Prop.RailwayCrossingAheadSign: return new PropTemplate("1779509676.R2 W10-1 Railroad Crossing Sign_Data");
			case Prop.TrafficLightAheadSign: return new PropTemplate("1779509676.R2 W3-3 Signal Ahead Sign_Data");
			case Prop.StopSign: return new PropTemplate("Stop Sign");
			case Prop.RailwayCrossingVeryLong: return new PropTemplate("Railway Crossing Very Long");
			case Prop.RailwayCrossingLong: return new PropTemplate("Railway Crossing Long");
			case Prop.RailwayCrossingMedium: return new PropTemplate("Railway Crossing Medium");
			case Prop.RailwayCrossingShort: return new PropTemplate("Railway Crossing Short");
			case Prop.BicycleLaneDecal: return new PropTemplate("Bike Lane");
			case Prop.BusLaneDecal: return new PropTemplate("Bus Lane");
			case Prop.ArrowForward: return new PropTemplate("Road Arrow F");
			case Prop.ArrowLeft: return new PropTemplate("Road Arrow L");
			case Prop.ArrowRight: return new PropTemplate("Road Arrow R");
			case Prop.ArrowLeftRight: return new PropTemplate("Road Arrow LR");
			case Prop.ArrowForwardLeft: return new PropTemplate("Road Arrow LF");
			case Prop.ArrowForwardRight: return new PropTemplate("Road Arrow FR");
			case Prop.ArrowForwardLeftRight: return new PropTemplate("Road Arrow LFR");
			case Prop.BicycleParking: return new PropTemplate("bicycle_stand");
			case Prop.TrashBin: return new PropTemplate("Park Trashbin 01");
			case Prop.StreetAd: return new PropTemplate("904031558.Street Ads 01_Data");
			case Prop.SingleStreetLight: return new PropTemplate("Toll Road Light Single");
			case Prop.DoubleStreetLight: return new PropTemplate("Toll Road Light Double");
			case Prop.Bench: return new PropTemplate("Bench 01");
			case Prop.Hedge: return new PropTemplate("Plant Pot 06");
			case Prop.FlowerPot: return new PropTemplate("Flowerpot 04");
			case Prop.Bollard: return new PropTemplate("1650964670.Bollard A 05_Data");
			case Prop.Grass: return new PropTemplate("Roof Vegetation 01");
			case Prop.Tree: return new PropTemplate("mp9-YoungLinden", true);
			case Prop.TreePlanter: return new PropTemplate("2086553476.Tree Planter 03 1m_Data");
			case Prop.TramPole: return new PropTemplate("Tram Pole Side");
			case Prop.TramSidePole: return new PropTemplate("Tram Pole Wide Side");
			case Prop.TramCenterPole: return new PropTemplate("Tram Pole Center");
			case Prop.ParkingMeter: return new PropTemplate("Parking Meter");
			case Prop.BusStopLarge: return new PropTemplate("Bus Stop Large");
			case Prop.BusStopSmall: return new PropTemplate("Bus Stop Small");
			case Prop.TramStopLarge: return new PropTemplate("Tram Stop");
			case Prop.TramStopSmall: return new PropTemplate("Tram Stop Sign");
			case Prop.TrolleyStopLarge: return new PropTemplate("Trolleybus Stop");
			case Prop.TrolleyStopSmall: return new PropTemplate("Sightseeing Bus Stop Small");

			default: return null;
		}
	}
}