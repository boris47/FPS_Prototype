
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

	public	abstract	StreamUnit		OnSave( StreamData streamData );

	public	abstract	StreamUnit		OnLoad( StreamData streamData );

	public	abstract	void			OnHit( IBullet bullet );

	public	abstract	void			OnHit( Vector3 startPosition, Entity whoRef, float damage, bool canPenetrate = false );

	public	abstract	void			OnThink();

	public	abstract	void			OnPhysicFrame( float FixedDeltaTime );

	public	abstract	void			OnFrame( float DeltaTime );

	public	abstract	void			OnPauseSet( bool isPaused );

	public	abstract	void			OnTargetAcquired();

	public	abstract	void			OnTargetChange();

	public	abstract	void			OnTargetLost();

	public	abstract	void			OnDestinationReached( Vector3 Destination );

	public	abstract	void			OnKilled();
}
