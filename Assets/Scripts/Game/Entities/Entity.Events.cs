
using UnityEngine;

public partial interface IEntity {

	void					OnHit							( IBullet bullet );
	void					OnHit							( Vector3 startPosition, Entity whoRef, float damage, bool canPenetrate = false );
	void					OnKill							();

}

public struct EntityEvents {
	public	delegate	void		HitWithBullet( IBullet bullet );
	public	delegate	void		HitDetailsEvent( Vector3 startPosition, Entity whoRef, float damage, bool canPenetrate = false );
	public	delegate	void		TargetEvent( TargetInfo_t targetInfo );
	public	delegate	void		NavigationEvent( Vector3 Destination );
	public	delegate	void		KilledEvent();
}


public abstract partial class Entity : MonoBehaviour, IEntity {
	
	// Generic Game Events
	protected		GameEvents.StreamingEvent		Behaviour_OnSave					= null;
	protected		GameEvents.StreamingEvent		Behaviour_OnLoad					= null;

	protected		EntityEvents.HitWithBullet		Behaviour_OnHitWithBullet			= null;
	protected		EntityEvents.HitDetailsEvent	Behaviour_OnHitWithDetails			= null;

	protected		GameEvents.OnPhysicFrameEvent	Behaviour_OnPhysicFrame				= null;
	protected		GameEvents.OnThinkEvent			Behaviour_OnThink					= null;
	protected		GameEvents.OnFrameEvent			Behaviour_OnFrame					= null;
	protected		GameEvents.OnPauseSetEvent		Behaviour_OnPauseSet				= null;

	// Internal Entity Events
	protected		EntityEvents.TargetEvent		Behaviour_OnTargetAcquired			= null;
	protected		EntityEvents.TargetEvent		Behaviour_OnTargetUpdate			= null;
	protected		EntityEvents.TargetEvent		Behaviour_OnTargetChange			= null;
	protected		EntityEvents.TargetEvent		Behaviour_OnTargetLost				= null;

	protected		EntityEvents.NavigationEvent	Behaviour_OnDestinationReached		= null;



	public			EntityEvents.KilledEvent		OnKilled							= null;
	
	



	// Questa funzione viene chiamata durante il caricamento dello script o quando si modifica un valore nell'inspector (chiamata solo nell'editor)
	protected	virtual		void		OnValidate()
	{
		// get call 3 times plus 1 on application quit
	}


	//////////////////////////////////////////////////////////////////////////
	protected	virtual		void		OnEnable()
	{
		Brain_SetActive( true );

		// Events registration
		{
			GameManager.StreamEvents.OnSave				+= OnSave;
			GameManager.StreamEvents.OnLoad				+= OnLoad;

			GameManager.UpdateEvents.OnPhysicFrame		+= OnPhysicFrame;
			GameManager.UpdateEvents.OnFrame			+= OnFrame;
			GameManager.UpdateEvents.OnThink			+= OnThink;

			if ( m_IsPlayer == false )
			{
				m_FieldOfView.Setup( maxVisibleEntities : 10 );
				m_FieldOfView.OnTargetAquired			= OnTargetAquired;
				m_FieldOfView.OnTargetUpdate			= OnTargetUpdate;
				m_FieldOfView.OnTargetChanged			= OnTargetChanged;
				m_FieldOfView.OnTargetLost				= OnTargetLost;
			}
		}
		
	}


	//////////////////////////////////////////////////////////////////////////
	protected	virtual		void		OnDisable()
	{
		Brain_SetActive( false );

		// Events un-registration
		if ( GameManager.Instance != null )
		{
			GameManager.StreamEvents.OnSave				-= OnSave;
			GameManager.StreamEvents.OnLoad				-= OnLoad;

			GameManager.UpdateEvents.OnPhysicFrame		-= OnPhysicFrame;
			GameManager.UpdateEvents.OnFrame			-= OnFrame;
			GameManager.UpdateEvents.OnThink			-= OnThink;

			if ( m_IsPlayer == false )
			{
				m_FieldOfView.OnTargetAquired			= null;
				m_FieldOfView.OnTargetUpdate			= null;
				m_FieldOfView.OnTargetChanged			= null;
				m_FieldOfView.OnTargetLost				= null;
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	protected	virtual		StreamUnit	OnSave( StreamData streamData )
	{
		if ( m_IsActive == false )
			return null;

		StreamUnit streamUnit		= streamData.NewUnit( gameObject );
		streamUnit.Position			= transform.position;
		streamUnit.Rotation			= transform.rotation;

		if ( m_IsPlayer == false )
		{
			Behaviour_OnSave( streamData );
		}

		return streamUnit;
	}


	//////////////////////////////////////////////////////////////////////////
	protected	virtual		StreamUnit	OnLoad( StreamData streamData )
	{
		StreamUnit streamUnit = null;
		if ( streamData.GetUnit( gameObject, ref streamUnit ) == false )
		{
			gameObject.SetActive( false );
			m_IsActive = false;
			return null;
		}

		gameObject.SetActive( true );
		m_IsActive						= true;

		// Entity
		m_TargetInfo					= default( TargetInfo_t );
		m_HasDestination				= false;

		m_NavCanMoveAlongPath			= false;
		m_IsAllignedBodyToPoint			= false;

		// NonLiveEntity
		m_IsAllignedHeadToPoint			= false;

		transform.position = streamUnit.Position;
		transform.rotation = streamUnit.Rotation;

		if ( m_IsPlayer == false )
		{
			Behaviour_OnLoad( streamData );
		}

		return streamUnit;
	}
	

	//////////////////////////////////////////////////////////////////////////
	protected	virtual		void		OnDestinationReached( Vector3 Destination )
	{
		Behaviour_OnDestinationReached( Destination );
	}


	//////////////////////////////////////////////////////////////////////////
	public		virtual		void		OnHit( IBullet bullet )
	{
		Behaviour_OnHitWithBullet( bullet );
	}


	//////////////////////////////////////////////////////////////////////////
	public		virtual		void		OnHit( Vector3 startPosition, Entity whoRef, float damage, bool canPenetrate = false )
	{
		Behaviour_OnHitWithDetails( startPosition, whoRef, damage, canPenetrate );
	}


	//////////////////////////////////////////////////////////////////////////
	protected	virtual		void		OnThink()
	{
		m_FieldOfView.UpdateFOV();

		Behaviour_OnThink();
	}


	//////////////////////////////////////////////////////////////////////////
	protected	virtual		void		OnPhysicFrame( float FixedDeltaTime )
	{
		Behaviour_OnPhysicFrame( FixedDeltaTime );
	}


	//////////////////////////////////////////////////////////////////////////
	protected	virtual		void		OnFrame( float DeltaTime )
	{
		Behaviour_OnFrame( DeltaTime );
	}


	//////////////////////////////////////////////////////////////////////////
	protected	virtual		void		OnTargetAquired( TargetInfo_t targetInfo )
	{
		Behaviour_OnTargetAcquired( targetInfo );
	}


	//////////////////////////////////////////////////////////////////////////
	protected	virtual		void		OnTargetUpdate( TargetInfo_t targetInfo )
	{
		Behaviour_OnTargetUpdate( targetInfo );
	}
	

	//////////////////////////////////////////////////////////////////////////
	protected	virtual		void		OnTargetChanged( TargetInfo_t targetInfo )
	{
		Behaviour_OnTargetChange( targetInfo );
	}

	
	//////////////////////////////////////////////////////////////////////////
	protected	virtual		void		OnTargetLost( TargetInfo_t targetInfo )
	{
		Behaviour_OnTargetLost( targetInfo );
	}


	//////////////////////////////////////////////////////////////////////////
	public		virtual		void		OnKill()
	{
		if ( m_IsActive == false )
			return;

		m_RigidBody.velocity			= Vector3.zero;
		m_RigidBody.angularVelocity		= Vector3.zero;

		m_IsActive = false;

		EffectManager.Instance.PlayEntityExplosion( transform.position, transform.up );
		EffectManager.Instance.PlayExplosionSound( transform.position );

		OnKilled();
	}


	//////////////////////////////////////////////////////////////////////////
	protected	virtual		void		OnDestroy()
	{
		
	}

}