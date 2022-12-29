using AlgernonCommons.UI;

using BlankRoadBuilder.Domain;
using BlankRoadBuilder.Domain.Options;
using BlankRoadBuilder.ThumbnailMaker;
using BlankRoadBuilder.Util.Markings;
using ColossalFramework.UI;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEngine;

namespace BlankRoadBuilder.UI.Options;
internal partial class FillerOptions : MarkingsOptions
{
	public override string TabName => "Custom Filler Settings";

	public FillerOptions(MarkingType markingType, UITabstrip tabStrip, int tabIndex) : base(markingType, tabStrip, tabIndex)
	{
		foreach (var option in MarkingStyleUtil.CustomFillerMarkings[markingType])
		{
			AddFillerOption(option.Key, option.Value);
		}
	}
}

internal partial class LineOptions : MarkingsOptions
{
	public override string TabName => "Custom Line Settings";

	public LineOptions(MarkingType markingType, UITabstrip tabStrip, int tabIndex) : base(markingType, tabStrip, tabIndex)
	{
		foreach (var option in MarkingStyleUtil.CustomLineMarkings[markingType])
		{
			AddLineOption(option.Key, option.Value);
		}
	}
}