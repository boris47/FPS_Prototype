using UnityEngine;
using System.Collections;
using System;

public interface IShield : IStreamableByEvents {

	/// <summary> Event called when shiled is hitted </summary>
	event		Shield.ShieldHitEvent		OnHit;

	void		OnTriggerHit				( GameObject collidingObject );

	float		Status						{ get; }
	bool		IsUnbreakable				{ get; }


	void		Setup						( float StartStatus, bool IsUnbreakable = false );
	void		OnReset						();
}

[RequireComponent( typeof ( Collider ) )]
public class Shield : MonoBehaviour, IShield {

	// Delegate definition
	public	delegate	void	ShieldHitEvent( Vector3 startPosition, Entity whoRef, Weapon weaponRef, float damage, bool canPenetrate = false );

	// Internal Event
	protected	ShieldHitEvent	m_ShielHitEvent			= null;

	[SerializeField]
	protected		bool		m_IsUnbreakable			= false;

	/// INTERFACE START
	/// <summary> Event called when shiled is hitted </summary>
	event ShieldHitEvent	IShield.OnHit
	{
		add		{ if ( value != null )	m_ShielHitEvent += value; }
		remove	{ if ( value != null )	m_ShielHitEvent -= value; }
	}

	bool		IShield.IsUnbreakable				{	get { return m_IsUnbreakable;	}	}
	float		IShield.Status						{	get { return m_CurrentStatus;	}	}

	/// INTERFACE END
	/// 
	
	private		Collider	m_Collider			= null;
	private		Renderer	m_Renderer			= null;
	private		float		m_CurrentStatus		= 100f;
	private		float		m_StartStatus		= 0.0f;


	//////////////////////////////////////////////////////////////////////////
	// Awake
	private void Awake()
	{
		Utils.Base.SearchComponent( gameObject, ref m_Renderer, SearchContext.LOCAL );
		Utils.Base.SearchComponent( gameObject, ref m_Collider, SearchContext.LOCAL );

		// First assignment
		ResetDelegate();
	}


	//////////////////////////////////////////////////////////////////////////
	// ResetDelegate
	private	void	ResetDelegate()
	{
		Shield.ShieldHitEvent onShiledHit = delegate( Vector3 startPosition, Entity whoRef, Weapon weaponRef, float damage, bool canPenetrate )
		{
			TakeDamage( startPosition, whoRef, weaponRef, damage, canPenetrate );
		};
		m_ShielHitEvent = onShiledHit;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnTriggerHit
	public		void		OnTriggerHit				( GameObject collidingObject )
	{
		IBullet bullet = null;
		bool bIsBullet = Utils.Base.SearchComponent( collidingObject, ref bullet, SearchContext.CHILDREN );
		if ( bIsBullet == true )
		{
			m_ShielHitEvent( bullet.StartPosition, bullet.WhoRef, bullet.Weapon, bullet.DamageRandom, bullet.CanPenetrate );
		}
	}

/*	//////////////////////////////////////////////////////////////////////////
	// OnTriggerEnter
	private		void		OnTriggerEnter( Collider other )
	{
		OnTriggerHit( other.gameObject );
	}
*/
	private void OnCollisionEnter( Collision collision )
	{
		print("Shield collision");
		IBullet bullet = null;
		bool bIsBullet = Utils.Base.SearchComponent( collision.gameObject, ref bullet, SearchContext.CHILDREN );
		if ( bIsBullet == true )
		{
			m_ShielHitEvent( bullet.StartPosition, bullet.WhoRef, bullet.Weapon, bullet.DamageRandom, bullet.CanPenetrate );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// OnEnable
	private void OnEnable()
	{
		m_Renderer.enabled = true;
		m_Collider.enabled = true;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnDisable
	private void OnDisable()
	{
		m_Renderer.enabled = false;
		m_Collider.enabled = false;
	}

	//////////////////////////////////////////////////////////////////////////
	// OnSave
	StreamUnit IStreamableByEvents.OnSave( StreamData streamData )
	{
		StreamUnit streamUnit = null;
		if ( streamData.GetUnit( gameObject, ref streamUnit ) == false )
		{
			enabled = false;
			ResetDelegate();
//			gameObject.SetActive( false );
			return null;
		}

		streamUnit.SetInternal( "CurrentStatus", m_CurrentStatus );

		return streamUnit;
	}

	//////////////////////////////////////////////////////////////////////////
	// OnLoad
	StreamUnit IStreamableByEvents.OnLoad( StreamData streamData )
	{
		StreamUnit streamUnit = null;
		if ( streamData.GetUnit( gameObject, ref streamUnit ) == false )
		{
			enabled = false;
			ResetDelegate();
//			gameObject.SetActive( false );
			return null;
		}

		m_CurrentStatus = streamUnit.GetAsFloat( "CurrentStatus" );

		return streamUnit;
	}


	//////////////////////////////////////////////////////////////////////////
	// Setup
	void		IShield.Setup( float StartStatus, bool IsUnbreakable )
	{
		m_StartStatus	= StartStatus;
		m_IsUnbreakable	= IsUnbreakable;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnReset
	void		IShield.OnReset()
	{
		m_Renderer.enabled = true;
		m_Collider.enabled = true;
		m_CurrentStatus = m_StartStatus;
	}


	//////////////////////////////////////////////////////////////////////////
	// TakeDamage
	protected		void	TakeDamage( Vector3 startPosition, Entity whoRef, Weapon weaponRef, float damage, bool canPenetrate )
	{
		if ( m_IsUnbreakable == true )
		{
			return;
		}

		m_CurrentStatus -= damage;
		if ( m_CurrentStatus <= 0.0f )
		{
			m_Renderer.enabled = false;
			m_Collider.enabled = false;
		}
	}

}
