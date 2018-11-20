

using UnityEngine;

namespace CutScene {

	public class MovingTarget : MonoBehaviour {

		[SerializeField]
		private		PathBase	m_Path				= null;
		[SerializeField]
		private		float		m_Speed				= 1f;


		private void Update()
		{
			Vector3	position	= transform.position;
			Quaternion rotation = transform.rotation;
			if ( false == m_Path.Move( m_Speed, ref position, ref rotation, Vector3.up ) )
			{
				enabled = false;
			}
			else
			{
				transform.position = position;
				transform.rotation = rotation;
			}
		}
		
	}

}
