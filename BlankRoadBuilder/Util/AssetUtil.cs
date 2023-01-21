using BlankRoadBuilder.Domain;
using BlankRoadBuilder.Domain.Options;
using BlankRoadBuilder.ThumbnailMaker;

using System;
using System.IO;
using System.Text.RegularExpressions;

namespace BlankRoadBuilder.Util;

public static class AssetUtil
{
	public static AssetModel ImportAsset(RoadInfo road, MeshType meshType, ElevationType elevationType, RoadAssetType type, CurbType curb, string name, bool inverted, bool sidewalkTransition)
	{
		var curbless = curb == CurbType.Curbless;

		if (curbless)
		{
			curb = CurbType.HC;
		}

		var fileName = $"{MeshName(elevationType)}_{type.ToString().ToLower()}-{(int)curb}_{curb}.obj";
		var exportName = $"{elevationType}_{(inverted ? "inverted_" : "")}{type.ToString().ToLower()}-{(int)curb}_{curb}.obj";

		PrepareMeshFiles(road, name, meshType, elevationType, curb, curbless, inverted, sidewalkTransition, fileName, exportName);

		return ImportAsset(
			elevationType == ElevationType.Basic ? ShaderType.Basic : ShaderType.Bridge,
			meshType,
			exportName,
			filesReady: true);
	}

	public static AssetModel ImportAsset(ShaderType shaderType, MeshType meshType, string fileName, bool filesReady = false, float? scale = null)
	{
		if (!filesReady)
		{
			PrepareFiles(meshType, fileName);
		}

		var assetModel = new AssetModelUtil(shaderType, scale: scale);

		return assetModel.Import(fileName);
	}

	private static void PrepareFiles(MeshType meshType, string fileName)
	{
		var baseName = Path.GetFileNameWithoutExtension(fileName);

		Directory.CreateDirectory(BlankRoadBuilderMod.ImportFolder);

		foreach (var file in Directory.GetFiles(Path.Combine(BlankRoadBuilderMod.MeshesFolder, meshType.ToString()), $"{baseName}*"))
		{
			File.Copy(file, Path.Combine(BlankRoadBuilderMod.ImportFolder, Path.GetFileName(file)), true);
		}

		foreach (var file in Directory.GetFiles(Path.Combine(BlankRoadBuilderMod.TexturesFolder, meshType.ToString()), $"{baseName}*"))
		{
			File.Copy(file, Path.Combine(BlankRoadBuilderMod.ImportFolder, Path.GetFileName(file)), true);
		}
	}

	private static void PrepareMeshFiles(RoadInfo road, string name, MeshType meshType, ElevationType elevationType, CurbType curb, bool curbless, bool inverted, bool sidewalkTransition, string fileName, string exportFile)
	{
		Directory.CreateDirectory(BlankRoadBuilderMod.ImportFolder);

		var baseName = Path.GetFileNameWithoutExtension(fileName);
		var exportName = Path.GetFileNameWithoutExtension(exportFile);

		foreach (var file in Directory.GetFiles(Path.Combine(BlankRoadBuilderMod.MeshesFolder, meshType.ToString()), $"{baseName}*"))
		{
			var lines = Resize(file, name, road, curb, curbless, inverted, sidewalkTransition);
			var exportedFile = Path.Combine(BlankRoadBuilderMod.ImportFolder, $"{exportName}{(file.Contains("_lod") ? "_lod" : "")}.obj");

			File.WriteAllLines(exportedFile, lines);
		}

		foreach (var file in Directory.GetFiles(Path.Combine(BlankRoadBuilderMod.TexturesFolder, meshType.ToString()), $"{MeshName(elevationType)}*".ToLower()))
		{
			var regex = Regex.Match(Path.GetFileName(file), @"(_[^l][A-z]+)?_(\w)\.png", RegexOptions.IgnoreCase);
			var lod = file.Contains("_lod");
			var mesh = regex.Groups[2].Value.ToLower();
			var type = regex.Groups[1].Value.ToLower().TrimStart('_');
			var newName = $"{exportName}{(lod ? "_lod" : "")}_{mesh}.png";

			if (mesh is "p" or "r")
			{
				var ashpalt = elevationType == ElevationType.Basic ? (road.SideTexture == TextureType.Asphalt) : elevationType <= ElevationType.Bridge && (road.BridgeSideTexture == BridgeTextureType.Asphalt);
				var noashpalt = !ashpalt && road.AsphaltStyle == AsphaltStyle.None;
				var noCurb = (road.RoadType == RoadType.Highway || (road.RoadType == RoadType.Flat && ModOptions.RemoveCurbOnFlatRoads)) && curb != CurbType.TR;

				var requiredType = (ashpalt ? "asphalt" : noashpalt ? "noasphalt" : "") + (noCurb ? "nocurb" : "");

				if (type == requiredType)
				{
					File.Copy(file, Path.Combine(BlankRoadBuilderMod.ImportFolder, newName), true);
				}
			}
			else if (string.IsNullOrEmpty(type))
			{
				File.Copy(file, Path.Combine(BlankRoadBuilderMod.ImportFolder, newName), true);
			}
		}
	}

	public static string[] Resize(string file, string name, RoadInfo road, CurbType curb, bool curbless, bool inverted, bool sidewalkTransition)
	{
		var baseWidth = 8F;
		var newWidth = road.TotalWidth;
		var diff = (newWidth / 2F) - (baseWidth / 2F);

		var lines = File.ReadAllLines(file);

		for (var i = 0; i < lines.Length; i++)
		{
			if (lines[i].Length == 0)
			{
				continue;
			}

			var data = lines[i].Split(' ');

			switch (data[0])
			{
				case "g":
				case "G":
					lines[i] = "g " + name;
					break;

				case "v":
				case "V":
					var xPos = double.Parse(data[1]);
					var yPos = double.Parse(data[2]);
					var aPos = Math.Abs(xPos);
					var xDiff = diff;

					if (curbless && (aPos >= 1 || yPos < -0.3))
					{
						if (aPos >= 1)
						{
							xPos = xPos > 0 ? 1 : -1;
							aPos = 1;
						}

						yPos = -0.3;
					}

					if (aPos < 4F && yPos >= -0.3)
					{
						if (sidewalkTransition)
						{
							var start = (xPos < 0) == inverted ? road.LeftPavementWidth : road.RightPavementWidth;
							var end = (xPos < 0) == inverted ? road.RightPavementWidth : road.LeftPavementWidth;
							var step = (float.Parse(data[3]) + 32) / 64;

							xDiff -= fn((2 * step) - 1, Math.Min(start, end), Math.Max(start, end)) - 3;

							//https://www.desmos.com/calculator/5rulizhfsh
							static float fn(float x, float s, float e) => ((float)(-x + Math.Pow(-x, 3)) / (4 / (e - s))) + (e / 2) + 1;
						}
						else
						{
							xDiff -= ((xPos < 0) == inverted ? road.LeftPavementWidth : road.RightPavementWidth) - 3;
						}
					}

					if (aPos > 0.1F)
					{
						xPos += xPos < 0 ? -xDiff : xDiff;
					}

					if (yPos == 0D || (road.RoadType != RoadType.Road && curb != CurbType.TR && Math.Round(yPos, 2) == -0.3D))
					{
						yPos = 0.0005;
					}

					data[1] = xPos.ToString("0.00000000");
					data[2] = yPos.ToString("0.00000000");

					lines[i] = string.Join(" ", data);
					break;
			}
		}

		return lines;
	}

	private static string MeshName(ElevationType elevationType)
	{
		return elevationType switch
		{
			ElevationType.Bridge or ElevationType.Elevated => "elevated",
			_ => "basic",
		};
	}
}