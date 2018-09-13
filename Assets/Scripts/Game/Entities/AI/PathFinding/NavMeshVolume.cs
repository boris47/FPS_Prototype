using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Collections.ObjectModel;

namespace AI.Pathfinding {

	[RequireComponent(typeof(MeshRenderer))]
	public class NavMeshVolume : MonoBehaviour {

		[SerializeField]
		private		float			m_StepSize		= 1f;
		public		float			StepSize
		{
			get { return m_StepSize; }
			set { m_StepSize = Mathf.Max( 0.01f, value ); }
		}

		private		MeshRenderer	m_MeshRenderer	= null;
		private		MeshFilter		m_MeshFilter	= null;



		//////////////////////////////////////////////////////////////////////////
		private void Awake()
		{
			EnsureComponents();
		}


		//////////////////////////////////////////////////////////////////////////
		private	void	EnsureComponents()
		{
			if ( ( m_MeshRenderer == null ) && ( m_MeshRenderer = GetComponent<MeshRenderer>() ) == null )
			{
				m_MeshRenderer = gameObject.AddComponent<MeshRenderer>();
			}

			if ( ( m_MeshFilter == null ) && ( m_MeshFilter = GetComponent<MeshFilter>() ) == null )
			{
				m_MeshFilter = gameObject.AddComponent<MeshFilter>();
			}
		}


		//////////////////////////////////////////////////////////////////////////
		private void	Start()
		{
			m_MeshRenderer.enabled = false;
		}


		//////////////////////////////////////////////////////////////////////////
		public	void	IterateOver( System.Action<Vector3> OnPosition )
		{
			if ( OnPosition == null )
				return;

			float extentsX = transform.localScale.x / 2.0f;
			float extentsZ = transform.localScale.z / 2.0f;

			float currentStepX = transform.localScale.x;
			float currentStepZ = transform.localScale.z;

			Vector3 position = Vector3.zero;

			while ( true )
			{
				float currentX = transform.position.x - extentsX + currentStepX;
				float currentZ = transform.position.z - extentsZ + currentStepZ;
				float currentY = transform.position.y;

				position.Set( currentX, currentY, currentZ );

				OnPosition( position );

				currentStepX -= m_StepSize;
				if ( currentStepX <= 0.0f )
				{
					currentStepZ -= m_StepSize;
					if ( currentStepZ <= 0.0f )
					{
						break;
					}
					currentStepX = transform.localScale.x;
				}
			}
		}
		

		//////////////////////////////////////////////////////////////////////////
		public	void	Clear()
		{
			PathFinder.ReleaseNodesByVolume( this );
		}


		//////////////////////////////////////////////////////////////////////////
		public	bool	IsPositionInside( Vector3 Position )
		{
			EnsureComponents();
		
			return IsPointInside( Position );
		}

		// TODO move this into utils
		//////////////////////////////////////////////////////////////////////////
		private bool IsPointInside( Vector3 WorldPosition )
		{
			Mesh aMesh = m_MeshFilter.sharedMesh;
			Vector3 aLocalPoint = m_MeshRenderer.transform.InverseTransformPoint(WorldPosition);
			Plane plane = new Plane();
			
			var verts = aMesh.vertices;
			var tris = aMesh.triangles;
			int triangleCount = tris.Length / 3;
			for ( int i = 0; i < triangleCount; i++ )
			{
				var V1 = verts[ tris[ i * 3 ] ];
				var V2 = verts[ tris[ i * 3 + 1 ] ];
				var V3 = verts[ tris[ i * 3 + 2 ] ];
				plane.Set3Points( V1, V2, V3 );
				if ( plane.GetSide( aLocalPoint ) )
					return false;
			}
			return true;
		}
	}

}