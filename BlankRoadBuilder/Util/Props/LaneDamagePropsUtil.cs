using AdaptiveRoads.Manager;

using BlankRoadBuilder.ThumbnailMaker;
using BlankRoadBuilder.Util.Props.Templates;

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace BlankRoadBuilder.Util.Props;
public partial class LanePropsUtil
{
	private static readonly System.Random _random = new(Guid.NewGuid().GetHashCode());

	private IEnumerable<NetLaneProps.Prop> GetRoadDamageProps()
	{
		foreach (var prop in GenerateDamageProp(60F, _potHoleProps, null))
			yield return prop;

		foreach (var prop in GenerateDamageProp(85F, _roadCracksProps, null))
			yield return prop;

		foreach (var prop in GenerateDamageProp(30F, _roadPatchProps))
			yield return prop;

		foreach (var prop in GenerateDamageProp(6.5F, _wetnessProps))
			yield return prop;

		foreach (var prop in GenerateDamageProp(10F, _stainsProps))
			yield return prop;

		foreach (var prop in GenerateDamageProp(40F, _roadWearProps, 90F))
			yield return prop;

		foreach (var prop in GenerateDamageProp(30F, _roadDamageProps))
			yield return prop;
	}

	private IEnumerable<NetLaneProps.Prop> GenerateDamageProp(float weight, List<KeyValuePair<float, string>> props, float? angle = 0F)
	{
		var totalProps = GetPropCount(Road, weight, props);
		var propsReturned = 0;
		var halfWidth = (int)((Road.AsphaltWidth - 1) / 2);
		var validProps = props.Where(x => x.Key < Road.AsphaltWidth).ToList();

		while (propsReturned < totalProps)
		{
			var prop = validProps[_random.Next(0, validProps.Count)];
			var propTemplate = new PropTemplate(prop.Value);
			var halfSize = (int)((prop.Key / 2F) + 1F);
			var x = halfWidth < halfSize ? 0 : _random.Next(-halfWidth + halfSize, halfWidth - halfSize);

			yield return new NetLaneProps.Prop
			{
				m_prop = propTemplate,
				m_finalProp = propTemplate,
				m_minLength = prop.Key + 4F,
				m_repeatDistance = (prop.Key + 4) * 2F,
				m_angle = angle ?? _random.Next(0, 360),
				m_probability = _random.Next(1, 13),
				m_position = new Vector3(x, 0, _random.Next(-4, 4)),
			}.Extend(propInfo => new NetInfoExtionsion.LaneProp(propInfo)
			{
				SeedIndex = _random.Next(),
				SegmentFlags = new NetInfoExtionsion.SegmentInfoFlags
				{ Required = RoadUtils.Flags.S_AddRoadDamage }
			});

			propsReturned++;
		}
	}

	private static int GetPropCount(RoadInfo road, float weight, List<KeyValuePair<float, string>> props)
	{
		var validProps = props.Count(x => x.Key < road.AsphaltWidth);

		if (validProps == 0)
			return 0;

		weight *= (float)validProps / props.Count;

		return (int)Math.Ceiling(weight * road.AsphaltWidth / 100);
	}

	private static readonly List<KeyValuePair<float, string>> _potHoleProps = new()
	{
		new KeyValuePair<float, string>(1F, "1631466271.UKR-D Pothole #1_Data"),
		new KeyValuePair<float, string>(1F, "1631466271.UKR-D Pothole #2_Data"),
		new KeyValuePair<float, string>(1F, "1631466271.UKR-D Pothole #3_Data"),
		new KeyValuePair<float, string>(1F, "1631466271.UKR-D Pothole #4_Data"),
		new KeyValuePair<float, string>(1F, "1631466271.UKR-D Pothole #5_Data"),
		new KeyValuePair<float, string>(1F, "1631466271.UKR-D Pothole #6_Data"),
		new KeyValuePair<float, string>(1F, "1631466271.UKR-D Pothole #7_Data"),
		new KeyValuePair<float, string>(1F, "1631466271.UKR-D Pothole #8_Data"),
		new KeyValuePair<float, string>(1F, "1631466271.UKR-D Pothole #9_Data"),
		new KeyValuePair<float, string>(1F, "1631466271.UKR-D Pothole #10_Data"),
		new KeyValuePair<float, string>(1F, "1631466271.UKR-D Pothole #11_Data")
	};

	private static readonly List<KeyValuePair<float, string>> _roadCracksProps = new()
	{
		new KeyValuePair<float, string>(10F, "1631466271.UKR-D Road Crack #1_Data"),
		new KeyValuePair<float, string>(10F, "1631466271.UKR-D Road Crack #2_Data"),
		new KeyValuePair<float, string>(3F, "1631466271.UKR-D Road Crack #3_Data"),
		new KeyValuePair<float, string>(3F, "1631466271.UKR-D Road Crack #4_Data"),
		new KeyValuePair<float, string>(3F, "1631466271.UKR-D Road Crack #5_Data"),
		new KeyValuePair<float, string>(1F, "1631466271.UKR-D Road Crack #6_Data"),
		new KeyValuePair<float, string>(3F, "1631466271.UKR-D Road Crack #7_Data")
	};

	private static readonly List<KeyValuePair<float, string>> _roadPatchProps = new()
	{
		new KeyValuePair<float, string>(1F, "1631466271.UKR-D Road Patch #1_Data"),
		new KeyValuePair<float, string>(1F, "1631466271.UKR-D Road Patch #2_Data"),
		new KeyValuePair<float, string>(1F, "1631466271.UKR-D Road Patch #3_Data"),
		new KeyValuePair<float, string>(1F, "1631466271.UKR-D Road Patch #4_Data")
	};

	private static readonly List<KeyValuePair<float, string>> _wetnessProps = new()
	{
		new KeyValuePair<float, string>(6F, "887659579.R69 Big Decal Wetness 1_Data"),
		new KeyValuePair<float, string>(20F, "887659579.R69 Big Decal Wetness 2_Data")
	};

	private static readonly List<KeyValuePair<float, string>> _stainsProps = new()
	{
		new KeyValuePair<float, string>(28F, "881628000.R69 Big Decal More Stains 1_Data"),
		new KeyValuePair<float, string>(28F, "881628000.R69 Big Decal More Stains 2_Data"),
		new KeyValuePair<float, string>(28F, "881628000.R69 Big Decal More Stains 3_Data")
	};

	private static readonly List<KeyValuePair<float, string>> _roadWearProps = new()
	{
		new KeyValuePair<float, string>(10F, "883492302.R69 Big Decal Road Wear 1_Data"),
		new KeyValuePair<float, string>(6F, "883492302.R69 Big Decal Road Wear 2_Data"),
		new KeyValuePair<float, string>(10F, "883492302.R69 Big Decal Road Wear 3_Data")
	};

	private static readonly List<KeyValuePair<float, string>> _roadDamageProps = new()
	{
		new KeyValuePair<float, string>(10F, "882856875.R69 Big Decal Road Damage 1_Data"),
		new KeyValuePair<float, string>(7F, "882856875.R69 Big Decal Road Damage 2_Data"),
		new KeyValuePair<float, string>(6F, "882856875.R69 Big Decal Road Damage 3_Data"),
		new KeyValuePair<float, string>(6F, "882856875.R69 Big Decal Road Damage 4_Data")
	};
}
