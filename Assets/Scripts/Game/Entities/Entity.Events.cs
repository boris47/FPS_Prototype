
using UnityEngine;
using System.Collections.Generic;

public	delegate	void	VoidArgsDelegate();

public partial interface IEntity {

	void					OnTargetAquired					( TargetInfo_t targetInfo );
	void					OnTargetChanged					( TargetInfo_t targetInfo );
	void					OnTargetLost					( TargetInfo_t targetInfo );

	void					OnHit							( IBullet bullet );
	void					OnHit							( Vector3 startPosition, Entity whoRef, float damage, bool canPenetrate = false );
	void					OnKill							();

	void					OnThink							();

}


public abstract partial class Entity : MonoBehaviour, IEntity, IEntitySimulation {

	public	event		VoidArgsDelegate	OnKilled		= null;

	//////////////////////////////////////////////////////////////////////////
	// OnTargetAquired ( Abstract )
	public	abstract	void				OnTargetAquired( TargetInfo_t targetInfo );


	//////////////////////////////////////////////////////////////////////////
	// OnTargetUpdate ( Abstract )
	public abstract		void				OnTargetUpdate( TargetInfo_t targetInfo );
	
	//////////////////////////////////////////////////////////////////////////
	// OnTargetChanged ( Abstract )
	public	abstract	void				OnTargetChanged( TargetInfo_t targetInfo );

	
	//////////////////////////////////////////////////////////////////////////
	// OnTargetLost ( Abstract )
	public	abstract	void				OnTargetLost( TargetInfo_t targetInfo );


	//////////////////////////////////////////////////////////////////////////
	// OnHit ( Abstract )
	public	abstract	void				OnHit( IBullet bullet );

	public	abstract	void				OnHit( Vector3 startPosition, Entity whoRef, float damage, bool canPenetrate = false );


	//////////////////////////////////////////////////////////////////////////
	// OnHurt
//	public	abstract	void				OnHurt( ref IBullet bullet );


	//////////////////////////////////////////////////////////////////////////
	// OnKill ( Virtual )
	public	virtual		void				OnKill()
	{
		if ( m_IsActive == false )
			return;

		m_RigidBody.velocity			= Vector3.zero;
		m_RigidBody.angularVelocity		= Vector3.zero;

		if ( OnKilled != null )
			OnKilled();

		m_IsActive = false;

		EffectManager.Instance.PlayEntityExplosion( transform.position, transform.up );
		EffectManager.Instance.PlayExplosionSound( transform.position );
	}


	//////////////////////////////////////////////////////////////////////////
	// OnThink ( Abstract )
	public	abstract	void				OnThink();

}