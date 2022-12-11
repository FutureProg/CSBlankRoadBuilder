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
		var fileName = $"br4_{elevationType.ToString().ToLower()}_{type.ToString().ToLower()}-{(int)curb}_{curb}.obj";

		PrepareMeshFiles(road, meshType, fileName);

		return ImportAsset(
			elevationType == ElevationType.Basic ? ShaderType.Basic : ShaderType.Bridge,
			meshType,
			fileName,
			false);
	}

	public static AssetModel ImportAsset(ShaderType shaderType, MeshType meshType, string fileName, bool prepareMesh = true, bool filesReady = false)
	{
		if (!filesReady)
			PrepareFiles(meshType, fileName, prepareMesh);

		var assetModel = new AssetModelUtil(shaderType);

		return assetModel.Import(fileName);
	}

	private static void PrepareFiles(MeshType meshType, string fileName, bool prepareMesh)
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
			File.Copy(file, Path.Combine(BlankRoadBuilderMod.ImportFolder, Path.GetFileName(file)), true);
		}
	}

	private static void PrepareMeshFiles(RoadInfo road, MeshType meshType, string fileName)
	{
		var baseName = Path.GetFileNameWithoutExtension(fileName);
		
		foreach (var file in Directory.GetFiles(Path.Combine(BlankRoadBuilderMod.MeshesFolder, meshType.ToString()), $"{baseName}*"))
		{
			Resize(file,
				baseWidth: 8F,
				newWidth: road.AsphaltWidth,
				newWidth: road.PavementWidth,
				flattenRoad: road.RoadType == RoadType.Flat);
		}
	}

	public static string Resize(string file, float baseWidth, float newWidth, string? newName = null, bool flattenRoad = false)
	{
		var diff = newWidth / 2F - baseWidth / 2F;

		var lines = File.ReadAllLines(file);

		for (var i = 0; i < lines.Length; i++)
		{
			if (lines[i].Length == 0)
				continue;

			var data = lines[i].Split(' ');

			switch (data[0])
			{
				case "g":
				case "G":
					if (!string.IsNullOrEmpty(newName))
						lines[i] = lines[i].Substring(0, 2) + newName;
					break;

				case "v":
				case "V":
					var xPos = float.Parse(data[1]);

					xPos += xPos <= -1 ? -diff : xPos >= 1 ? diff : 0;

					data[1] = xPos.ToString("0.00000000");

					if (flattenRoad && Math.Round(double.Parse(data[2]), 2) == -0.3D)
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