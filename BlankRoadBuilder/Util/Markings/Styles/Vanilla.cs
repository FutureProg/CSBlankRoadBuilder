using BlankRoadBuilder.Domain;
using BlankRoadBuilder.ThumbnailMaker;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using static BlankRoadBuilder.Util.Markings.MarkingStyleUtil;

namespace BlankRoadBuilder.Util.Markings;
public partial class MarkingStylesTemplates
{
	private static Color32 _vanillaWhiteIMTLineColor = new Color32(180, 180, 180, 100);
	private static Color32 _vanillaYellowIMTLineColor = new Color32(161, 136, 88, 135);

	public static Dictionary<GenericMarkingType, LineInfo> Vanilla(MarkingType type)
	{
		return new Dictionary<GenericMarkingType, LineInfo>
		{
			{ GenericMarkingType.End, new LineInfo
				{
					MarkingStyle = MarkingLineType.Solid,
					Color = type == MarkingType.IMT ?_vanillaWhiteIMTLineColor: _whiteLineColor,
					LineWidth = 0.15F
				}
			},
			{ GenericMarkingType.Bike | GenericMarkingType.Flipped, new LineInfo
				{
					MarkingStyle = MarkingLineType.Dashed,
					Color = type == MarkingType.IMT ?_vanillaWhiteIMTLineColor: _whiteLineColor,
					DashLength = 0.75F,
					DashSpace = 1F,
					LineWidth = 0.15F
				}
			},
			{ GenericMarkingType.Parking, new LineInfo
				{
					MarkingStyle = MarkingLineType.Dashed,
					Color = type == MarkingType.IMT ? _vanillaWhiteIMTLineColor : _whiteLineColor,
					DashLength = 0.3F,
					DashSpace = 0.6F,
					LineWidth = 0.3F
				}
			},
			{ GenericMarkingType.End | GenericMarkingType.Flipped, new LineInfo
				{
					MarkingStyle = MarkingLineType.Solid,
					Color = type == MarkingType.IMT ?_vanillaYellowIMTLineColor: _yellowLineColor,
					LineWidth = 0.15F
				}
			},
			{ GenericMarkingType.Hard | GenericMarkingType.Flipped, new LineInfo
				{
					MarkingStyle = MarkingLineType.SolidDouble,
					Color = type == MarkingType.IMT ?_vanillaYellowIMTLineColor: _yellowLineColor,
					LineWidth = 0.15F
				}
			},
			{ GenericMarkingType.Hard | GenericMarkingType.Normal, new LineInfo
				{
					MarkingStyle = MarkingLineType.Solid,
					Color = type == MarkingType.IMT ? _vanillaWhiteIMTLineColor : _whiteLineColor,
					LineWidth = 0.15F
				}
			},
			{ GenericMarkingType.Soft | GenericMarkingType.Normal, new LineInfo
				{
					MarkingStyle = MarkingLineType.Dashed,
					Color = type == MarkingType.IMT ? _vanillaWhiteIMTLineColor : _whiteLineColor,
					DashLength = 1.5F,
					DashSpace = 2F,
					LineWidth = 0.15F
				}
			},
		};
	}

	private static readonly Color32 _vanillaIMTBikeFillerColor = new(85,165,135,105);
	private static readonly Color32 _vanillaIMTTramFillerColor = new(85,155,115,70);
	private static readonly Color32 _vanillaIMTBusFillerColor = new(132,94,88,134);

	public static Dictionary<LaneType, FillerInfo> Vanilla_Fillers(MarkingType type)
	{
		return new Dictionary<LaneType, FillerInfo>
		{
			{ LaneType.Tram, new FillerInfo
				{
					MarkingStyle = MarkingFillerType.Filled,
					Color =type == MarkingType.IMT ?_vanillaIMTTramFillerColor: _tramFillerColor,
				}
			},
			{ LaneType.Bike, new FillerInfo
				{
					MarkingStyle = MarkingFillerType.Filled,
					Color =type == MarkingType.IMT ?_vanillaIMTBikeFillerColor: _bikeFillerColor,
				}
			},
			{ LaneType.Bus, new FillerInfo
				{
					MarkingStyle = MarkingFillerType.Filled,
					Color =type == MarkingType.IMT ? _vanillaIMTBusFillerColor:_busFillerColor,
				}
			},
			{ LaneType.Trolley, new FillerInfo
				{
					MarkingStyle = MarkingFillerType.Filled,
					Color = type == MarkingType.IMT ?_vanillaIMTBusFillerColor:_busFillerColor,
				}
			},
			{ LaneType.Filler, new FillerInfo
				{
					MarkingStyle = MarkingFillerType.Striped,
					Color = _vanillaWhiteIMTLineColor,
					DashLength = 0.25F,
					DashSpace = 6F
				}
			},
			{ LaneType.Parking, new FillerInfo
				{
					MarkingStyle = MarkingFillerType.Filled,
					Color = new Color32(155,155,255,45),
				}
			},
			{ LaneType.Car, new FillerInfo
				{
					MarkingStyle = MarkingFillerType.Arrows,
					Color = _vanillaWhiteIMTLineColor
				}
			},
			{ LaneType.Emergency, new FillerInfo
				{
					MarkingStyle = MarkingFillerType.Filled,
					Color = new Color32(112,64,108,174),
				}
			},
			{ LaneType.Pedestrian, new FillerInfo
				{
					MarkingStyle = MarkingFillerType.Striped,
					Color = _vanillaWhiteIMTLineColor,
					DashLength = 0.25F,
					DashSpace = 6F
				}
			},
		};
	}
}
