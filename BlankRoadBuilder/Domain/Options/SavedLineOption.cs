using BlankRoadBuilder.Util.Markings;
using ColossalFramework;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlankRoadBuilder.Domain.Options;
public class SavedLineOption
{
    private readonly SavedInt _lineType;
    private readonly SavedFloat _dashLength;
    private readonly SavedFloat _dashSpace;
    private readonly SavedFloat _lineWidth;
    private readonly SavedInt _colorR;
    private readonly SavedInt _colorG;
    private readonly SavedInt _colorB;
    private readonly SavedInt _colorA;

    public GenericMarkingType GenericMarking { get; }

    public SavedLineOption(GenericMarkingType genericMarking, MarkingType type)
    {
        var baseName = $"Marking_{(int)type}_{(int)genericMarking}";

        _lineType = new(baseName + nameof(_lineType), nameof(BlankRoadBuilder), (int)MarkingLineType.None);
		_dashSpace = new(baseName + nameof(_dashSpace), nameof(BlankRoadBuilder), 1F);
        _dashLength = new(baseName + nameof(_dashLength), nameof(BlankRoadBuilder), 1F);
        _lineWidth = new(baseName + nameof(_lineWidth), nameof(BlankRoadBuilder), 0.15F);
        _colorR = new(baseName + nameof(_colorR), nameof(BlankRoadBuilder), 255);
        _colorG = new(baseName + nameof(_colorG), nameof(BlankRoadBuilder), 255);
        _colorB = new(baseName + nameof(_colorB), nameof(BlankRoadBuilder), 255);
        _colorA = new(baseName + nameof(_colorA), nameof(BlankRoadBuilder), 255);

        GenericMarking = genericMarking;
    }

    public MarkingLineType MarkingType { get => (MarkingLineType)_lineType.value; set => _lineType.value = (int)value; }
    public float DashSpace { get => _dashSpace; set => _dashSpace.value = value; }
    public float DashLength { get => _dashLength; set => _dashLength.value = value; }
    public float LineWidth { get => _lineWidth; set => _lineWidth.value = value; }
    public int R { get => _colorR; set => _colorR.value = value; }
    public int G { get => _colorG; set => _colorG.value = value; }
    public int B { get => _colorB; set => _colorB.value = value; }
    public int A { get => _colorA; set => _colorA.value = value; }

	public void Set(MarkingStyleUtil.LineInfo lineInfo)
    {
		MarkingType = lineInfo.MarkingStyle;
        DashSpace = lineInfo.DashSpace;
		DashLength = lineInfo.DashLength;
		LineWidth = lineInfo.LineWidth;
        R = lineInfo.Color.r;
        G = lineInfo.Color.g;
        B = lineInfo.Color.b;
        A = lineInfo.Color.a;
	}

	public MarkingStyleUtil.LineInfo? AsLine()
    {
        return new MarkingStyleUtil.LineInfo
        {
            MarkingStyle = MarkingType,
            LineWidth = LineWidth,
            DashLength = DashLength,
            DashSpace = DashSpace,
            Color = new UnityEngine.Color32((byte)R, (byte)G, (byte)B, (byte)A)
        };
    }
}
