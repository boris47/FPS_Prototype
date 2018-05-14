

using UnityEngine;

namespace CutScene {

	public class MovingTarget : MonoBehaviour {

		public	Path	m_path	= null;
		public	float	m_Speed = 1f;


		private void Update()
		{
		
			Vector3 position = Vector3.zero;
			if ( m_path.Move( m_Speed, ref position ) )
			{
				transform.position = position;
			}

		}

	}

}
