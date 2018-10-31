
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

	protected			IFieldOfView				m_FieldOfView					= null;
	protected			IEntity						m_ThisEntity					= null;

	protected			bool						m_IsBrainActive					= true;
	[SerializeField]
	protected			AIBehaviour					m_CurrentBehaviour				= null;
	[SerializeField]
	protected			AIBehaviour[]				m_Behaviours					= null;


	//////////////////////////////////////////////////////////////////////////
	protected	virtual	void	EnableBrain()
	{
		m_FieldOfView.Setup( maxVisibleEntities : 10 );

		GameManager.UpdateEvents.OnThink += OnThinkBrain;

		m_BrainIstance = this as IBrain;
	}


	//////////////////////////////////////////////////////////////////////////
	protected	virtual	void	DisableBrain()
	{
		m_BrainIstance = null;

		GameManager.UpdateEvents.OnThink -= OnThinkBrain;
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

//		print( "State changing " + m_CurrentBrainState + " to " + newState );
		m_CurrentBrainState = newState;

		m_CurrentBehaviour = m_Behaviours[ (int)newState ];

		m_BlackBoardData.BrainState = newState;
	}


	//////////////////////////////////////////////////////////////////////////
	protected virtual void	Brain_OnReset()
	{
		ChangeState( BrainState.NORMAL );
		m_FieldOfView.OnReset();
	}

}