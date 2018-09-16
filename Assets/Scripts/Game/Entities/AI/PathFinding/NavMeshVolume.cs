using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Collections.ObjectModel;

namespace AI.Pathfinding {

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

			float extentsX = -transform.lossyScale.x / 2.0f;
			float extentsY =  transform.lossyScale.y / 2.0f;
			float extentsZ = -transform.lossyScale.z / 2.0f;

			float currentStepX = transform.lossyScale.x;
			float currentStepZ = transform.lossyScale.z;

			Vector3 position = Vector3.zero;

			while ( true )
			{
				float currentX = extentsX + currentStepX;
				float currentZ = extentsZ + currentStepZ;
				float currentY = extentsY;

				position.Set( currentX, currentY, currentZ );

				OnPosition( transform.position + transform.rotation * position );

				currentStepX -= m_StepSize;
				if ( currentStepX <= 0.0f )
				{
					currentStepZ -= m_StepSize;
					if ( currentStepZ <= 0.0f )
					{
						break;
					}
					currentStepX = transform.lossyScale.x;
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
		
			return Utils.Math.IsPointInside( m_MeshFilter, Position );
		}
	}

}