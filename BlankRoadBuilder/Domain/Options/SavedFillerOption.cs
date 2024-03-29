﻿using BlankRoadBuilder.ThumbnailMaker;
using BlankRoadBuilder.Util.Markings;

using ColossalFramework;

using UnityEngine;

namespace BlankRoadBuilder.Domain.Options;
public class SavedFillerOption
{
	private readonly SavedInt _fillerType;
	private readonly SavedFloat _dashLength;
	private readonly SavedFloat _dashSpace;
	private readonly SavedInt _colorR;
	private readonly SavedInt _colorG;
	private readonly SavedInt _colorB;
	private readonly SavedInt _colorA;

	public LaneType LaneType { get; }

	public SavedFillerOption(LaneType laneType, MarkingType type)
	{
		var baseName = $"Filler_{(int)type}_{(int)laneType}";

		_fillerType = new(baseName + nameof(_fillerType), nameof(BlankRoadBuilder), (int)MarkingFillerType.Filled);
		_dashLength = new(baseName + nameof(_dashLength), nameof(BlankRoadBuilder), 1F);
		_dashSpace = new(baseName + nameof(_dashSpace), nameof(BlankRoadBuilder), 1F);
		_colorR = new(baseName + nameof(_colorR), nameof(BlankRoadBuilder), 255);
		_colorG = new(baseName + nameof(_colorG), nameof(BlankRoadBuilder), 255);
		_colorB = new(baseName + nameof(_colorB), nameof(BlankRoadBuilder), 255);
		_colorA = new(baseName + nameof(_colorA), nameof(BlankRoadBuilder), 255);

		LaneType = laneType;
	}

	public MarkingFillerType MarkingType { get => (MarkingFillerType)_fillerType.value; set => _fillerType.value = (int)value; }
	public float DashSpace { get => _dashSpace; set => _dashSpace.value = value; }
	public float DashLength { get => _dashLength; set => _dashLength.value = value; }
	public int R { get => _colorR; set => _colorR.value = value; }
	public int G { get => _colorG; set => _colorG.value = value; }
	public int B { get => _colorB; set => _colorB.value = value; }
	public int A { get => _colorA; set => _colorA.value = value; }

	public Color32 Color => new Color32((byte)R, (byte)G, (byte)B, (byte)A);

	public void Set(MarkingStyleUtil.FillerInfo lineInfo)
	{
		MarkingType = lineInfo.MarkingStyle;
		DashSpace = lineInfo.DashSpace;
		DashLength = lineInfo.DashLength;
		R = lineInfo.Color.r;
		G = lineInfo.Color.g;
		B = lineInfo.Color.b;
		A = lineInfo.Color.a;
	}

	public MarkingStyleUtil.FillerInfo? AsFiller()
	{
		return new MarkingStyleUtil.FillerInfo
		{
			MarkingStyle = MarkingType,
			DashLength = DashLength,
			DashSpace = DashSpace,
			Color = new UnityEngine.Color32((byte)R, (byte)G, (byte)B, (byte)A)
		};
	}
}
