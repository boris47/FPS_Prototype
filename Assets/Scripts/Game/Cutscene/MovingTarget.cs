

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

		private void Update()
		{
			Vector3	position	= transform.position;
			Quaternion rotation = transform.rotation;
			bool result = m_UseUpVector ? m_Path.Move( m_Speed, ref thisTransform, Vector3.up ) : m_Path.Move( m_Speed, ref thisTransform, null );

			if ( result == false )
			{
				enabled = false;
			}
		}
		
	}

}
