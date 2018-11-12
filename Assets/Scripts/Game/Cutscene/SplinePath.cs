
using System.Collections.Generic;
using UnityEngine;


namespace CutScene {

	public class SplinePath : MonoBehaviour {
	
		[SerializeField]
		private		GameEvent			m_OnPathCompleted	= null;

		private		Vector3[]			m_Nodes				= null;
		private		float				m_Interpolant		= 0f;
		private		bool				m_IsCompleted		= false;

		private		Vector3				m_PrevPosition		= Vector3.zero;
		private		float				m_PathLength		= 0.0f;


		private	Vector3[]	FindNodes()
		{
			List<Vector3> vectors = new List<Vector3>();
			foreach( Transform child in transform.GetComponentOnlyInChildren<Transform>() )
			{
				vectors.Add( child.position );
			}

			vectors.Insert( 0, vectors[0] );
			vectors.Insert( vectors.Count-1, vectors[vectors.Count-1] );

			return vectors.ToArray();
		}


		private void	Awake()
		{
			m_Nodes = FindNodes();

			Vector3 prevPosition = m_Nodes[0];

			IterateSpline( 100.0f, 1.0f, 
				( Vector3 position ) => {
					m_PathLength = ( prevPosition - position ).magnitude;
					prevPosition = position;
				}
			);

		}

		// Spline iteration
		private void	IterateSpline( float Steps, float StepLength, System.Action<Vector3> OnPosition )
		{
			if ( OnPosition == null )
			{
				return;
			}

			Vector3 prevPosition = m_Nodes[0];
			float currentStep = 0.001f;
			while ( currentStep < Steps )
			{
				float interpolant = currentStep / Steps;
				Vector3 position = Utils.Math.GetPoint( m_Nodes, interpolant );
				
				OnPosition( position );

				currentStep += StepLength;
			}
		}

		public	bool	Move( float speed, ref Vector3 position )
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
		
		// called by childs
		public	void	DrawGizmos()
		{
			OnDrawGizmosSelected();
		}
		

		private void	OnDrawGizmosSelected()
		{
			m_Nodes = FindNodes();

			Vector3 prevPosition = m_Nodes[0];

			IterateSpline( 100.0f, 1.0f, 
				( Vector3 position ) => {
					m_PathLength = ( prevPosition - position ).magnitude;
					Gizmos.DrawLine( prevPosition, position );
					prevPosition = position;
				}
			);
			
		}

	}

}