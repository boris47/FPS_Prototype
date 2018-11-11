
using System.Collections.Generic;
using UnityEngine;


namespace CutScene {

	public class Path : MonoBehaviour {
	
		[SerializeField]
		private		GameEvent			m_OnPathCompleted	= null;

		private		Vector3[]			m_Nodes				= null;
		private		int					m_CurrentSegment	= 0;
		private		float				m_Interpolant		= 0f;
		private		bool				m_IsCompleted		= false;

		private		Vector3				m_PrevPosition		= Vector3.zero;
		private		float				m_PathLength		= 0.0f;


		private void Awake()
		{
			List<Vector3> vectors = new List<Vector3>();
			foreach( Transform child in transform.GetComponentOnlyInChildren<Transform>() )
			{
				vectors.Add( child.position );
			}

			vectors.Insert( 0, vectors[0] );
			vectors.Insert( vectors.Count-1, vectors[vectors.Count-1] );

			m_Nodes = vectors.ToArray();

			const float density = 100.0f;
			float step = 0.01f;
			while ( step < density )
			{
				float interpolant = step / density;
				Vector3 position = Utils.Math.GetPoint( m_Nodes, interpolant );
				m_PathLength = ( m_PrevPosition - position ).magnitude;

				m_PrevPosition = position;

				step += 1.0f;
			}

		}


		public	bool Move( float speed, ref Vector3 position )
		{
			if ( m_IsCompleted )
				return false;

			m_Interpolant += ( Time.deltaTime ) * speed;
			position = Utils.Math.GetPoint( m_Nodes, m_Interpolant );

			if ( m_Interpolant >= 1.0f )
			{
				m_IsCompleted = true;

				if ( m_OnPathCompleted != null && m_OnPathCompleted.GetPersistentEventCount() > 0 )
				{
					m_OnPathCompleted.Invoke();
				}
			}

			return true;
		}
		

		public	void	DrawGizmos()
		{
			OnDrawGizmosSelected();
		}
		

		private void OnDrawGizmosSelected()
		{
			List<Vector3> vectors = new List<Vector3>();
			foreach( Transform child in transform.GetComponentOnlyInChildren<Transform>() )
			{
				vectors.Add( child.position );
			}

			vectors.Insert( 0, vectors[0] );
			vectors.Insert( vectors.Count-1, vectors[vectors.Count-1] );

			m_Nodes = vectors.ToArray();

			Vector3 prevPosition = Vector3.zero;
			const float density = 100.0f;
			float step = 0.01f;
			while ( step < density )
			{
				float interpolant = step / density;
				Vector3 position = Utils.Math.GetPoint( m_Nodes, interpolant );
				Gizmos.DrawLine( prevPosition, position );
				prevPosition = position;

				step += 1.0f;
			}
		}

	}

}