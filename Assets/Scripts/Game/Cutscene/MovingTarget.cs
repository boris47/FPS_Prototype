

using UnityEngine;

namespace CutScene {

	public class MovingTarget : MonoBehaviour {

		[SerializeField]
		private		SplinePath	m_Path				= null;
		[SerializeField]
		private		float		m_Speed				= 1f;

		private		Vector3		m_Position			= Vector3.zero;

		


		private void Update()
		{
			if ( false == m_Path.Move( m_Speed, ref m_Position ) )
			{
				enabled = false;
			}
			else
			{
				transform.position = m_Position;
			}
		}
		
	}

}
