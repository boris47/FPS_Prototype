
using UnityEngine;

[System.Serializable]
public struct TargetInfo_t {
	public	bool	HasTarget;
	public	IEntity	CurrentTarget;
	public	float	TargetSqrDistance;

	public	void	Update( TargetInfo_t Infos )
	{
		HasTarget			= Infos.HasTarget;
		CurrentTarget		= Infos.CurrentTarget;
		TargetSqrDistance	= Infos.TargetSqrDistance;
	}

	public	void	Reset()
	{
		HasTarget = false;
		CurrentTarget = null;
		TargetSqrDistance = 0.0f;
	}
}

[ System.Serializable ]
public enum BrainState {
	EVASIVE,
	NORMAL		= 1,
	SEEKER		= 2,
	ALARMED		= 3,
	ATTACKER	= 4
}

namespace AI {

	using UnityEngine.AI;
	using AI.Behaviours;

	// Brain Interface
	public interface IBrain {
		IFieldOfView				FieldOfView				{ get; }
		BrainState					State					{ get; }

		void						ChangeState				( BrainState newState );
		void						OnReset					();
	}


	public class Brain : MonoBehaviour, IBrain {

		public	const		float						THINK_TIMER						= 0.2f; // 200 ms


		[SerializeField, ReadOnly]
		private				BrainState					m_CurrentBrainState				= BrainState.NORMAL;


		// INTERFACE START
							IFieldOfView				IBrain.FieldOfView				{	get { return m_FieldOfView;				}	}
							BrainState					IBrain.State					{	get { return m_CurrentBrainState;		}	}
		// INTERFACE END
		
		private				Behaviour_Evasive			m_BehaviourEvasive				= null;
		private				Behaviour_Normal			m_BehaviourNormal				= null;
		private				Behaviour_Alarmed			m_BehaviourAlarmed				= null;
		private				Behaviour_Seeker			m_BehaviourSeeker				= null;
		private				Behaviour_Attacker			m_BehaviourAttacker				= null;

		private				Behaviour_Base				m_CurrentBehaviour					= null;
		
		private				IFieldOfView				m_FieldOfView					= null;
		private				IEntity						m_ThisEntity					= null;
							


		//////////////////////////////////////////////////////////////////////////
		private void	Awake()
		{
			m_ThisEntity			= transform.GetComponent<IEntity>();
			m_FieldOfView			= transform.GetComponentInChildren<IFieldOfView>();
			m_FieldOfView.Setup( maxVisibleEntities : 10 );


			GameObject behaviours = transform.Find("Behaviours").gameObject;

			// SEARCH AND SETUP FOR BEHAVIOUR:	EVASIVE
			if ( Utils.Base.SearchComponent( behaviours, ref m_BehaviourEvasive, SearchContext.LOCAL ) )
			{
				m_BehaviourEvasive.Setup( this,		m_ThisEntity	).enabled = false;
			}

			// SEARCH AND SETUP FOR BEHAVIOUR:	NORMAL
			if ( Utils.Base.SearchComponent( behaviours, ref m_BehaviourNormal, SearchContext.LOCAL ) )
			{
				m_BehaviourNormal.Setup( this,		m_ThisEntity	);
				m_BehaviourNormal.enabled = false;
			}

			// SEARCH AND SETUP FOR BEHAVIOUR:	ALARMED
			if ( Utils.Base.SearchComponent( behaviours, ref m_BehaviourAlarmed, SearchContext.LOCAL ) )
			{
				m_BehaviourAlarmed.Setup( this,		m_ThisEntity	);
				m_BehaviourAlarmed.enabled = false;
			}

			// SEARCH AND SETUP FOR BEHAVIOUR:	SEEKER
			if ( Utils.Base.SearchComponent( behaviours, ref m_BehaviourSeeker, SearchContext.LOCAL ) )
			{
				m_BehaviourSeeker.Setup( this,		m_ThisEntity	);
				m_BehaviourSeeker.enabled = false;
			}

			// SEARCH AND SETUP FOR BEHAVIOUR:	ATTACKER
			if ( Utils.Base.SearchComponent( behaviours, ref m_BehaviourAttacker, SearchContext.LOCAL ) )
			{
				m_BehaviourAttacker.Setup( this,	m_ThisEntity	);
				m_BehaviourAttacker.enabled = false;
			}

			// Enable current behaviour ( Normal )
			m_CurrentBehaviour		= m_BehaviourNormal;
			m_CurrentBehaviour.enabled = false;
		}


		//////////////////////////////////////////////////////////////////////////
		private void	OnEnable()
		{
			if ( GameManager.Instance != null )
			{
				GameManager.UpdateEvents.OnThink += OnThink;
			}
		}


		//////////////////////////////////////////////////////////////////////////
		private void	OnDisable()
		{
			if ( GameManager.Instance != null )
			{
				GameManager.UpdateEvents.OnThink -= OnThink;
			}
		}


		//////////////////////////////////////////////////////////////////////////
		private	void	OnThink()
		{
			m_FieldOfView.UpdateFOV();
			m_CurrentBehaviour.OnThink();
		}


		//////////////////////////////////////////////////////////////////////////
		public	void	TryToReachPoint( Vector3 destination )
		{
			m_ThisEntity.RequestMovement( destination );
		}


		//////////////////////////////////////////////////////////////////////////
		public	void	Stop()
		{
			m_ThisEntity.NavStop();
		}


		//////////////////////////////////////////////////////////////////////////
		public	void	ChangeState( BrainState newState )
		{
			if ( newState == m_CurrentBrainState )
				return;

			m_CurrentBehaviour.enabled = false;
			m_CurrentBrainState = newState;
	
			switch( m_CurrentBrainState )
			{
				case BrainState.EVASIVE:
					m_CurrentBehaviour = m_BehaviourEvasive;		break;

				case BrainState.NORMAL:
					m_CurrentBehaviour = m_BehaviourNormal;			break;

				case BrainState.ALARMED:
					m_CurrentBehaviour = m_BehaviourAlarmed;		break;

				case BrainState.SEEKER:
					m_CurrentBehaviour = m_BehaviourSeeker;			break;

				case BrainState.ATTACKER:
					m_CurrentBehaviour = m_BehaviourAttacker;		break;
			}
	
				m_CurrentBehaviour.enabled = true;
		}


		//////////////////////////////////////////////////////////////////////////
		void	IBrain.OnReset()
		{

			ChangeState( BrainState.NORMAL );

			m_FieldOfView.OnReset();
		}

	}

}