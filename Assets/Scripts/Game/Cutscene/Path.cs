
using UnityEngine;


namespace CutScene {

	public class Path : MonoBehaviour {
	
		private		Transform[]			m_Nodes				= null;
		private		int					m_CurrentSegment	= 0;
		private		float				m_Interpolant		= 0f;
		private		bool				m_IsCompleted		= false;



		private void Awake()
		{
			m_Nodes = GetComponentsInChildren<Transform>();
		}


		public	bool Move( float speed, ref Vector3 position )
		{
			if ( m_IsCompleted )
				return false;

			float magnitude = ( m_Nodes[ m_CurrentSegment + 1 ].position - m_Nodes[ m_CurrentSegment ].position ).magnitude;
			m_Interpolant += ( Time.deltaTime * 1 / magnitude ) * speed;
			if ( m_Interpolant > 1f )
			{
				m_Interpolant = 0.0f;
				m_CurrentSegment ++;

				if ( m_CurrentSegment == m_Nodes.Length - 1 )
				{
					m_IsCompleted = true;
					return false;
				}
			}

			position = CutMullInterpolation( m_CurrentSegment, m_Interpolant );
			return true;
		}


		private	Vector3	CutMullInterpolation( int segment, float interpolant )
		{
			Vector3 p1, p2, p3, p4;

			if ( segment == 0 )
			{
				p1 = m_Nodes[ segment + 0 ].position;
				p3 = m_Nodes[ segment + 1 ].position;
				p4 = m_Nodes[ segment + 2 ].position;
				p2 = p1;
			}
			else
			if ( segment == m_Nodes.Length - 2 )
			{
				p1 = m_Nodes[ segment - 1 ].position;
				p2 = m_Nodes[ segment + 0 ].position;
				p3 = m_Nodes[ segment + 1 ].position;
				p4 = p3;
			}
			else
			{
				p1 = m_Nodes[ segment - 1 ].position;
				p2 = m_Nodes[ segment + 0 ].position;
				p3 = m_Nodes[ segment + 1 ].position;
				p4 = m_Nodes[ segment + 2 ].position;
			}
			return Interpolate( p1, p2, p3, p4, interpolant );
		}


		private    Vector3 Interpolate( Vector3 a, Vector3 b, Vector3 c, Vector3 d, float interpolant )
		{
			float t1 = interpolant;
			float t2 = interpolant * interpolant;
			float t3 = t2 * interpolant;
        
			return 0.5f * 
			(
				( 2f * b )								 +
				( -a + c )							* t1 +
				( 2f * a - 5f * b + 4f * c - d )	* t2 +
				( -a + 3f * b - 3f * c + d )		* t3
			);
		}


		public	void	DraawGizmos()
		{
			OnDrawGizmosSelected();
		}

		private void OnDrawGizmosSelected()
		{
			m_Nodes = GetComponentsInChildren<Transform>();

			for ( int i = 0; i < m_Nodes.Length - 1; i++ )
			{
				Transform p1 = m_Nodes[i];
				Transform p2 = m_Nodes[i+1];
//				Debug.DrawLine ( p1.position, p2.position );
				Gizmos.DrawLine ( p1.position, p2.position );
			}

		}

	}

}