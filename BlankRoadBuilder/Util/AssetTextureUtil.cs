using ColossalFramework.Importers;
using ColossalFramework.IO;

using System.Linq;

using UnityEngine;

namespace BlankRoadBuilder.Util;

public sealed class AssetTextureUtil : AssetImporterTextureLoader
{
	private static readonly ResultType[] _textureTypes = new ResultType[]
	{
		ResultType.RGB,
		ResultType.XYS,
		ResultType.APR
	};

	private struct Size
	{
		public int Width;
		public int Height;
	}

	public static void LoadTextures(GameObject? model, string file, bool lod)
	{
		var textureBaseName = file.Substring(0, file.LastIndexOf("."));

		if (lod)
		{
			textureBaseName += "_lod";
		}

		var sourceTypes = _textureTypes.SelectMany(x => NeededSources[(int)x]).Distinct().ToList();
		var textureImages = new Image[8];

		foreach (var sourceType in sourceTypes)
		{
			textureImages[(int)sourceType] = ImportTexture(textureBaseName, sourceType);
		}

		var textureSize = GetTextureSize(textureImages);

		for (var m = 0; m < _textureTypes.Length; m++)
		{
			var sampler = ResultSamplers[(int)_textureTypes[m]];
			var textureInfo = BuildTexture(textureImages, _textureTypes[m], textureSize.Width, textureSize.Height, !lod);
			Texture2D? texture2D = null;

			if (textureInfo != null)
			{
				var defaultColor = ResultDefaults[(int)_textureTypes[m]];
				var resultLinear = ResultLinear[sampler];

				texture2D = new Texture2D(textureSize.Width, textureSize.Height, sampler == "_XYCAMap" ? TextureFormat.RGBA32 : TextureFormat.RGB24, false, resultLinear);
				texture2D.SetPixels(textureInfo);
				texture2D.anisoLevel = 4;
				texture2D.Apply();
			}

			ApplyTexture(model, sampler, texture2D);
		}
	}

	private static Size GetTextureSize(Image[] textureImages)
	{
		for (var i = 0; i < textureImages.Length; i++)
		{
			if (textureImages[i] == null || textureImages[i].width <= 0 || textureImages[i].height <= 0)
			{
				continue;
			}

			return new Size
			{
				Width = textureImages[i].width,
				Height = textureImages[i].height
			};
		}

		return new Size();
	}

	private static Image ImportTexture(string baseName, SourceType source)
	{
		var textureFile = $"{baseName}{SourceTypeSignatures[(int)source]}.png";

		if (FileUtils.Exists(textureFile))
		{
			return new Image(textureFile);
		}

		return null;
	}
}