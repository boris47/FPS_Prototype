

using UnityEngine;

namespace CutScene {

	public class MovingTarget : MonoBehaviour {

		[SerializeField]
		private		PathBase	m_Path				= null;
		[SerializeField]
		private		float		m_Speed				= 1f;

		[SerializeField]
		private		bool		m_UseUpVector		= true;


		private	Transform thisTransform = null;

		private void Awake()
		{
			thisTransform = transform;
		}

		private void FixedUpdate()
		{
			Vector3	position	= transform.position;
			Quaternion rotation = transform.rotation;
			bool completed = m_UseUpVector ? m_Path.Move( ref thisTransform, m_Speed, Vector3.up ) : m_Path.Move( ref thisTransform, m_Speed, null );

			if ( completed == true )
			{
				enabled = false;
			}
		}
		
	}

}
