
using UnityEngine;


public interface IBullet {

	Transform	Transform				{ get; }
	Rigidbody	RigidBody				{ get; }
	Collider	Collider				{ get; }

	Entity		WhoRef					{ get; }
	Weapon		Weapon					{ get; }
	float		DamageMin				{ get; set; }
	float		DamageMax				{ get; set; }
	float		DamageMult				{ get; }
	float		RecoilMult				{ get; }
	bool		CanPenetrate			{ get; set; }
	Vector3		StartPosition			{ get; }

	void		Setup( float damageMin, float damageMax, bool canPenetrate, Entity whoRef, Weapon weapon );	// For Entities
	void		Setup( float damage, bool canPenetrate, Entity whoRef, Weapon weapon );						// bullets
	void		Setup( Entity whoRef, Weapon weapon );														// granades and missiles
	void		SetActive( bool state );
	void		Shoot( Vector3 position, Vector3 direction, float velocity = 0f );
}


[RequireComponent( typeof( Rigidbody ), typeof( Collider ), typeof( Renderer ) )]
public abstract class Bullet : MonoBehaviour, IBullet {
	
	[SerializeField]
	protected		float		m_Velocity				= 15f;

	[SerializeField]
	protected		float		m_Range					= 30f;

	[SerializeField, Range( 0.5f, 3f )]
	protected		float		m_DamageMult			= 1f;

	[SerializeField, Range( 0.5f, 3f )]
	protected		float		m_RecoilMult			= 1f;

	public			Rigidbody	RigidBody				{		get { return m_RigidBody; }		}
	public			Collider	Collider				{		get { return m_Collider; }		}
	public			float		Velocity				{		get { return m_Velocity; }		}


	protected		Rigidbody	m_RigidBody				= null;
	protected		Collider	m_Collider				= null;
	protected		Entity		m_WhoRef				= null;
	protected		Weapon		m_Weapon				= null;
	protected		float		m_DamageMin				= 0f;
	protected		float		m_DamageMax				= 0f;
	protected		bool		m_CanPenetrate			= false;

	// INTERFACE
					Transform	IBullet.Transform		{	get { return transform; }		}
					Rigidbody	IBullet.RigidBody		{	get { return m_RigidBody; }		}
					Collider	IBullet.Collider		{	get { return m_Collider; }		}
					Entity		IBullet.WhoRef			{	get { return m_WhoRef; }		}
					Weapon		IBullet.Weapon			{	get { return m_Weapon; }		}
					float		IBullet.DamageMin		{	get { return m_DamageMin; }		set { m_DamageMax = value; } }
					float		IBullet.DamageMax		{	get { return m_DamageMax;}		set { m_DamageMin = value; } }
					float		IBullet.DamageMult		{	get { return m_DamageMult; }	}
					float		IBullet.RecoilMult		{	get { return m_RecoilMult; }	}
					bool		IBullet.CanPenetrate	{	get { return m_CanPenetrate; }	set { m_CanPenetrate = value; }	}
					Vector3		IBullet.StartPosition	{	get { return m_StartPosition; }	}


	protected		Renderer	m_Renderer				= null;
	protected		IBullet		m_Instance				= null;

	protected		Vector3		m_StartPosition			= Vector3.zero;


	//////////////////////////////////////////////////////////////////////////
	// Awake ( Virtual )
	protected	virtual		void	Awake()
	{
		m_Instance = this as IBullet;

		m_RigidBody	= GetComponent<Rigidbody>();
		m_Collider	= GetComponent<Collider>();
		m_Renderer	= GetComponent<Renderer>();

		m_RigidBody.interpolation				= RigidbodyInterpolation.Interpolate;
		m_RigidBody.collisionDetectionMode		= CollisionDetectionMode.ContinuousDynamic;
		m_RigidBody.maxAngularVelocity			= 0f;
	}


	public		virtual		void	Hide()
	{
		m_Renderer.enabled = false;
		m_Collider.enabled = false;
	}

	//////////////////////////////////////////////////////////////////////////
	// Setup ( Virtual )
	/// <summary> For Generic Bullets </summary>
	public		virtual		void	Setup( float damage, bool canPenetrate, Entity whoRef, Weapon weapon )
	{}

	//////////////////////////////////////////////////////////////////////////
	// Setup ( Virtual )
	/// <summary> For Entities </summary>
	public		virtual		void	Setup( float damageMin, float damageMax, bool canPenetrate, Entity whoRef, Weapon weapon )
	{}

	//////////////////////////////////////////////////////////////////////////
	// Setup ( Virtual )
	/// <summary> For Granades and Missiles </summary>
	public		virtual		void	Setup( Entity whoRef, Weapon weapon )
	{}

	//////////////////////////////////////////////////////////////////////////
	// OnEnable ( Abstract )
	protected	abstract	void	OnEnable();


	//////////////////////////////////////////////////////////////////////////
	// Update ( Abstract )
	protected	abstract	void	Update();
	

	//////////////////////////////////////////////////////////////////////////
	// Shoot ( Abstract )
	public		abstract	void	Shoot( Vector3 position, Vector3 direction, float velocity = 0f );


	//////////////////////////////////////////////////////////////////////////
	// SetActive ( Abstract )
	public		abstract	void	SetActive( bool state );


	//////////////////////////////////////////////////////////////////////////
	// OnCollisionEnter ( Abstract )
	protected	abstract	void	OnCollisionEnter( Collision collision );

}
