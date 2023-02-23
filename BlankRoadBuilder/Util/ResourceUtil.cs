using ColossalFramework.Importers;
using ColossalFramework.IO;
using ColossalFramework.UI;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEngine;

namespace BlankRoadBuilder.Util;
internal class ResourceUtil
{
	private static readonly Dictionary<string?, Image?> _resources;

	static ResourceUtil()
	{
		var baseName = $"{nameof(BlankRoadBuilder)}.Properties.Icons.";
		var assembly = typeof(BlankRoadBuilderMod).Assembly;
		var resources = assembly.GetManifestResourceNames().Where(x => x.StartsWith(baseName));

		_resources = resources.ToDictionary(x => (string?)x.Substring(baseName.Length), x =>
		{
			var stream = assembly.GetManifestResourceStream(x);

			if (stream == null)
				return null;

			using var ms = new MemoryStream();
			stream.CopyTo(ms);
			return new Image(ms.ToArray());
		}, StringComparer.InvariantCultureIgnoreCase);
	}

	public static Image? GetImage(string? name)
	{
		return _resources.TryGetValue(name, out var image) ? image : null;
	}

	internal static UITextureAtlas? GetAtlas(string name, UITextureAtlas.SpriteInfo[] sprites)
	{
		return GetAtlas(name, null, sprites: sprites);
	}

	internal static UITextureAtlas? GetAtlas(string name, string? spriteName = "normal", int border = 0)
	{
		return GetAtlas(name, spriteName, border, null);
	}

	private static UITextureAtlas? GetAtlas(string name, string? spriteName = "normal", int border = 0, UITextureAtlas.SpriteInfo[]? sprites = null)
	{
		return GetAtlas(GetImage(name)?.CreateTexture(), spriteName, border, sprites);
	}

	internal static UITextureAtlas? GetAtlas(Texture2D? texture2D, UITextureAtlas.SpriteInfo[] sprites)
	{
		return GetAtlas(texture2D, null, 0, sprites);
	}

	internal static UITextureAtlas? GetAtlas(Texture2D? texture2D, string? spriteName = "normal", int border = 0)
	{
		return GetAtlas(texture2D, spriteName, border, null);
	}

	private static UITextureAtlas? GetAtlas(Texture2D? texture2D, string? spriteName = "normal", int border = 0, UITextureAtlas.SpriteInfo[]? sprites = null)
	{
		if (texture2D == null)
			return null;

		var uITextureAtlas = ScriptableObject.CreateInstance<UITextureAtlas>();
		uITextureAtlas.name = Guid.NewGuid().ToString();
		uITextureAtlas.material = UnityEngine.Object.Instantiate(UIView.GetAView().defaultAtlas.material);
		uITextureAtlas.material.mainTexture = texture2D;
		uITextureAtlas.padding = border;

		if (sprites != null)
		{
			uITextureAtlas.AddSprites(sprites);
		}
		else if (!string.IsNullOrEmpty(spriteName))
		{
			uITextureAtlas.AddSprite(new UITextureAtlas.SpriteInfo
			{
				name = spriteName,
				texture = texture2D,
				border = new(border, border, border, border),
				region = new(0f, 0f, 1f, 1f)
			});
		}

		return uITextureAtlas;
	}
}
