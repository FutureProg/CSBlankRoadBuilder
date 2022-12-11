using BlankRoadBuilder.Domain;

using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace BlankRoadBuilder.ThumbnailMaker;

public class RoadInfo
{
	public List<LaneInfo> Lanes { get; set; }
	public RoadType RoadType { get; set; }
	public RegionType RegionType { get; set; }
	public float AsphaltWidth { get; set; }
	public float PavementWidth { get; set; }
	public float BufferWidth { get; set; }
	public float SpeedLimit { get; set; }
	public bool LHT { get; set; }
	public string Name { get; set; }
	public string Description { get; set; }
	public byte[] SmallThumbnail { get; set; }
	public byte[] LargeThumbnail { get; set; }
	public byte[] TooltipImage { get; set; }

	[Obsolete] public float BufferSize { get; set; }
	[Obsolete] public float Width { get; set; }

	[XmlIgnore] public RoadOptions? Options { get; set; }

	[XmlIgnore] public bool? OneWay { get; set; }
	[XmlIgnore] public bool ContainsCenterMedian { get; set; }
	[XmlIgnore] public bool DiagonalParking { get; set; }
	[XmlIgnore] public bool InvertedDiagonalParking { get; set; }
	[XmlIgnore] public bool HorizontalParking { get; set; }
	[XmlIgnore] public bool ContainsWiredLanes { get; set; }
	[XmlIgnore] public bool WiredLanesAreNextToMedians { get; set; }
}