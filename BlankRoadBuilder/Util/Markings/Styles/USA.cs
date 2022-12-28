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
	private static readonly Color32 _yellowLineColor = new(167, 113, 19, 100);
	private static readonly Color32 _whiteLineColor = new(200, 200, 200, 55);
	private static readonly Color32 _bikeFillerColor = new(9, 85, 39, 55);
	private static readonly Color32 _tramFillerColor = new(9, 85, 39, 25);
	private static readonly Color32 _busFillerColor = new(55, 17, 4, 128);

	private static readonly Color32 _imtYellowLineColor = new(167, 136, 88, 255);
	private static readonly Color32 _imtWhiteLineColor = new(180, 180, 180, 180);
	private static readonly Color32 _imtBikeFillerColor = new(35, 145, 105, 130);
	private static readonly Color32 _imtBusFillerColor = new(112, 64, 48, 174);

	public static Dictionary<GenericMarkingType, LineInfo> USA(MarkingType type)
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
					Color = type == MarkingType.IMT ? _imtYellowLineColor : _yellowLineColor,
					DashLength = 0.5F,
					DashSpace = 1.5F,
					LineWidth = 0.15F
				}
			},
			{ GenericMarkingType.Parking, new LineInfo
				{
					MarkingStyle = MarkingLineType.None
				}
			},
			{ GenericMarkingType.End | GenericMarkingType.Flipped, new LineInfo
				{
					MarkingStyle = MarkingLineType.Solid,
					Color = type == MarkingType.IMT ? _imtYellowLineColor :  _yellowLineColor,
					LineWidth = 0.15F
				}
			},
			{ GenericMarkingType.Hard | GenericMarkingType.Flipped, new LineInfo
				{
					MarkingStyle = MarkingLineType.SolidDouble,
					Color = type == MarkingType.IMT ? _imtYellowLineColor :  _yellowLineColor,
					LineWidth = 0.15F
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
					DashSpace = 2F,
					LineWidth = 0.15F
				}
			},
		};
	}

	public static Dictionary<LaneType, FillerInfo> USA_Fillers(MarkingType type)
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
