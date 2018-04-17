
using UnityEngine;

public	delegate	void	VoidArgsDelegate();


public abstract partial class Entity {

	public	event		VoidArgsDelegate	OnKilled		= null;


	//////////////////////////////////////////////////////////////////////////
	// OnTargetAquired ( Abstract )
	public	abstract	void				OnTargetAquired( TargetInfo_t targetInfo );
	
	
	//////////////////////////////////////////////////////////////////////////
	// OnTargetChanged ( Abstract )
	public	abstract	void				OnTargetChanged( TargetInfo_t targetInfo );

	
	//////////////////////////////////////////////////////////////////////////
	// OnTargetLost ( Abstract )
	public	abstract	void				OnTargetLost( TargetInfo_t targetInfo );


	//////////////////////////////////////////////////////////////////////////
	// OnHit ( Abstract )
	public	abstract	void				OnHit( ref IBullet bullet );


	//////////////////////////////////////////////////////////////////////////
	// OnHurt
//	public	abstract	void				OnHurt( ref IBullet bullet );


	//////////////////////////////////////////////////////////////////////////
	// OnKill ( Virtual )
	public	virtual		void				OnKill()
	{
		m_RigidBody.velocity			= Vector3.zero;
		m_RigidBody.angularVelocity		= Vector3.zero;

		if ( OnKilled != null )
			OnKilled();

		m_IsActive = false;

		EffectManager.Instance.PlayEntityExplosion( transform.position, transform.up );
		EffectManager.Instance.PlayerExplosionSound( transform.position );
	}


	//////////////////////////////////////////////////////////////////////////
	// OnThink ( Abstract )
	public	abstract	void				OnThink();

}