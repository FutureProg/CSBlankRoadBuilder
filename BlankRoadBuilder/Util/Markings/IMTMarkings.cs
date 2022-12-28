using BlankRoadBuilder.Patches;
using ModsCommon;
using NodeMarkup.Manager;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace BlankRoadBuilder.Util.Markings;
public class IMTMarkings
{
	public static void ApplyMarkings(ushort segmentId)
	{
		if (RoadBuilderUtil.CurrentRoad == null)
			return;

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

			var info = item.FillerInfo;

			if (info == null)
				continue;

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
				case ThumbnailMaker.LaneDecoration.Filler:
					style = new SolidFillerStyle(info.Color, 0F, 0F);
					break;
				case ThumbnailMaker.LaneDecoration.Grass:
					style = new GrassFillerStyle(info.Color, 0F, 0.2F, 0F, 0.01F, 0F, 0F, 0F, 0F);
					break;
				case ThumbnailMaker.LaneDecoration.Pavement:
					style = new PavementFillerStyle(info.Color, 0F, 0.2F, 0F, 0.01F, 0F, 0F);
					break;
				case ThumbnailMaker.LaneDecoration.Gravel:
					style = new GravelFillerStyle(info.Color, 0F, 0.2F, 0F, 0.01F, 0F, 0F, 0F, 0F);
					break;
				default:
					continue;
			}

			if (item.Type != ThumbnailMaker.LaneDecoration.Filler)
			{
				var curb = PrefabCollection<NetInfo>.FindLoaded("");

				if (curb == null)
				{
					if (style is CurbTriangulationFillerStyle cstyle)
						cstyle.CurbSize.Value = 0.2F;
				}
				else
				{
					markup.AddRegularLine(new MarkupPointPair(pointsA[item.LeftPoint.X], pointsB[item.LeftPoint.X]), new NetworkLineStyle(curb, 0.15F, 0F, 100F, 0F, 0F, 0, false));
					markup.AddRegularLine(new MarkupPointPair(pointsA[item.RightPoint.X], pointsB[item.RightPoint.X]), new NetworkLineStyle(curb, -0.15F, 0F, 100F, 0F, 0F, 0, false));
				}
			}
			
			markup.AddFiller(new MarkupFiller(new FillerContour(markup, vertices), style));
		}
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
			var info = item.LineInfo;

			if (info == null)
				continue;

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
			points[item.LinePosition] = item;
		}

		return points;
	}
}
