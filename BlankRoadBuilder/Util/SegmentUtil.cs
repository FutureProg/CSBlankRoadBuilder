extern alias NC;

using BlankRoadBuilder.Domain;

using ColossalFramework;
using ColossalFramework.Math;

using KianCommons;

using ModsCommon.Utilities;

using UnityEngine;

namespace BlankRoadBuilder.Util;
public static class SegmentUtil
{
	public static void GenerateTemplateSegments(NetInfo info)
	{
		var netManager = Singleton<NetManager>.instance;
		var randomizer = new Randomizer();

		var i = 0F;

		foreach (var item in info.GetElevations())
		{
			if (item.Key > ElevationType.Bridge)
				break;

			var elevation = item.Key switch { ElevationType.Bridge => 84F, ElevationType.Elevated => 72F, _ => 60F };

			_ = netManager.CreateNode(out var node1, ref randomizer, item.Value, new Vector3(i, elevation, 30), 1);
			_ = netManager.CreateNode(out var node2, ref randomizer, item.Value, new Vector3(i, elevation, -30), 2);
			_ = netManager.CreateSegment(out _, ref randomizer, item.Value, node1, node2, new Vector3(0, 0, -1), new Vector3(0, 0, 1), 1, 1, false);

			if (item.Key == ElevationType.Basic)
			{
				_ = netManager.CreateNode(out var node3, ref randomizer, item.Value, new Vector3(i, elevation, -90 - info.m_halfWidth), 3);
				_ = netManager.CreateNode(out var node5, ref randomizer, item.Value, new Vector3(i - 60 - info.m_halfWidth, elevation, -90 - info.m_halfWidth), 5);
				_ = netManager.CreateNode(out var node6, ref randomizer, item.Value, new Vector3(i + 60 + info.m_halfWidth, elevation, -90 - info.m_halfWidth), 6);
				_ = netManager.CreateNode(out var node7, ref randomizer, item.Value, new Vector3(i - 30, elevation, 60), 1);

				_ = netManager.CreateSegment(out _, ref randomizer, item.Value, node2, node3, new Vector3(0, 0, -1), new Vector3(0, 0, 1), 2, 2, false);
				_ = netManager.CreateSegment(out _, ref randomizer, item.Value, node3, node5, new Vector3(-1, 0, 0), new Vector3(1, 0, 0), 4, 4, false);
				_ = netManager.CreateSegment(out _, ref randomizer, item.Value, node6, node3, new Vector3(-1, 0, 0), new Vector3(1, 0, 0), 5, 5, false);
				_ = netManager.CreateSegment(out _, ref randomizer, item.Value, node7, node1, new Vector3(1, 0, 0), new Vector3(0, 0, 1), 5, 5, false);
			}
			else if (item.Key == ElevationType.Elevated)
			{
				_ = netManager.CreateNode(out var node3, ref randomizer, item.Value, new Vector3(i, 60F, 90), 3);
				_ = netManager.CreateNode(out var node4, ref randomizer, item.Value, new Vector3(i, 60F, 150), 4);

				_ = netManager.CreateSegment(out _, ref randomizer, item.Value, node3, node1, new Vector3(0, 0, -1), new Vector3(0, 0, 1), 2, 2, false);
				_ = netManager.CreateSegment(out _, ref randomizer, info, node4, node3, new Vector3(0, 0, -1), new Vector3(0, 0, 1), 2, 2, false);

				node3.GetNode().m_flags &= ~NetNode.Flags.Bend;
				node3.GetNode().m_flags |= NetNode.Flags.Junction;
				node3.GetNode().UpdateNode(node3);
			}
			else if (item.Key == ElevationType.Bridge)
			{
				_ = netManager.CreateNode(out var node3, ref randomizer, item.Value, new Vector3(i, elevation, -120 - info.m_halfWidth), 3);

				_ = netManager.CreateSegment(out _, ref randomizer, item.Value, node2, node3, new Vector3(0, 0, -1), new Vector3(0, 0, 1), 2, 2, false);

				if (NC::ModsCommon.SingletonManager<NC::NodeController.Manager>.Instance.GetOrCreateNodeData(node3) is NC::NodeController.NodeData data)
				{
					data.Type = NC::NodeController.NodeStyleType.Custom;
				}
			}

			i += (info.m_halfWidth * 2) + 15;
		}
	}

	public static void ClearNodes()
	{
		var netManager = Singleton<NetManager>.instance;

		for (var i = 0; i < netManager.m_nodes.m_buffer.Length; i++)
		{
			var node = netManager.m_nodes.m_buffer[i];

			netManager.ReleaseNode(node.GetID());
		}
	}
}
