

using UnityEngine;

namespace CutScene
{
	public class MovingTarget : MonoBehaviour
	{
		[SerializeField]
		private		PathBase	m_Path				= null;
		[SerializeField]
		private		float		m_Speed				= 1f;

		[SerializeField]
		private		bool		m_UseUpVector		= true;

		private void FixedUpdate()
		{
			bool bIsCompleted = m_UseUpVector ? m_Path.Move(transform, m_Speed, Vector3.up) : m_Path.Move(transform, m_Speed, null);
			if ( bIsCompleted )
			{
				enabled = false;
			}
		}
	}
}
