

using UnityEngine;

namespace CutScene {

	public class MovingTarget : MonoBehaviour {

		[SerializeField]
		private		PathBase	m_Path				= null;
		[SerializeField]
		private		float		m_Speed				= 1f;

		[SerializeField]
		bool useUpVector = true;


		private void Update()
		{
			Vector3	position	= transform.position;
			Quaternion rotation = transform.rotation;
			bool result = useUpVector ? m_Path.Move( m_Speed, ref position, ref rotation, Vector3.up ) : m_Path.Move( m_Speed, ref position, ref rotation, null );

			if ( result == false )
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
