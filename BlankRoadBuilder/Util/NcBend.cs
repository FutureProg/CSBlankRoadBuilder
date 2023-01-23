extern alias NC;

namespace BlankRoadBuilder.Util;
public static partial class SegmentUtil
{
	public static class NcBend
	{
		public static void BendNode(ushort node)
		{
			if (NC::ModsCommon.SingletonManager<NC::NodeController.Manager>.Instance.GetOrCreateNodeData(node) is NC::NodeController.NodeData data)
			{
				data.Type = NC::NodeController.NodeStyleType.Bend;
				data.Offset = 12F;
			}
		}
	}
}
