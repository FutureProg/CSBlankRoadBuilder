using BlankRoadBuilder.Domain;
using BlankRoadBuilder.Util;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace BlankRoadBuilder.ThumbnailMaker;

public partial class LaneInfo
{
	[XmlIgnore, CloneIgnore] public float Position { get; set; }
	[XmlIgnore, CloneIgnore] public LaneTag Tags { get; set; } = LaneTag.Asphalt;
	[XmlIgnore, CloneIgnore] public LaneInfo? LeftLane { get; set; }
	[XmlIgnore, CloneIgnore] public LaneInfo? RightLane { get; set; }
	[XmlIgnore, CloneIgnore] public TrafficLight TrafficLight { get; set; }
	[XmlIgnore, CloneIgnore] public List<NetInfo.Lane> NetLanes { get; set; } = new();
	[XmlIgnore, CloneIgnore] public float LaneElevation { get; set; }
	[XmlIgnore, CloneIgnore] public float SurfaceElevation { get; set; }
	[XmlIgnore, CloneIgnore] public RoadInfo? Road { get; set; }
	[XmlIgnore, CloneIgnore] public float LaneWidth => (float)Math.Round((CustomWidth ?? DefaultLaneWidth()) + (Type == LaneType.Curb ? (Road?.BufferWidth ?? 0F) : 0F), 3);

	public LaneVehicleCategory GetVehicleCategory()
	{
		if (Type is LaneType.Filler or LaneType.Curb)
			return LaneVehicleCategory.Filler;

		if (Type == LaneType.Bike)
			return LaneVehicleCategory.Bike;

		if (Type == LaneType.Pedestrian)
			return LaneVehicleCategory.Pedestrian;

		if (Type == LaneType.Parking)
			return LaneVehicleCategory.Parking;

		if (Type.HasFlag(LaneType.Tram))
			return LaneVehicleCategory.Tram;

		if (Type.HasAnyFlag(LaneType.Car, LaneType.Bus, LaneType.Trolley, LaneType.Emergency))
			return LaneVehicleCategory.Vehicle;

		return LaneVehicleCategory.Unknown;
	}

	public static List<LaneType> GetLaneTypes(LaneType laneType)
	{
		if (laneType == LaneType.Empty)
			return new List<LaneType> { LaneType.Empty };

		return Enum
			.GetValues(typeof(LaneType))
			.Cast<LaneType>()
			.Where(e => e != LaneType.Empty && laneType.HasFlag(e))
			.ToList();
	}

	public LaneInfo Duplicate(LaneType newLaneType, float positionDifference = 0F)
	{
		return new()
		{
			Type = newLaneType,
			Direction = Direction,
			CustomWidth = CustomWidth,
			Elevation = Elevation,
			Decorations = Decorations,
			ParkingAngle = ParkingAngle,
			LeftLane = LeftLane,
			RightLane = RightLane,
			SpeedLimit = SpeedLimit,
			NetLanes = NetLanes,
			FillerPadding = FillerPadding,
			PropAngle = PropAngle,
			TrafficLight = TrafficLight,
			Tags = Tags | LaneTag.StackedLane,
			Position = (float)Math.Round(Position + positionDifference, 3),
		};
	}

	public override string ToString()
	{
		var name = string.Empty;
		var types = Type.GetValues().Select(x => x.ToString());

		if (Type is > LaneType.Pedestrian and not LaneType.Parking)
			name += Direction switch { LaneDirection.Forward => "1WF ", LaneDirection.Backwards => "1WB ", _ => "2W " };

		name += types.Count() > 1 ? $"Shared {string.Join(" & ", types.ToArray())}" : types.First();

		name += $" at ({Position})";

		return name;
	}
}