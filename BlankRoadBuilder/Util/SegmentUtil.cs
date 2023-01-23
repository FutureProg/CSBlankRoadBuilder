using BlankRoadBuilder.Domain;

using ColossalFramework;
using ColossalFramework.Math;
using ColossalFramework.Plugins;

using KianCommons;

using ModsCommon.Utilities;

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

			var elevation = item.Key switch { ElevationType.Bridge => 84F, ElevationType.Elevated => 72F, _ => 60F };

			netManager.CreateNode(out var node1, ref randomizer, item.Value, new Vector3(i, elevation, 30), 1);
			netManager.CreateNode(out var node2, ref randomizer, item.Value, new Vector3(i, elevation, -30), 2);
			netManager.CreateSegment(out _, ref randomizer, item.Value, node1, node2, new Vector3(0, 0, -1), new Vector3(0, 0, 1), 1, 1, false);

			if (item.Key == ElevationType.Basic)
			{
				netManager.CreateNode(out var node3, ref randomizer, item.Value, new Vector3(i, elevation, -90 - info.m_halfWidth), 3);
				netManager.CreateNode(out var node5, ref randomizer, item.Value, new Vector3(i - 60 - info.m_halfWidth, elevation, -90 - info.m_halfWidth), 5);
				netManager.CreateNode(out var node6, ref randomizer, item.Value, new Vector3(i + 60 + info.m_halfWidth, elevation, -90 - info.m_halfWidth), 6);
				netManager.CreateNode(out var node7, ref randomizer, item.Value, new Vector3(i - 30, elevation, 60), 1);

				netManager.CreateSegment(out _, ref randomizer, item.Value, node2, node3, new Vector3(0, 0, -1), new Vector3(0, 0, 1), 2, 2, false);
				netManager.CreateSegment(out _, ref randomizer, item.Value, node3, node5, new Vector3(-1, 0, 0), new Vector3(1, 0, 0), 4, 4, false);
				netManager.CreateSegment(out _, ref randomizer, item.Value, node6, node3, new Vector3(-1, 0, 0), new Vector3(1, 0, 0), 5, 5, false);
				netManager.CreateSegment(out _, ref randomizer, item.Value, node7, node1, new Vector3(1, 0, 0), new Vector3(0, 0, 1), 5, 5, false);
			}
			else if (item.Key == ElevationType.Elevated)
			{
				netManager.CreateNode(out var node3, ref randomizer, item.Value, new Vector3(i, 64F, 90), 3);
				netManager.CreateNode(out var node4, ref randomizer, item.Value, new Vector3(i, 64F, 150), 4);

				netManager.CreateSegment(out _, ref randomizer, item.Value, node3, node1, new Vector3(0, 0, -1), new Vector3(0, 0, 1), 2, 2, false);
				netManager.CreateSegment(out _, ref randomizer, info, node4, node3, new Vector3(0, 0, -1), new Vector3(0, 0, 1), 2, 2, false);

				try
				{
					if (PluginManager.instance.GetPluginsInfo().Any(x => x.publishedFileID.AsUInt64 is 2472062376 or 2462845270))
					{
						NcBend.BendNode(node3);
					}
					else
					{
						var trNode = node3.GetNode();

						trNode.m_flags &= ~NetNode.Flags.Junction;
						trNode.m_flags &= ~NetNode.Flags.Middle;
						trNode.m_flags |= NetNode.Flags.Bend;
					}
				}
				catch
				{ }
			}
			else if (item.Key == ElevationType.Bridge)
			{
				netManager.CreateNode(out var node3, ref randomizer, item.Value, new Vector3(i, elevation, -120 - info.m_halfWidth), 3);

				netManager.CreateSegment(out _, ref randomizer, item.Value, node2, node3, new Vector3(0, 0, -1), new Vector3(0, 0, 1), 2, 2, false);
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
