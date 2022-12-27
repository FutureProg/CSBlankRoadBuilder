using BlankRoadBuilder.Util.Markings;

using ColossalFramework.Math;

using HarmonyLib;

using KianCommons;

using ModsCommon;

using NodeMarkup.Manager;
using NodeMarkup.Tools;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace BlankRoadBuilder.Patches;
internal class Segment
{
	public static class UpdateEndSegments
	{
		public static void Postfix(ushort segment)
		{
			if (!NetUtil.IsSegmentValid(segment) || SavePanelPatch.LastLoadedRoad == null)
				return;

			var markings = MarkingsUtil.GenerateMarkings(SavePanelPatch.LastLoadedRoad);
			var markup = SingletonManager<SegmentMarkupManager>.Instance[segment];
			var pointsA = GetPoints(((Markup)markup).Enters.First());
			var pointsB = GetPoints(((Markup)markup).Enters.Last());

			Debug.Log(string.Format("\r\n",pointsA.Keys.Select(x=>x.ToString()).ToArray()));

			foreach (var item in markings.Lines)
			{
				if(!pointsA.ContainsKey(item.Key.X)||! pointsB.ContainsKey(-item.Key.X))
				{
					Debug.LogError("Point Not Found: " + item.Key.X);
					continue;
				}
				var pair = new MarkupPointPair(pointsA[item.Key.X], pointsB[-item.Key.X]);
				var info = item.Value.LineInfo;
				
				if (info == null)
					continue;
				
				switch (info.MarkingStyle)
				{
					case Domain.MarkingLineType.None:
						break;
					case Domain.MarkingLineType.Solid:
						markup.AddRegularLine(pair, new SolidLineStyle(info.Color, info.LineWidth));
						break;
					case Domain.MarkingLineType.SolidDouble:
						markup.AddRegularLine(pair, new DoubleSolidLineStyle(info.Color, info.Color, false, info.LineWidth, info.LineWidth));
						break;
					case Domain.MarkingLineType.Dashed:
						markup.AddRegularLine(pair, new DashedLineStyle(info.Color, info.LineWidth, info.DashLength, info.DashSpace));
						break;
					case Domain.MarkingLineType.DashedDouble:
						markup.AddRegularLine(pair, new DoubleDashedLineStyle(info.Color, info.Color, false, info.LineWidth, info.DashLength, info.DashSpace, info.LineWidth));
						break;
					case Domain.MarkingLineType.SolidDashed:
						markup.AddRegularLine(pair, new SolidAndDashedLineStyle(info.Color, info.Color, false, info.LineWidth, info.DashLength, info.DashSpace, info.LineWidth));
						break;
					case Domain.MarkingLineType.DashedSolid:
						var line = new SolidAndDashedLineStyle(info.Color, info.Color, false, info.LineWidth, info.DashLength, info.DashSpace, info.LineWidth);
						line.Invert.Value = true;
						markup.AddRegularLine(pair, line);
						break;
				}
			}
		}

		private static Dictionary<float, MarkupEnterPoint> GetPoints(Enter enter)
		{
			var points = new Dictionary<float, MarkupEnterPoint>();

			foreach (var item in enter.Points)
			{
				Debug.Log($"{item.Index} {item.LinePosition}");

				points[item.LinePosition] = item;
			}

			return points;
		}
	}
}
