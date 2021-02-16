
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
		add		{ if ( value != null ) m_ShielHitEvent += value; }
		remove	{ if ( value != null ) m_ShielHitEvent -= value; }
	}

	EShieldContext	IShield.Context						{	get { return m_Context; } }
	float			IShield.StartStatus					{	get { return m_StartStatus; } }
	float			IShield.Status						{	get { return m_CurrentStatus;	} set { m_CurrentStatus = value; }  }
	bool			IShield.IsUnbreakable				{	get { return m_IsUnbreakable;	}	}
	

	/// INTERFACE END
	/// 
	
	private		EShieldContext	m_Context			= EShieldContext.NONE;
	private		Collider		m_Collider			= null;
	private		Renderer		m_Renderer			= null;
	private		float			m_CurrentStatus		= 100f;
	private		float			m_StartStatus		= 100f;


	//////////////////////////////////////////////////////////////////////////
	private void Awake()
	{
		Utils.Base.TrySearchComponent(gameObject, ESearchContext.LOCAL, out m_Renderer );
		Utils.Base.TrySearchComponent(gameObject, ESearchContext.LOCAL, out m_Collider );

		// First assignment
		ResetDelegate();
	}


	//////////////////////////////////////////////////////////////////////////
	private	void	ResetDelegate()
	{
		m_ShielHitEvent = TakeDamage;
	}


	//////////////////////////////////////////////////////////////////////////
	public		void		CollisionHit				( GameObject collidingObject )
	{
		if ( Utils.Base.TrySearchComponent( collidingObject, ESearchContext.CHILDREN, out IBullet bullet ) )
		{
			m_ShielHitEvent( bullet.StartPosition, bullet.WhoRef, bullet.Weapon, bullet.DamageType, bullet.Damage, bullet.CanPenetrate );
		}
	}
	

	//////////////////////////////////////////////////////////////////////////
	private void OnCollisionEnter( Collision collision )
	{
		if ( Utils.Base.TrySearchComponent( collision.gameObject, ESearchContext.CHILDREN, out IBullet bullet ) )
		{
			m_ShielHitEvent( bullet.StartPosition, bullet.WhoRef, bullet.Weapon, bullet.DamageType, bullet.Damage, bullet.CanPenetrate );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnTriggerEnter( Collider other )
	{
		if ( Utils.Base.TrySearchComponent( other.gameObject, ESearchContext.CHILDREN, out IBullet bullet ) )
		{
			m_ShielHitEvent( bullet.StartPosition, bullet.WhoRef, bullet.Weapon, bullet.DamageType, bullet.Damage, bullet.CanPenetrate );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnEnable()
	{
		m_Renderer.enabled = true;
		m_Collider.enabled = true;
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnDisable()
	{
		m_Renderer.enabled = false;
		m_Collider.enabled = false;
	}


	//////////////////////////////////////////////////////////////////////////
	StreamUnit IStreamableByEvents.OnSave( StreamData streamData )
	{
		StreamUnit streamUnit = null;
		if ( streamData.TryGetUnit(gameObject, out streamUnit ) == false )
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
	StreamUnit IStreamableByEvents.OnLoad( StreamData streamData )
	{
		StreamUnit streamUnit = null;
		if ( streamData.TryGetUnit(gameObject, out streamUnit ) == false )
		{
			enabled = false;
			ResetDelegate();
//			gameObject.SetActive( false );
			return null;
		}

		m_CurrentStatus = streamUnit.GetAsFloat( "CurrentStatus" );

		bool bIsActive = m_CurrentStatus > 0.0f;
		m_Renderer.enabled = bIsActive;
		m_Collider.enabled = bIsActive;

		return streamUnit;
	}


	//////////////////////////////////////////////////////////////////////////
	void		IShield.Setup( float StartStatus, EShieldContext Context, bool IsUnbreakable )
	{
		m_StartStatus	= StartStatus;
		m_Context		= Context;
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
	protected		void	TakeDamage( Vector3 startPosition, Entity whoRef, Weapon weaponRef, EDamageType damageType, float damage, bool canPenetrate )
	{
		if (m_IsUnbreakable == true )
		{
			return;
		}

		m_CurrentStatus -= damage;
		if (m_CurrentStatus <= 0.0f )
		{
			enabled = false;
		}
	}

}
