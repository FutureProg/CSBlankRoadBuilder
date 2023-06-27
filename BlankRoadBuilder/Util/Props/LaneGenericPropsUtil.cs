using AdaptiveRoads.Manager;

using BlankRoadBuilder.Domain;
using BlankRoadBuilder.Domain.Options;
using BlankRoadBuilder.ThumbnailMaker;
using BlankRoadBuilder.Util.Props.Templates;

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using static AdaptiveRoads.Manager.NetInfoExtionsion;

namespace BlankRoadBuilder.Util.Props;

public partial class LanePropsUtil
{
	private IEnumerable<NetLaneProps.Prop> GetWirePoleProps()
	{
		if (!Road.ContainsWiredLanes)
		{
			yield break;
		}

		GetLaneTramInfo(Lane, Road, out var tramLanesAreNextToMedians, out var leftTram, out var rightTram);

		var angle = !leftTram ? 0 : 180;

		var position = 0F;//lane.Tags.HasFlag(LaneTag.Asphalt) ? 0 : lane.Position < 0 ? 1F : -1F;
		var verticalOffset = Lane.Tags.HasFlag(LaneTag.Asphalt) ? -0.45F : -0.75F;

		PropTemplate poleProp;

		if (!tramLanesAreNextToMedians && Lane.Tags.HasFlag(LaneTag.WirePoleLane))
		{
			poleProp = GetProp(Prop.TramPole);

			verticalOffset = Lane.Tags.HasFlag(LaneTag.Asphalt) ? -0.1F : -0.4F;
		}
		else if (leftTram && rightTram)
		{
			if (Lane.LaneWidth > 4F)
			{
				poleProp = GetProp(Prop.TramSidePole);

				position = -(float)Math.Round(Math.Max(0F, Lane.LaneWidth - 4F) / 2F, 3);

				yield return pole(-1F);
				yield return pole(0F);
				yield return pole(1F, true);

				position *= -1;
				angle = 0;
			}
			else
			{
				poleProp = GetProp(Prop.TramCenterPole);
			}
		}
		else if (leftTram || rightTram)
		{
			var tramLane = leftTram ? Lane.LeftLane : Lane.RightLane;
			var nextLane = leftTram ? tramLane?.LeftLane : tramLane?.RightLane;

			if (nextLane?.Type.HasAnyFlag(LaneType.Filler, LaneType.Curb, LaneType.Pedestrian) ?? false)
			{
				GetLaneTramInfo(nextLane, Road, out _, out var l, out var r);

				if ((l && r) || (l && rightTram))
				{
					yield break;
				}
			}

			if (rightTram && nextLane != null && ((nextLane.Type & LaneType.Tram) != 0))
			{
				var nextNextLane = leftTram ? nextLane?.LeftLane : nextLane?.RightLane;

				if (nextNextLane?.Type.HasAnyFlag(LaneType.Filler, LaneType.Curb, LaneType.Pedestrian) ?? false)
				{
					yield break;
				}
			}

			poleProp = GetProp(Prop.TramSidePole);
			position = nextLane != null && nextLane.Type.HasFlag(LaneType.Tram)
				? (float)Math.Round(Math.Max(0F, (Lane.Type == LaneType.Curb ? 0.1F : 0F) + ((Lane.LaneWidth - 1F) / 2F)), 3) * (leftTram ? -1F : 1F)
				: (float)Math.Round(Math.Max(0F, Lane.LaneWidth - 4F) / 2F, 3) * (leftTram ? -1F : 1F);
		}
		else
		{
			yield break;
		}

		yield return pole(-1F);
		yield return pole(0F);
		yield return pole(1F, true);

		NetLaneProps.Prop pole(float segment, bool end = false) => new NetLaneProps.Prop
		{
			m_prop = poleProp,
			m_tree = poleProp,
			m_endFlagsForbidden = end ? NetNode.Flags.Middle : NetNode.Flags.None,
			m_cornerAngle = 0.75F,
			m_minLength = segment != 0 || verticalOffset == -0.2F ? 0F : 22F,
			m_repeatDistance = 0F,
			m_segmentOffset = segment,
			m_angle = angle,
			m_probability = 100,
			m_position = new Vector3(position, verticalOffset, 0) + PropPosition(poleProp)
		}.Extend(prop => new NetInfoExtionsion.LaneProp(prop)
		{
			SegmentFlags = new NetInfoExtionsion.SegmentInfoFlags { Forbidden = RoadUtils.Flags.S_RemoveTramSupports }
		});
	}

	public static void GetLaneTramInfo(LaneInfo lane, RoadInfo road, out bool tramLanesAreNextToMedians, out bool leftTram, out bool rightTram)
	{
		tramLanesAreNextToMedians = road.WiredLanesAreNextToMedians /*&& road.AsphaltWidth > 10F*/;
		leftTram = tramLanesAreNextToMedians && lane.LeftLane != null && ((lane.LeftLane.Type & (LaneType.Tram | LaneType.Trolley)) != 0);
		rightTram = tramLanesAreNextToMedians && lane.RightLane != null && ((lane.RightLane.Type & (LaneType.Tram | LaneType.Trolley)) != 0);
	}

	private IEnumerable<NetLaneProps.Prop> GetParkingProps()
	{
		if (Lane.GetLaneWidth(true) < 0.75F)
		{
			yield break;
		}

		if (Lane.LeftLane?.Type == LaneType.Parking)
		{
			var parkingMeter = GetProp(Prop.ParkingMeter);

			if (parkingMeter is DecorationProp decorationProp && decorationProp.OnlyOnGround && Elevation != ElevationType.Basic)
			{
				yield break;
			}

			yield return new NetLaneProps.Prop
			{
				m_prop = parkingMeter,
				m_tree = parkingMeter,
				m_angle = parkingMeter.Angle,
				m_minLength = 22,
				m_segmentOffset = parkingMeter.SegmentOffset,
				m_probability = parkingMeter.Probability,
				m_repeatDistance = parkingMeter.RepeatInterval,
				m_position = new Vector3((float)Math.Round((Lane.LaneWidth - (Lane.Tags.HasFlag(LaneTag.Sidewalk) ? 1.2F : 1.4F)) / -2, 3), 0, 0) + parkingMeter.Position,
			}.Extend(prop => new NetInfoExtionsion.LaneProp(prop)
			{
				SegmentFlags = new NetInfoExtionsion.SegmentInfoFlags
				{ Forbidden = ModOptions.HideRoadClutter ? NetSegmentExt.Flags.None : RoadUtils.Flags.S_RemoveRoadClutter, Required = ModOptions.HideRoadClutter ? RoadUtils.Flags.S_RemoveRoadClutter : NetSegmentExt.Flags.None }
			});
		}

		if (Lane.RightLane?.Type == LaneType.Parking)
		{
			var parkingMeter = GetProp(Prop.ParkingMeter);

			if (parkingMeter is DecorationProp decorationProp && decorationProp.OnlyOnGround && Elevation != ElevationType.Basic)
			{
				yield break;
			}

			yield return new NetLaneProps.Prop
			{
				m_prop = parkingMeter,
				m_tree = parkingMeter,
				m_angle = parkingMeter.Angle + 180,
				m_minLength = 22,
				m_segmentOffset = -parkingMeter.SegmentOffset,
				m_probability = parkingMeter.Probability,
				m_repeatDistance = parkingMeter.RepeatInterval,
				m_position = new Vector3((float)Math.Round((Lane.LaneWidth - (Lane.Tags.HasFlag(LaneTag.Sidewalk) ? 1F : 1.4F)) / 2, 3), 0, 0) - parkingMeter.Position,
			}.Extend(prop => new NetInfoExtionsion.LaneProp(prop)
			{
				SegmentFlags = new NetInfoExtionsion.SegmentInfoFlags
				{ Forbidden = ModOptions.HideRoadClutter ? NetSegmentExt.Flags.None : RoadUtils.Flags.S_RemoveRoadClutter, Required = ModOptions.HideRoadClutter ? RoadUtils.Flags.S_RemoveRoadClutter : NetSegmentExt.Flags.None }
			});
		}
	}

	private IEnumerable<NetLaneProps.Prop> GetLights()
	{
		if (Elevation == ElevationType.Tunnel || Road.Lanes.Any(x => x.Decorations.HasAnyFlag(LaneDecoration.LampPost, LaneDecoration.StreetLight, LaneDecoration.DoubleStreetLight)))
		{
			return new NetLaneProps.Prop[0];
		}

		if (Lane.Tags.HasFlag(LaneTag.CenterMedian))
		{
			if (!Lane.Decorations.HasFlag(LaneDecoration.DoubleStreetLight))
			{
				return GetStreetLights(LaneDecoration.DoubleStreetLight);
			}
		}
		else if (Lane.Type == LaneType.Curb)
		{
			if ((!Road.ContainsCenterMedian || Road.AsphaltWidth >= 30F)
				&& (Road.ContainsCenterMedian || Road.AsphaltWidth >= 20F || Lane.Position >= 0)
				&& !Lane.Decorations.HasFlag(LaneDecoration.StreetLight))
			{
				return GetStreetLights(LaneDecoration.StreetLight);
			}
		}

		return new NetLaneProps.Prop[0];
	}

	private IEnumerable<NetLaneProps.Prop> GetTunnelProps()
	{
		if (Elevation is ElevationType.Tunnel && Lane.Type is LaneType.Curb)
		{
			var ventProp = GetProp(Prop.TunnelVent);

			yield return new NetLaneProps.Prop
			{
				m_prop = ventProp,
				m_tree = ventProp,
				m_segmentOffset = ventProp.SegmentOffset,
				m_probability = ventProp.Probability,
				m_angle = PropAngle() * ventProp.Angle,
				m_repeatDistance = ventProp.RepeatInterval,
				m_position = PropPosition(ventProp)
			};
		}

		if (Elevation < ElevationType.Slope || !Lane.Tags.HasFlag(LaneTag.Damage))
		{
			yield break;
		}

		var position = (Road.TotalRoadWidth / 2) - 0.1F;

		for (var i = 0; i < 2; i++)
		{
			var left = i == 0;

			if (Elevation is ElevationType.Tunnel)
			{
				var exitProp = GetProp(Prop.TunnelExitProp);
				var lightProp = GetProp(Prop.TunnelLight);

				yield return new NetLaneProps.Prop
				{
					m_prop = lightProp,
					m_tree = lightProp,
					m_angle = lightProp.Angle,
					m_minLength = 10,
					m_repeatDistance = lightProp.RepeatInterval,
					m_probability = 100,
					m_position = new Vector3(position, 0, 0) + lightProp.Position
				}.Extend(prop => new LaneProp(prop)
				{
					LaneFlags = new LaneInfoFlags
					{ Forbidden = RoadUtils.Flags.L_RemoveStreetLights }
				}).ToggleForwardBackward(left);

				yield return new NetLaneProps.Prop
				{
					m_prop = exitProp,
					m_tree = exitProp,
					m_segmentOffset = exitProp.SegmentOffset,
					m_probability = exitProp.Probability,
					m_angle = PropAngle() * exitProp.Angle,
					m_repeatDistance = exitProp.RepeatInterval,
					m_position = PropPosition(exitProp) + new Vector3(position - 0.5F, 0, 0)
				}.ToggleForwardBackward(left);
			}
			else
			{
				foreach (var item in GetTunnelLightProps(Prop.TunnelLight, position))
				{
					yield return item.ToggleForwardBackward(left);
				}
			}
		}
	}

	private IEnumerable<NetLaneProps.Prop> GetTunnelLightProps(Prop prop, float position)
	{
		var lightProp = GetProp(prop);

		if (prop == Prop.TunnelLight)
		{
			yield return new NetLaneProps.Prop
			{
				m_prop = lightProp,
				m_tree = lightProp,
				m_angle = lightProp.Angle,
				m_minLength = 10,
				m_probability = 100,
				m_position = new Vector3(position, 0, 0) + lightProp.Position
			}.Extend(prop => new LaneProp(prop)
			{
				LaneFlags = new LaneInfoFlags
				{ Forbidden = RoadUtils.Flags.L_RemoveStreetLights }
			});
		}

		yield return new NetLaneProps.Prop
		{
			m_prop = lightProp,
			m_tree = lightProp,
			m_angle = lightProp.Angle,
			m_segmentOffset = -0.5F,
			m_endFlagsForbidden = prop == Prop.TunnelLight ? NetNode.Flags.None : NetNode.Flags.OnGround,
			m_startFlagsForbidden = prop == Prop.TunnelLight ? NetNode.Flags.OnGround : NetNode.Flags.None,
			m_minLength = 10,
			m_probability = 100,
			m_position = new Vector3(position, 0, 0) + lightProp.Position
		}.Extend(prop => new LaneProp(prop)
		{
			LaneFlags = new LaneInfoFlags
			{ Forbidden = RoadUtils.Flags.L_RemoveStreetLights }
		});

		yield return new NetLaneProps.Prop
		{
			m_prop = lightProp,
			m_tree = lightProp,
			m_angle = lightProp.Angle,
			m_segmentOffset = 0.5F,
			m_endFlagsForbidden = prop == Prop.TunnelLight ? NetNode.Flags.OnGround : NetNode.Flags.None,
			m_startFlagsForbidden = prop == Prop.TunnelLight ? NetNode.Flags.None : NetNode.Flags.OnGround,
			m_minLength = 10,
			m_probability = 100,
			m_position = new Vector3(position, 0, 0) + lightProp.Position
		}.Extend(prop => new LaneProp(prop)
		{
			LaneFlags = new LaneInfoFlags
			{ Forbidden = RoadUtils.Flags.L_RemoveStreetLights }
		});
	}
}
