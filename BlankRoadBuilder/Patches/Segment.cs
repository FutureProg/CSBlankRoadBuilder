﻿using BlankRoadBuilder.Util.Markings;

namespace BlankRoadBuilder.Patches;
internal class Segment
{
	public static class UpdateEndSegments
	{
		public static void Postfix(ushort segment)
		{
			if (ToolsModifierControl.isAssetEditor)
			{
				new IMTMarkings().ApplyMarkings(segment);
			}
		}
	}
}
