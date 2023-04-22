using BlankRoadBuilder.Domain;

using ColossalFramework;
using ColossalFramework.Math;
using ColossalFramework.Plugins;

using KianCommons;

using ModsCommon.Utilities;

using System;
using System.Linq;

using UnityEngine;

namespace BlankRoadBuilder.Util;
public static partial class SegmentUtil
{
	public static void GenerateTemplateSegments(NetInfo info)
	{
		var netManager = Singleton<NetManager>.instance;
		var randomizer = new Randomizer();

		var i = 0F;
		ushort node11_ = 0, node6_ = 0, node_s1 = 0, node_s2 = 0;

		foreach (var item in info.GetElevations())
		{
			var elevation = item.Key switch { ElevationType.Bridge => 92F, ElevationType.Elevated => 76F, ElevationType.Tunnel or ElevationType.Slope => 48F, _ => 60F };

			if (item.Key == ElevationType.Basic)
			{
				netManager.CreateNode(out var node1, ref randomizer, item.Value, new Vector3(i, elevation, 32), 1);
				netManager.CreateNode(out var node2, ref randomizer, item.Value, new Vector3(i, elevation, -32), 2);
				netManager.CreateSegment(out _, ref randomizer, item.Value, node1, node2, new Vector3(0, 0, -1), new Vector3(0, 0, 1), 1, 1, false);

				netManager.CreateNode(out var node3, ref randomizer, item.Value, new Vector3(i, elevation, -96), 3);
				netManager.CreateNode(out var node5, ref randomizer, item.Value, new Vector3(i - 64, elevation, -96), 5);
				netManager.CreateNode(out node6_, ref randomizer, item.Value, new Vector3(i + 64, elevation, -96), 6);
				netManager.CreateNode(out var node8, ref randomizer, item.Value, new Vector3(i - 116, elevation, -96), 8);
				netManager.CreateNode(out var node10, ref randomizer, item.Value, new Vector3(i - 180, elevation, -96), 10);
				netManager.CreateNode(out var node7, ref randomizer, item.Value, new Vector3(i - 32, elevation, 64), 7);
				netManager.CreateNode(out var node4, ref randomizer, item.Value, new Vector3(i - 80, elevation, 64), 4);
				netManager.CreateNode(out var node9, ref randomizer, item.Value, new Vector3(i - 180, elevation, 64), 9);
				netManager.CreateNode(out node11_, ref randomizer, item.Value, new Vector3(i - 180, elevation, 0), 9);

				netManager.CreateSegment(out _, ref randomizer, item.Value, node2, node3, new Vector3(0, 0, -1), new Vector3(0, 0, 1), 2, 2, false);
				netManager.CreateSegment(out _, ref randomizer, item.Value, node3, node5, new Vector3(-1, 0, 0), new Vector3(1, 0, 0), 4, 4, false);
				netManager.CreateSegment(out _, ref randomizer, item.Value, node6_, node3, new Vector3(-1, 0, 0), new Vector3(1, 0, 0), 5, 5, false);
				netManager.CreateSegment(out _, ref randomizer, item.Value, node7, node1, new Vector3(1, 0, 0), new Vector3(0, 0, 1), 5, 5, false);
				netManager.CreateSegment(out _, ref randomizer, item.Value, node5, node8, new Vector3(-1, 0, 0), new Vector3(1, 0, 0), 8, 8, false);
				netManager.CreateSegment(out _, ref randomizer, item.Value, node8, node10, new Vector3(-1, 0, 0), new Vector3(1, 0, 0), 8, 8, false);

				var trainNet = PrefabCollection<NetInfo>.FindLoaded("Train Track");
				var vanillaNet = PrefabCollection<NetInfo>.FindLoaded("Basic Road");

				netManager.CreateNode(out var tnode1, ref randomizer, trainNet, new Vector3(i - 116, elevation, -96 + 64), 9);
				netManager.CreateNode(out var tnode2, ref randomizer, trainNet, new Vector3(i - 116, elevation, -96 - 64), 10);
			
				netManager.CreateSegment(out _, ref randomizer, trainNet, tnode1, node8, new Vector3(0, 0, -1), new Vector3(0, 0, 1), 9, 9, false);
				netManager.CreateSegment(out _, ref randomizer, trainNet, node8, tnode2, new Vector3(0, 0, -1), new Vector3(0, 0, 1), 10, 10, false);

				netManager.CreateNode(out var vnode1, ref randomizer, vanillaNet, new Vector3(i - 80, elevation, 64 + 64), 10);
				netManager.CreateNode(out var vnode2, ref randomizer, vanillaNet, new Vector3(i - 80, elevation, 64 - 64), 11);

				netManager.CreateSegment(out _, ref randomizer, item.Value, node4, node7, new Vector3(1, 0, 0), new Vector3(-1, 0, 0), 8, 8, false);
				netManager.CreateSegment(out _, ref randomizer, item.Value, node9, node4, new Vector3(1, 0, 0), new Vector3(-1, 0, 0), 8, 8, false);
				netManager.CreateSegment(out _, ref randomizer, vanillaNet, vnode1, node4, new Vector3(0, 0, -1), new Vector3(0, 0, 1), 12, 12, false);
				netManager.CreateSegment(out _, ref randomizer, vanillaNet, node4, vnode2, new Vector3(0, 0, -1), new Vector3(0, 0, 1), 13, 13, false);

				netManager.CreateSegment(out _, ref randomizer, item.Value, node11_, node9, new Vector3(0, 0, 1), new Vector3(0, 0, -1), 1, 1, false);
			}
			else if (item.Key == ElevationType.Elevated)
			{
				netManager.CreateNode(out var node3, ref randomizer, item.Value, new Vector3(i, elevation - 12F, 90), 3);
				netManager.CreateNode(out var node4, ref randomizer, item.Value, new Vector3(i, elevation - 12F, 150), 4);

				netManager.CreateNode(out var node1, ref randomizer, item.Value, new Vector3(i, elevation, 32), 1);
				netManager.CreateNode(out var node2, ref randomizer, item.Value, new Vector3(i, elevation, -32), 2);
				netManager.CreateNode(out var node5, ref randomizer, item.Value, new Vector3(i + 64, elevation, -32), 5);
				netManager.CreateNode(out var node6, ref randomizer, item.Value, new Vector3(i + 64 + 64, elevation, -32), 6);
				netManager.CreateNode(out var node7, ref randomizer, item.Value, new Vector3(i + 64, elevation, 32), 7);

				netManager.CreateSegment(out _, ref randomizer, item.Value, node1, node2, new Vector3(0, 0, -1), new Vector3(0, 0, 1), 1, 1, false);
				netManager.CreateSegment(out _, ref randomizer, item.Value, node3, node1, new Vector3(0, 0, -1), new Vector3(0, 0, 1), 2, 2, false);
				netManager.CreateSegment(out _, ref randomizer, info, node4, node3, new Vector3(0, 0, -1), new Vector3(0, 0, 1), 2, 2, false);
				netManager.CreateSegment(out _, ref randomizer, item.Value, node2, node5, new Vector3(1, 0, 0), new Vector3(-1, 0, 0), 2, 2, false);
				netManager.CreateSegment(out _, ref randomizer, item.Value, node5, node6, new Vector3(1, 0, 0), new Vector3(-1, 0, 0), 2, 2, false);
				netManager.CreateSegment(out _, ref randomizer, item.Value, node5, node7, new Vector3(0, 0, 1), new Vector3(0, 0, -1), 2, 2, false);

				try
				{
					if (PluginManager.instance.GetPluginsInfo().Any(x => x.publishedFileID.AsUInt64 is 2472062376 or 2462845270))
					{
						NcBend.BendNode(node3);
					}
				}
				catch
				{ }
			}
			else if (item.Key == ElevationType.Bridge)
			{
				netManager.CreateNode(out var node1, ref randomizer, item.Value, new Vector3(-180, elevation, -128), 1);
				netManager.CreateNode(out var node2, ref randomizer, item.Value, new Vector3(0, elevation, -128), 2);
				netManager.CreateNode(out var node3, ref randomizer, item.Value, new Vector3(64, elevation, -128), 3);

				netManager.CreateSegment(out _, ref randomizer, item.Value, node1, node2, new Vector3(1, 0, 0), new Vector3(-1, 0, 0), 2, 2, false);
				netManager.CreateSegment(out _, ref randomizer, item.Value, node2, node3, new Vector3(1, 0, 0), new Vector3(-1, 0, 0), 2, 2, false);
			}
			else if (item.Key == ElevationType.Slope)
			{
				netManager.CreateNode(out node_s1, ref randomizer, item.Value, new Vector3(64 + 64, elevation, -96), 1);
				netManager.CreateNode(out node_s2, ref randomizer, item.Value, new Vector3(-180, elevation, -64), 2);

				node_s1.GetNode().m_flags |= NetNode.Flags.Underground | NetNode.Flags.Bend;
				node_s2.GetNode().m_flags |= NetNode.Flags.Underground | NetNode.Flags.Bend;
				node6_.GetNode().m_flags |= NetNode.Flags.OnGround;
				node11_.GetNode().m_flags |= NetNode.Flags.OnGround;

				netManager.CreateSegment(out _, ref randomizer, item.Value, node_s1, node6_, new Vector3(-1, 1, 0), new Vector3(1, -1, 0),  2, 2, false);
				netManager.CreateSegment(out _, ref randomizer, item.Value, node_s2, node11_, new Vector3(0, 1, 1), new Vector3(0, -1, -1),  2, 2, false);
			}
			else if (item.Key == ElevationType.Tunnel)
			{
				netManager.CreateNode(out var node1, ref randomizer, item.Value, new Vector3(64 + 64 + 64, elevation, -96), 1);
				netManager.CreateNode(out var node2, ref randomizer, item.Value, new Vector3(-180, elevation, -64 - 64), 2);

				node1.GetNode().m_flags |= NetNode.Flags.Underground;
				node2.GetNode().m_flags |= NetNode.Flags.Underground;

				netManager.CreateSegment(out _, ref randomizer, item.Value, node_s1, node1, new Vector3(1, 0, 0), new Vector3(-1, 0, 0), 2, 2, true);
				netManager.CreateSegment(out _, ref randomizer, item.Value, node_s2, node2, new Vector3(0, 0, -1), new Vector3(0, 0, 1), 2, 2, true);
			}

			i += 48;
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
