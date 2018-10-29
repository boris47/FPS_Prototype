
using System.Collections.Generic;
using UnityEngine;


[ System.Serializable ]
public enum BrainState {
	EVASIVE		,
	NORMAL		= 1,
	ALARMED		= 2,
	SEEKER		= 3,
	ATTACKER	= 4,
	COUNT		= 5
}

// Brain Interface
public interface IBrain {
	IFieldOfView				FieldOfView				{ get; }
	BrainState					State					{ get; }

	void						SetBehaviour( BrainState brainState, string behaviourId, bool State ); 

	void						ChangeState				( BrainState newState );
}


public abstract partial class Entity : IBrain {

	public	const		float						THINK_TIMER						= 0.2f; // 200 ms

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
		Utils.Base.SearchComponent( gameObject, ref m_FieldOfView, SearchContext.CHILDREN );
		m_FieldOfView.Setup( maxVisibleEntities : 10 );

		GameManager.UpdateEvents.OnThink += OnThinkBrain;
	}

	protected	virtual	void	DisableBrain()
	{
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

		if ( this is Drone )
		{
			print( newState );
		}

//		print( "State changing " + m_CurrentBrainState + " to " + newState );
		m_CurrentBehaviour.OnDisable();
		m_CurrentBrainState = newState;

		m_CurrentBehaviour = m_Behaviours[ (int)newState ];

		m_CurrentBehaviour.OnEnable();
		m_BlackBoardData.BrainState = newState;
	}


	//////////////////////////////////////////////////////////////////////////
	protected virtual void	Brain_OnReset()
	{
		ChangeState( BrainState.NORMAL );
		m_FieldOfView.OnReset();
	}

}

[System.Serializable]
public class TargetInfo {
	public	bool	HasTarget;
	public	IEntity	CurrentTarget;
	public	float	TargetSqrDistance;

	public	void	Update( TargetInfo Infos )
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