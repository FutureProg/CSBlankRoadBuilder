using AdaptiveRoads.Manager;

using BlankRoadBuilder.Domain;
using BlankRoadBuilder.Domain.Options;
using BlankRoadBuilder.ThumbnailMaker;
using BlankRoadBuilder.Util.Props.Templates;

using System;
using System.Collections.Generic;

using UnityEngine;

using static AdaptiveRoads.Manager.NetInfoExtionsion;
using static PathUnit;

namespace BlankRoadBuilder.Util.Props;

public partial class LanePropsUtil
{
	private IEnumerable<NetLaneProps.Prop> GetDecorationProps()
	{
		foreach (var deco in Lane.Decorations.GetValues())
		{
			switch (deco)
			{
				case LaneDecoration.Grass:
					if (Lane.LaneWidth >= 2 && ModOptions.AddGrassPropsToGrassLanes)
					{
						foreach (var prop in GetGrassProps())
						{
							yield return prop;
						}
					}

					break;
				case LaneDecoration.Tree:
					foreach (var prop in GetTrees())
					{
						yield return prop;
					}

					break;
				case LaneDecoration.Benches:
					foreach (var prop in GetBenches())
					{
						yield return prop;
					}

					break;
				case LaneDecoration.FlowerPots:
					foreach (var prop in Lane.Decorations.HasFlag(LaneDecoration.Grass) ? GetFlowers() : GetFlowerPots())
					{
						yield return prop;
					}

					break;
				case LaneDecoration.Bollard:
					foreach (var prop in GetBollards())
					{
						yield return prop;
					}

					break;
				case LaneDecoration.Hedge:
					foreach (var prop in GetHedge())
					{
						yield return prop;
					}

					break;
				case LaneDecoration.LampPost:
					foreach (var prop in GetStreetLights(LaneDecoration.LampPost))
					{
						yield return prop;
					}

					break;
				case LaneDecoration.StreetLight:
				case LaneDecoration.DoubleStreetLight:
					foreach (var prop in GetStreetLights(deco))
					{
						yield return prop;
					}

					break;
				case LaneDecoration.BikeParking:
					foreach (var prop in GetBikeParking())
					{
						yield return prop;
					}

					break;
				case LaneDecoration.TrashBin:
					foreach (var prop in GetTrashBin())
					{
						yield return prop;
					}

					break;
				case LaneDecoration.StreetAds:
					foreach (var prop in GetStreetAds())
					{
						yield return prop;
					}

					break;
				case LaneDecoration.FireHydrant:
					foreach (var prop in GetFireHydrant())
					{
						yield return prop;
					}

					break;
			}
		}
	}

	private IEnumerable<NetLaneProps.Prop> GetFireHydrant()
	{
		var prop = GetProp(Prop.FireHydrant);

		if (prop is DecorationProp decorationProp && decorationProp.OnlyOnGround && Elevation != ElevationType.Basic)
			yield break;

		yield return new NetLaneProps.Prop
		{
			m_prop = prop,
			m_tree = prop,
			m_probability = prop.Probability,
			m_repeatDistance = prop.RepeatInterval,
			m_angle = PropAngle() * prop.Angle,
			m_segmentOffset = prop.SegmentOffset,
			m_position = new Vector3(0, 0, -PropAngle() * Lane.LaneWidth / 2) + prop.Position
		};
	}

	private IEnumerable<NetLaneProps.Prop> GetBikeParking()
	{
		var prop = GetProp(Prop.BicycleParking);
		var bench = GetProp(Prop.Bench);

		if (prop is DecorationProp decorationProp && decorationProp.OnlyOnGround && Elevation != ElevationType.Basic)
			yield break;

		yield return new NetLaneProps.Prop
		{
			m_prop = prop,
			m_tree = prop,
			m_probability = 100,
			m_angle = PropAngle() * 90,
			m_repeatDistance = Lane.Decorations.HasFlag(LaneDecoration.Benches) ? bench.RepeatInterval : prop.RepeatInterval,
			m_position = new Vector3(0, 0, Lane.Decorations.HasFlag(LaneDecoration.Benches) ? -6F : 0) + prop.Position + (Lane.Decorations.HasFlag(LaneDecoration.Benches) ? bench.Position : new())
		};
	}

	private IEnumerable<NetLaneProps.Prop> GetTrashBin()
	{
		var prop = GetProp(Prop.TrashBin);

		if (prop is DecorationProp decorationProp && decorationProp.OnlyOnGround && Elevation != ElevationType.Basic)
			yield break;

		yield return new NetLaneProps.Prop
		{
			m_prop = prop,
			m_tree = prop,
			m_segmentOffset = prop.SegmentOffset,
			m_probability = prop.Probability,
			m_angle = PropAngle() * prop.Angle,
			m_repeatDistance = prop.RepeatInterval,
			m_position = prop.Position
		};
	}

	private IEnumerable<NetLaneProps.Prop> GetStreetAds()
	{
		var prop = GetProp(Prop.StreetAd);
		var hasOtherDecos = Lane.Decorations.HasAnyFlag(LaneDecoration.Benches, LaneDecoration.FlowerPots);

		if (prop is DecorationProp decorationProp && decorationProp.OnlyOnGround && Elevation != ElevationType.Basic)
			yield break;

		yield return new NetLaneProps.Prop
		{
			m_prop = prop,
			m_tree = prop,
			m_segmentOffset = prop.SegmentOffset,
			m_probability = prop.Probability,
			m_angle = PropAngle() * prop.Angle,
			m_repeatDistance = prop.RepeatInterval,
			m_position = prop.Position + new Vector3(hasOtherDecos ? PropAngle() * Lane.LaneWidth / 2 : 0, 0, 0)
		};
	}

	private IEnumerable<NetLaneProps.Prop> GetStreetLights(LaneDecoration decoration)
	{
		var lightProp = GetProp(decoration switch { LaneDecoration.LampPost => Prop.LampPost, LaneDecoration.DoubleStreetLight => Prop.DoubleStreetLight, _ => Prop.SingleStreetLight });
		var xPos = decoration == LaneDecoration.DoubleStreetLight ? 0 : (-Lane.LaneWidth / 2) + 0.5F;

		yield return getLight(Road.ContainsWiredLanes ? (PropAngle() * 2F) : 0F);

		if (ModOptions.VanillaStreetLightPlacement)
		{
			yield break;
		}

		for (var i = lightProp.RepeatInterval; i <= 96; i += lightProp.RepeatInterval)
		{
			yield return getLight(i);
			yield return getLight(-i);
		}

		NetLaneProps.Prop getLight(float position) => new NetLaneProps.Prop
		{
			m_prop = lightProp,
			m_tree = lightProp,
			m_angle = lightProp.Angle,
			m_minLength = ModOptions.VanillaStreetLightPlacement ? 10 : (Math.Abs(position) * 1.95F),
			m_repeatDistance = ModOptions.VanillaStreetLightPlacement ? lightProp.RepeatInterval : 0,
			m_probability = 100,
			m_position = new Vector3(xPos, 0, position) + lightProp.Position
		}.Extend(prop => new LaneProp(prop)
		{
			LaneFlags = new LaneInfoFlags
			{ Forbidden = RoadUtils.Flags.L_RemoveStreetLights }
		}).ToggleForwardBackward(PropAngle() < 0);
	}

	private IEnumerable<NetLaneProps.Prop> GetBenches()
	{
		var position = Lane.Type != LaneType.Curb ? 0 : Lane.Direction == LaneDirection.Both && Lane.Position < 0 ? -0.25F : 0.25F;
		var prop = GetProp(Prop.Bench);

		if (prop is DecorationProp decorationProp && decorationProp.OnlyOnGround && Elevation != ElevationType.Basic)
			yield break;

		yield return new NetLaneProps.Prop
		{
			m_prop = prop,
			m_tree = prop,
			m_segmentOffset = prop.SegmentOffset,
			m_probability = prop.Probability,
			m_angle = PropAngle() * prop.Angle,
			m_repeatDistance = prop.RepeatInterval,
			m_position = prop.Position + new Vector3(position, 0, 0)
		};
	}

	private IEnumerable<NetLaneProps.Prop> GetHedge()
	{
		var position = PropAngle() * 0.05F;
		var prop = GetProp(Prop.Hedge);

		if (prop is DecorationProp decorationProp && decorationProp.OnlyOnGround && Elevation != ElevationType.Basic)
			yield break;

		if (Lane.Decorations != LaneDecoration.Hedge)
		{
			var hasOtherDecos = Lane.Decorations.HasFlag(LaneDecoration.Bollard) && Lane.Decorations.HasAnyFlag(LaneDecoration.Benches, LaneDecoration.FlowerPots, LaneDecoration.StreetAds, LaneDecoration.BikeParking);

			position = PropAngle() * ((-Lane.LaneWidth / 2) + (hasOtherDecos ? 0.75F : 0.55F));
		}

		yield return new NetLaneProps.Prop
		{
			m_prop = prop,
			m_tree = prop,
			m_segmentOffset = prop.SegmentOffset,
			m_probability = prop.Probability,
			m_angle = PropAngle() * prop.Angle,
			m_repeatDistance = prop.RepeatInterval,
			m_position = prop.Position + new Vector3(position, 0, 0)
		};
	}

	private IEnumerable<NetLaneProps.Prop> GetBollards()
	{
		var prop = GetProp(Prop.Bollard);
		var hasOtherDecos = Lane.Decorations.HasAnyFlag(LaneDecoration.Benches, LaneDecoration.FlowerPots, LaneDecoration.StreetAds, LaneDecoration.BikeParking);

		if (prop is DecorationProp decorationProp && decorationProp.OnlyOnGround && Elevation != ElevationType.Basic)
			yield break;

		yield return new NetLaneProps.Prop
		{
			m_prop = prop,
			m_tree = prop,
			m_segmentOffset = prop.SegmentOffset,
			m_probability = prop.Probability,
			m_angle = PropAngle() * prop.Angle,
			m_repeatDistance = prop.RepeatInterval,
			m_position = prop.Position + new Vector3(hasOtherDecos ? PropAngle() * Lane.LaneWidth / -2 : 0, 0, 0)
		};
	}

	private IEnumerable<NetLaneProps.Prop> GetFlowerPots()
	{
		var prop = GetProp(Prop.FlowerPot);

		if (prop is DecorationProp decorationProp && decorationProp.OnlyOnGround && Elevation != ElevationType.Basic)
			yield break;

		if (Lane.Decorations.HasFlag(LaneDecoration.Benches))
		{
			var bench = GetProp(Prop.Bench);

			yield return new NetLaneProps.Prop
			{
				m_prop = prop,
				m_tree = prop,
				m_segmentOffset = bench.SegmentOffset,
				m_probability = prop.Probability,
				m_angle = PropAngle() * prop.Angle,
				m_repeatDistance = bench.RepeatInterval,
				m_position = new Vector3(0, 0, Lane.Decorations.HasFlag(LaneDecoration.TrashBin) ? 2.75F : 2F) + prop.Position + bench.Position
			};

			yield return new NetLaneProps.Prop
			{
				m_prop = prop,
				m_tree = prop,
				m_segmentOffset = bench.SegmentOffset,
				m_probability = prop.Probability,
				m_angle = PropAngle() * prop.Angle,
				m_repeatDistance = bench.RepeatInterval,
				m_position = new Vector3(0, 0, -2F) + prop.Position + bench.Position
			};

			yield break;
		}

		yield return new NetLaneProps.Prop
		{
			m_prop = prop,
			m_tree = prop,
			m_segmentOffset = prop.SegmentOffset,
			m_probability = prop.Probability,
			m_angle = PropAngle() * prop.Angle,
			m_repeatDistance = prop.RepeatInterval,
			m_position = prop.Position
		};
	}

	private IEnumerable<NetLaneProps.Prop> GetFlowers()
	{
		var prop = GetProp(Prop.Flowers);
		var numLines = Math.Max((int)Math.Ceiling(Lane.LaneWidth / 0.75) - 1, 1);
		var odd = numLines % 2 == 1;
		var pos = numLines == 1 ? 0 : numLines * 0.75F * -0.5F;

		if (prop is DecorationProp decorationProp && decorationProp.OnlyOnGround && Elevation != ElevationType.Basic)
			yield break;

		for (var i = 0; i < numLines; i++)
		{
			if (i > 0)
			{
				pos += (Lane.LaneWidth - 2) / (numLines - 1);
			}

			yield return new NetLaneProps.Prop
			{
				m_prop = prop,
				m_tree = prop,
				m_probability = prop.Probability,
				m_angle = prop.IsTree ? (((float)_random.NextDouble() * 360) - 180) : (PropAngle() * prop.Angle),
				m_repeatDistance = prop.RepeatInterval,
				m_segmentOffset = prop.SegmentOffset,
				m_position = new Vector3(pos, 0, (float)Math.Round(_random.NextDouble() * 3, 2)) + prop.Position
			}.Extend(prop => new LaneProp(prop)
			{
				VanillaSegmentFlags = new VanillaSegmentInfoFlags
				{
					Forbidden = Lane.Decorations.HasFlag(LaneDecoration.TransitStop) ? NetSegment.Flags.StopAll : NetSegment.Flags.None
				},
				SegmentFlags = new SegmentInfoFlags
				{
					Required = !ModOptions.MarkingsGenerated.HasAnyFlag(MarkingsSource.MeshFillers, MarkingsSource.IMTMarkings) && ModOptions.MarkingsGenerated.HasFlag(MarkingsSource.HiddenVanillaMarkings) ? RoadUtils.Flags.S_RemoveMarkings : NetSegmentExt.Flags.None,
					Forbidden = !ModOptions.MarkingsGenerated.HasAnyFlag(MarkingsSource.MeshFillers, MarkingsSource.IMTMarkings) && !ModOptions.MarkingsGenerated.HasFlag(MarkingsSource.HiddenVanillaMarkings) ? RoadUtils.Flags.S_RemoveMarkings : NetSegmentExt.Flags.None,
				}
			});
		}
	}

	public IEnumerable<NetLaneProps.Prop> GetGrassProps()
	{
		var prop = GetProp(Prop.Grass);
		var numLines = Math.Max((int)Math.Ceiling(Lane.LaneWidth) - 1, 1);
		var odd = numLines % 2 == 1;
		var pos = numLines == 1 ? 0 : (1 - (Lane.LaneWidth / 2));

		if (prop is DecorationProp decorationProp && decorationProp.OnlyOnGround && Elevation != ElevationType.Basic)
			yield break;

		for (var i = 0; i < numLines; i++)
		{
			if (i > 0)
			{
				pos += (Lane.LaneWidth - 2) / (numLines - 1);
			}

			yield return new NetLaneProps.Prop
			{
				m_prop = prop,
				m_tree = prop,
				m_segmentOffset = prop.SegmentOffset,
				m_probability = prop.Probability,
				m_angle = prop.IsTree ? (((float)_random.NextDouble() * 360) - 180) : (PropAngle() * prop.Angle),
				m_repeatDistance = prop.RepeatInterval,
				m_position = prop.Position
			}.Extend(prop => new LaneProp(prop)
			{
				VanillaSegmentFlags = new VanillaSegmentInfoFlags
				{
					Forbidden = Lane.Decorations.HasFlag(LaneDecoration.TransitStop) ? NetSegment.Flags.StopAll : NetSegment.Flags.None
				},
				SegmentFlags = new SegmentInfoFlags
				{
					Required = !ModOptions.MarkingsGenerated.HasAnyFlag(MarkingsSource.MeshFillers, MarkingsSource.IMTMarkings) && ModOptions.MarkingsGenerated.HasFlag(MarkingsSource.HiddenVanillaMarkings) ? RoadUtils.Flags.S_RemoveMarkings : NetSegmentExt.Flags.None,
					Forbidden = !ModOptions.MarkingsGenerated.HasAnyFlag(MarkingsSource.MeshFillers, MarkingsSource.IMTMarkings) && !ModOptions.MarkingsGenerated.HasFlag(MarkingsSource.HiddenVanillaMarkings) ? RoadUtils.Flags.S_RemoveMarkings : NetSegmentExt.Flags.None,
				}
			});
		}
	}

	public IEnumerable<NetLaneProps.Prop> GetTrees()
	{
		var tree = GetProp(Prop.Tree);
		var planter = GetProp(Prop.TreePlanter);
		var hasOtherDecos = Lane.Decorations.HasAnyFlag(LaneDecoration.Benches, LaneDecoration.FlowerPots);

		if (tree is DecorationProp decorationProp && decorationProp.OnlyOnGround && Elevation != ElevationType.Basic)
			yield break;

		if (ModOptions.VanillaTreePlacement)
		{
			yield return getProp(2, tree);

			if (planter != null && !Lane.Decorations.HasAnyFlag(LaneDecoration.Grass, LaneDecoration.Gravel))
				yield return getProp(2, planter);

			yield break;
		}

		for (var i = tree.RepeatInterval / 2; i <= 96; i += tree.RepeatInterval)
		{
			var pos1 = i + (float)(ModOptions.RandomizeTreeDistance ? ((_random.NextDouble() * tree.RepeatInterval / 2) - (tree.RepeatInterval / 4)) : 0);
			var pos2 = -i + (float)(ModOptions.RandomizeTreeDistance ? ((_random.NextDouble() * tree.RepeatInterval / 2) - (tree.RepeatInterval / 4)) : 0);

			yield return getProp(pos1, tree);
			yield return getProp(pos2, tree);

			if (planter != null && !Lane.Decorations.HasAnyFlag(LaneDecoration.Grass, LaneDecoration.Gravel))
			{
				yield return getProp(pos1, planter);
				yield return getProp(pos2, planter);
			}
		}

		NetLaneProps.Prop getProp(float position, PropTemplate prop) => new NetLaneProps.Prop
		{
			m_tree = prop,
			m_prop = prop,
			m_minLength = ModOptions.VanillaTreePlacement ? 10 : (Math.Abs(position) * 1.95F),
			m_upgradable = true,
			m_probability = 100,
			m_repeatDistance = ModOptions.VanillaTreePlacement ? tree.RepeatInterval : 0,
			m_position = new Vector3(hasOtherDecos ? PropAngle() * -Math.Min(1, Lane.LaneWidth / 2) : 0, 0, position)
		}.Extend(prop => new LaneProp(prop)
		{
			EndNodeFlags = new NodeInfoFlags
			{ Forbidden = position > 0 && Lane.Tags.HasFlag(LaneTag.Asphalt) && !ModOptions.VanillaTreePlacement ? RoadUtils.Flags.N_HideTreesCloseToIntersection : NetNodeExt.Flags.None },
			StartNodeFlags = new NodeInfoFlags
			{ Forbidden = position < 0 && Lane.Tags.HasFlag(LaneTag.Asphalt) && !ModOptions.VanillaTreePlacement ? RoadUtils.Flags.N_HideTreesCloseToIntersection : NetNodeExt.Flags.None },
			LaneFlags = new LaneInfoFlags
			{ Forbidden = RoadUtils.Flags.L_RemoveTrees },
			VanillaSegmentFlags = new VanillaSegmentInfoFlags
			{ Forbidden = Lane.Decorations.HasFlag(LaneDecoration.TransitStop) ? NetSegment.Flags.StopAll : NetSegment.Flags.None }
		});
	}

	private int PropAngle()
	{
		return (Lane.Position < 0 ? -1 : 1) * (Lane.PropAngle == ThumbnailMaker.PropAngle.Right == (Lane.Direction != LaneDirection.Backwards || Lane.Type == LaneType.Curb) ? 1 : -1);
	}
}
