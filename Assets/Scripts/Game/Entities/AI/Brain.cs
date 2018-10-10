
using UnityEngine;
using UnityEngine.AI;
using AI.Behaviours;

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


// Brain Interface
public interface IBrain {
	IFieldOfView				FieldOfView				{ get; }
	BrainState					State					{ get; }

	void						SetBehaviour			( BrainState State, string behaviourId, bool state = false );

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
	[ SerializeField, ReadOnly ]
	private				Behaviour_Base				m_BehaviourEvasive				= null;
	[ SerializeField, ReadOnly ]
	private				Behaviour_Base				m_BehaviourNormal				= null;
	[ SerializeField, ReadOnly ]
	private				Behaviour_Base				m_BehaviourAlarmed				= null;
	[ SerializeField, ReadOnly ]
	private				Behaviour_Base				m_BehaviourSeeker				= null;
	[ SerializeField, ReadOnly ]
	private				Behaviour_Base				m_BehaviourAttacker				= null;
	[ SerializeField, ReadOnly ]
	private				Behaviour_Base				m_CurrentBehaviour				= null;
		
	private				IFieldOfView				m_FieldOfView					= null;
	private				IEntity						m_ThisEntity					= null;
							


	//////////////////////////////////////////////////////////////////////////
	private void	Awake()
	{
		m_ThisEntity	= transform.GetComponent<IEntity>();
		m_FieldOfView	= transform.GetComponentInChildren<IFieldOfView>();
		m_FieldOfView.Setup( maxVisibleEntities : 10 );
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
	public	void	SetBehaviour( BrainState State, string behaviourId, bool state )
	{
		if ( behaviourId == null || behaviourId.Trim().Length == 0 )
		{
			Debug.Log( "Brain.SetBehaviour Setting invalid behaviour for state " + State + ", " + behaviourId );
			return;
		}

		System.Type type = System.Type.GetType( behaviourId.Trim() );
		if ( type == null )
		{
			Debug.Log( "Brain.SetBehaviour Setting invalid behaviour with id " + behaviourId );
			return;
		}

		Behaviour_Base behaviour = System.Activator.CreateInstance(type) as Behaviour_Base; //gameObject.AddComponent( type ) as Behaviour_Base;
		behaviour.Setup( this, m_ThisEntity );
		if ( state == true )
		{
			behaviour.Enable();
			m_CurrentBehaviour = behaviour;
		}
		else
		{
			behaviour.Disable();
		}

		switch ( State )
		{
			case BrainState.EVASIVE:	m_BehaviourEvasive	= behaviour;
				break;
			case BrainState.NORMAL:		m_BehaviourNormal	= behaviour;
				break;
			case BrainState.SEEKER:		m_BehaviourSeeker	= behaviour;
				break;
			case BrainState.ALARMED:	m_BehaviourAlarmed	= behaviour;
				break;
			case BrainState.ATTACKER:	m_BehaviourAttacker	= behaviour;
				break;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private	void	OnThink()
	{
		m_FieldOfView.UpdateFOV();
//		m_CurrentBehaviour.OnThink();
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

		m_CurrentBehaviour.Disable();
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

		m_CurrentBehaviour.Enable();
	}


	//////////////////////////////////////////////////////////////////////////
	void	IBrain.OnReset()
	{

		ChangeState( BrainState.NORMAL );

		m_FieldOfView.OnReset();
	}

}
