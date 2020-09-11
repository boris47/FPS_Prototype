
using UnityEngine;

public struct EntityEvents {
	public	delegate	void		HitWithBullet( IBullet bullet );
	public	delegate	void		HitDetailsEvent( Vector3 startPosition, Entity whoRef, EDamageType damageType, float damage, bool canPenetrate = false );
	public	delegate	void		TargetEvent( TargetInfo targetInfo );
	public	delegate	void		NavigationEvent( Vector3 Destination );
	public	delegate	void		KilledEvent( Entity entityKilled );
}

public interface IEntityEvents {


	event				EntityEvents.KilledEvent			OnEvent_Killed;
	///<summary> Directly damage </summary>
	event				EntityEvents.HitDetailsEvent		OnEvent_HittedDetails;
	///<summary> Evaluate bullet damage </summary>
	event				EntityEvents.HitWithBullet			OnEvent_HittedBullet;
	event				EntityEvents.TargetEvent			OnEvent_Target;
	event				EntityEvents.NavigationEvent		OnEvent_Navigation;

	///<summary> Call to enable events on this entity </summary>
	void				EnableEvents						();

	///<summary> Call to disable events on this entity </summary>
	void				DisableEvents						();

	///<summary> Returns if event for this entity is are enabled </summary>
	bool				HasEventsEnabled					{ get; }

	///<summary> Call when destination is reached </summary>
	void				OnDestinationReached				( Vector3 Destination );

	///<summary> Call when head rotation has reached final rotation </summary>
	void				OnLookRotationReached				( Vector3 Direction );
	
	///<summary> Call with hitting bullet reference </summary>
	void				OnHittedBullet						( Bullet hittingBullet );

	///<summary> Call with details about hit </summary>
	void				OnHittedDetails						( Vector3 startPosition, Entity whoRef, EDamageType damageType, float damage, bool canPenetrate = false );
}


public abstract partial class Entity : MonoBehaviour, IEntityEvents {
	
	private				IEntityEvents				m_EventsInterface				= null;
	public				IEntityEvents				EventsInterface					{ get { return this.m_EventsInterface; } }

	protected	event	EntityEvents.KilledEvent			m_OnKilled			= delegate { };
	protected	event	EntityEvents.HitDetailsEvent		m_OnHittedDetails	= delegate { };
	protected	event	EntityEvents.HitWithBullet			m_OnHittedBullet	= delegate { };
	protected	event	EntityEvents.TargetEvent			m_OnTarget			= delegate { };
	protected	event	EntityEvents.NavigationEvent		m_OnNavigation		= delegate { };

	public		event	EntityEvents.KilledEvent			OnEvent_Killed
	{
		add		{ if ( value != null ) m_OnKilled += value; }
		remove	{ if ( value != null ) m_OnKilled -= value; }
	}
	public		event	EntityEvents.HitDetailsEvent		OnEvent_HittedDetails
	{
		add		{ if ( value != null ) m_OnHittedDetails += value; }
		remove	{ if ( value != null ) m_OnHittedDetails -= value; }
	}
	public		event	EntityEvents.HitWithBullet			OnEvent_HittedBullet
	{
		add		{ if ( value != null ) m_OnHittedBullet += value; }
		remove	{ if ( value != null ) m_OnHittedBullet -= value; }
	}
	public		event	EntityEvents.TargetEvent			OnEvent_Target
	{
		add		{ if ( value != null ) m_OnTarget += value; }
		remove	{ if ( value != null ) m_OnTarget -= value; }
	}
	public		event	EntityEvents.NavigationEvent		OnEvent_Navigation
	{
		add		{ if ( value != null ) m_OnNavigation += value; }
		remove	{ if ( value != null ) m_OnNavigation -= value; }
	}

	private		bool			m_HasEventsEnabled			= false;

	//-
	bool IEntityEvents.HasEventsEnabled
	{
		get { return this.m_HasEventsEnabled; }
	}


	//////////////////////////////////////////////////////////////////////////
	// EnableEvents
	public		virtual		void		EnableEvents()
	{
		this.m_HasEventsEnabled = true;
	}


	//////////////////////////////////////////////////////////////////////////
	// DisableEvents
	public		virtual		void		DisableEvents()
	{
		this.m_HasEventsEnabled = false;
	}


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
			GameManager.StreamEvents.OnSave				+= this.OnSave;
			GameManager.StreamEvents.OnLoad				+= this.OnLoad;

			GameManager.UpdateEvents.OnThink			+= this.OnThink;
			GameManager.UpdateEvents.OnPhysicFrame		+= this.OnPhysicFrame;
			GameManager.UpdateEvents.OnFrame			+= this.OnFrame;
			
			// Field Of View Callbacks
			if ( (this.m_EntityType == EEntityType.ACTOR ) == false )
			{
				string targetType = this.m_SectionRef.AsString( "DefaultTarget" );
				EEntityType type = EEntityType.NONE;
				Utils.Converters.StringToEnum( targetType, ref type );
				this.m_BrainInstance.FieldOfView.TargetType = type;

				//				m_FieldOfView.Setup( maxVisibleEntities : 10 );
				this.m_FieldOfView.OnTargetAquired			= this.OnTargetAquired;
				this.m_FieldOfView.OnTargetChanged			= this.OnTargetChanged;
				this.m_FieldOfView.OnTargetLost				= this.OnTargetLost;
			}		
		}
		
	}


	//////////////////////////////////////////////////////////////////////////
	protected	virtual		void		OnDisable()
	{
		this.Brain_SetActive( false );

		// Events un-registration
		if ( GameManager.Instance != null )
		{
			GameManager.StreamEvents.OnSave				-= this.OnSave;
			GameManager.StreamEvents.OnLoad				-= this.OnLoad;

			GameManager.UpdateEvents.OnPhysicFrame		-= this.OnPhysicFrame;
			GameManager.UpdateEvents.OnFrame			-= this.OnFrame;
			GameManager.UpdateEvents.OnThink			-= this.OnThink;

			if ( (this.m_EntityType == EEntityType.ACTOR ) == false )
			{
				this.Destroy_Brain();

				this.m_FieldOfView.OnTargetAquired			= null;
				this.m_FieldOfView.OnTargetChanged			= null;
				this.m_FieldOfView.OnTargetLost				= null;
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	protected	virtual		StreamUnit	OnSave( StreamData streamData )
	{
		if (this.m_IsActive == false )
			return null;

		StreamUnit streamUnit		= streamData.NewUnit(this.gameObject );
		streamUnit.Position			= this.transform.position;
		streamUnit.Rotation			= this.transform.rotation;

		if ( (this.m_EntityType == EEntityType.ACTOR ) == false )
		{
			// save data of every behaviour
			this.m_Behaviours.ForEach( ( AIBehaviour b ) => b.OnSave( streamUnit ) );
		}

		return streamUnit;
	}


	//////////////////////////////////////////////////////////////////////////
	protected	virtual		StreamUnit	OnLoad( StreamData streamData )
	{
		StreamUnit streamUnit = null;
		if ( streamData.GetUnit(this.gameObject, ref streamUnit ) == false )
		{
			this.gameObject.SetActive( false );
			this.m_IsActive = false;
			return null;
		}

		this.gameObject.SetActive( true );
		this.m_IsActive						= true;

		// Entity
		this.m_TargetInfo					= new TargetInfo();
		this.m_HasDestination				= false;

		this.m_NavCanMoveAlongPath			= false;
		this.m_IsAllignedBodyToPoint			= false;

		// NonLiveEntity
		this.m_IsAllignedHeadToPoint			= false;

		this.transform.position = streamUnit.Position;
		this.transform.rotation = streamUnit.Rotation;

		if ( (this.m_EntityType == EEntityType.ACTOR ) == false )
		{
			this.m_Behaviours.ForEach( ( AIBehaviour b ) => b.OnLoad( streamUnit ) );
		}

		return streamUnit;
	}


	//////////////////////////////////////////////////////////////////////////
	public		virtual		void		OnDestinationReached( Vector3 Destination )
	{
		this.m_CurrentBehaviour.OnDestinationReached( Destination );
	}


	//////////////////////////////////////////////////////////////////////////
	public		virtual		void		OnLookRotationReached( Vector3 Direction )
	{
		this.m_CurrentBehaviour.OnLookRotationReached( Direction );
	}


	//////////////////////////////////////////////////////////////////////////
	protected	virtual		void		OnShieldHit( Vector3 startPosition, Entity whoRef, Weapon weaponRef, EDamageType damageType, float damage, bool canPenetrate = false )
	{
		// Notify this entity of the received hit
		this.NotifyHit( startPosition, whoRef, damageType, damage, canPenetrate );
	}


	//////////////////////////////////////////////////////////////////////////
	public	void			OnHittedBullet( Bullet hittingBullet )
	{
		bool bIsBullet = hittingBullet is IBullet;
		if ( bIsBullet )
		{
			IBullet bullet = hittingBullet as IBullet;
			float dmgMultiplier = (this.m_Shield != null && this.m_Shield.Status > 0.0f ) ? 
				( bullet.CanPenetrate ) ? 0.5f : 0.0f
				: 
				1.0f;

			this.OnHittedDetails( bullet.StartPosition, bullet.WhoRef, bullet.DamageType, bullet.Damage * dmgMultiplier, bullet.CanPenetrate );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	protected	virtual		void		NotifyHit( Vector3 startPosition, Entity whoRef, EDamageType damageType, float damage, bool canPenetrate = false )
	{
		this.m_CurrentBehaviour.OnHit( startPosition, whoRef, damage, canPenetrate );

		m_OnHittedDetails( startPosition, whoRef, damageType, damage, canPenetrate );

		if (this.m_Group)
		{
			this.m_Group.GetOthers( this ).ForEach( e => e.NotifyHit( startPosition, null, EDamageType.NONE, 0.0f ) );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public		virtual		void		OnHittedDetails( Vector3 startPosition, Entity whoRef, EDamageType damageType, float damage, bool canPenetrate = false )
	{
		// Notify behaviur
		this.NotifyHit( startPosition, whoRef, damageType, damage, canPenetrate );
		
		this.OnTakeDamage( damage );
//		print( name + ":Taking damage " + damage );
	}


	//////////////////////////////////////////////////////////////////////////
	protected	virtual		void		OnThink()
	{
		this.OnThinkBrain();
		this.m_CurrentBehaviour.OnThink();
	}


	//////////////////////////////////////////////////////////////////////////
	protected	virtual		void		OnPhysicFrame( float FixedDeltaTime )
	{
		this.m_CurrentBehaviour.OnPhysicFrame( FixedDeltaTime );
	}


	//////////////////////////////////////////////////////////////////////////
	protected	virtual		void		OnFrame( float DeltaTime )
	{
		this.UpdateHeadRotation();

		this.m_CurrentBehaviour.OnFrame( DeltaTime );

		if (this.m_NavAgent != null )
			this.m_NavAgent.speed = this.m_BlackBoardData.AgentSpeed;
	}


	//////////////////////////////////////////////////////////////////////////
	protected	virtual		void		OnTargetAquired( TargetInfo targetInfo )
	{
		this.m_CurrentBehaviour.OnTargetAcquired();
//		Memory.Add( targetInfo.CurrentTarget as Entity );

		m_OnTarget( targetInfo );
	}
	

	//////////////////////////////////////////////////////////////////////////
	protected	virtual		void		OnTargetChanged( TargetInfo targetInfo )
	{
		this.m_CurrentBehaviour.OnTargetChange();

		m_OnTarget( targetInfo );
	}

	
	//////////////////////////////////////////////////////////////////////////
	protected	virtual		void		OnTargetLost( TargetInfo targetInfo )
	{
		this.m_CurrentBehaviour.OnTargetLost();

		m_OnTarget( targetInfo );
//		Memory.Remove( targetInfo.CurrentTarget as Entity );
	}


	//////////////////////////////////////////////////////////////////////////
	protected	virtual		void		OnTakeDamage( float Damage )
	{
		// DAMAGE
		{
			this.m_Health -= Damage;
			if (this.m_Health <= 0f )
			{
				this.OnKill();
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	protected	virtual		void		OnKill()
	{
		if (this.m_IsActive == false )
			return;

		this.m_IsActive = false;

		this.m_RigidBody.velocity			= Vector3.zero;
		this.m_RigidBody.angularVelocity		= Vector3.zero;

		EffectsManager.Instance.PlayEffect( EffectsManager.EEffecs.EXPLOSION, this.transform.position, this.transform.up, 0 );
		EffectsManager.Instance.PlayExplosionSound(EffectsManager.EEffecs.EXPLOSION, this.transform.position );

		this.m_CurrentBehaviour.OnKilled();

		m_OnKilled( this );

		Blackboard.UnRegister( this );
	}


	//////////////////////////////////////////////////////////////////////////
	protected	virtual		void		OnDestroy()
	{
		Blackboard.UnRegister( this );

		if (this.m_Group )
		{
			this.m_Group.UnregisterEntity(this);
		}
	}

}