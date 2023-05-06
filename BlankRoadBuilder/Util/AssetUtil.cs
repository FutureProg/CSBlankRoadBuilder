using BlankRoadBuilder.Domain;
using BlankRoadBuilder.Domain.Options;
using BlankRoadBuilder.ThumbnailMaker;

using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace BlankRoadBuilder.Util;

public static class AssetUtil
{
	public static AssetModel ImportAsset(RoadInfo road, MeshType meshType, ElevationType elevationType, RoadAssetType type, CurbType curb, string name, bool inverted, bool noAsphaltTransition, bool bridgeShader)
	{
		if (elevationType >= ElevationType.Slope)
		{
			type = RoadAssetType.Segment;
		}

		var curbId = curb > CurbType.TR ? $"1_HC" : $"{(int)curb}_{curb}";
		var fileName = $"{MeshName(elevationType)}_{type.ToString().ToLower()}-{curbId}.obj";
		var exportName = $"{elevationType}_{(inverted ? "inverted_" : "")}{type.ToString().ToLower()}-{curbId}.obj";

		PrepareMeshFiles(road, name, meshType, elevationType, curb, inverted, noAsphaltTransition, fileName, exportName, bridgeShader);

		return ImportAsset(
			elevationType == ElevationType.Basic && !bridgeShader ? ShaderType.Basic : ShaderType.Bridge,
			meshType,
			exportName,
			filesReady: true);
	}

	public static AssetModel ImportAsset(ShaderType shaderType, MeshType meshType, string fileName, bool filesReady = false, float? scale = null, float? offset = null)
	{
		var outputName = filesReady ? fileName : $"{Guid.NewGuid()}{Path.GetExtension(fileName)}";

		if (!filesReady)
		{
			PrepareFiles(meshType, fileName, outputName, offset);
		}

		var assetModel = new AssetModelUtil(shaderType, scale: scale);

		return assetModel.Import(outputName);
	}

	private static void PrepareFiles(MeshType meshType, string fileName, string outputName, float? offset = null)
	{
		var baseName = Path.GetFileNameWithoutExtension(fileName);

		Directory.CreateDirectory(BlankRoadBuilderMod.ImportFolder);

		foreach (var file in Directory.GetFiles(Path.Combine(BlankRoadBuilderMod.MeshesFolder, meshType.ToString()), $"{baseName}*"))
		{
			if (offset is null)
			{
				File.Copy(file, Path.Combine(BlankRoadBuilderMod.ImportFolder, replace(file)), true);
			}
			else
			{
				File.WriteAllLines(Path.Combine(BlankRoadBuilderMod.ImportFolder, replace(file)), Offset(file, offset.Value));
			}
		}

		foreach (var file in Directory.GetFiles(Path.Combine(BlankRoadBuilderMod.TexturesFolder, meshType.ToString()), $"{baseName}*"))
		{
			File.Copy(file, Path.Combine(BlankRoadBuilderMod.ImportFolder, replace(file)), true);
		}

		string replace(string file) => Path.GetFileName(file).Remove(0, baseName!.Length).Insert(0, Path.GetFileNameWithoutExtension(outputName));
	}

	private static void PrepareMeshFiles(RoadInfo road, string name, MeshType meshType, ElevationType elevationType, CurbType curb, bool inverted, bool noAsphaltTransition, string fileName, string exportFile, bool tunnel)
	{
		Directory.CreateDirectory(BlankRoadBuilderMod.ImportFolder);

		var baseName = Path.GetFileNameWithoutExtension(fileName);
		var exportName = Path.GetFileNameWithoutExtension(exportFile);

		foreach (var file in Directory.GetFiles(Path.Combine(BlankRoadBuilderMod.MeshesFolder, meshType.ToString()), $"{baseName}*"))
		{
			var lines = Resize(file, name, road, elevationType, curb, inverted, tunnel);
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

			if (mesh is "p" or "r" || (mesh == "d" && noAsphaltTransition))
			{
				var ashpalt = elevationType == ElevationType.Basic ? (road.SideTexture == TextureType.Asphalt) : elevationType <= ElevationType.Bridge && (road.BridgeSideTexture == BridgeTextureType.Asphalt);
				var noashpalt = !ashpalt && road.AsphaltStyle == AsphaltStyle.None;
				var noCurb = (road.RoadType == RoadType.Highway || (road.RoadType == RoadType.Flat && ModOptions.RemoveCurbOnFlatRoads)) && curb != CurbType.TR;

				var requiredType = noAsphaltTransition ? "noasphalttransition" : ((ashpalt ? "asphalt" : noashpalt ? "noasphalt" : "") + (noCurb ? "nocurb" : ""));

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

	public static string[] Resize(string file, string name, RoadInfo road, ElevationType elevation, CurbType curb, bool inverted, bool tunnel)
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

					if (curb == CurbType.Curbless && (aPos >= 1 || yPos < -0.3))
					{
						if (aPos >= 1)
						{
							xPos = xPos > 0 ? 1 : -1;
							aPos = 1;
						}

						yPos = -0.3;
					}

					if (aPos < 3.75F && yPos >= -0.3)
					{
						xDiff -= ((xPos < 0) == inverted ? road.LeftPavementWidth : road.RightPavementWidth) - 3;

						if (curb is CurbType.BayFull || (curb is CurbType.BaySingle && xPos > 0))
						{
							var lane = road.Lanes.FirstOrDefault(x => x.Decorations.HasFlag(LaneDecoration.BusBay));
							var step = (int)(ModOptions.BusBaySize * 12 / 100);
							var size = decimal.Round((decimal)step * 32 / 12, 2);
							var perc = size == 32 ? 1 : (1 - (float)Math.Max(0, Math.Min(1, (Math.Abs(decimal.Round(decimal.Parse(data[3]), 2)) - size) / (32 - size))));

							xDiff += lane.LaneWidth * perc;
						}
					}
					else if (elevation is ElevationType.Elevated or ElevationType.Bridge)
					{
						xDiff += 0.8F;
					}
					else if (tunnel)
					{
						xDiff += 1.5F;
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

	public static string[] Offset(string file, float offset)
	{
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
				case "v":
				case "V":
					var xPos = double.Parse(data[1]) + offset;

					data[1] = xPos.ToString("0.00000000");

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
			ElevationType.Slope => "slope",
			ElevationType.Tunnel => "tunnel",
			ElevationType.Bridge or ElevationType.Elevated => "elevated",
			_ => "basic",
		};
	}
}