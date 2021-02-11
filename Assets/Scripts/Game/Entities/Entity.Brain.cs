
using System.Collections.Generic;
using UnityEngine;


[ System.Serializable ]
public enum EBrainState {
	EVASIVE		= 0,
	NORMAL		= 1,
	ALARMED		= 2,
	SEEKER		= 3,
	ATTACKER	= 4,
	COUNT		= 5
}


// Brain Interface
public interface IBrain {
	IFieldOfView				FieldOfView				{ get; }
	EBrainState					State					{ get; }

	void						SetBehaviour			( EBrainState brainState, string behaviourId, bool State ); 

	void						ChangeState				( EBrainState newState );
}


public abstract partial class Entity : IBrain {

	public	const		float						THINK_TIMER						= 0.2f; // 200 ms

	private				IBrain						m_BrainInstance					= null;
	public				IBrain						Brain							{ get { return m_BrainInstance; } }

	[Header( "Brain" )]

	[SerializeField]
	protected			EBrainState					m_CurrentBrainState				= EBrainState.COUNT;

	// INTERFACE START
						IFieldOfView				IBrain.FieldOfView				{	get { return m_FieldOfView;				}	}
						EBrainState					IBrain.State					{	get { return m_CurrentBrainState;		}	}
	// INTERFACE END

	[SerializeField]
	protected			AIBehaviour					m_CurrentBehaviour				= new Behaviour_Empty();
	[SerializeField]
	protected			AIBehaviour[]				m_Behaviours					= new AIBehaviour[5] {null,null,null,null,null};

	protected			FieldOfView					m_FieldOfView					= null;
	protected			bool						m_HasFieldOfView				= false;
	protected			bool						m_IsBrainActive					= true;



	//////////////////////////////////////////////////////////////////////////
	protected	void	Brain_Setup()
	{
		m_FieldOfView.Setup( maxVisibleEntities : 10 );

		m_BrainInstance = this;

		EnableMemory();
	}



	//////////////////////////////////////////////////////////////////////////
	protected	void	Destroy_Brain()
	{
		m_BrainInstance = null;

		DisableMemory();
	}



	//////////////////////////////////////////////////////////////////////////
	public	void	SetBehaviour( EBrainState brainState, string behaviourId, bool state ) 
	{
		// Pre-set empty behaviour as default
		m_Behaviours[ (int)brainState ] = new Behaviour_Empty();

		if ( behaviourId == null || behaviourId.Trim().Length == 0 )
		{
			Debug.Log( "Brain.SetBehaviour Setting invalid behaviour for state " + brainState + ", with id" + behaviourId + ", for entity (section) " + m_SectionName );
			return;
		}

		// Check behaviour id validity
		System.Type type = System.Type.GetType( behaviourId.Trim() );
		if ( type == null )
		{
			Debug.Log( "Brain.SetBehaviour Setting invalid behaviour with id " + behaviourId );
			return;
		}

		// Check behaviour type as child of AIBehaviour
		if ( type.IsSubclassOf( typeof( AIBehaviour ) ) == false )
		{
			Debug.Log( "Brain.SetBehaviour Class Requested is not a supported behaviour " + behaviourId );
			return;
		}

		// Setup of the instanced behaviour
		AIBehaviour behaviour = System.Activator.CreateInstance( type ) as AIBehaviour;
		behaviour.Setup(m_ID );
		if ( state == true )
		{
			m_CurrentBehaviour = behaviour;
		}

		// Behaviour assignment
		m_Behaviours[ (int)brainState ] = behaviour;
	}



	//////////////////////////////////////////////////////////////////////////
	public	virtual void	Brain_SetActive( bool State )
	{
		m_IsBrainActive = State;

		if (m_IsBrainActive )
		{
			GameManager.FieldsOfViewManager.RegisterAgent(m_FieldOfView, m_FieldOfView.UpdateFOV);
		}
		else
		{
			GameManager.FieldsOfViewManager?.UnregisterAgent(m_FieldOfView);
		}
	}



	//////////////////////////////////////////////////////////////////////////
	protected	virtual	void	OnThinkBrain()
	{
		if (m_IsBrainActive == false )
			return;

		//	m_FieldOfView.UpdateFOV();

		UpdateMemory();
	}



	//////////////////////////////////////////////////////////////////////////
	public	virtual	void	ChangeState( EBrainState newState )
	{
		if ( newState == m_CurrentBrainState )
			return;

		m_CurrentBehaviour.OnDisable();
		{
			m_CurrentBrainState	= newState;
			AIBehaviour nextBehaviour = m_Behaviours[ (int)newState ];
			UnityEngine.Assertions.Assert.IsNotNull
			(
				nextBehaviour,
				$"next behaviour is not valid"
			);
			m_CurrentBehaviour	= m_Behaviours[ (int)newState ];
		}
		m_CurrentBehaviour.OnEnable();
	}



	//////////////////////////////////////////////////////////////////////////
	protected virtual void	Brain_OnReset()
	{
		ChangeState( EBrainState.NORMAL );
		m_FieldOfView.OnReset();
	}

}
