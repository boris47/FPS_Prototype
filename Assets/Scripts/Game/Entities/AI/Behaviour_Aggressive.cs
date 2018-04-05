
using UnityEngine;

namespace AI_Behaviours {

	public enum AggresiveMode {
		ALARMED,
		SEEKER,
		ATTACKER
	}

	public class Behaviour_Aggressive : Behaviour_Base {


		private		AggresiveMode		m_Mode				= default( AggresiveMode );


		public Behaviour_Aggressive( Brain brain, MonoBehaviour monoBehaviour, AggresiveMode mode )
		{
			this.m_Brain = brain;
			this.m_MonoBehaviour = monoBehaviour;
			this.m_Mode = mode;
		}

		public override void OnEnable()
		{
			if ( m_MonoBehaviour != null )
				m_MonoBehaviour.enabled = true;
		}

		public override void OnDisable()
		{
			if ( m_MonoBehaviour != null )
				m_MonoBehaviour.enabled = true;
		}


		public override void OnFrame( float deltaTime )
		{
			
		}


		public override void OnThink()
		{
			switch( m_Mode )
			{
				case AggresiveMode.ALARMED:
					OnAlarmedState();			break;

				case AggresiveMode.SEEKER:
					OnSeekerState();			break;

				case AggresiveMode.ATTACKER:
					OnAttackerState();			break;
			}
		}



		private	void	OnAlarmedState()
		{

		}



		private void	OnSeekerState()
		{

		}



		private	void	OnAttackerState()
		{

		}

	}

}
