using HarmonyLib;

using KianCommons;

using NodeMarkup.Manager;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace BlankRoadBuilder.Patches;
internal class Segment
{
	//[HarmonyPatch(typeof(NetSegment), nameof(NetSegment.UpdateEndSegments))]
	//static class UpdateEndSegments
	//{
	//	static void Postfix(ushort segmentID)
	//	{
	//		if (!NetUtil.IsSegmentValid(segmentID))
	//			return;

	//		ref var segment = ref segmentID.ToSegment();

	//		var markup = new SegmentMarkup(segmentID);

	//		foreach (var x in markup.Enters)
	//		{
	//			Debug.LogError($"{x.Position} {string.Join(",", x.Points.Select(y => y.Id.ToString()).ToArray())}");
	//		}
	//	}
	//}

	//// also called in after deserialize
	//[HarmonyPatch(typeof(NetSegment), nameof(NetSegment.UpdateEndSegments))]
	//static class UpdateEndSegments
	//{
	//	static void Postfix(ushort segmentID) => UpdateSegmentsCommons.Postfix(segmentID, false);
	//}
}
