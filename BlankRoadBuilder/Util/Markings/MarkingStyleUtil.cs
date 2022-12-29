using BlankRoadBuilder.Domain;
using BlankRoadBuilder.Domain.Options;
using BlankRoadBuilder.ThumbnailMaker;

using System;
using System.Collections.Generic;
using System.Linq;

namespace BlankRoadBuilder.Util.Markings;

public partial class MarkingStyleUtil
{
	public static Dictionary<MarkingType, Dictionary<GenericMarkingType, SavedLineOption>> CustomLineMarkings { get; }
	public static Dictionary<MarkingType, Dictionary<LaneType, SavedFillerOption>> CustomFillerMarkings { get; }

	public static LineInfo? GetLineMarkingInfo(GenericMarkingType type, MarkingType markingType)
    {
        if (ModOptions.MarkingsStyle == MarkingStyle.Custom)
        {
            return CustomLineMarkings[markingType].ContainsKey(type) ? CustomLineMarkings[markingType][type].AsLine() : null;
		}

		var markings = _markings(ModOptions.MarkingsStyle, markingType);

		return (markings?.ContainsKey(type) ?? false) ? markings[type] : null;
    }

    public static FillerInfo? GetFillerMarkingInfo(LaneType type, MarkingType markingType)
    {
        if (ModOptions.MarkingsStyle == MarkingStyle.Custom)
        {
            return CustomFillerMarkings[markingType].ContainsKey(type) ? CustomFillerMarkings[markingType][type].AsFiller() : null;
        }

        var fillers = _fillers(ModOptions.MarkingsStyle, markingType);

        return (fillers?.ContainsKey(type) ?? false) ? fillers[type] : null;
    }

    internal static readonly Func<MarkingStyle, MarkingType, Dictionary<GenericMarkingType, LineInfo>?> _markings;

	internal static readonly Func<MarkingStyle, MarkingType, Dictionary<LaneType, FillerInfo>?> _fillers;

    static MarkingStyleUtil()
    {
        _markings = (m, s) => m switch
        {
            MarkingStyle.Vanilla => MarkingStylesTemplates.Vanilla(s),
            MarkingStyle.USA => MarkingStylesTemplates.USA(s),
            MarkingStyle.UK => MarkingStylesTemplates.UK(s),
            MarkingStyle.Canada => MarkingStylesTemplates.Canada(s),
            MarkingStyle.Netherlands => MarkingStylesTemplates.Netherlands(s),
            _ => null
        };

		_fillers = (m, s) => m switch
		{
			MarkingStyle.Vanilla => MarkingStylesTemplates.Vanilla_Fillers(s),
			MarkingStyle.USA => MarkingStylesTemplates.USA_Fillers(s),
			MarkingStyle.UK => MarkingStylesTemplates.UK_Fillers(s),
			MarkingStyle.Canada => MarkingStylesTemplates.Canada_Fillers(s),
			MarkingStyle.Netherlands => MarkingStylesTemplates.Netherlands_Fillers(s),
            _ => null
		};

		var customMarkingTypes = new[]
        {
            GenericMarkingType.End,
            GenericMarkingType.Parking,
            GenericMarkingType.Normal | GenericMarkingType.Soft,
            GenericMarkingType.Normal | GenericMarkingType.Hard,
            GenericMarkingType.Flipped | GenericMarkingType.Hard,
            GenericMarkingType.Flipped | GenericMarkingType.End,
            GenericMarkingType.Normal | GenericMarkingType.Bike,
            GenericMarkingType.Flipped | GenericMarkingType.Bike,
            GenericMarkingType.Normal | GenericMarkingType.Tram,
            GenericMarkingType.Flipped | GenericMarkingType.Tram,
        };

        var customFillerTypes = new[]
        {
            LaneType.Filler,
            LaneType.Car,
			LaneType.Parking,
            LaneType.Bike,
            LaneType.Tram,
            LaneType.Bus,
            LaneType.Trolley,
            LaneType.Emergency,
            LaneType.Pedestrian,
        };

        CustomLineMarkings = new Dictionary<MarkingType, Dictionary<GenericMarkingType, SavedLineOption>>
        {
            { MarkingType.IMT, customMarkingTypes.ToDictionary(x => x, x => new SavedLineOption(x, MarkingType.IMT)) },
            { MarkingType.AN, customMarkingTypes.ToDictionary(x => x, x => new SavedLineOption(x, MarkingType.AN)) },
        };

        CustomFillerMarkings = new Dictionary<MarkingType, Dictionary<LaneType, SavedFillerOption>>
		{
			{ MarkingType.IMT, customFillerTypes.ToDictionary(x => x, x => new SavedFillerOption(x, MarkingType.IMT)) },
			{ MarkingType.AN, customFillerTypes.ToDictionary(x => x, x => new SavedFillerOption(x, MarkingType.AN)) },
		};
    }
}