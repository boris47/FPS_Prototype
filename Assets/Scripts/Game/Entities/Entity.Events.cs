
using UnityEngine;

public partial interface IEntity {

	void					OnHit							( IBullet bullet );
	void					OnHit							( Vector3 startPosition, Entity whoRef, float damage, bool canPenetrate = false );
}


public abstract partial class Entity : MonoBehaviour, IEntity {
	
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

			if ( ( m_EntityType == ENTITY_TYPE.ACTOR ) == false )
			{
				EnableBrain();

				string TargetType = m_SectionRef.AsString( "DefaultTarget" );
				Brain.FieldOfView.TargetType = (ENTITY_TYPE) System.Enum.Parse( typeof( ENTITY_TYPE ), TargetType );

				m_FieldOfView.Setup( maxVisibleEntities : 10 );
				m_FieldOfView.OnTargetAquired			= OnTargetAquired;
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

			if ( ( m_EntityType == ENTITY_TYPE.ACTOR ) == false )
			{
				DisableBrain();

				m_FieldOfView.OnTargetAquired			= null;
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

		if ( ( m_EntityType == ENTITY_TYPE.ACTOR ) == false )
		{
			m_CurrentBehaviour.OnSave( streamData );
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
		m_TargetInfo					= new TargetInfo();
		m_HasDestination				= false;

		m_NavCanMoveAlongPath			= false;
		m_IsAllignedBodyToPoint			= false;

		// NonLiveEntity
		m_IsAllignedHeadToPoint			= false;

		transform.position = streamUnit.Position;
		transform.rotation = streamUnit.Rotation;

		if ( ( m_EntityType == ENTITY_TYPE.ACTOR ) == false )
		{
			m_CurrentBehaviour.OnLoad( streamData );
		}

		return streamUnit;
	}
	

	//////////////////////////////////////////////////////////////////////////
	public		virtual		void		OnDestinationReached( Vector3 Destination )
	{
		m_CurrentBehaviour.OnDestinationReached( Destination );
	}


	//////////////////////////////////////////////////////////////////////////
	public		virtual		void		OnHit( IBullet bullet )
	{
//		m_CurrentBehaviour.OnHit( bullet );

		float damage = UnityEngine.Random.Range( bullet.DamageMin, bullet.DamageMax );
		this.OnHit( bullet.StartPosition, bullet.WhoRef, damage, bullet.CanPenetrate ); 
	}


	//////////////////////////////////////////////////////////////////////////
	public		virtual		void		OnHit( Vector3 startPosition, Entity whoRef, float damage, bool canPenetrate = false )
	{
		m_CurrentBehaviour.OnHit( startPosition, whoRef, damage, canPenetrate );
		
		this.OnTakeDamage( damage );
	}


	//////////////////////////////////////////////////////////////////////////
	protected	virtual		void		OnThink()
	{
		OnThinkBrain();
		m_CurrentBehaviour.OnThink();
	}


	//////////////////////////////////////////////////////////////////////////
	protected	virtual		void		OnPhysicFrame( float FixedDeltaTime )
	{
		m_CurrentBehaviour.OnPhysicFrame( FixedDeltaTime );
	}


	//////////////////////////////////////////////////////////////////////////
	protected	virtual		void		OnFrame( float DeltaTime )
	{
		UpdateHeadRotation();

		m_CurrentBehaviour.OnFrame( DeltaTime );

		if ( m_NavAgent != null )
			m_NavAgent.speed = m_BlackBoardData.AgentSpeed;
	}


	//////////////////////////////////////////////////////////////////////////
	protected	virtual		void		OnTargetAquired( TargetInfo targetInfo )
	{
		m_CurrentBehaviour.OnTargetAcquired();
	}
	

	//////////////////////////////////////////////////////////////////////////
	protected	virtual		void		OnTargetChanged( TargetInfo targetInfo )
	{
		m_CurrentBehaviour.OnTargetChange();
	}

	
	//////////////////////////////////////////////////////////////////////////
	protected	virtual		void		OnTargetLost( TargetInfo targetInfo )
	{
		m_CurrentBehaviour.OnTargetLost();
	}


	//////////////////////////////////////////////////////////////////////////
	protected	virtual		void		OnTakeDamage( float Damage )
	{
		// DAMAGE
		{
			m_Health -= Damage;
			if ( m_Health <= 0f )
			{
				OnKill();
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	protected	virtual		void		OnKill()
	{
		if ( m_IsActive == false )
			return;

		m_IsActive = false;

		m_RigidBody.velocity			= Vector3.zero;
		m_RigidBody.angularVelocity		= Vector3.zero;

		EffectManager.Instance.PlayEffect( EffectType.EXPLOSION, transform.position, transform.up, 0 );
		EffectManager.Instance.PlayExplosionSound( transform.position );

		m_CurrentBehaviour.OnKilled();

		if ( OnKilled != null )
			OnKilled();

		Blackboard.UnRegister( this );
	}


	//////////////////////////////////////////////////////////////////////////
	protected	virtual		void		OnDestroy()
	{
		Blackboard.UnRegister( this );
	}

}