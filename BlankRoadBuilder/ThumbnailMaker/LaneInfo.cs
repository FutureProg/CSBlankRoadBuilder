using BlankRoadBuilder.Domain.Options;

using System;
using System.Linq;
using System.Xml.Serialization;

namespace BlankRoadBuilder.ThumbnailMaker;
public partial class LaneInfo
{
	public LaneType Type { get; set; }
	public LaneDirection Direction { get; set; }
	public LaneDecoration Decorations { get; set; }
	public ParkingAngle ParkingAngle { get; set; }
	public PropAngle PropAngle { get; set; }
	public FillerPadding FillerPadding { get; set; }

	public float? Elevation { get; set; }
	public float? CustomWidth { get; set; }
	public float? SpeedLimit { get; set; }

	[XmlIgnore] public RoadInfo? Road { get; set; }
	[XmlIgnore] public float LaneWidth => (float)Math.Round((CustomWidth ?? DefaultLaneWidth()) + (Type == LaneType.Curb ? (Road?.BufferWidth ?? 0F) : 0F), 3);

	public float DefaultLaneWidth()
	{
		if (Type == LaneType.Empty)
			return 1F;

		if (Type == LaneType.Parking)
		{
			if (ParkingAngle == ParkingAngle.Horizontal)
				return ModOptions.LaneSizes.HorizontalParkingSize;

			if (ParkingAngle == ParkingAngle.Diagonal || ParkingAngle == ParkingAngle.InvertedDiagonal)
				return ModOptions.LaneSizes.DiagonalParkingSize;
		}

		return Type.GetValues().Max(x => ModOptions.LaneSizes[x]);
	}
}
