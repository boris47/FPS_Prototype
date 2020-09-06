
using System.Collections.Generic;
using UnityEngine;


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
	public				IBrain						Brain							{ get { return this.m_BrainInstance; } }

	[Header( "Brain" )]

	[SerializeField]
	protected			EBrainState					m_CurrentBrainState				= EBrainState.COUNT;

	// INTERFACE START
						IFieldOfView				IBrain.FieldOfView				{	get { return this.m_FieldOfView;				}	}
						EBrainState					IBrain.State					{	get { return this.m_CurrentBrainState;		}	}
	// INTERFACE END

	[SerializeField]
	protected			AIBehaviour					m_CurrentBehaviour				= new Behaviour_Empty();
	[SerializeField]
	protected			List<AIBehaviour>			m_Behaviours					= new List<AIBehaviour>( new AIBehaviour[5] );

	protected			FieldOfView					m_FieldOfView					= null;
	protected			bool						m_HasFieldOfView				= false;
	protected			bool						m_IsBrainActive					= true;



	//////////////////////////////////////////////////////////////////////////
	protected	void	Brain_Setup()
	{
		this.m_FieldOfView.Setup( maxVisibleEntities : 10 );

		this.m_BrainInstance = this;

		this.EnableMemory();
	}



	//////////////////////////////////////////////////////////////////////////
	protected	void	Destroy_Brain()
	{
		this.m_BrainInstance = null;

		this.DisableMemory();
	}



	//////////////////////////////////////////////////////////////////////////
	public	void	SetBehaviour( EBrainState brainState, string behaviourId, bool state ) 
	{
		// Pre-set empty behaviour as default
		this.m_Behaviours[ (int)brainState ] = new Behaviour_Empty();

		if ( behaviourId == null || behaviourId.Trim().Length == 0 )
		{
			Debug.Log( "Brain.SetBehaviour Setting invalid behaviour for state " + brainState + ", with id" + behaviourId + ", for entity (section) " + this.m_SectionName );
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
		behaviour.Setup(this.m_ID );
		if ( state == true )
		{
			this.m_CurrentBehaviour = behaviour;
		}

		// Behaviour assignment
		this.m_Behaviours[ (int)brainState ] = behaviour;
	}



	//////////////////////////////////////////////////////////////////////////
	public	virtual void	Brain_SetActive( bool State )
	{
		this.m_IsBrainActive = State;

		if (this.m_IsBrainActive )
		{
			GameManager.FieldsOfViewManager.RegisterAgent(this.m_FieldOfView, this.m_FieldOfView.UpdateFOV );
		}
		else
		{
			GameManager.FieldsOfViewManager?.UnregisterAgent(this.m_FieldOfView );
		}
	}



	//////////////////////////////////////////////////////////////////////////
	protected	virtual	void	OnThinkBrain()
	{
		if (this.m_IsBrainActive == false )
			return;

		//	m_FieldOfView.UpdateFOV();

		this.UpdateMemory();
	}



	//////////////////////////////////////////////////////////////////////////
	public	virtual	void	ChangeState( EBrainState newState )
	{
		if ( newState == this.m_CurrentBrainState )
			return;

		this.m_CurrentBehaviour.OnDisable();
		{
			this.m_CurrentBrainState	= newState;
			this.m_CurrentBehaviour	= this.m_Behaviours[ (int)newState ];
		}
		this.m_CurrentBehaviour.OnEnable();
	}



	//////////////////////////////////////////////////////////////////////////
	protected virtual void	Brain_OnReset()
	{
		this.ChangeState( EBrainState.NORMAL );
		this.m_FieldOfView.OnReset();
	}

}



[ System.Serializable ]
public enum EBrainState {
	EVASIVE		= 0,
	NORMAL		= 1,
	ALARMED		= 2,
	SEEKER		= 3,
	ATTACKER	= 4,
	COUNT		= 5
}
