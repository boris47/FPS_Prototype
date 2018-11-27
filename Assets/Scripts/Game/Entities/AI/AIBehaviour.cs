
using UnityEngine;

public abstract	class AIBehaviour {

	protected	EntityBlackBoardData	EntityData	= null;


	public	virtual	void				Setup( uint EntityId )
	{
		EntityData = Blackboard.GetData( EntityId );
	}

	protected			void			print( string msg )
	{
		UnityEngine.Debug.Log( msg );
	}

	public	virtual	void			OnEnable()
	{ }

	public	virtual	void			OnDisable()
	{ }

	public virtual void OnSave( StreamUnit streamUnit )
	{ }

	public virtual void OnLoad( StreamUnit streamUnit )
	{ }

	public	virtual	void			OnHit( IBullet bullet )
	{ }

	public	virtual	void			OnHit( Vector3 startPosition, Entity whoRef, float damage, bool canPenetrate = false )
	{ }

	public	virtual	void			OnThink()
	{ }

	public	virtual	void			OnPhysicFrame( float FixedDeltaTime )
	{ }

	public	virtual	void			OnFrame( float DeltaTime )
	{ }

	public	virtual	void			OnPauseSet( bool isPaused )
	{ }

	public	virtual	void			OnTargetAcquired()
	{ }

	public	virtual	void			OnTargetChange()
	{ }

	public	virtual	void			OnTargetLost()
	{ }

	public	virtual	void			OnDestinationReached( Vector3 Destination )
	{ }

	public	virtual	void			OnLookRotationReached( Vector3 Direction )
	{ }

	public	virtual	void			OnKilled()
	{ }

}


public class Behaviour_Empty : AIBehaviour { }