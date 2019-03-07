
using UnityEngine;

public interface IBullet {

	float				Velocity		{ get; }
	Entity				WhoRef			{ get; }
	Weapon				Weapon			{ get; }
	Object				Effect			{ get; }
	float				DamageMin		{ get; set; }
	float				DamageMax		{ get; set; }
	float				DamageRandom	{ get; }
	float				DamageMult		{ get; }
	float				RecoilMult		{ get; }
	bool				CanPenetrate	{ get; set; }
	Vector3				StartPosition	{ get; }
	BulletMotionType	GetMotionType	{ get; }

	void				Setup			( bool canPenetrate, Entity whoRef, Weapon weaponRef, float damageMin = -1.0f, float damageMax = -1.0f );
	void				SetActive		( bool state );
	void				Shoot			( Vector3 position, Vector3 direction, float velocity = 0f );
}


[RequireComponent( typeof( Rigidbody ), typeof( Collider ), typeof( Renderer ) )]
public abstract class Bullet : MonoBehaviour, IBullet {
	
	[SerializeField]
	protected		float				m_Velocity				= 15f;

	[SerializeField]
	protected		float				m_Range					= 30f;

	[SerializeField, Range( 0.5f, 3f )]
	protected		float				m_DamageMult			= 1f;

	[SerializeField, Range( 0.5f, 3f )]
	protected		float				m_RecoilMult			= 1f;

	[SerializeField]
	protected		BulletMotionType	m_MotionType			= BulletMotionType.DIRECT;

	// TODO remove this getter
	public			Rigidbody			RigidBody				{		get { return m_RigidBody; }		}
	public			Collider			Collider				{		get { return m_Collider; }		}
	public			float				Velocity				{		get { return m_Velocity; }		}
	public			BulletMotionType	MotionType				{		get { return m_MotionType; }	}
	

	protected		Rigidbody			m_RigidBody				= null;
	protected		Collider			m_Collider				= null;
	protected		Entity				m_WhoRef				= null;
	protected		Weapon				m_Weapon				= null;
	protected		Object				m_BulletEffect			= null;

	protected		float				m_DamageMin				= 0f;
	protected		float				m_DamageMax				= 0f;
	protected		bool				m_CanPenetrate			= false;

	// INTERFACE START
					float				IBullet.Velocity		{	get { return m_Velocity; }		}
					Entity				IBullet.WhoRef			{	get { return m_WhoRef; }		}
					Weapon				IBullet.Weapon			{	get { return m_Weapon; }		}
					Object				IBullet.Effect			{	get { return m_BulletEffect; }	}
					float				IBullet.DamageMin		{	get { return m_DamageMin; }		set { m_DamageMax = value; } }
					float				IBullet.DamageMax		{	get { return m_DamageMax;}		set { m_DamageMin = value; } }
					float				IBullet.DamageRandom	{	get { return UnityEngine.Random.Range( m_DamageMin, m_DamageMax ); } }
					float				IBullet.DamageMult		{	get { return m_DamageMult; }	}
					float				IBullet.RecoilMult		{	get { return m_RecoilMult; }	}
					bool				IBullet.CanPenetrate	{	get { return m_CanPenetrate; }	set { m_CanPenetrate = value; }	}
					Vector3				IBullet.StartPosition	{	get { return m_StartPosition; }	}
					BulletMotionType	IBullet.GetMotionType	{	get { return m_MotionType; } }
	// INTERFACE END

	protected		Renderer			m_Renderer				= null;
	protected		IBullet				m_Instance				= null;

	protected		Vector3				m_StartPosition			= Vector3.zero;
	protected		Vector3				m_RigidBodyVelocity		= Vector3.zero;

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


	//////////////////////////////////////////////////////////////////////////
	// Setup ( Abstract )
	/// <summary> For Bullet Setup </summary>
	public		abstract	void	Setup( bool canPenetrate, Entity whoRef, Weapon weaponRef, float damageMin = -1.0f, float damageMax = -1.0f );


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


	//////////////////////////////////////////////////////////////////////////
	// OnTriggerEnter ( Abstract )
	protected	abstract	void	OnTriggerEnter( Collider other );

}


public enum BulletMotionType {
	INSTANT,
	DIRECT,
	PARABOLIC
}
