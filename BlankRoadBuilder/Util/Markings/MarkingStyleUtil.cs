using BlankRoadBuilder.Domain;
using BlankRoadBuilder.Domain.Options;
using BlankRoadBuilder.ThumbnailMaker;

using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace BlankRoadBuilder.Util.Markings;

public class MarkingStyleUtil
{
    public static LineInfo? GetLineMarkingInfo(GenericMarkingType type)
    {
        if (ModOptions.MarkingsStyle == MarkingStyle.Custom)
        {
            return CustomLineMarkings.ContainsKey(type) ? CustomLineMarkings[type].AsLine() : null;
        }

        if (!_markings.ContainsKey(ModOptions.MarkingsStyle))
        {
            return null;
        }

        return !_markings[ModOptions.MarkingsStyle].ContainsKey(type) ? null : _markings[ModOptions.MarkingsStyle][type];
    }

    public static FillerInfo? GetFillerMarkingInfo(LaneType type)
    {
        if (ModOptions.MarkingsStyle == MarkingStyle.Custom)
        {
            return CustomFillerMarkings.ContainsKey(type) ? CustomFillerMarkings[type].AsFiller() : null;
        }

        if (!_fillers.ContainsKey(ModOptions.MarkingsStyle))
        {
            return null;
        }

        return !_fillers[ModOptions.MarkingsStyle].ContainsKey(type) ? null : _fillers[ModOptions.MarkingsStyle][type];
    }

    public static Dictionary<GenericMarkingType, SavedLineOption> CustomLineMarkings { get; }
    public static Dictionary<LaneType, SavedFillerOption> CustomFillerMarkings { get; }

    private static readonly Dictionary<MarkingStyle, Dictionary<GenericMarkingType, LineInfo>> _markings;
    private static readonly Dictionary<MarkingStyle, Dictionary<LaneType, FillerInfo>> _fillers;

    static MarkingStyleUtil()
    {
        _markings = new Dictionary<MarkingStyle, Dictionary<GenericMarkingType, LineInfo>>
        {
            { MarkingStyle.Vanilla, MarkingStylesTemplates.Vanilla() },
            { MarkingStyle.USA, MarkingStylesTemplates.USA() },
            { MarkingStyle.UK, MarkingStylesTemplates.UK() },
            { MarkingStyle.Canada, MarkingStylesTemplates.Canada() },
            { MarkingStyle.Netherlands, MarkingStylesTemplates.Netherlands() }
        };

        _fillers = new Dictionary<MarkingStyle, Dictionary<LaneType, FillerInfo>>
        {
            { MarkingStyle.Vanilla, MarkingStylesTemplates.Vanilla_Fillers() },
            { MarkingStyle.USA, MarkingStylesTemplates.USA_Fillers() },
            { MarkingStyle.UK, MarkingStylesTemplates.UK_Fillers() },
            { MarkingStyle.Canada, MarkingStylesTemplates.Canada_Fillers() },
            { MarkingStyle.Netherlands, MarkingStylesTemplates.Netherlands_Fillers() }
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
            LaneType.Car,
            LaneType.Parking,
            LaneType.Bike,
            LaneType.Tram,
            LaneType.Bus,
            LaneType.Trolley,
            LaneType.Emergency,
            LaneType.Pedestrian,
        };

        CustomLineMarkings = customMarkingTypes.ToDictionary(x => x, x => new SavedLineOption(x));
        CustomFillerMarkings = customFillerTypes.ToDictionary(x => x, x => new SavedFillerOption(x));
    }

    public class LineInfo
    {
        public MarkingLineType MarkingStyle { get; set; }
        public float LineWidth { get; set; } = 0.15F;
        public float DashLength { get; set; } = 1F;
        public float DashSpace { get; set; } = 1F;
        public Color32 Color { get; set; } = new Color32(255, 255, 255, 255);
    }

    public class FillerInfo
    {
        public MarkingFillerType MarkingStyle { get; set; }
        public float DashLength { get; set; } = 1F;
        public float DashSpace { get; set; } = 1F;
        public Color32 Color { get; set; } = new Color32(255, 255, 255, 255);
    }
}