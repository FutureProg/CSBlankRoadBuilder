using AdaptiveRoads.DTO;

using BlankRoadBuilder.Domain;
using BlankRoadBuilder.ThumbnailMaker;

using System;
using System.IO;

using UnityEngine;

namespace BlankRoadBuilder.Util;

public static class AssetUtil
{
	public static AssetModel ImportAsset(RoadInfo road, MeshType meshType, ElevationType elevationType, RoadAssetType type, CurbType curb)
	{
		var fileName = $"{elevationType.ToString().ToLower()}_{type.ToString().ToLower()}-{(int)curb}_{curb}.obj";

		PrepareMeshFiles(road, meshType, type, curb, fileName);

		return ImportAsset(
			elevationType == ElevationType.Basic ? ShaderType.Basic : ShaderType.Bridge,
			meshType,
			fileName,
			prepareMesh: false,
			noCurb: road.RoadType == RoadType.Highway);
	}

	public static AssetModel ImportAsset(ShaderType shaderType, MeshType meshType, string fileName, bool prepareMesh = true, bool filesReady = false, bool noCurb = false)
	{
		if (!filesReady)
			PrepareFiles(meshType, fileName, prepareMesh, noCurb);

		var assetModel = new AssetModelUtil(shaderType);

		return assetModel.Import(fileName);
	}

	private static void PrepareFiles(MeshType meshType, string fileName, bool prepareMesh, bool noCurb)
	{
		var baseName = Path.GetFileNameWithoutExtension(fileName);

		Directory.CreateDirectory(BlankRoadBuilderMod.ImportFolder);

		if (prepareMesh)
		{
			foreach (var file in Directory.GetFiles(Path.Combine(BlankRoadBuilderMod.MeshesFolder, meshType.ToString()), $"{baseName}*"))
			{
				File.Copy(file, Path.Combine(BlankRoadBuilderMod.ImportFolder, Path.GetFileName(file)), true);
			}
		}

		foreach (var file in Directory.GetFiles(Path.Combine(BlankRoadBuilderMod.TexturesFolder, meshType.ToString()), $"{baseName}*"))
		{
			if (noCurb && file.EndsWith("_p.png"))
				File.Copy(file.RegexReplace(@"\w+_(\w+-\d_\w+)(_lod)?_\w\.", x => x.Value.Replace(x.Groups[1].Value, "highway")), Path.Combine(BlankRoadBuilderMod.ImportFolder, Path.GetFileName(file)), true);
			else
				File.Copy(file, Path.Combine(BlankRoadBuilderMod.ImportFolder, Path.GetFileName(file)), true);
		}
	}

	private static void PrepareMeshFiles(RoadInfo road, MeshType meshType, RoadAssetType assetType, CurbType curb, string fileName)
	{
		var baseName = Path.GetFileNameWithoutExtension(fileName);
		
		foreach (var file in Directory.GetFiles(Path.Combine(BlankRoadBuilderMod.MeshesFolder, meshType.ToString()), $"{baseName}*"))
		{
			Resize(file, road, curb, assetType);
		}
	}

	public static string Resize(string file, RoadInfo road, CurbType curb, RoadAssetType assetType)
	{
		var baseWidth = 8F;
		var newWidth = road.TotalWidth;
		var diff = newWidth / 2F - baseWidth / 2F;

		var lines = File.ReadAllLines(file);

		for (var i = 0; i < lines.Length; i++)
		{
			if (lines[i].Length == 0)
				continue;

			var data = lines[i].Split(' ');

			switch (data[0])
			{
				//case "g":
				//case "G":
				//	if (!string.IsNullOrEmpty(newName))
				//		lines[i] = lines[i].Substring(0, 2) + newName;
				//	break;

				case "v":
				case "V":
					var xPos = double.Parse(data[1]);
					var yPos = double.Parse(data[2]);
					var aPos = Math.Abs(xPos);
					var xDiff = diff;

					if (aPos < 4F && yPos >= -0.3)
						xDiff -= road.PavementWidth - 3;

					if (aPos > 0.1F)
						xPos += xPos < 0 ? -xDiff : xDiff;

					data[1] = xPos.ToString("0.00000000");

					if (road.RoadType != RoadType.Road && curb != CurbType.TR && Math.Round(double.Parse(data[2]), 2) == -0.3D)
						data[2] = "0.00000000";

					lines[i] = string.Join(" ", data);
					break;
			}
		}

		var exportedFile = Path.Combine(BlankRoadBuilderMod.ImportFolder, Path.GetFileName(file));

		Directory.CreateDirectory(BlankRoadBuilderMod.ImportFolder);

		File.WriteAllLines(exportedFile, lines);

		return exportedFile;
	}
}