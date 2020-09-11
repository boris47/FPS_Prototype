
using UnityEngine;

public enum EShieldContext {
	NONE,
	ENTITY, WEAPON, MESH
}

public interface IShield : IStreamableByEvents {

	/// <summary> Event called when shield is hitted </summary>
	event			Shield.ShieldHitEvent		OnHit;
	
	EShieldContext	Context						{ get; }

	void			CollisionHit				( GameObject collidingObject );

	float			StartStatus					{ get; }
	float			Status						{ get; set; }
	bool			IsUnbreakable				{ get; }

	void			Setup						( float StartStatus, EShieldContext Context, bool IsUnbreakable = false );
	void			OnReset						();
}

[RequireComponent( typeof ( Collider ) )]
public class Shield : MonoBehaviour, IShield {

	// Delegate definition
	public	delegate	void	ShieldHitEvent( Vector3 startPosition, Entity whoRef, Weapon weaponRef, EDamageType damageType, float damage, bool canPenetrate = false );

	// Internal Event
	protected	ShieldHitEvent	m_ShielHitEvent			= delegate { };

	[SerializeField]
	protected		bool		m_IsUnbreakable			= false;

	/// INTERFACE START
	/// <summary> Event called when shiled is hitted </summary>
	event ShieldHitEvent	IShield.OnHit
	{
		add		{ if ( value != null ) this.m_ShielHitEvent += value; }
		remove	{ if ( value != null ) this.m_ShielHitEvent -= value; }
	}

	EShieldContext	IShield.Context						{	get { return this.m_Context; } }
	float			IShield.StartStatus					{	get { return this.m_StartStatus; } }
	float			IShield.Status						{	get { return this.m_CurrentStatus;	} set { this.m_CurrentStatus = value; }  }
	bool			IShield.IsUnbreakable				{	get { return this.m_IsUnbreakable;	}	}
	

	/// INTERFACE END
	/// 
	
	private		EShieldContext	m_Context			= EShieldContext.NONE;
	private		Collider		m_Collider			= null;
	private		Renderer		m_Renderer			= null;
	private		float			m_CurrentStatus		= 100f;
	private		float			m_StartStatus		= 100f;


	//////////////////////////////////////////////////////////////////////////
	// Awake
	private void Awake()
	{
		Utils.Base.SearchComponent(this.gameObject, out this.m_Renderer, ESearchContext.LOCAL );
		Utils.Base.SearchComponent(this.gameObject, out this.m_Collider, ESearchContext.LOCAL );

		// First assignment
		this.ResetDelegate();
	}


	//////////////////////////////////////////////////////////////////////////
	// ResetDelegate
	private	void	ResetDelegate()
	{
		ShieldHitEvent onShiledHit = delegate( Vector3 startPosition, Entity whoRef, Weapon weaponRef, EDamageType damageType, float damage, bool canPenetrate )
		{
			this.TakeDamage( startPosition, whoRef, weaponRef, damageType, damage, canPenetrate );
		};
		this.m_ShielHitEvent = onShiledHit;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnTriggerHit
	public		void		CollisionHit				( GameObject collidingObject )
	{
		if ( Utils.Base.SearchComponent( collidingObject, out IBullet bullet, ESearchContext.CHILDREN ) )
		{
			this.m_ShielHitEvent( bullet.StartPosition, bullet.WhoRef, bullet.Weapon, bullet.DamageType, bullet.Damage, bullet.CanPenetrate );
		}
	}
	

	//////////////////////////////////////////////////////////////////////////
	// OnCollisionEnter
	private void OnCollisionEnter( Collision collision )
	{
		if ( Utils.Base.SearchComponent( collision.gameObject, out IBullet bullet, ESearchContext.CHILDREN ) )
		{
			this.m_ShielHitEvent( bullet.StartPosition, bullet.WhoRef, bullet.Weapon, bullet.DamageType, bullet.Damage, bullet.CanPenetrate );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// OnTriggerEnter
	private void OnTriggerEnter( Collider other )
	{
		if ( Utils.Base.SearchComponent( other.gameObject, out IBullet bullet, ESearchContext.CHILDREN ) )
		{
			this.m_ShielHitEvent( bullet.StartPosition, bullet.WhoRef, bullet.Weapon, bullet.DamageType, bullet.Damage, bullet.CanPenetrate );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// OnEnable
	private void OnEnable()
	{
		this.m_Renderer.enabled = true;
		this.m_Collider.enabled = true;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnDisable
	private void OnDisable()
	{
		this.m_Renderer.enabled = false;
		this.m_Collider.enabled = false;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnSave
	StreamUnit IStreamableByEvents.OnSave( StreamData streamData )
	{
		StreamUnit streamUnit = null;
		if ( streamData.GetUnit(this.gameObject, ref streamUnit ) == false )
		{
			this.enabled = false;
			this.ResetDelegate();
//			gameObject.SetActive( false );
			return null;
		}

		streamUnit.SetInternal( "CurrentStatus", this.m_CurrentStatus );	

		return streamUnit;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnLoad
	StreamUnit IStreamableByEvents.OnLoad( StreamData streamData )
	{
		StreamUnit streamUnit = null;
		if ( streamData.GetUnit(this.gameObject, ref streamUnit ) == false )
		{
			this.enabled = false;
			this.ResetDelegate();
//			gameObject.SetActive( false );
			return null;
		}

		this.m_CurrentStatus = streamUnit.GetAsFloat( "CurrentStatus" );

		bool bIsActive = this.m_CurrentStatus > 0.0f;
		this.m_Renderer.enabled = bIsActive;
		this.m_Collider.enabled = bIsActive;

		return streamUnit;
	}


	//////////////////////////////////////////////////////////////////////////
	// Setup
	void		IShield.Setup( float StartStatus, EShieldContext Context, bool IsUnbreakable )
	{
		this.m_StartStatus	= StartStatus;
		this.m_Context		= Context;
		this.m_IsUnbreakable	= IsUnbreakable;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnReset
	void		IShield.OnReset()
	{
		this.m_Renderer.enabled = true;
		this.m_Collider.enabled = true;
		this.m_CurrentStatus = this.m_StartStatus;
	}


	//////////////////////////////////////////////////////////////////////////
	// TakeDamage
	protected		void	TakeDamage( Vector3 startPosition, Entity whoRef, Weapon weaponRef, EDamageType damageType, float damage, bool canPenetrate )
	{
		if (this.m_IsUnbreakable == true )
		{
			return;
		}

		this.m_CurrentStatus -= damage;
		if (this.m_CurrentStatus <= 0.0f )
		{
			this.enabled = false;
		}
	}

}
