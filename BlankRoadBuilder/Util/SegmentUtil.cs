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

		foreach (var item in info.GetElevations())
		{
			if (item.Key > ElevationType.Bridge)
				break;

			var elevation = item.Key switch { ElevationType.Bridge => 92F, ElevationType.Elevated => 76F, _ => 60F };

			netManager.CreateNode(out var node1, ref randomizer, item.Value, new Vector3(i, elevation, 32), 1);
			netManager.CreateNode(out var node2, ref randomizer, item.Value, new Vector3(i, elevation, -32), 2);
			netManager.CreateSegment(out _, ref randomizer, item.Value, node1, node2, new Vector3(0, 0, -1), new Vector3(0, 0, 1), 1, 1, false);

			if (item.Key == ElevationType.Basic)
			{
				netManager.CreateNode(out var node3, ref randomizer, item.Value, new Vector3(i, elevation, -96), 3);
				netManager.CreateNode(out var node5, ref randomizer, item.Value, new Vector3(i - 64, elevation, -96), 5);
				netManager.CreateNode(out var node6, ref randomizer, item.Value, new Vector3(i + 64, elevation, -96), 6);
				netManager.CreateNode(out var node8, ref randomizer, item.Value, new Vector3(i - 116, elevation, -96), 8);
				netManager.CreateNode(out var node10, ref randomizer, item.Value, new Vector3(i - 180, elevation, -96), 10);
				netManager.CreateNode(out var node7, ref randomizer, item.Value, new Vector3(i - 32, elevation, 64), 7);
				netManager.CreateNode(out var node4, ref randomizer, item.Value, new Vector3(i - 80, elevation, 64), 4);
				netManager.CreateNode(out var node9, ref randomizer, item.Value, new Vector3(i - 144, elevation, 64), 9);
				netManager.CreateNode(out var node11, ref randomizer, item.Value, new Vector3(i - 144, elevation, 0), 9);

				netManager.CreateSegment(out _, ref randomizer, item.Value, node2, node3, new Vector3(0, 0, -1), new Vector3(0, 0, 1), 2, 2, false);
				netManager.CreateSegment(out _, ref randomizer, item.Value, node3, node5, new Vector3(-1, 0, 0), new Vector3(1, 0, 0), 4, 4, false);
				netManager.CreateSegment(out _, ref randomizer, item.Value, node6, node3, new Vector3(-1, 0, 0), new Vector3(1, 0, 0), 5, 5, false);
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

				netManager.CreateSegment(out _, ref randomizer, item.Value, node11, node9, new Vector3(0, 0, 1), new Vector3(0, 0, -1), 1, 1, false);
			}
			else if (item.Key == ElevationType.Elevated)
			{
				netManager.CreateNode(out var node3, ref randomizer, item.Value, new Vector3(i, elevation - 8F, 90), 3);
				netManager.CreateNode(out var node4, ref randomizer, item.Value, new Vector3(i, elevation - 8F, 150), 4);
				netManager.CreateNode(out var node5, ref randomizer, item.Value, new Vector3(i, elevation, -128), 5);
				netManager.CreateNode(out var node6, ref randomizer, item.Value, new Vector3(i - 48 - 32 - (float)Math.Floor(item.Value.m_halfWidth / 8F) * 8, elevation, -32), 6);
				netManager.CreateNode(out var node7, ref randomizer, item.Value, new Vector3(i - 48 - 32 - (float)Math.Floor(item.Value.m_halfWidth / 8F) * 8, elevation, 16), 7);

				netManager.CreateSegment(out _, ref randomizer, item.Value, node3, node1, new Vector3(0, 0, -1), new Vector3(0, 0, 1), 2, 2, false);
				netManager.CreateSegment(out _, ref randomizer, info, node4, node3, new Vector3(0, 0, -1), new Vector3(0, 0, 1), 2, 2, false);
				netManager.CreateSegment(out _, ref randomizer, item.Value, node2, node5, new Vector3(0, 0, -1), new Vector3(0, 0, 1), 2, 2, false);
				netManager.CreateSegment(out _, ref randomizer, item.Value, node2, node6, new Vector3(-1, 0, 0), new Vector3(1, 0, 0), 2, 2, false);
				netManager.CreateSegment(out _, ref randomizer, item.Value, node6, node7, new Vector3(0, 0, 1), new Vector3(0, 0, -1), 2, 2, false);

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
				netManager.CreateNode(out var node3, ref randomizer, item.Value, new Vector3(i, elevation, -128), 3);

				netManager.CreateSegment(out _, ref randomizer, item.Value, node2, node3, new Vector3(0, 0, -1), new Vector3(0, 0, 1), 2, 2, false);
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
