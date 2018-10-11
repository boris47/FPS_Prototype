
using UnityEngine;

public partial interface IEntity {

	void					OnTargetAquired					( TargetInfo_t targetInfo );
	void					OnTargetChanged					( TargetInfo_t targetInfo );
	void					OnTargetLost					( TargetInfo_t targetInfo );

	void					OnHit							( IBullet bullet );
	void					OnHit							( Vector3 startPosition, Entity whoRef, float damage, bool canPenetrate = false );
	void					OnKill							();

}


public abstract partial class Entity : MonoBehaviour, IEntity, IEntitySimulation {

	public		event		GameEvents.VoidArgsEvent	OnKilled		= null;


	// Questa funzione viene chiamata durante il caricamento dello script o quando si modifica un valore nell'inspector (chiamata solo nell'editor)
	protected	virtual		void		OnValidate()
	{
		// get call 3 times plus 1 on application quit
	}


	//////////////////////////////////////////////////////////////////////////
	protected	virtual		void		OnEnable()
	{
		// Events registration
		{
			GameManager.StreamEvents.OnSave				+= OnSave;
			GameManager.StreamEvents.OnLoad				+= OnLoad;

			GameManager.UpdateEvents.OnPhysicFrame		+= OnPhysicFrame;
			GameManager.UpdateEvents.OnFrame			+= OnFrame;
			GameManager.UpdateEvents.OnThink			+= OnThink;

			// Only register for non player entities ( AI )
			if ( ( this is Player ) == false )
			{
				m_Brain.FieldOfView.OnTargetAquired += OnTargetAquired;
				m_Brain.FieldOfView.OnTargetChanged += OnTargetChanged;
				m_Brain.FieldOfView.OnTargetUpdate	+= OnTargetUpdate;
				m_Brain.FieldOfView.OnTargetLost	+= OnTargetLost;
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	protected	virtual		void		OnDisable()
	{
		// Events un-registration
		if ( GameManager.Instance != null )
		{
			GameManager.StreamEvents.OnSave				-= OnSave;
			GameManager.StreamEvents.OnLoad				-= OnLoad;

			GameManager.UpdateEvents.OnPhysicFrame		-= OnPhysicFrame;
			GameManager.UpdateEvents.OnFrame			-= OnFrame;
			GameManager.UpdateEvents.OnThink			-= OnThink;

			// Only un-register for non player entities ( AI )
			if ( ( this is Player ) == false )
			{
				m_Brain.FieldOfView.OnTargetAquired	-= OnTargetAquired;
				m_Brain.FieldOfView.OnTargetChanged	-= OnTargetChanged;
				m_Brain.FieldOfView.OnTargetUpdate	-= OnTargetUpdate;
				m_Brain.FieldOfView.OnTargetLost	-= OnTargetLost;
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
		return streamUnit;
	}


	//////////////////////////////////////////////////////////////////////////
	public		abstract	void		OnTargetAquired( TargetInfo_t targetInfo );


	//////////////////////////////////////////////////////////////////////////
	public		abstract	void		OnTargetUpdate( TargetInfo_t targetInfo );
	

	//////////////////////////////////////////////////////////////////////////
	public		abstract	void		OnTargetChanged( TargetInfo_t targetInfo );

	
	//////////////////////////////////////////////////////////////////////////
	public		abstract	void		OnTargetLost( TargetInfo_t targetInfo );


	public		abstract	void		OnDestinationReached();


	//////////////////////////////////////////////////////////////////////////
	public		abstract	void		OnHit( IBullet bullet );


	//////////////////////////////////////////////////////////////////////////
	public		abstract	void		OnHit( Vector3 startPosition, Entity whoRef, float damage, bool canPenetrate = false );


	//////////////////////////////////////////////////////////////////////////
//	public		abstract	void		OnHurt( ref IBullet bullet );


	//////////////////////////////////////////////////////////////////////////
	public		abstract	void		OnThink();


	//////////////////////////////////////////////////////////////////////////
	protected	abstract	void		OnPhysicFrame( float fixedDeltaTime );


	//////////////////////////////////////////////////////////////////////////
	protected	abstract	void		OnFrame( float deltaTime );


	//////////////////////////////////////////////////////////////////////////
	public		virtual		void		OnKill()
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
	protected	virtual		void		OnDestroy()
	{
		
	}

}