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
	private static readonly Color32 _bikeNLFillerColor = new(55, 17, 4, 100);

	public static Dictionary<GenericMarkingType, LineInfo> Netherlands(MarkingType type)
	{
		return new Dictionary<GenericMarkingType, LineInfo>
		{
			{ GenericMarkingType.End, new LineInfo
				{
					MarkingStyle = MarkingLineType.Solid,
					Color = type == MarkingType.IMT ? _imtWhiteLineColor :  _whiteLineColor,
					LineWidth = 0.15F
				}
			},
			{ GenericMarkingType.Bike | GenericMarkingType.Flipped, new LineInfo
				{
					MarkingStyle = MarkingLineType.Dashed,
					Color = type == MarkingType.IMT ? _imtWhiteLineColor :  _whiteLineColor,
					DashLength = 1F,
					DashSpace = 2F,
					LineWidth = 0.25F
				}
			},
			{ GenericMarkingType.Parking, new LineInfo
				{
					MarkingStyle = MarkingLineType.Dashed,
					Color = type == MarkingType.IMT ? _imtWhiteLineColor :  _whiteLineColor,
					DashLength = 0.6F,
					DashSpace = 1F,
					LineWidth = 0.1F
				}
			},
			{ GenericMarkingType.End | GenericMarkingType.Flipped, new LineInfo
				{
					MarkingStyle = MarkingLineType.Solid,
					Color = type == MarkingType.IMT ? _imtWhiteLineColor :   _whiteLineColor,
					LineWidth = 0.15F
				}
			},
			{ GenericMarkingType.Hard | GenericMarkingType.Flipped, new LineInfo
				{
					MarkingStyle = MarkingLineType.Solid,
					Color = type == MarkingType.IMT ? _imtWhiteLineColor :   _whiteLineColor,
					LineWidth = 0.15F
				}
			},
			{ GenericMarkingType.Hard | GenericMarkingType.Normal, new LineInfo
				{
					MarkingStyle = MarkingLineType.Solid,
					Color = type == MarkingType.IMT ? _imtWhiteLineColor :  _whiteLineColor,
					LineWidth = 0.15F
				}
			},
			{ GenericMarkingType.Soft | GenericMarkingType.Normal, new LineInfo
				{
					MarkingStyle = MarkingLineType.Dashed,
					Color = type == MarkingType.IMT ? _imtWhiteLineColor :  _whiteLineColor,
					DashLength = 1.5F,
					LineWidth = 0.15F
				}
			},
		};
	}

	public static Dictionary<LaneType, FillerInfo> Netherlands_Fillers(MarkingType type)
	{
		return new Dictionary<LaneType, FillerInfo>
		{
			{ LaneType.Bike, new FillerInfo
				{
					MarkingStyle = MarkingFillerType.Filled,
					Color = type == MarkingType.IMT ? _imtBusFillerColor : _bikeNLFillerColor,
				}
			}
		};
	}
}
