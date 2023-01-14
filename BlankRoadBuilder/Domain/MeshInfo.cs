using BlankRoadBuilder.Domain;

using PrefabMetadata.API;
using PrefabMetadata.Helpers;

using static AdaptiveRoads.Manager.NetInfoExtionsion;

namespace BlankRoadBuilder.Util;

public struct MeshInfo<MeshType, MetaDataType>
{
	public MeshType Mesh { get; set; }
	public MetaDataType MetaData { get; set; }

	public MeshInfo(AssetModel model)
	{
		if (typeof(MeshType) == typeof(NetInfo.Segment))
		{
			var mesh = new NetInfo.Segment().Extend().Base;
			var data = new Segment(mesh)
			{
				Forward = new SegmentInfoFlags { Forbidden = RoadUtils.S_Curbless },
				Backward = new SegmentInfoFlags { Forbidden = RoadUtils.S_Curbless }
			};

			(mesh as IInfoExtended)?.SetMetaData(data);

			mesh.m_mesh = model.m_mesh;
			mesh.m_material = model.m_material;
			mesh.m_lodMesh = model.m_lodMesh;
			mesh.m_lodMaterial = model.m_lodMaterial;

			Mesh = (MeshType)(object)mesh;
			MetaData = (MetaDataType)(object)data;
		}
		else
		{
			var mesh = new NetInfo.Node().Extend().Base;
			var data = new Node(mesh) { NodeFlags = new NodeInfoFlags { Forbidden = RoadUtils.N_Nodeless } };

			mesh.m_tagsForbidden = new string[0];
			mesh.m_tagsRequired = new string[0];

			(mesh as IInfoExtended)?.SetMetaData(data);

			mesh.m_mesh = model.m_mesh;
			mesh.m_material = model.m_material;
			mesh.m_lodMesh = model.m_lodMesh;
			mesh.m_lodMaterial = model.m_lodMaterial;

			Mesh = (MeshType)(object)mesh;
			MetaData = (MetaDataType)(object)data;
		}
	}

	public static implicit operator MeshType(MeshInfo<MeshType, MetaDataType> mesh) => mesh.Mesh;
}