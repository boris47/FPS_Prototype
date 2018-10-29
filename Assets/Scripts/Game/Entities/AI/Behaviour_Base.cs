
using UnityEngine;


[System.Serializable]
public abstract	class	AIBehaviour {

	[SerializeField]
	protected	EntityBlackBoardData	EntityData	= null;


	public	virtual	void				Setup( uint EntityId )
	{
		EntityData = Blackboard.GetData( EntityId );
	}

	public	virtual	void				OnEnable()
	{
		
	}

	public	virtual	void				OnDisable()
	{
		
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

	public	abstract	void			OnTargetUpdate();

	public	abstract	void			OnTargetChange();

	public	abstract	void			OnTargetLost();

	public	abstract	void			OnDestinationReached( Vector3 Destination );

	public	abstract	void			OnKilled();
}
