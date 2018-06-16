
using UnityEngine;

[ System.Serializable ]
public enum BrainState {
//	EVASIVE,
	NORMAL,
	ALARMED,
	SEEKER,
	ATTACKING
}

[System.Serializable]
public struct TargetInfo_t {
	public	bool	HasTarget;
	public	IEntity	CurrentTarget;
	public	float	TargetSqrDistance;
}


// Brain Interface
public interface IBrain {
	IFieldOfView				FieldOfView				{	get;	}
	BrainState					State					{	get;	}

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


	private				float						m_ThinkTimer					= 0f;
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

	/*
	//////////////////////////////////////////////////////////////////////////
	// Update
	private void	Update()
	{
		m_ThinkTimer += Time.deltaTime;
		if ( m_ThinkTimer > THINK_TIMER )
		{
			m_FieldOfView.UpdateFOV();
			m_ThisEntity.OnThink();
			m_ThinkTimer = 0f;
		}
	}
	*/

	private	void	OnThink()
	{
		m_FieldOfView.UpdateFOV();
		m_ThisEntity.OnThink();
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
		m_ThinkTimer			= 0f;

		m_FieldOfView.OnReset();
	}

}