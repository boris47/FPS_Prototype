
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract	class	AIBehaviour_Static {

	protected			EntityBlackBoardData	m_EntityData	= null;

	// // // // // // // // // // // // // // // // // // // // // // // //

	public	void		Setup( uint EntityID )
	{
		if ( m_EntityData != null )
			return;

		m_EntityData = Blackboard.GetData( EntityID );
	}


	// // // // // // // // // // // // // // // // // // // // // // // //

	
	public	abstract	void			OnEnable();

	public	abstract	void			OnDisable();

	public	abstract	StreamUnit		OnSave( StreamData streamData );

	public	abstract	StreamUnit		OnLoad( StreamData streamData );

	public	abstract	void			OnHit( IBullet bullet );

	public	abstract	void			OnHit( Vector3 startPosition, Entity whoRef, float damage, bool canPenetrate = false );

	public	abstract	void			OnThink();

	public	abstract	void			OnPhysicFrame( float FixedDeltaTime );

	public	abstract	void			OnFrame( float DeltaTime );

	public	abstract	void			OnPauseSet( bool isPaused );

	public	abstract	void			OnTargetAcquired( TargetInfo_t targetInfo );

	public	abstract	void			OnTargetUpdate( TargetInfo_t targetInfo );

	public	abstract	void			OnTargetChange( TargetInfo_t targetInfo );

	public	abstract	void			OnTargetLost( TargetInfo_t targetInfo );

	public	abstract	void			OnDestinationReached( Vector3 Destination );

	public	abstract	void			OnKilled();
}


