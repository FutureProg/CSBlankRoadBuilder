using BlankRoadBuilder.Domain;

using UnityEngine;

namespace BlankRoadBuilder.Util.Markings;

public partial class MarkingStyleUtil
{
	public class LineInfo
    {
        public MarkingLineType MarkingStyle { get; set; }
        public float LineWidth { get; set; } = 0.15F;
        public float DashLength { get; set; } = 1F;
        public float DashSpace { get; set; } = 1F;
        public Color32 Color { get; set; } = new Color32(255, 255, 255, 255);
    }
}