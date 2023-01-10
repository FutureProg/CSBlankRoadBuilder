using BlankRoadBuilder.Domain;
using BlankRoadBuilder.Util;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace BlankRoadBuilder.ThumbnailMaker;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
public class RoadInfo
{
	[XmlAttribute] public int Version { get; set; }
	public string Name { get; set; }
	public string CustomName { get; set; }
	public string Description { get; set; }
	public string CustomText { get; set; }
	public DateTime DateCreated { get; set; }
	public RoadType RoadType { get; set; }
	public RegionType RegionType { get; set; }
	public TextureType SideTexture { get; set; }
	public BridgeTextureType BridgeSideTexture { get; set; }
	public AsphaltStyle AsphaltStyle { get; set; }
	public float RoadWidth { get; set; }
	public float BufferWidth { get; set; }
	public int SpeedLimit { get; set; }
	public bool LHT { get; set; }
	public bool VanillaWidth { get; set; }
	public List<LaneInfo> Lanes { get; set; }
	public byte[] SmallThumbnail { get; set; }
	public byte[] LargeThumbnail { get; set; }
	public byte[] TooltipImage { get; set; }

	[XmlIgnore] public float TotalWidth { get; set; }
	[XmlIgnore] public float AsphaltWidth { get; set; }
	[XmlIgnore] public float LeftPavementWidth { get; set; }
	[XmlIgnore] public float RightPavementWidth { get; set; }
	[XmlIgnore] public bool ContainsWiredLanes { get; set; }
	[XmlIgnore] public bool WiredLanesAreNextToMedians { get; set; }
	[XmlIgnore] public bool ContainsCenterMedian => Lanes.Any(x => x.Tags.HasFlag(LaneTag.CenterMedian));
	[XmlIgnore] public ParkingAngle ParkingAngle => Lanes.Max(x => x.ParkingAngle);
}

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.