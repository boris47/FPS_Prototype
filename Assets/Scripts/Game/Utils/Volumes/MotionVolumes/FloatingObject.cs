using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AI.Pathfinding;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class Voxel
{
	[SerializeField, HideInInspector]
	private Component m_Owner = null;

	[SerializeField]
	private Vector3 m_LocalPosition = Vector3.zero;

	[SerializeField]
	private Vector3 m_Size = Vector3.zero;

	public Vector3 Position => m_Owner.transform.TransformPoint(m_LocalPosition);
	public Vector3 Size => m_Size;

	public Voxel(in Component InOwner, in Vector3 InLocalPosition, in Vector3 InVoxelSize)
	{
		m_Owner = InOwner;
		m_LocalPosition = InLocalPosition;
		m_Size = InVoxelSize;
	}
}


[RequireComponent(typeof(Rigidbody))]
public class FloatingObject : MonoBehaviour
{
	[SerializeField, ReadOnly]
	private Rigidbody m_Rigidbody = null;

	[SerializeField, Min(0.1f)]
	private float m_Density = 0.5f;

	[SerializeField]
	private float m_AngularTimeDrag = 1f;

	[SerializeField]
	private Vector3 m_VoxelsPerWorldAxis = Vector3.one * 3f;

	[SerializeField]
	private Transform[] m_VoxelsCandidates = new Transform[0];

	[SerializeField, HideInInspector]
	private Voxel[] m_BakedVoxels = null;

	[SerializeField]
	public bool m_UseChildren = false;


	public float Density => m_Density;
	public float AngularTimeDrag => m_AngularTimeDrag;
	public Voxel[] Voxels => m_BakedVoxels;
	public Rigidbody Rigidbody => m_Rigidbody;



	//////////////////////////////////////////////////////////////////////////
	private void Awake()
	{
		Utils.CustomAssertions.IsTrue(gameObject.TryGetComponent(out m_Rigidbody));
	}

	//////////////////////////////////////////////////////////////////////////
	private void OnValidate()
	{
		gameObject.TryGetComponent(out m_Rigidbody);

		// TODO Move to inspector button the bake

		BakeVoxels();
	}

	//////////////////////////////////////////////////////////////////////////
	private void BakeVoxels()
	{
		if (m_UseChildren)
		{
			if (m_VoxelsCandidates.Length == 0)
			{
				m_VoxelsCandidates = transform
				.GetComponentsOnlyInChildren<Transform>(true, t => t.gameObject.activeInHierarchy && t.TryGetComponent(out Renderer _));
			}
			m_BakedVoxels = m_VoxelsCandidates
				.Where(t => t.IsNotNull())
				.Select(t => new Voxel(this, transform.InverseTransformPoint(t.position), Utils.Math.GetBoundsOf(t).size))
				.ToArray();
		}
		else
		{
			(Vector3, Vector3)[] points = Utils.Math.IterateBoxVolume
			(
				InVolumePosition: transform.position,
				InVolumeRotation: transform.rotation,
				InVolumeSize: Utils.Math.GetBoundsOf(gameObject).size,
				InCountForAxis: m_VoxelsPerWorldAxis
			);
		//	(Vector3, Vector3)[] points = Utils.Math.IterateSphereVolume
		//	(
		//		InSpherePosition: transform.position,
		//		InSphereRadius: Utils.Math.GetBoundsOf(gameObject).size.magnitude*0.5f,
		//		InMaxLats: m_VoxelsPerWorldAxis.x,
		//		InMaxLongs: m_VoxelsPerWorldAxis.y
		//	);
			m_BakedVoxels = points.Select(tuple =>
			{
				Vector3 localPosition = transform.InverseTransformPoint(tuple.Item1);
				return new Voxel(InOwner: this, InLocalPosition: localPosition, InVoxelSize: tuple.Item2);
			}).ToArray();
		}
	}

	//////////////////////////////////////////////////////////////////////////
	private void OnDrawGizmosSelected()
	{
		using (new Utils.Editor.GizmosHelper.UseGizmoColor(Color.magenta - new Color(0f, 0f, 0f, 0.75f)))
		{
			foreach (Voxel voxel in m_BakedVoxels)
			{
				Gizmos.DrawCube(voxel.Position, voxel.Size * 0.9f);
			}
		}
	}

	//////////////////////////////////////////////////////////////////////////
	[CustomEditor(typeof(FloatingObject))]
	private class FloatingObjectEditor : Editor
	{
		private FloatingObject m_Instance = null;

		//////////////////////////////////////////////////////////////////////////
		private void OnEnable()
		{
			m_Instance = (FloatingObject)target;
		}

		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			if (GUILayout.Button("Bake Voxels"))
			{
				m_Instance.BakeVoxels();
			}
		}
	}
}
