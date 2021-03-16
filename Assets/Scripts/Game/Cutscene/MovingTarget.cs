

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

		private void Update()
		{
			float deltaTime = Time.deltaTime;
			bool bIsCompleted = m_UseUpVector ? m_Path.Move(transform, m_Speed * deltaTime, Vector3.up) : m_Path.Move(transform, m_Speed * deltaTime, null);
			if ( bIsCompleted )
			{
				enabled = false;
			}
		}
	}
}
