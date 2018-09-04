
using UnityEngine;


namespace AI.Behaviours {

	public abstract class Behaviour_Base {

		protected			Brain				m_Brain				= null;
		protected			MonoBehaviour		m_MonoBehaviour		= null;

		public	abstract	void	OnEnable();

		public	abstract	void	OnDisable();

		public	abstract	void	OnFrame( float deltaTime );

		public	abstract	void	OnThink();

	}

}