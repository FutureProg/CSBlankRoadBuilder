using BlankRoadBuilder.ThumbnailMaker;

namespace BlankRoadBuilder.Domain;

public struct StopsInfo
{
	public VehicleInfo.VehicleType StopType { get; set; }
	public Side BusSide { get; set; }
	public Side TramSide { get; set; }
	public Side TrolleySide { get; set; }

	public VehicleInfo.VehicleType LeftStopType { get; set; }
	public Side LeftBusSide { get; set; }
	public Side LeftTramSide { get; set; }
	public Side LeftTrolleySide { get; set; }

	public VehicleInfo.VehicleType RightStopType { get; set; }
	public Side RightBusSide { get; set; }
	public Side RightTramSide { get; set; }
	public Side RightTrolleySide { get; set; }

	public LaneInfo? CanStopAt { get; set; }
	public float Distance { get; internal set; }

	public StopsInfo AsLeft()
	{
		return new StopsInfo
		{
			StopType = LeftStopType,
			BusSide = LeftBusSide,
			TramSide = LeftTramSide,
			TrolleySide = LeftTrolleySide,
		};
	}

	public StopsInfo AsRight()
	{
		return new StopsInfo
		{
			StopType = RightStopType,
			BusSide = RightBusSide,
			TramSide = RightTramSide,
			TrolleySide = RightTrolleySide,
		};
	}

	public enum Side
	{
		Left,
		Right,
		Both
	}
}