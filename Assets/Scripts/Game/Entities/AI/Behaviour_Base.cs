
using UnityEngine;


namespace AI.Behaviours {

	public abstract class Behaviour_Base : MonoBehaviour {

		protected			IBrain				m_Brain				= null;
		protected			IEntity				m_ThisEntity		= null;

		public Behaviour_Base	Setup( Brain brain, IEntity ThisEntity )
		{
			m_ThisEntity = ThisEntity;
			m_Brain = brain;
			return this;
		}

		protected	abstract	void	OnEnable();

		protected	abstract	void	OnDisable();

		public		abstract	void	OnThink();

	}

}