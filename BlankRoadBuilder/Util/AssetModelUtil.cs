using BlankRoadBuilder.Domain;

using ColossalFramework;
using ColossalFramework.Importers;
using ColossalFramework.Threading;

using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Xml.Linq;

using UnityEngine;

namespace BlankRoadBuilder.Util;

public class AssetModelUtil : ImportAssetLodded
{
	private readonly Shader _templateShader;
	private readonly bool _importLOD;
	private readonly float? _scale;

	protected override int textureAnisoLevel => 4;

	public AssetModelUtil(ShaderType shaderType, float? scale = null) : base(new GameObject(), new PreviewCamera())
	{
		m_LodTriangleTarget = 50;

		_templateShader = shaderType switch
		{	
			ShaderType.Bridge => Shader.Find("Custom/Net/RoadBridge"),
			ShaderType.Rail => Shader.Find("Custom/Net/TrainBridge"),
			ShaderType.Wire => Shader.Find("Custom/Net/Electricity"),
			_ => Shader.Find("Custom/Net/Road"),
		};

		_importLOD = shaderType != ShaderType.Wire;
		_scale = scale;
	}

	public AssetModel Import(string filename)
	{
		m_Path = BlankRoadBuilderMod.ImportFolder;
		m_Filename = filename;

		if (!File.Exists(Path.Combine(m_Path, m_Filename)))
		{
			Debug.LogError("Could not find file to import: " + filename);

			return new AssetModel();
		}

		m_Object = CreateObject(false);

		if (_importLOD)
		{
			m_LODObject = CreateObject(true);
		}

		GenerateAssetData();

		if (_scale != null ? _scale != 1F : !(m_Filename?.EndsWith(".fbx", StringComparison.CurrentCultureIgnoreCase) ?? false))
		{
			ApplyTransform(new Vector3(_scale ?? 100F, _scale ?? 100F, _scale ?? 100F), Vector3.zero, false);
		}

		if (_importLOD)
		{
			AssetTextureUtil.LoadTextures(m_Object, Path.Combine(m_Path, m_Filename), false);

			AssetTextureUtil.LoadTextures(m_LODObject, Path.Combine(m_Path, m_Filename), true);
		}

		CompressTextures();

		return new AssetModel
		{
			m_material = m_Object?.GetComponent<MeshRenderer>()?.sharedMaterial,
			m_mesh = m_Object?.GetComponent<MeshFilter>()?.sharedMesh,
			m_lodMesh = m_LODObject?.GetComponent<MeshFilter>()?.sharedMesh,
			m_lodMaterial = m_LODObject?.GetComponent<MeshRenderer>()?.sharedMaterial,
		};
	}

	protected GameObject? CreateObject(bool lod)
	{
		var fileName = Path.Combine(m_Path,
			lod ? m_Filename.Insert(m_Filename.LastIndexOf('.'), sLODModelSignature)
			: m_Filename);

		var importer = new SceneImporter
		{
			filePath = fileName,
			importSkinMesh = true
		};

		var @object = importer.Import();

		@object?.SetActive(false);

		return @object;
	}

	protected void GenerateAssetData()
	{
		var meshFiler = m_Object.GetComponent<MeshFilter>();

		if ((meshFiler?.sharedMesh) != null)
		{
			m_OriginalMesh = RuntimeMeshUtils.CopyMesh(meshFiler.sharedMesh);
		}

		var renderer = m_Object.GetComponent<MeshRenderer>();

		if (renderer?.sharedMaterial != null)
		{
			renderer.sharedMaterial = new Material(_templateShader);

			if (renderer.sharedMaterial.HasProperty("_Color"))
			{
				renderer.sharedMaterial.SetColor("_Color", Color.gray);
			}
		}

		if (!_importLOD)
			return;
		
		meshFiler = m_LODObject?.GetComponent<MeshFilter>();

		if ((meshFiler?.sharedMesh) != null)
		{
			m_OriginalLodMesh = RuntimeMeshUtils.CopyMesh(meshFiler.sharedMesh);
		}

		renderer = m_LODObject?.GetComponent<MeshRenderer>();

		if (renderer?.sharedMaterial != null)
		{
			renderer.sharedMaterial = new Material(_templateShader);

			if (renderer.sharedMaterial.HasProperty("_Color"))
			{
				renderer.sharedMaterial.SetColor("_Color", Color.gray);
			}
		}
	}

	protected override void CompressTextures()
	{
		var material = m_Object.GetComponent<Renderer>()?.sharedMaterial;

		if (material == null)
		{
			return;
		}

		if (material.HasProperty("_MainTex"))
		{
			CompressTexture(material, "_MainTex", false);
		}
		if (material.HasProperty("_XYSMap"))
		{
			CompressTexture(material, "_XYSMap", true);
		}
		if (material.HasProperty("_ARPMap"))
		{
			CompressTexture(material, "_APRMap", true);
		}
	}

	public static void CompressTexture(Material mat, string textureName, bool linear)
	{
		var texture = mat.GetTexture(textureName) as Texture2D;

		if (texture == null)
		{
			return;
		}

		var image = new Image(texture);

		if (!textureName.Equals("_XYCAMap"))
		{
			image.Convert(TextureFormat.RGB24);
		}

		image.Compress();

		var texture2 = image.CreateTexture(linear);

		texture2.name = textureName;
		texture2.anisoLevel = texture.anisoLevel;

		mat.SetTexture(textureName, texture2);

		UnityEngine.Object.Destroy(texture);
	}

	protected override void InitializeObject() { }

	protected override void CreateInfo() { }
}