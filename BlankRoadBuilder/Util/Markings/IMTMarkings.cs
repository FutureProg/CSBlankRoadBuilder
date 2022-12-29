using BlankRoadBuilder.Domain.Options;
using BlankRoadBuilder.ThumbnailMaker;

using ModsCommon;

using NodeMarkup.Manager;

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace BlankRoadBuilder.Util.Markings;
public class IMTMarkings
{
	public static void ApplyMarkings(ushort segmentId)
	{
		if (RoadBuilderUtil.CurrentRoad == null)
		{
			return;
		}

		if (!(ModOptions.MarkingsGenerated == Domain.MarkingsSource.IMTOnly || ModOptions.MarkingsGenerated == Domain.MarkingsSource.IMTWithANHelpers || ModOptions.MarkingsGenerated == Domain.MarkingsSource.IMTWithANHelpersAndHiddenANMarkings))
		{
			return;
		}

		var markings = MarkingsUtil.GenerateMarkings(RoadBuilderUtil.CurrentRoad);
		var markup = SingletonManager<SegmentMarkupManager>.Instance[segmentId];
		var pointsA = GetPoints(((Markup)markup).Enters.First());
		var pointsB = GetPoints(((Markup)markup).Enters.Last());

		AddLines(markings, markup, pointsA, pointsB);

		AddFillers(markings, markup, pointsA, pointsB);
	}

	private static void AddFillers(MarkingsInfo markings, SegmentMarkup markup, Dictionary<float, MarkupEnterPoint> pointsA, Dictionary<float, MarkupEnterPoint> pointsB)
	{
		foreach (var item in markings.Fillers)
		{
			if (!pointsA.ContainsKey(item.LeftPoint.X) || !pointsB.ContainsKey(item.LeftPoint.X))
			{
				Debug.LogError("Point Not Found: " + item.LeftPoint.X);
				continue;
			}

			if (!pointsA.ContainsKey(item.RightPoint.X) || !pointsB.ContainsKey(item.RightPoint.X))
			{
				Debug.LogError("Point Not Found: " + item.RightPoint.X);
				continue;
			}

			var elevation = getElevation(item);
			var curb = item.Type != LaneDecoration.Filler ? PrefabCollection<NetInfo>.FindLoaded("2187355678.R69 Prague Curb Network_Data") : null;
			var vertices = new[]
			{
				new EnterFillerVertex(pointsA[item.LeftPoint.X]),
				new EnterFillerVertex(pointsA[item.RightPoint.X]),
				new EnterFillerVertex(pointsB[item.RightPoint.X]),
				new EnterFillerVertex(pointsB[item.LeftPoint.X]),
			};

			FillerStyle style;

			switch (item.Type)
			{
				case LaneDecoration.Filler:
					var info = item.IMT_Info;

					if (info == null)
					{
						continue;
					}

					switch (info.MarkingStyle)
					{
						case Domain.MarkingFillerType.Filled:
							style = new SolidFillerStyle(info.Color, 0F, 0F);
							break;
						case Domain.MarkingFillerType.Dashed:
							style = new StripeFillerStyle(info.Color, info.DashLength, 0F, 0F, 90F, info.DashSpace, true);
							break;
						case Domain.MarkingFillerType.Striped:
							style = new StripeFillerStyle(info.Color, info.DashLength, 0F, 0F, 45F, info.DashSpace, true);
							break;
						case Domain.MarkingFillerType.Arrows:
							style = new ChevronFillerStyle(info.Color, info.DashLength, 0F, 0F, 90F, info.DashSpace);
							break;
						case Domain.MarkingFillerType.FilledWithArrows:
							markup.AddFiller(new MarkupFiller(new FillerContour(markup, vertices), new SolidFillerStyle(info.Color, 0F, 0F)));
							style = new ChevronFillerStyle(info.Color, info.DashLength, 0F, 0F, 90F, info.DashSpace);
							break;
						default:
							continue;
					}
					break;
				case LaneDecoration.Grass:
					style = new GrassFillerStyle(Color.white, 1F, 0.2F, 0F, elevation + 0.01F, 0F, 0F, 0F, 0F);
					break;
				case LaneDecoration.Pavement:
					style = new PavementFillerStyle(Color.white, 1F, 0.2F, 0F, elevation + 0.01F, 0F, 0F);
					break;
				case LaneDecoration.Gravel:
					style = new GravelFillerStyle(Color.white, 1F, 0.2F, 0F, elevation + 0.01F, 0F, 0F, 0F, 0F);
					break;
				default:
					continue;
			}

			if (item.Type != LaneDecoration.Filler)
			{
				if (curb == null)
				{
					if (style is CurbTriangulationFillerStyle cstyle)
					{
						cstyle.CurbSize.Value = 0.2F;
					}
				}
				else
				{
					var networkLine1 = new NetworkLineStyle(curb, (item.LeftPoint.RightLane?.FillerPadding.HasFlag(FillerPadding.Left) ?? false) ? 0.15F : 0.3F, elevation + 0.05F, 1F, 0F, 0F, 64, false);
					var networkLine2 = new NetworkLineStyle(curb, (item.RightPoint.LeftLane?.FillerPadding.HasFlag(FillerPadding.Right) ?? false) ? 0.15F : 0.3F, elevation + 0.05F, 1F, 0F, 0F, 64, false);

					if (pointsA[item.LeftPoint.X].Lines.FirstOrDefault() is MarkupRegularLine line1)
					{
						line1.AddRule(networkLine1, false);
					}
					else
					{
						markup.AddRegularLine(new MarkupPointPair(pointsA[item.LeftPoint.X], pointsB[item.LeftPoint.X]), networkLine1);
					}

					if (pointsA[item.RightPoint.X].Lines.FirstOrDefault() is MarkupRegularLine line2)
					{
						line2.AddRule(networkLine2, false);
					}
					else
					{
						markup.AddRegularLine(new MarkupPointPair(pointsA[item.RightPoint.X], pointsB[item.RightPoint.X]), networkLine2);
					}
				}
			}

			markup.AddFiller(new MarkupFiller(new FillerContour(markup, vertices), style));
		}
	}

	private static float getElevation(FillerMarking item)
	{
		var elevation = item.Elevation;
		var lanes = new List<LaneInfo>(item.Lanes);

		if (lanes[0].LeftLane != null)
			lanes.Add(lanes.First().LeftLane);

		if (lanes.Last()?.RightLane != null)
			lanes.Add(lanes.Last().RightLane);

		return elevation - lanes.Select(x => ThumbnailMakerUtil.GetLaneVerticalOffset(x, RoadBuilderUtil.CurrentRoad)).Average();
	}

	private static void AddLines(MarkingsInfo markings, SegmentMarkup markup, Dictionary<float, MarkupEnterPoint> pointsA, Dictionary<float, MarkupEnterPoint> pointsB)
	{
		foreach (var item in markings.Lines.Values)
		{
			if (!pointsA.ContainsKey(item.Point.X) || !pointsB.ContainsKey(item.Point.X))
			{
				Debug.LogError("Point Not Found: " + item.Point.X);
				continue;
			}

			var pair = new MarkupPointPair(pointsA[item.Point.X], pointsB[item.Point.X]);
			var info = item.IMT_Info;

			if (info == null)
			{
				continue;
			}

			RegularLineStyle style;

			switch (info.MarkingStyle)
			{
				case Domain.MarkingLineType.Solid:
					style = new SolidLineStyle(info.Color, info.LineWidth);
					break;
				case Domain.MarkingLineType.SolidDouble:
					style = new DoubleSolidLineStyle(info.Color, info.Color, false, info.LineWidth, info.LineWidth);
					break;
				case Domain.MarkingLineType.Dashed:
					style = new DashedLineStyle(info.Color, info.LineWidth, info.DashLength, info.DashSpace);
					break;
				case Domain.MarkingLineType.DashedDouble:
					style = new DoubleDashedLineStyle(info.Color, info.Color, false, info.LineWidth, info.DashLength, info.DashSpace, info.LineWidth);
					break;
				case Domain.MarkingLineType.SolidDashed:
					style = new SolidAndDashedLineStyle(info.Color, info.Color, false, info.LineWidth, info.DashLength, info.DashSpace, info.LineWidth);
					break;
				case Domain.MarkingLineType.DashedSolid:
					var line = new SolidAndDashedLineStyle(info.Color, info.Color, false, info.LineWidth, info.DashLength, info.DashSpace, info.LineWidth);
					line.Invert.Value = true;
					style = line;
					break;
				default:
					continue;
			}

			markup.AddRegularLine(pair, style);
		}
	}

	private static Dictionary<float, MarkupEnterPoint> GetPoints(Enter enter)
	{
		var points = new Dictionary<float, MarkupEnterPoint>();

		foreach (var item in enter.Points)
		{
			points[GetSegmentPosition(item)] = item;
		}

		return points;
	}

	public static float GetSegmentPosition(MarkupEnterPoint point)
	{
		var source = (NetInfoPointSource)point.Source;

		if ((source.Location & MarkupPoint.LocationType.Between) != MarkupPoint.LocationType.None)
		{
			return (point.Enter.IsLaneInvert ? -source.RightLane.HalfWidth : source.RightLane.HalfWidth) + source.RightLane.Position;
		}

		else if ((source.Location & MarkupPoint.LocationType.Edge) != MarkupPoint.LocationType.None)
		{
			switch (source.Location)
			{
				case MarkupPoint.LocationType.LeftEdge:
					return (point.Enter.IsLaneInvert ? -source.RightLane.HalfWidth : source.RightLane.HalfWidth) + source.RightLane.Position;

				case MarkupPoint.LocationType.RightEdge:
					return (point.Enter.IsLaneInvert ? source.LeftLane.HalfWidth : -source.LeftLane.HalfWidth) + source.LeftLane.Position;
			}
		}

		return 0F;
	}
}
