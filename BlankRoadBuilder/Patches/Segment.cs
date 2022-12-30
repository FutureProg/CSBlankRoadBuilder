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
			IMTMarkings.ApplyMarkings(segment);
		}
	}
}
