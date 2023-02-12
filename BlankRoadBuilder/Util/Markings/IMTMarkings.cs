using BlankRoadBuilder.Domain.Options;
using BlankRoadBuilder.ThumbnailMaker;

using IMT.API;

using KianCommons;

using ModsCommon.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace BlankRoadBuilder.Util.Markings;
public class IMTMarkings
{
	private IDataProviderV1 Provider { get; }

	public IMTMarkings()
	{
		Provider = Helper.GetProvider(nameof(BlankRoadBuilder));
	}

	public void ApplyMarkings(ushort segmentId)
	{
		if (!ModOptions.MarkingsGenerated.HasFlag(Domain.MarkingsSource.IMTMarkings))
		{
			return;
		}

		if (!NetUtil.IsSegmentValid(segmentId) || RoadBuilderUtil.CurrentRoad == null || !(ToolsModifierControl.toolController.m_editPrefabInfo is NetInfo net && net.GetElevations().Any(x => x.Value == segmentId.GetSegment().Info)))
		{
			return;
		}

		var markings = MarkingsUtil.GenerateMarkings(RoadBuilderUtil.CurrentRoad);
		var markup = Provider?.GetOrCreateSegmentMarking(segmentId);

		if (markup == null || markings == null)
		{
			Debug.LogError(Provider == null ? "IMT provider is missing" : "Failed to get the Markup information");
			return;
		}

		markup.ClearMarkings();
		markup.ResetPointOffsets();

		var pointsA = GetPoints(markup.StartEntrance);
		var pointsB = GetPoints(markup.EndEntrance);

		foreach (var item in markings.Lines.Values)
		{
			try
			{ AddLines(item, markup, pointsA, pointsB); }
			catch (Exception ex) { Debug.LogException(ex); }
		}

		foreach (var item in markings.Fillers)
		{
			try
			{ AddFillers(item, markup, pointsA, pointsB); }
			catch (Exception ex) { Debug.LogException(ex); }
		}
	}

	private void AddFillers(FillerMarking item, ISegmentMarkingData markup, Dictionary<float, IEntrancePointData> pointsA, Dictionary<float, IEntrancePointData> pointsB)
	{
		if (item.Type != LaneDecoration.Filler && ModOptions.MarkingsGenerated.HasAnyFlag(Domain.MarkingsSource.MeshFillers, Domain.MarkingsSource.HiddenVanillaMarkings))
		{
			return;
		}

		if (!pointsA.ContainsKey(item.LeftPoint.X) || !pointsB.ContainsKey(item.LeftPoint.X))
		{
			Debug.LogError("Point Not Found: " + item.LeftPoint.X);
			return;
		}

		if (!pointsA.ContainsKey(item.RightPoint.X) || !pointsB.ContainsKey(item.RightPoint.X))
		{
			Debug.LogError("Point Not Found: " + item.RightPoint.X);
			return;
		}

		var style = GenerateFillerStyle(item);

		if (style == null)
			return;

		if (item.Type == LaneDecoration.Filler && item.IMT_Info?.MarkingStyle != Domain.MarkingFillerType.Filled && style is IGuidedFillerStyleData guidedFiller)
		{
			var forward = item.Lanes.First().Direction == LaneDirection.Forward;

			guidedFiller.LeftGuideA = forward ? 2 : 4;
			guidedFiller.LeftGuideB = forward ? 3 : 1;
			guidedFiller.RightGuideA = forward ? 4 : 2;
			guidedFiller.RightGuideB = forward ? 1 : 3;
		}

		markup.AddFiller(new[]
		{
			pointsA[item.LeftPoint.X],
			pointsA[item.RightPoint.X],
			pointsB[item.RightPoint.X],
			pointsB[item.LeftPoint.X],
		}, style);
	}

	private IFillerStyleData? GenerateFillerStyle(FillerMarking item)
	{
		if (item.Type is LaneDecoration.Filler)
		{
			var info = item.IMT_Info;

			if (info == null)
			{
				return null;
			}

			switch (info.MarkingStyle)
			{
				case Domain.MarkingFillerType.Filled:
					var solidFiller = Provider.SolidFillerStyle;

					solidFiller.Color = info.Color;

					return solidFiller;

				case Domain.MarkingFillerType.Dashed:
					var dashedFiller = Provider.StripeFillerStyle;

					dashedFiller.Color = info.Color;
					dashedFiller.Width = info.DashLength;
					dashedFiller.Step = info.DashSpace;
					dashedFiller.FollowGuides = true;

					return dashedFiller;

				case Domain.MarkingFillerType.Striped:
					var stripeFiller = Provider.StripeFillerStyle;

					stripeFiller.Color = info.Color;
					stripeFiller.Width = info.DashLength;
					stripeFiller.Step = info.DashSpace;
					stripeFiller.Angle = 45F;
					stripeFiller.FollowGuides = true;

					return stripeFiller;

				case Domain.MarkingFillerType.Arrows:
					var arrowFiller = Provider.ChevronFillerStyle;

					arrowFiller.Color = info.Color;
					arrowFiller.Width = info.DashLength;
					arrowFiller.Step = info.DashSpace;
					arrowFiller.AngleBetween = 90F;

					return arrowFiller;

					//case Domain.MarkingFillerType.FilledWithArrows:
					//	if (info.MarkingStyle == Domain.MarkingFillerType.FilledWithArrows)
					//		markup.AddFiller(new MarkupFiller(new FillerContour(markup, vertices), new SolidFillerStyle(info.Color, 0F, 0F)));

					//	style = new ChevronFillerStyle(new Color32(180, 180, 180, 140), info.DashLength, 0F, 0F, 90F, info.DashSpace);
					//	break;
					//default:
					//	return;
			}

			return null;
		}

		var leftPadded = item.LeftPoint.RightLane?.FillerPadding.HasFlag(FillerPadding.Left) ?? false;
		var rightPadded = item.RightPoint.LeftLane?.FillerPadding.HasFlag(FillerPadding.Right) ?? false;

		switch (item.Type)
		{
			case LaneDecoration.Grass:
				var grass = Provider.GrassFillerStyle;
				grass.Elevation = getElevation() + 0.01F;
				grass.LineOffset = leftPadded || rightPadded ? 0F : 0.2F;
				grass.CurbSize = 0.3F;
				return grass;

			case LaneDecoration.Pavement:
				var pavement = Provider.PavementFillerStyle;
				pavement.Elevation = getElevation() + 0.01F;
				pavement.LineOffset = leftPadded || rightPadded ? 0F : 0.2F;
				return pavement;

			case LaneDecoration.Gravel:
				var gravel = Provider.GravelFillerStyle;
				gravel.Elevation = getElevation() + 0.01F;
				gravel.LineOffset = leftPadded || rightPadded ? 0F : 0.2F;
				gravel.CurbSize = item.Elevation == item.Lanes.Min(x => x.SurfaceElevation) ? 0F : 0.3F;
				return gravel;
		}

		return null;

		float getElevation()
		{
			var elevation = item.Elevation;
			var lanes = new List<LaneInfo>(item.Lanes);
			var left = lanes[0]?.LeftLane;
			var right = lanes[lanes.Count - 1]?.RightLane;

			if (left != null)
				lanes.Add(left);
			if (right != null)
				lanes.Add(right);

			return Math.Max(0, elevation - lanes.Select(x => x.LaneElevation).Average());
		}
	}

	private void AddLines(LineMarking item, ISegmentMarkingData markup, Dictionary<float, IEntrancePointData> pointsA, Dictionary<float, IEntrancePointData> pointsB)
	{
		if (!pointsA.ContainsKey(item.Point.X) || !pointsB.ContainsKey(item.Point.X))
		{
			Debug.LogError("Point Not Found: " + item.Point.X);
			return;
		}

		var style = GetLineStyle(item);

		if (style == null)
		{
			return;
		}

		markup.AddRegularLine(pointsA[item.Point.X], pointsB[item.Point.X], style);
	}

	private IRegularLineStyleData? GetLineStyle(LineMarking item)
	{
		var info = item.IMT_Info;

		switch (info?.MarkingStyle)
		{
			case Domain.MarkingLineType.Solid:
				var solidline = Provider.SolidLineStyle;

				solidline.Color = info.Color;
				solidline.Width = info.LineWidth;

				return solidline;

			case Domain.MarkingLineType.SolidDouble:
				var doubleSolidLine = Provider.DoubleSolidLineStyle;

				doubleSolidLine.Color = info.Color;
				doubleSolidLine.Width = info.LineWidth;
				doubleSolidLine.Offset = info.LineWidth;

				return doubleSolidLine;

			case Domain.MarkingLineType.Dashed:
				var dashedLine = Provider.DashedLineStyle;

				dashedLine.Color = info.Color;
				dashedLine.Width = info.LineWidth;
				dashedLine.DashLength = info.DashLength;
				dashedLine.SpaceLength = info.DashSpace;

				return dashedLine;

			case Domain.MarkingLineType.DashedDouble:
				var doubleDashedLine = Provider.DoubleDashedLineStyle;

				doubleDashedLine.Color = info.Color;
				doubleDashedLine.Width = info.LineWidth;
				doubleDashedLine.Offset = info.LineWidth;
				doubleDashedLine.DashLength = info.DashLength;
				doubleDashedLine.SpaceLength = info.DashSpace;

				return doubleDashedLine;

			case Domain.MarkingLineType.SolidDashed:
			case Domain.MarkingLineType.DashedSolid:
				var solidDashedLine = Provider.SolidAndDashedLineStyle;

				solidDashedLine.Color = info.Color;
				solidDashedLine.Width = info.LineWidth;
				solidDashedLine.Offset = info.LineWidth;
				solidDashedLine.DashLength = info.DashLength;
				solidDashedLine.SpaceLength = info.DashSpace;
				solidDashedLine.Invert = info?.MarkingStyle == Domain.MarkingLineType.DashedSolid;

				return solidDashedLine;
		}

		return null;
	}

	private static Dictionary<float, IEntrancePointData> GetPoints(INodeEntranceData enter)
	{
		var points = new Dictionary<float, IEntrancePointData>(new PointCompare());
		foreach (var item in enter.EntrancePoints)
		{
			points[item.Position] = item;
		}

		return points;
	}

	private class PointCompare : IEqualityComparer<float>
	{
		public bool Equals(float x, float y)
		{
			return x.ToString() == y.ToString();
		}

		public int GetHashCode(float obj)
		{
			return obj.ToString().GetHashCode();
		}
	}
}
