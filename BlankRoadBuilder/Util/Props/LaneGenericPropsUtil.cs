using AdaptiveRoads.Manager;

using BlankRoadBuilder.ThumbnailMaker;

using PrefabMetadata.Helpers;

using System;
using System.Collections.Generic;

using UnityEngine;

namespace BlankRoadBuilder.Util.Props;

public static partial class LanePropsUtil
{
	private static IEnumerable<NetLaneProps.Prop> GetWirePoleProps(LaneInfo lane, RoadInfo road)
	{
		if (!road.ContainsWiredLanes)
			yield break;

		getLaneTramInfo(lane, road, out var tramLanesAreNextToMedians, out var leftTram, out var rightTram);

		var angle = !leftTram ? 0 : 180;

		var position = 0F;//lane.Tags.HasFlag(LaneTag.Asphalt) ? 0 : lane.Position < 0 ? 1F : -1F;
		var verticalOffset = lane.Tags.HasFlag(LaneTag.Asphalt) ? -0.45F : -0.75F;

		PropInfo poleProp;

		if (!tramLanesAreNextToMedians && lane.Tags.HasFlag(LaneTag.WirePoleLane))
		{
			poleProp = Prop("Tram Pole Side");

			verticalOffset = lane.Tags.HasFlag(LaneTag.Asphalt) ? -0.1F : -0.4F;
		}
		else if (leftTram && rightTram)
		{
			if (lane.Width > 4F)
			{
				poleProp = Prop("Tram Pole Wide Side");

				position = -(float)Math.Round(Math.Max(0F, lane.Width - 4F) / 2F, 3);

				yield return pole(-1F);
				yield return pole(0F);
				yield return pole(1F, true);

				position *= -1;
				angle = 0;
			}
			else
			{
				poleProp = Prop("Tram Pole Center");
			}
		}
		else if (leftTram || rightTram)
		{
			var tramLane = leftTram ? lane.LeftLane : lane.RightLane;
			var nextLane = leftTram ? tramLane?.LeftLane : tramLane?.RightLane;

			if (nextLane?.Type.HasAnyFlag(LaneType.Filler, LaneType.Curb, LaneType.Pedestrian) ?? false)
			{
				getLaneTramInfo(nextLane, road, out _, out var l, out var r);

				if ((l && r) || (l && rightTram))
					yield break;
			}

			if (rightTram && nextLane != null && ((nextLane.Type & LaneType.Tram) != 0))
			{
				var nextNextLane = leftTram ? nextLane?.LeftLane : nextLane?.RightLane;

				if (nextNextLane?.Type.HasAnyFlag(LaneType.Filler, LaneType.Curb, LaneType.Pedestrian) ?? false)
					yield break;

			}

			poleProp = Prop("Tram Pole Wide Side");
			position = nextLane != null && nextLane.Type.HasFlag(LaneType.Tram)
				? (float)Math.Round(Math.Max(0F, lane.Tags.HasFlag(LaneTag.Sidewalk) ? (0.1 + lane.Width / 2F + road.BufferWidth) : ((lane.Width - 1F) / 2F)), 3) * (leftTram ? -1F : 1F)
				: (float)Math.Round(Math.Max(0F, lane.Width - 4F) / 2F, 3) * (leftTram ? -1F : 1F);
		}
		else
		{
			yield break;
		}

		yield return pole(-1F);
		yield return pole(0F);
		yield return pole(1F, true);

		NetLaneProps.Prop pole(float segment, bool end = false)
		{
			return new NetLaneProps.Prop
			{
				m_prop = poleProp,
				m_finalProp = poleProp,
				m_endFlagsForbidden = end ? NetNode.Flags.Middle : NetNode.Flags.None,
				m_cornerAngle = 0.75F,
				m_minLength = segment != 0 || verticalOffset == -0.2F ? 0F : 22F,
				m_repeatDistance = 0F,
				m_segmentOffset = segment,
				m_angle = angle,
				m_probability = 100,
				m_position = new Vector3(position, verticalOffset, 0)
			}.Extend(prop => new NetInfoExtionsion.LaneProp(prop)
			{
				SegmentFlags = new NetInfoExtionsion.SegmentInfoFlags { Forbidden = RoadUtils.S_RemoveTramSupports }
			});
		}

		static void getLaneTramInfo(LaneInfo lane, RoadInfo road, out bool tramLanesAreNextToMedians, out bool leftTram, out bool rightTram)
		{
			tramLanesAreNextToMedians = road.WiredLanesAreNextToMedians /*&& road.AsphaltWidth > 10F*/;
			leftTram = tramLanesAreNextToMedians && lane.LeftLane != null && ((lane.LeftLane.Type & (LaneType.Tram | LaneType.Trolley)) != 0);
			rightTram = tramLanesAreNextToMedians && lane.RightLane != null && ((lane.RightLane.Type & (LaneType.Tram | LaneType.Trolley)) != 0);
		}
	}

	private static IEnumerable<NetLaneProps.Prop> GetParkingProps(LaneInfo lane)
	{
		if (lane.LeftLane?.Type == LaneType.Parking)
		{
			var parkingMeter = Prop("Parking Meter");

			yield return new NetLaneProps.Prop
			{
				m_prop = parkingMeter,
				m_finalProp = parkingMeter,
				m_angle = 90,
				m_minLength = 22,
				m_segmentOffset = -0.65F,
				m_probability = 100,
				m_position = new Vector3((float)Math.Round((lane.Width - (lane.Tags.HasFlag(LaneTag.Sidewalk) ? 1F : 1.4F)) / -2, 3), 0, 4F),
			}.Extend(prop => new NetInfoExtionsion.LaneProp(prop)
			{
				SegmentFlags = new NetInfoExtionsion.SegmentInfoFlags
				{ Forbidden = RoadUtils.S_RemoveRoadClutter }
			});
		}

		if (lane.RightLane?.Type == LaneType.Parking)
		{
			var parkingMeter = Prop("Parking Meter");

			yield return new NetLaneProps.Prop
			{
				m_prop = parkingMeter,
				m_finalProp = parkingMeter,
				m_angle = 270,
				m_minLength = 22,
				m_segmentOffset = 0.65F,
				m_probability = 100,
				m_position = new Vector3((float)Math.Round((lane.Width - (lane.Tags.HasFlag(LaneTag.Sidewalk) ? 1F : 1.4F)) / 2, 3), 0, -4F),
			}.Extend(prop => new NetInfoExtionsion.LaneProp(prop)
			{
				SegmentFlags = new NetInfoExtionsion.SegmentInfoFlags
				{ Forbidden = RoadUtils.S_RemoveRoadClutter }
			});
		}
	}

	private static IEnumerable<NetLaneProps.Prop> GetLights(LaneInfo lane, RoadInfo road)
	{
		PropInfo lightProp;
		var xPos = 0F;

		if (lane.Tags.HasFlag(LaneTag.CenterMedian))
		{
			lightProp = Prop("Toll Road Light Double");//Prop("Avenue Light");
		}
		else if (lane.Type == LaneType.Curb)
		{
			if (road.ContainsCenterMedian && road.AsphaltWidth < 30F)
				yield break;

			if (!road.ContainsCenterMedian && road.AsphaltWidth < 20F && lane.Position < 0)
				yield break;

			xPos = -lane.Width / 2 + 0.5F;
			lightProp = Prop("Toll Road Light Single");//Prop("New Street Light Avenue");
		}
		else
			yield break;

		yield return getLight(road.ContainsWiredLanes ? 2F : 0F);

		for (var i = 24; i <= 24 * 4; i += 24)
		{
			yield return getLight(i);
			yield return getLight(-i);
		}

		NetLaneProps.Prop getLight(float position) => new NetLaneProps.Prop
		{
			m_prop = lightProp,
			m_finalProp = lightProp,
			m_minLength = Math.Abs(position) * 2F + 10F,
			m_probability = 100,
			m_position = new Vector3(xPos, 0, position)
		}.Extend(prop => new NetInfoExtionsion.LaneProp(prop)
		{
			LaneFlags = new NetInfoExtionsion.LaneInfoFlags
			{ Forbidden = RoadUtils.L_RemoveStreetLights }
		}).ToggleForwardBackward(lane.Position < 0 && lane.Type == LaneType.Curb);
	}
}
