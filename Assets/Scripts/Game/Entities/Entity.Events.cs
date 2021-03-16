
using UnityEngine;

public struct EntityEvents
{
	public	delegate	void		KilledEvent      (Entity entityKilled);
//	public	delegate	void		HitDetailsEvent  (Vector3 startPosition, Entity whoRef, EDamageType damageType, float damage, bool canPenetrate = false);
//	public	delegate	void		HitWithBullet    (IBullet bullet);
//	public	delegate	void		NavigationEvent  (Vector3 Destination);
}


public abstract partial class Entity : MonoBehaviour
{	
	protected	event	EntityEvents.KilledEvent			m_OnKilled			= delegate { };
//	protected	event	EntityEvents.HitDetailsEvent		m_OnHittedDetails	= delegate { };
//	protected	event	EntityEvents.HitWithBullet			m_OnHittedBullet	= delegate { };
//	protected	event	EntityEvents.NavigationEvent		m_OnNavigation		= delegate { };

	public		event	EntityEvents.KilledEvent			OnEvent_Killed
	{
		add		{ if (value.IsNotNull()) m_OnKilled += value; }
		remove	{ if (value.IsNotNull()) m_OnKilled -= value; }
	}
/*	public		event	EntityEvents.HitDetailsEvent		OnEvent_HittedDetails
	{
		add		{ if ( value != null ) m_OnHittedDetails += value; }
		remove	{ if ( value != null ) m_OnHittedDetails -= value; }
	}
	public		event	EntityEvents.HitWithBullet			OnEvent_HittedBullet
	{
		add		{ if ( value != null ) m_OnHittedBullet += value; }
		remove	{ if ( value != null ) m_OnHittedBullet -= value; }
	}
	public		event	EntityEvents.NavigationEvent		OnEvent_Navigation
	{
		add		{ if ( value != null ) m_OnNavigation += value; }
		remove	{ if ( value != null ) m_OnNavigation -= value; }
	}
*/
	private		bool			m_HasEventsEnabled			= false;

	//-
	public		bool			HasEventsEnabled			=> m_HasEventsEnabled;


	//////////////////////////////////////////////////////////////////////////
	///<summary> Call to enable events on this entity </summary>
	public		virtual		void		EnableEvents()
	{
		m_HasEventsEnabled = true;
	}


	//////////////////////////////////////////////////////////////////////////
	///<summary> Call to disable events on this entity </summary>
	public		virtual		void		DisableEvents()
	{
		m_HasEventsEnabled = false;
	}


	//////////////////////////////////////////////////////////////////////////
	protected	virtual		void		OnEnable()
	{
		if (CustomAssertions.IsNotNull(GameManager.UpdateEvents))
		{
			GameManager.UpdateEvents.OnPhysicFrame		+= OnPhysicFrame;
			GameManager.UpdateEvents.OnThink			+= OnThink;
			GameManager.UpdateEvents.OnFrame			+= OnFrame;
			GameManager.UpdateEvents.OnLateFrame		+= OnLateFrame;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	protected	virtual		void		OnDisable()
	{
		if (GameManager.UpdateEvents.IsNotNull())
		{
			GameManager.UpdateEvents.OnPhysicFrame		-= OnPhysicFrame;
			GameManager.UpdateEvents.OnThink			-= OnThink;
			GameManager.UpdateEvents.OnFrame			-= OnFrame;
			GameManager.UpdateEvents.OnLateFrame		-= OnLateFrame;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnPhysicFrame(float FixedDeltaTime)
	{

	}


	//////////////////////////////////////////////////////////////////////////
	private void OnThink()
	{

	}


	//////////////////////////////////////////////////////////////////////////
	private void OnFrame(float DeltaTime)
	{

	}


	//////////////////////////////////////////////////////////////////////////
	private void OnLateFrame(float DeltaTime)
	{

	}


	//////////////////////////////////////////////////////////////////////////
	protected	virtual		bool	OnSave( StreamData streamData, ref StreamUnit streamUnit )
	{
		streamUnit = streamData.NewUnit(gameObject);

		streamUnit.Position = transform.position;
		streamUnit.Rotation = transform.rotation;

		// Health
		streamUnit.SetInternal("Health", m_Health);

		// Shield
		if (m_Shield.IsNotNull())
		{
			streamUnit.SetInternal("ShieldStatus", m_Shield.Status);
		}

		foreach (EntityComponent component in GetComponents<EntityComponent>())
		{
			component.OnSave(streamUnit);
		}
		return true;
	}


	//////////////////////////////////////////////////////////////////////////
	protected	virtual		bool	OnLoad(StreamData streamData, ref StreamUnit streamUnit)
	{
		bool bResult = streamData.TryGetUnit(gameObject, out streamUnit);
		if (bResult)
		{
			transform.position = streamUnit.Position;
			transform.rotation = streamUnit.Rotation;

			// Health
			m_Health = streamUnit.GetAsFloat("Health");

			// Shield
			if (m_Shield.IsNotNull())
			{
				// TODO
			//	m_Shield.load
			//	streamUnit.SetInternal("ShieldStatus", m_Shield.Status);
			}

			foreach (EntityComponent component in GetComponents<EntityComponent>())
			{
				component.OnLoad(streamUnit);
			}
		}
		else
		{
			gameObject.SetActive(false);
		}
		return bResult;
	}


	//////////////////////////////////////////////////////////////////////////
	public		virtual		void		OnLookRotationReached( Vector3 Direction )
	{
//		m_CurrentBehaviour.OnLookRotationReached( Direction ); //TODO
	}


	//////////////////////////////////////////////////////////////////////////
	protected	virtual		void		OnShieldHit( Vector3 startPosition, Entity whoRef, Weapon weaponRef, EDamageType damageType, float damage, bool canPenetrate = false )
	{
		// Notify this entity of the received hit
		NotifyHit(startPosition, whoRef, damageType, damage, canPenetrate);
	}


	//////////////////////////////////////////////////////////////////////////
	public	void			OnHittedBullet( Bullet hittingBullet )
	{
		if ( hittingBullet is IBullet bullet )
		{
			float dmgMultiplier = (m_HasShield && m_Shield.Status > 0.0f ) ? 
				( bullet.CanPenetrate ) ? 0.5f : 0.0f
				: 
				1.0f;

			OnHittedDetails(bullet.StartPosition, bullet.WhoRef, bullet.DamageType, bullet.Damage * dmgMultiplier, bullet.CanPenetrate);
		}
	}


	//////////////////////////////////////////////////////////////////////////
	protected	virtual		void		NotifyHit( Vector3 startPosition, Entity whoRef, EDamageType damageType, float damage, bool canPenetrate = false )
	{
		m_Behaviours.CurrentBehaviour.OnHit(startPosition, whoRef, damage, canPenetrate);

	//	if (m_Group)
	//	{
	//		m_Group.GetOthers( this ).ForEach( e => e.NotifyHit( startPosition, null, EDamageType.NONE, 0.0f ) );
	//	}
	}


	//////////////////////////////////////////////////////////////////////////
	public		virtual		void		OnHittedDetails( Vector3 startPosition, Entity whoRef, EDamageType damageType, float damage, bool canPenetrate = false )
	{
		// Notify behaviur
		NotifyHit( startPosition, whoRef, damageType, damage, canPenetrate );
		
		OnTakeDamage( damage );
//		print( name + ":Taking damage " + damage );
	}


	//////////////////////////////////////////////////////////////////////////
	protected	virtual		void		OnTakeDamage( float Damage )
	{
		// DAMAGE
		{
			m_Health -= Damage;
			if (m_Health <= 0f)
			{
				OnKill();
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	protected	virtual		void		OnKill()
	{
		m_RigidBody.velocity			= Vector3.zero;
		m_RigidBody.angularVelocity		= Vector3.zero;

		m_OnKilled(this);
	}
}