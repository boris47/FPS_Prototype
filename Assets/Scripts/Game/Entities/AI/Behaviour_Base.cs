
using UnityEngine;


namespace AI.Behaviours {

	public abstract class Behaviour_Base : MonoBehaviour {

		protected			Brain				m_Brain				= null;
		protected			IEntity				m_ThisEntity		= null;

		public void	Setup( Brain brain, IEntity ThisEntity )
		{
			m_ThisEntity = ThisEntity;
			this.m_Brain = brain;
		}

		public	abstract	void	OnEnable();

		public	abstract	void	OnDisable();

		public	abstract	void	OnThink();

	}

}