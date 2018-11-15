

using UnityEngine;

namespace CutScene {

	public class MovingTarget : MonoBehaviour {

		[SerializeField]
		private		PathBase	m_Path				= null;
		[SerializeField]
		private		float		m_Speed				= 1f;


		private void Update()
		{
			Vector3	position = Vector3.zero;
			if ( false == m_Path.Move( m_Speed, ref position ) )
			{
				enabled = false;
			}
			else
			{
				transform.position = position;
			}
		}
		
	}

}
