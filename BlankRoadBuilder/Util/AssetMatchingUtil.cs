﻿using ColossalFramework.IO;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

using UnityEngine;

namespace BlankRoadBuilder.Util;
public static class AssetMatchingUtil
{

	static string AssetMatcherFilePath => Path.Combine(BlankRoadBuilderMod.BuilderFolder, "SavedAssets.xml");

	static MatchingFile? _matchingFileData;

	static MatchingFile? MatchingData
	{
		get
		{
			if (_matchingFileData != null) // 
			{
				return _matchingFileData;
			}
			try
			{
				var matchingFile = AssetMatcherFilePath;
				if (!File.Exists(matchingFile))
				{
					_matchingFileData = new MatchingFile();
					UpdateMatchingFile();
					return _matchingFileData;
				}

				var serializer = new XmlSerializer(typeof(AssetMatchingUtil.MatchingFile));
				using var stream = File.OpenRead(matchingFile);
				_matchingFileData = (MatchingFile)serializer.Deserialize(stream);
				_matchingFileData.ConvertToDictionary();
				return _matchingFileData;
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
			return null;
		}
	}

	static bool UpdateMatchingFile()
	{
		try
		{
			var matchingFile = AssetMatcherFilePath;
			var serializer = new XmlSerializer(typeof(AssetMatchingUtil.MatchingFile));
			_matchingFileData?.ConvertToAssetsList();
			using var stream = File.Open(matchingFile, FileMode.Create);
			serializer.Serialize(stream, _matchingFileData);
			return true;
		}
		catch (Exception ex)
		{
			Debug.LogException(ex);
		}
		return false;
	}

	public static bool SetMatch(string crpFileName, string roadConfigFile, Domain.RoadOptions roadOptions)
	{
		Debug.Log("Setting Road Configuration Match: " + roadConfigFile + " = " + crpFileName);
		if (MatchingData?.Assets != null)
		{
			MatchingData.Assets[roadConfigFile] = crpFileName;
			MatchingData.RoadOptions[roadConfigFile] = roadOptions;
		}
		return UpdateMatchingFile();
	}
	public static Asset? GetMatchForRoadConfig(string roadConfigFile)
	{
		var matchingData = MatchingData;
		if (matchingData == null)
		{
			return null;
		}

		var re = new Asset
		{
			RoadConfigFile = roadConfigFile
		};
		if (matchingData.Assets.TryGetValue(roadConfigFile, out var outStr))
		{
			re.CrpFile = outStr;

			var inf = new FileInfo(Path.Combine(DataLocation.assetsPath, outStr));

			if (!inf.Exists)
			{
				inf = new DirectoryInfo(DataLocation.assetsPath).GetFiles(outStr, SearchOption.AllDirectories).FirstOrDefault() ?? inf;
			}

			re.DateGenerated = inf.Exists ? inf.LastWriteTimeUtc : DateTime.MinValue;
		}
		else
		{
			return null;
		}
		if (matchingData.RoadOptions.TryGetValue(roadConfigFile, out var outRO))
		{
			re.RoadOptions = outRO;
		}
		return re;
	}

	public static Asset? RemoveMatch(string roadConfigFile)
	{
		var matchingData = MatchingData;
		if (matchingData == null || !matchingData.Assets.ContainsKey(roadConfigFile))
		{
			return null;
		}
		var toRemove = matchingData.Assets[roadConfigFile];
		var re = new Asset { CrpFile = toRemove, RoadConfigFile = roadConfigFile };
		matchingData.Assets.Remove(roadConfigFile);
		return re;
	}

	public class MatchingFile
	{
		[XmlIgnore]
		public Dictionary<string, string> Assets; // <road config file, crp file>
		[XmlIgnore]
		public Dictionary<string, Domain.RoadOptions> RoadOptions;// road options set onload

		public List<Asset> AssetsList;

		public void ConvertToDictionary()
		{
			FixNulls();
			Assets.Clear();
			RoadOptions.Clear();
			foreach (var a in AssetsList)
			{
				Assets[a.RoadConfigFile] = a.CrpFile;
				RoadOptions[a.RoadConfigFile] = a.RoadOptions ?? new Domain.RoadOptions();
			}
		}
		public void ConvertToAssetsList()
		{
			FixNulls();
			AssetsList.Clear();
			foreach (var a in Assets.Keys)
			{
				AssetsList.Add(new Asset { CrpFile = Assets[a], RoadConfigFile = a, RoadOptions = RoadOptions[a] });
			}
		}
		void FixNulls()
		{
			Assets ??= new Dictionary<string, string>();
			AssetsList ??= new List<Asset>();
			RoadOptions ??= new Dictionary<string, Domain.RoadOptions>();
		}
	}

	public class Asset
	{
		public string CrpFile;
		public string RoadConfigFile;
		public Domain.RoadOptions? RoadOptions;
		public DateTime DateGenerated;
	}
}

