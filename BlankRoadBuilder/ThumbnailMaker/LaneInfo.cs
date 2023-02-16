using BlankRoadBuilder.Domain.Options;

using System.Linq;

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

	public float DefaultLaneWidth()
	{
		if (Type == LaneType.Empty)
			return 1F;

		if (Type == LaneType.Parking)
		{
			if (ParkingAngle == ParkingAngle.Horizontal)
				return ModOptions.LaneSizes.HorizontalParkingSize;

			if (ParkingAngle is ParkingAngle.Diagonal or ParkingAngle.InvertedDiagonal)
				return ModOptions.LaneSizes.DiagonalParkingSize;
		}

		return Type.GetValues().Max(x => ModOptions.LaneSizes[x]);
	}
}
