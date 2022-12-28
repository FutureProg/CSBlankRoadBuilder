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
	public static Dictionary<GenericMarkingType, LineInfo> UK(MarkingType type)
	{
		return new Dictionary<GenericMarkingType, LineInfo>
		{
			{ GenericMarkingType.End, new LineInfo
				{
					MarkingStyle = MarkingLineType.Solid,
					Color = type == MarkingType.IMT ? _imtWhiteLineColor : _whiteLineColor,
					LineWidth = 0.15F
				}
			},
			{ GenericMarkingType.Bike | GenericMarkingType.Flipped, new LineInfo
				{
					MarkingStyle = MarkingLineType.Dashed,
					Color = type == MarkingType.IMT ? _imtWhiteLineColor : _whiteLineColor,
					DashLength = 0.5F,
					DashSpace = 0.5F,
					LineWidth = 0.15F
				}
			},
			{ GenericMarkingType.Parking, new LineInfo
				{
					MarkingStyle = MarkingLineType.Dashed,
					Color = type == MarkingType.IMT ? _imtWhiteLineColor : _whiteLineColor,
					DashLength = 0.25F,
					DashSpace = 0.25F,
					LineWidth = 0.1F
				}
			},
			{ GenericMarkingType.End | GenericMarkingType.Flipped, new LineInfo
				{
					MarkingStyle = MarkingLineType.SolidDouble,
					Color = type == MarkingType.IMT ? _imtYellowLineColor :  _yellowLineColor,
					LineWidth = 0.15F
				}
			},
			{ GenericMarkingType.Hard | GenericMarkingType.Flipped, new LineInfo
				{
					MarkingStyle = MarkingLineType.SolidDouble,
					Color = type == MarkingType.IMT ? _imtYellowLineColor :  _yellowLineColor,
					LineWidth = 0.1F
				}
			},
			{ GenericMarkingType.Hard | GenericMarkingType.Normal, new LineInfo
				{
					MarkingStyle = MarkingLineType.Solid,
					Color = type == MarkingType.IMT ? _imtWhiteLineColor : _whiteLineColor,
					LineWidth = 0.15F
				}
			},
			{ GenericMarkingType.Soft | GenericMarkingType.Normal, new LineInfo
				{
					MarkingStyle = MarkingLineType.Dashed,
					Color = type == MarkingType.IMT ? _imtWhiteLineColor : _whiteLineColor,
					DashLength = 1.5F,
					LineWidth = 0.15F
				}
			},
		};
	}

	public static Dictionary<LaneType, FillerInfo> UK_Fillers(MarkingType type)
	{
		return new Dictionary<LaneType, FillerInfo>
		{
			{ LaneType.Bike, new FillerInfo
				{
					MarkingStyle = MarkingFillerType.Filled,
					Color = type == MarkingType.IMT ? _imtBikeFillerColor : _bikeFillerColor,
				}
			},
			{ LaneType.Bus, new FillerInfo
				{
					MarkingStyle = MarkingFillerType.Filled,
					Color = type == MarkingType.IMT ? _imtBusFillerColor : _busFillerColor,
				}
			},
			{ LaneType.Trolley, new FillerInfo
				{
					MarkingStyle = MarkingFillerType.Filled,
					Color = type == MarkingType.IMT ? _imtBusFillerColor : _busFillerColor,
				}
			},
		};
	}
}
