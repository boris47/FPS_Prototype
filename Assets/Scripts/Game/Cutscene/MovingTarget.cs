

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
			this.thisTransform = this.transform;
		}

		private void FixedUpdate()
		{
			Vector3	position	= this.transform.position;
			Quaternion rotation = this.transform.rotation;
			bool completed = this.m_UseUpVector ? this.m_Path.Move( ref this.thisTransform, this.m_Speed, Vector3.up ) : this.m_Path.Move( ref this.thisTransform, this.m_Speed, null );

			if ( completed == true )
			{
				this.enabled = false;
			}
		}
		
	}

}
