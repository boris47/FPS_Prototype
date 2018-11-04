
using System.Collections.Generic;
using UnityEngine;


// Brain Interface
public interface IBrain {
	IFieldOfView				FieldOfView				{ get; }
	BrainState					State					{ get; }

	void						SetBehaviour( BrainState brainState, string behaviourId, bool State ); 

	void						ChangeState				( BrainState newState );
}


public abstract partial class Entity : IBrain {

	public	const		float						THINK_TIMER						= 0.2f; // 200 ms

	private				IBrain						m_BrainIstance					= null;
	public				IBrain						Brain							{ get { return m_BrainIstance; } }

	[Header( "Brain" )]

	[SerializeField]
	protected			BrainState					m_CurrentBrainState				= BrainState.COUNT;

	// INTERFACE START
						IFieldOfView				IBrain.FieldOfView				{	get { return m_FieldOfView;				}	}
						BrainState					IBrain.State					{	get { return m_CurrentBrainState;		}	}
	// INTERFACE END

	[SerializeField]
	protected			AIBehaviour					m_CurrentBehaviour				= null;
	[SerializeField]
	protected			List<AIBehaviour>			m_Behaviours					= new List<AIBehaviour>( new AIBehaviour[5] );

	protected			FieldOfView					m_FieldOfView					= null;
	protected			bool						m_IsBrainActive					= true;


	//////////////////////////////////////////////////////////////////////////
	protected	virtual	void	EnableBrain()
	{
		m_FieldOfView.Setup( maxVisibleEntities : 10 );

		m_BrainIstance = this as IBrain;
	}


	//////////////////////////////////////////////////////////////////////////
	protected	virtual	void	DisableBrain()
	{
		m_BrainIstance = null;
	}


	//////////////////////////////////////////////////////////////////////////
	public	void	SetBehaviour( BrainState brainState, string behaviourId, bool state ) 
	{
		if ( behaviourId == null || behaviourId.Trim().Length == 0 )
		{
			Debug.Log( "Brain.SetBehaviour Setting invalid behaviour for state " + state + ", " + behaviourId );
			return;
		}

		System.Type type = System.Type.GetType( behaviourId.Trim() );
		if ( type == null )
		{
			Debug.Log( "Brain.SetBehaviour Setting invalid behaviour with id " + behaviourId );
			return;
		}

		AIBehaviour behaviour = System.Activator.CreateInstance( type ) as AIBehaviour;
		behaviour.Setup( m_ID );
		if ( state == true )
		{
			m_CurrentBehaviour = behaviour as AIBehaviour;
		}

		m_Behaviours[ (int)brainState ] = behaviour;
	}


	//////////////////////////////////////////////////////////////////////////
	public	virtual void	Brain_SetActive( bool State )
	{
		m_IsBrainActive = State;
	}


	//////////////////////////////////////////////////////////////////////////
	protected	virtual	void	OnThinkBrain()
	{
		if ( m_IsBrainActive == false )
			return;

		m_FieldOfView.UpdateFOV();
	}


	//////////////////////////////////////////////////////////////////////////
	public	virtual	void	ChangeState( BrainState newState )
	{
		if ( newState == m_CurrentBrainState )
			return;

		m_CurrentBehaviour.OnDisable();
		{
			m_CurrentBrainState	= newState;
			m_CurrentBehaviour	= m_Behaviours[ (int)newState ];
		}
		m_CurrentBehaviour.OnEnable();
	}


	//////////////////////////////////////////////////////////////////////////
	protected virtual void	Brain_OnReset( BrainState brainState = BrainState.NORMAL )
	{
		ChangeState( brainState );
		m_FieldOfView.OnReset();
	}

}