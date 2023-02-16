using BlankRoadBuilder.Domain;
using BlankRoadBuilder.ThumbnailMaker;

using System.Collections.Generic;
using System.Linq;

namespace BlankRoadBuilder.Util.Markings;
public static class MarkingsUtil
{
	public static MarkingsInfo GenerateMarkings(RoadInfo roadInfo)
	{
		var markingInfo = new MarkingsInfo();
		var currentFiller = (FillerMarking?)null;
		var currentLane = (LaneInfo?)null;

		foreach (var lane in roadInfo.Lanes.Where(x => !x.Tags.HasAnyFlag(LaneTag.StackedLane)))
		{
			var laneFiller = lane.Decorations & (LaneDecoration.Filler | LaneDecoration.Grass | LaneDecoration.Gravel | LaneDecoration.Pavement);

			if (laneFiller != 0)
			{
				if (currentFiller == null
					|| currentFiller.Type != laneFiller
					|| (laneFiller == LaneDecoration.Filler && (currentLane?.Type != lane.Type || currentLane?.Direction != lane.Direction))
					|| currentFiller.Elevation != lane.LaneElevation)
				{
					if (currentFiller != null)
					{
						markingInfo.Fillers.Add(currentFiller);
					}

					currentLane = lane;
					currentFiller = new FillerMarking
					{
						Type = laneFiller,
						Elevation = lane.LaneElevation,
						LeftPoint = new MarkingPoint(lane.LeftLane, lane)
					};
				}

				currentFiller.RightPoint = new MarkingPoint(lane, lane.RightLane);

				continue;
			}

			if (currentFiller != null)
			{
				markingInfo.Fillers.Add(currentFiller);
				currentFiller = null;
				currentLane = null;
			}
		}

		if (currentFiller != null)
		{
			markingInfo.Fillers.Add(currentFiller);
			currentFiller = null;
			currentLane = null;
		}

		foreach (var lane in roadInfo.Lanes.Where(x => !x.Tags.HasAnyFlag(LaneTag.StackedLane)))
		{
			var currentCategory = lane.GetVehicleCategory();

			foreach (var item in GenerateLaneMarkings(roadInfo, lane, currentCategory))
			{
				if (markingInfo.Fillers.Any(x =>
					(x.LeftPoint == item.Point && (x.LeftPoint.RightLane?.FillerPadding.HasFlag(FillerPadding.Left) ?? false)) ||
					(x.RightPoint == item.Point && (x.RightPoint.LeftLane?.FillerPadding.HasFlag(FillerPadding.Right) ?? false))))
				{
					continue;
				}

				if (!markingInfo.Lines.ContainsKey(item.Point) || markingInfo.Lines[item.Point].Marking < item.Marking)
				{
					markingInfo.Lines[item.Point] = item;
				}
			}
		}

		return markingInfo;
	}

	private static IEnumerable<LineMarking> GenerateLaneMarkings(RoadInfo roadInfo, LaneInfo lane, LaneVehicleCategory currentCategory)
	{
		var leftLane = lane.Direction == LaneDirection.Backwards ? lane.RightLane : lane.LeftLane;
		var rightLane = lane.Direction == LaneDirection.Backwards ? lane.LeftLane : lane.RightLane;

		var leftCategory = leftLane?.GetVehicleCategory() ?? LaneVehicleCategory.Unknown;
		var rightCategory = rightLane?.GetVehicleCategory() ?? LaneVehicleCategory.Unknown;

		if (currentCategory.HasAnyFlag(LaneVehicleCategory.Vehicle, LaneVehicleCategory.Bike, LaneVehicleCategory.Tram))
		{
			if (rightCategory != currentCategory && (rightCategory <= LaneVehicleCategory.Pedestrian || rightLane?.Direction != leftLane?.Direction))
			{
				yield return marking(true, GenericMarkingType.End);
			}

			if (leftCategory != currentCategory && leftCategory != LaneVehicleCategory.Parking)
			{
				yield return marking(false,
					currentCategory != LaneVehicleCategory.Vehicle ? GenericMarkingType.End
					: leftLane?.Direction == lane.Direction ? GenericMarkingType.Normal | GenericMarkingType.Hard : GenericMarkingType.Flipped | GenericMarkingType.End);
			}
		}

		if (currentCategory == LaneVehicleCategory.Parking)
		{
			if (leftCategory == LaneVehicleCategory.Vehicle)
			{
				yield return marking(false, GenericMarkingType.Parking);
			}

			if (rightCategory == LaneVehicleCategory.Vehicle)
			{
				yield return marking(true, GenericMarkingType.Parking);
			}
		}
		else
		{
			if (currentCategory == leftCategory && leftLane?.Direction != lane.Direction)
			{
				if (lane.Direction != LaneDirection.Backwards)
					yield return marking(false, GenericMarkingType.Flipped | (currentCategory == LaneVehicleCategory.Vehicle ? GenericMarkingType.Hard : (GenericMarkingType)(int)currentCategory));
				//	yield return marking(false, GenericMarkingType.Flipped | (currentCategory == LaneVehicleCategory.Vehicle ? (roadInfo.RoadType == RoadType.Highway ? GenericMarkingType.Hard : GenericMarkingType.End) : (GenericMarkingType)(int)currentCategory));
			}

			if (currentCategory == leftCategory && leftLane?.Direction == lane.Direction)
			{
				yield return marking(false, GenericMarkingType.Normal | (currentCategory == LaneVehicleCategory.Vehicle ? leftLane.Type.HasFlag(lane.Type) || lane.Type.HasFlag(leftLane.Type) ? GenericMarkingType.Soft : GenericMarkingType.Hard : (GenericMarkingType)(int)currentCategory));
			}
		}

		LineMarking marking(bool r, GenericMarkingType t) => new LineMarking
		{
			Marking = roadInfo.BufferWidth == 0F && (r ? rightLane : leftLane)?.Type == LaneType.Curb ? GenericMarkingType.None : t,
			Elevation = lane.LaneElevation,
			Point = r == (lane.Direction != LaneDirection.Backwards) ? new MarkingPoint(lane, lane.RightLane) : new MarkingPoint(lane.LeftLane, lane)
		};
	}
}
