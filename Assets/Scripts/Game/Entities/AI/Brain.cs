
using UnityEngine;

[System.Serializable]
public struct TargetInfo_t {
	public	bool	HasTarget;
	public	IEntity	CurrentTarget;
	public	float	TargetSqrDistance;
}

[ System.Serializable ]
public enum BrainState {
//	EVASIVE,
	NORMAL,
	ALARMED,
	SEEKER,
	ATTACKING
}

namespace AI {


	// Brain Interface
	public interface IBrain {
		IFieldOfView				FieldOfView				{	get;	}
		BrainState					State					{	get;	}

		void						TryToReachPoint( Vector3 destination );
		void						Stop();

		void						ChangeState				( BrainState newState );
		void						OnReset					();
	}


	public class Brain : MonoBehaviour, IBrain {

		public	const		float						THINK_TIMER						= 0.2f; // 200 ms

		[SerializeField, ReadOnly]
		private				BrainState					m_CurrentBrainState				= BrainState.NORMAL;

		/*
		[SerializeField]
		private				MonoBehaviour				m_MonoBehaviourEvasive			= null;

		[SerializeField]
		private				MonoBehaviour				m_MonoBehaviourNormal			= null;

		[SerializeField]
		private				MonoBehaviour				m_MonoBehaviourAlarmed			= null;

		[SerializeField]
		private				MonoBehaviour				m_MonoBehaviourSeeker			= null;

		[SerializeField]
		private				MonoBehaviour				m_MonoBehaviourAttacker			= null;
		*/

		// INTERFACE START
	//						TargetInfo_t				IBrain.CurrentTargetInfo		{	get { return m_CurrentTargetInfo;		}	}
							IFieldOfView				IBrain.FieldOfView				{	get { return m_FieldOfView;				}	}
							BrainState					IBrain.State					{	get { return m_CurrentBrainState;		}	}
		// INTERFACE END
		/*
		private				Behaviour_Normal			m_BehaviourNormal				= null;
		private				Behaviour_Aggressive		m_BehaviourAggressiveAlarmed	= null;
		private				Behaviour_Aggressive		m_BehaviourAggressiveSeeker		= null;
		private				Behaviour_Aggressive		m_BehaviourAggressiveAttacker	= null;
		private				Behaviour_Evasive			m_BehaviourEvasive				= null;

		private				Behaviour_Base				m_CurrentBrain					= null;
		*/
		private				IFieldOfView				m_FieldOfView					= null;
		private				IEntity						m_ThisEntity					= null;

	//	private				TargetInfo_t				m_CurrentTargetInfo				= default( TargetInfo_t );
							


		//////////////////////////////////////////////////////////////////////////
		// Awake
		private void	Awake()
		{
	/*		m_BehaviourEvasive				= new Behaviour_Evasive		( this,		null								);
			m_BehaviourNormal				= new Behaviour_Normal		( this,		null								);
			m_BehaviourAggressiveAlarmed	= new Behaviour_Aggressive	( this,		null,		AggresiveMode.ALARMED	);
			m_BehaviourAggressiveSeeker		= new Behaviour_Aggressive	( this,		null,		AggresiveMode.SEEKER	);
			m_BehaviourAggressiveAttacker	= new Behaviour_Aggressive	( this,		null,		AggresiveMode.ATTACKER	);
			m_CurrentBrain = m_BehaviourNormal;
	*/
			m_ThisEntity					= transform.GetComponent<IEntity>();
			m_FieldOfView					= transform.GetComponentInChildren<IFieldOfView>();
			m_FieldOfView.Setup( maxVisibleEntities : 10 );
		}


		//////////////////////////////////////////////////////////////////////////
		// OnEnable
		private void OnEnable()
		{
			GameManager.OnThink += OnThink;
		}


		//////////////////////////////////////////////////////////////////////////
		// OnDisable
		private void OnDisable()
		{
			GameManager.OnThink -= OnThink;
		}


		//////////////////////////////////////////////////////////////////////////
		// OnThink
		private	void	OnThink()
		{
			m_FieldOfView.UpdateFOV();
			m_ThisEntity.OnThink();
		}


		//////////////////////////////////////////////////////////////////////////
		// OnThink
		public	void	TryToReachPoint( Vector3 destination )
		{
			IAINode startNode	= Pathfinding.GraphMaker.Instance.GetNearestNode( transform.position );
			IAINode endNode		= Pathfinding.GraphMaker.Instance.GetNearestNode( destination );

			if ( startNode.ID == endNode.ID )
			{
				return;
			}

			IAINode[] path = null;
			uint nodeCount = Pathfinding.AStarSearch.FindPath( startNode, endNode, ref path );

			if ( nodeCount > 0 )
			{
				m_ThisEntity.NavSetPath( nodeCount, path );
			}
		}


		//////////////////////////////////////////////////////////////////////////
		// Stop
		public void Stop()
		{
			m_ThisEntity.NavStop();
		}


		//////////////////////////////////////////////////////////////////////////
		// ChangeState
		public	void	ChangeState( BrainState newState )
		{
			if ( newState == m_CurrentBrainState )
				return;

	//			m_CurrentBrain.OnDisable();
			m_CurrentBrainState = newState;
	/*			
			switch( m_CurrentBrainState )
			{
				case BrainState.EVASIVE:
					m_CurrentBrain = m_BehaviourEvasive;					break;

				case BrainState.NORMAL:
					m_CurrentBrain = m_BehaviourNormal;						break;

				case BrainState.ALARMED:
					m_CurrentBrain = m_BehaviourAggressiveAlarmed;			break;

				case BrainState.SEEKER:
					m_CurrentBrain = m_BehaviourAggressiveSeeker;			break;

				case BrainState.ATTACKING:
					m_CurrentBrain = m_BehaviourAggressiveAttacker;			break;
			}
	*/
	//			m_CurrentBrain.OnEnable();
		}


		//////////////////////////////////////////////////////////////////////////
		// OnReset
		void	IBrain.OnReset()
		{
			m_CurrentBrainState		= BrainState.NORMAL;

			m_FieldOfView.OnReset();
		}

	}

}