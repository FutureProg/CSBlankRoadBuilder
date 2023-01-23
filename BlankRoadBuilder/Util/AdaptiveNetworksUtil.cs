using AdaptiveRoads.Manager;
using AdaptiveRoads.UI.RoadEditor;
using AdaptiveRoads.UI.RoadEditor.MenuStyle;
using AdaptiveRoads.Util;

using ColossalFramework;
using ColossalFramework.UI;

using PrefabMetadata.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

using UnityEngine;

namespace BlankRoadBuilder.Util;

public static class AdaptiveNetworksUtil
{
	public static void Refresh()
	{
		try
		{
			NetInfoExtionsion.Ensure_EditedNetInfos(true);

			var mainPanel = UnityEngine.Object.FindObjectOfType<RoadEditorMainPanel>();
			if (mainPanel)
			{
				InvokeMethod(mainPanel, "Clear");
				InvokeMethod(mainPanel, "Initialize");
				InvokeMethod(mainPanel, "OnObjectModified");
			}
			MenuPanelBase.CloseAll();
			MiniPanel.CloseAll();
		}
		catch { }
	}

	private static object? InvokeMethod(object instance, string method)
	{
		var type = instance.GetType();
		return GetMethod(type, method, true)?.Invoke(instance, null);
	}

	private static MethodInfo GetMethod(Type type, string method, bool throwOnError = true)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}

		var ret = type.GetMethod(method, BindingFlags.Public
			| BindingFlags.NonPublic
			| BindingFlags.Instance
			| BindingFlags.Static
			| BindingFlags.GetField
			| BindingFlags.SetField
			| BindingFlags.GetProperty
			| BindingFlags.SetProperty);
		if (throwOnError && ret == null)
		{
			throw new Exception($"Method not found: {type.Name}.{method}");
		}

		return ret;
	}

	public static NetLaneProps.Prop Extend(this NetLaneProps.Prop prop, Func<NetLaneProps.Prop, NetInfoExtionsion.LaneProp> extension)
	{
		var extendedProp = prop.Extend();

		extendedProp.SetMetaData(extension(prop));

		return extendedProp.Base;
	}

	public static void InvertStartEnd(ref this NetLane.Flags flags)
	{
		flags.SwitchFlags(NetLane.Flags.YieldStart, NetLane.Flags.YieldEnd);
		flags.SwitchFlags(NetLane.Flags.StartOneWayRight, NetLane.Flags.EndOneWayRight);
		flags.SwitchFlags(NetLane.Flags.StartOneWayLeft, NetLane.Flags.EndOneWayLeft);
	}

	public static void InvertLeftRight(ref this NetLane.Flags flags)
	{
		flags.SwitchFlags(NetLane.Flags.StartOneWayRight, NetLane.Flags.StartOneWayLeft);
		flags.SwitchFlags(NetLane.Flags.EndOneWayRight, NetLane.Flags.EndOneWayLeft);
	}

	public static void InvertLeftRight(ref this NetSegmentEnd.Flags flags)
	{
		flags.SwitchFlags(NetSegmentEnd.Flags.HasRightSegment, NetSegmentEnd.Flags.HasLeftSegment);
		flags.SwitchFlags(NetSegmentEnd.Flags.CanTurnRight, NetSegmentEnd.Flags.CanTurnLeft);
	}

	public static void InvertLeftRight(ref this NetSegmentExt.Flags flags)
	{
		flags.SwitchFlags(NetSegmentExt.Flags.ParkingAllowedLeft, NetSegmentExt.Flags.ParkingAllowedRight);
	}

	public static void InvertLeftRight(ref this NetSegment.Flags flags)
	{
		flags.SwitchFlags(NetSegment.Flags.StopLeft, NetSegment.Flags.StopRight);
		flags.SwitchFlags(NetSegment.Flags.StopLeft2, NetSegment.Flags.StopRight2);
	}

	public static NetLaneProps.ColorMode InvertStartEnd(this NetLaneProps.ColorMode colorMode)
	{
		return colorMode switch
		{
			NetLaneProps.ColorMode.StartState => NetLaneProps.ColorMode.EndState,
			NetLaneProps.ColorMode.EndState => NetLaneProps.ColorMode.StartState,
			_ => colorMode,
		};
	}

	public static void SwitchFlags<T>(ref this T flags, T flag1, T flag2) where T : struct, Enum, IConvertible
	{
		var hasFlag1 = flags.IsFlagSet(flag1);
		var hasFlag2 = flags.IsFlagSet(flag2);
		flags = flags.SetFlags(flag1, hasFlag2);
		flags = flags.SetFlags(flag2, hasFlag1);
	}

	private static string Remove(this string s, string val)
	{
		return s.Replace(val, "");
	}

	public static bool TryMirrorMesh(PropInfo prop, out PropInfo prop2)
	{
		var name2 = prop.name.Replace("left", "right").Replace("Left", "Right")
			.Replace("LEFT", "RIGHT").Replace("LHT", "RHT").Replace("lht", "rht");
		var name3 = prop.name.Replace("right", "left").Replace("Right", "Left")
			.Replace("RIGHT", "LEFT").Replace("RHT", "LHT").Replace("rht", "lht");
		var rgx = Regex.Match(prop.name, @"(Traffic Light 0[12])( Mirror)?");

		if (rgx.Success)
		{
			if (rgx.Groups[2].Value.Length > 0)
				name2 = rgx.Groups[1].Value;
			else
				name2 = rgx.Groups[1].Value + " Mirror";
		}
		else
		{
			if (name2 == prop.name && name3 == prop.name)
			{
				prop2 = prop; // right and left is the same.
				return false;
			}
			if (name2 != prop.name && name3 != prop.name)
			{
				prop2 = null; //confusing.
				return false;
			}
			if (name3 != prop.name)
			{
				name2 = name3;
			}
		}

		prop2 = PrefabCollection<PropInfo>.FindLoaded(name2);
		return prop2 != null;
	}

	public static void ChangeInvertedFlag(this NetLaneProps.Prop prop)
	{
		{
			var InvertRequired = prop.m_flagsRequired.IsFlagSet(NetLane.Flags.Inverted);
			var InvertForbidden = prop.m_flagsForbidden.IsFlagSet(NetLane.Flags.Inverted);
			prop.m_flagsRequired = prop.m_flagsRequired.SetFlags(
				NetLane.Flags.Inverted,
				InvertForbidden);
			prop.m_flagsForbidden = prop.m_flagsForbidden.SetFlags(
				NetLane.Flags.Inverted,
				InvertRequired);
		}
		if (prop.GetMetaData() is NetInfoExtionsion.LaneProp metaData)
		{
			var InvertRequired = metaData.SegmentFlags.Required.IsFlagSet(NetSegmentExt.Flags.LeftHandTraffic);
			var InvertForbidden = metaData.SegmentFlags.Forbidden.IsFlagSet(NetSegmentExt.Flags.LeftHandTraffic);
			metaData.SegmentFlags.Required = metaData.SegmentFlags.Required.SetFlags(
				NetSegmentExt.Flags.LeftHandTraffic,
				InvertForbidden);
			metaData.SegmentFlags.Forbidden = metaData.SegmentFlags.Forbidden.SetFlags(
				NetSegmentExt.Flags.LeftHandTraffic,
				InvertRequired);
		}
	}

	public static void Swap<T>(ref T a, ref T b)
	{
		var t = a;
		a = b;
		b = t;
	}

	public static NetLaneProps.Prop ToggleRHT_LHT(this NetLaneProps.Prop prop, bool unidirectional)
	{
		if (prop.m_prop == null)
			return prop;

		if (unidirectional)
		{
			prop.m_position.x = -prop.m_position.x;
		}
		else
		{
			if (prop.m_angle == 70F)
				prop.m_angle += 40;

			prop.m_segmentOffset = -prop.m_segmentOffset;
			prop.m_angle = (prop.m_angle + 180) % 360;
			prop.m_position.z = -prop.m_position.z;
		}

		prop.ChangeInvertedFlag();
		if (!unidirectional)
		{
			prop.m_colorMode = prop.m_colorMode.InvertStartEnd();
			prop.m_flagsRequired.InvertStartEnd();
			prop.m_flagsForbidden.InvertStartEnd();
			Swap(ref prop.m_startFlagsRequired, ref prop.m_endFlagsRequired);
			Swap(ref prop.m_startFlagsForbidden, ref prop.m_endFlagsForbidden);
		}
		prop.m_flagsRequired.InvertLeftRight();
		prop.m_flagsForbidden.InvertLeftRight();

		var propExt = prop.GetMetaData();
		if (propExt != null)
		{
			if (!unidirectional)
			{
				Swap(ref propExt.StartNodeFlags, ref propExt.EndNodeFlags);
				Swap(ref propExt.SegmentStartFlags, ref propExt.SegmentEndFlags);
			}
			propExt.SegmentStartFlags.Required.InvertLeftRight();
			propExt.SegmentStartFlags.Forbidden.InvertLeftRight();
			propExt.SegmentEndFlags.Required.InvertLeftRight();
			propExt.SegmentEndFlags.Forbidden.InvertLeftRight();
			propExt.SegmentFlags.Required.InvertLeftRight();
			propExt.SegmentFlags.Forbidden.InvertLeftRight();
			propExt.VanillaSegmentFlags.Required.InvertLeftRight();
			propExt.VanillaSegmentFlags.Forbidden.InvertLeftRight();

			Swap(ref propExt.ForwardSpeedLimit, ref propExt.BackwardSpeedLimit);
		}

		if (TryMirrorMesh(prop.m_prop, out var propInfoInverted))
		{
			prop.m_prop = prop.m_finalProp = propInfoInverted;
		}

		if (prop.m_prop.name == "Traffic Light Pedestrian")
			prop.m_angle += 180;

		return prop;
	}

	public static NetLaneProps.Prop Clone(this NetLaneProps.Prop prop)
	{
		if (prop is ICloneable cloneable)
		{
			return cloneable.Clone() as NetLaneProps.Prop;
		}
		else
		{
			return prop.ShalowClone();
		}
	}

	public static T ShalowClone<T>(this T source) where T : class, new()
	{
		var target = new T();
		CopyProperties<T>(target, source);
		return target;
	}

	public const BindingFlags COPYABLE = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

	public static void CopyProperties<T>(object target, object origin)
	{
		var fields = typeof(T).GetFields(COPYABLE);
		foreach (var fieldInfo in fields)
		{
			//Log.Debug($"Copying field:<{fieldInfo.Name}> ...>");
			var value = fieldInfo.GetValue(origin);
			//string strValue = value?.ToString() ?? "null";
			//Log.Debug($"Got field value:<{strValue}> ...>");
			fieldInfo.SetValue(target, value);
			//Log.Debug($"Copied field:<{fieldInfo.Name}> value:<{strValue}>");
		}
	}

	public static NetLaneProps.Prop ToggleForwardBackward(this NetLaneProps.Prop prop, bool toggle = true, bool keepAngle = false)
	{
		if (!toggle || prop?.m_prop == null)
			return prop;

		prop.m_segmentOffset = -prop.m_segmentOffset;
		if (!keepAngle)
		{
			prop.m_angle = (prop.m_angle + 180) % 360;
		}
		prop.m_position.x = -prop.m_position.x;
		prop.m_position.z = -prop.m_position.z;

		prop.m_colorMode = prop.m_colorMode.InvertStartEnd();
		prop.m_flagsRequired.InvertStartEnd();
		prop.m_flagsForbidden.InvertStartEnd();

		Swap(ref prop.m_startFlagsRequired, ref prop.m_endFlagsRequired);
		Swap(ref prop.m_startFlagsForbidden, ref prop.m_endFlagsForbidden);

		var propExt = prop.GetMetaData();
		if (propExt != null)
		{
			Swap(ref propExt.StartNodeFlags, ref propExt.EndNodeFlags);
			Swap(ref propExt.SegmentStartFlags, ref propExt.SegmentEndFlags);

			// change parking/bus/tram left/right flags.
			propExt.SegmentFlags.Required.InvertLeftRight();
			propExt.SegmentFlags.Forbidden.InvertLeftRight();
			propExt.VanillaSegmentFlags.Required.InvertLeftRight();
			propExt.VanillaSegmentFlags.Forbidden.InvertLeftRight();

			Swap(ref propExt.ForwardSpeedLimit, ref propExt.BackwardSpeedLimit);
		}

		return prop;
	}

	public static List<string> RenameEditNet(string name, bool reportOnly)
	{
		try
		{
			if (name.IsNullOrWhiteSpace())
				throw new Exception("name is empty");
			var ground = ToolsModifierControl.toolController.m_editPrefabInfo as NetInfo;
			if (ground == null)
				throw new Exception("m_editPrefabInfo is not netInfo");

			var elevated = AssetEditorRoadUtils.TryGetElevated(ground);
			var bridge = AssetEditorRoadUtils.TryGetBridge(ground);
			var slope = AssetEditorRoadUtils.TryGetSlope(ground);
			var tunnel = AssetEditorRoadUtils.TryGetTunnel(ground);

			var ret = new List<string>();
			void Rename(NetInfo _info, string _postfix)
			{
				if (!_info)
					return;
				ret.Add(GetUniqueNetInfoName(name + _postfix, true));
				if (!reportOnly)
					_info.name = ret.Last();
				ret.AddRange(_info.NameAIBuildings(ret.Last(), reportOnly));
			}

			Rename(ground, "_Data");
			Rename(elevated, " E");
			Rename(bridge, " B");
			Rename(slope, " S");
			Rename(tunnel, " T");

			return ret;
		}
		catch (Exception)
		{
			return null;
		}
	}

	public static string GetUniqueNetInfoName(string name, bool excludeCurrent = false)
	{
		var strippedName = PackageHelper.StripName(name);
		if (excludeCurrent && strippedName == name)
			return name;
		var finalName = strippedName;
		for (var i = 0; PrefabCollection<NetInfo>.LoadedExists(finalName); i++)
		{
			finalName = $"instance{i}." + strippedName;
			if (i > 1000)
				throw new Exception("Infinite loop");
		}
		return finalName;
	}

	public static List<string> NameAIBuildings(this NetInfo info, string infoName, bool reportOnly)
	{
		var ret = new List<string>();
		var ai = info.m_netAI;
		var name = PackageHelper.StripName(infoName);
		foreach (var field in ai.GetType().GetFields())
		{
			if (field.GetValue(ai) is BuildingInfo buildingInfo)
			{
				var postfix = field.Name.Remove("m_").Remove("Info");
				ret.Add(name + "_" + postfix);
				if (!reportOnly)
				{
					buildingInfo.name = ret.Last();
				}
			}
		}
		return ret;
	}

	public static UITextureAtlas CreateTextureAtlas(string atlasName, Texture2D[] textures)
	{
		const int maxSize = 2048;
		var texture2D = new Texture2D(maxSize, maxSize, TextureFormat.ARGB32, false);
		var regions = texture2D.PackTextures(textures, 2, maxSize);
		var material = UnityEngine.Object.Instantiate<Material>(UIView.GetAView().defaultAtlas.material);
		material.mainTexture = texture2D;

		var textureAtlas = ScriptableObject.CreateInstance<UITextureAtlas>();
		textureAtlas.material = material;
		textureAtlas.name = atlasName;

		for (var i = 0; i < textures.Length; i++)
		{
			var item = new UITextureAtlas.SpriteInfo
			{
				name = textures[i].name,
				texture = textures[i],
				region = regions[i],
			};

			textureAtlas.AddSprite(item);
		}

		return textureAtlas;
	}

	public static ulong GetLaneIndeces(NetInfo net, VehicleInfo.VehicleType types)
	{
		ulong laneIndeces = 0;

		for (var i = 0; i < net.m_lanes.Length; i++)
		{
			if ((net.m_lanes[i].m_vehicleType & types) != 0)
			{
				laneIndeces |= (ulong)(1L << i);
			}
		}

		return laneIndeces;
	}

	public static ulong GetLaneIndeces(NetInfo net, params NetInfo.Lane[] lane)
	{
		ulong laneIndeces = 0;

		for (var i = 0; i < net.m_lanes.Length; i++)
		{
			if (lane.Contains(net.m_lanes[i]))
			{
				laneIndeces |= (ulong)(1L << i);
			}
		}

		return laneIndeces;
	}
}