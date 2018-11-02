

using UnityEngine;

namespace CutScene {

	public class MovingTarget : MonoBehaviour {

		public	Path		m_Path			= null;
		public	float		m_Speed			= 1f;

		private	Vector3		m_Position		= Vector3.zero;


		private void Update()
		{
			enabled = m_Path.Move( m_Speed, ref m_Position );
			transform.position = m_Position;
		}
		
	}

}
