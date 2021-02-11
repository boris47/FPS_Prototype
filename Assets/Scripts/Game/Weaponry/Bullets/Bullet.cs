
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum EBulletMotionType
{
	INSTANT,
	DIRECT,
	PARABOLIC
}


public interface IBullet
{
	EBulletMotionType	MotionType					{ get; }
	EDamageType			DamageType					{ get; }
	float				Damage						{ get; }
	bool				HasDamageOverTime			{ get; }
	float				OverTimeDamageDuration		{ get; }
	EDamageType			OverTimeDamageType			{ get; }
	bool				CanPenetrate				{ get; }
	float				Velocity					{ get; }


	Entity				WhoRef						{ get; }
	Weapon				Weapon						{ get; }
	Object				Effect						{ get; }
	float				RecoilMult					{ get; }
	Vector3				StartPosition				{ get; }

	void				Setup						( Entity whoRef, Weapon weaponRef );
	void				OverrideDamages				( float NewMinDamage, float NewMaxDamage );
	void				SetActive					( bool state );

	void				Shoot						(Vector3 position, Vector3 direction, float? velocity);
	void				ShootInstant				(Vector3 position, Vector3 direction, float? maxDistance);
	void				ShootDirect					(Vector3 position, Vector3 direction, float? velocity);
	void				ShootParabolic				(Vector3 position, Vector3 direction, float? velocity);
}




[RequireComponent( typeof( Rigidbody ), typeof( Collider ), typeof( Renderer ) )]
public abstract class Bullet : MonoBehaviour, IBullet
{
	public const float MAX_BULLET_DISTANCE = 900.0f;

	[SerializeField, ReadOnly]
	protected		EBulletMotionType	m_BulletMotionType		= EBulletMotionType.DIRECT;

	[SerializeField, ReadOnly]
	protected		EDamageType			m_DamageType			= EDamageType.BALLISTIC;

	[SerializeField, ReadOnly]
	protected		float				m_Damage				= 0f;

	[SerializeField, ReadOnly]
	protected		bool				m_HasDamageOverTime		= false;

	[SerializeField, ReadOnly]
	protected		float				m_OverTimeDamageDuration = 5.0f;

	[SerializeField, ReadOnly]
	protected		EDamageType			m_OverTimeDamageType	= EDamageType.NONE;

	[SerializeField, ReadOnly]
	protected		float				m_Velocity				= 15f;

	[Space(), Header("Prefab Data")]

	[SerializeField]
	protected		float				m_Range					= 30f;

	[SerializeField, Range( 0.5f, 3f )]
	protected		float				m_RecoilMult			= 1f;

	[SerializeField]
	protected		bool				m_CanPenetrate			= false;

	// INTERFACE START
					EBulletMotionType	IBullet.MotionType				=> m_BulletMotionType;
					EDamageType			IBullet.DamageType				=> m_DamageType;
					float				IBullet.Damage					=> m_Damage;
					bool				IBullet.HasDamageOverTime		=> m_HasDamageOverTime;
					float				IBullet.OverTimeDamageDuration	=> m_OverTimeDamageDuration;
					EDamageType			IBullet.OverTimeDamageType		=> m_OverTimeDamageType;
					bool				IBullet.CanPenetrate			=> m_CanPenetrate;
					float				IBullet.Velocity				=> m_Velocity;

					Entity				IBullet.WhoRef					=> m_WhoRef;
					Weapon				IBullet.Weapon					=> m_Weapon;
					Object				IBullet.Effect					=> m_BulletEffect;
					float				IBullet.RecoilMult				=> m_RecoilMult;
					Vector3				IBullet.StartPosition			=> m_StartPosition;
	// INTERFACE END

	protected		Rigidbody			m_RigidBody				= null;
	protected		Collider			m_Collider				= null;
	protected		Entity				m_WhoRef				= null;
	protected		Weapon				m_Weapon				= null;
	protected		Object				m_BulletEffect			= null;

	protected		Renderer			m_Renderer				= null;
	protected		Vector3				m_StartPosition			= Vector3.zero;
	protected		Vector3				m_RigidBodyVelocity		= Vector3.zero;
	protected		Database.Section	m_BulletSection			= null;


	private readonly static Dictionary<string, Database.Section> m_BulletsSections = new Dictionary<string, Database.Section>();
	private static Dictionary<string, GameObject> m_BulletsCache = new Dictionary<string, GameObject>();

	public static bool TryGetBulletModel(string bulletSectionName, out GameObject model)
	{
		if (!m_BulletsCache.TryGetValue( bulletSectionName, out model ))
		{
			if (!ResourceManager.LoadResourceSync($"Prefabs/Bullets/{bulletSectionName}", out model))
			{
				UnityEngine.Debug.Log($"Bullet:GetBulletModel Failed trying to load resource bullet of section {bulletSectionName}");
				return false;
			}
		}
		return true;
	}



	protected	virtual		void	Awake()
	{
		string sectionName = GetType().Name;
		if ( !m_BulletsSections.TryGetValue( sectionName, out m_BulletSection ) )
		{
			GlobalManager.Configs.TryGetSection( sectionName, out m_BulletSection );
			m_BulletsSections[sectionName] = m_BulletSection;
		}

		TryGetComponent( out m_RigidBody );
		TryGetComponent( out m_Collider );
		TryGetComponent( out m_Renderer );

		SetupBullet();
	}

	protected	virtual		void	SetupBullet()
	{
		m_RigidBody.interpolation				= RigidbodyInterpolation.None;
		m_RigidBody.collisionDetectionMode		= CollisionDetectionMode.ContinuousDynamic;
		m_RigidBody.maxAngularVelocity			= 0.0001f;

		// MotionType
		Utils.Converters.StringToEnum(m_BulletSection.AsString("eBulletMotionType"), out m_BulletMotionType );

		// DamageType
		Utils.Converters.StringToEnum(m_BulletSection.AsString("eDamageType"), out m_DamageType );

		// fDamage
		m_Damage = m_BulletSection.AsFloat( "fDamage", m_Damage );

		// bHasDamageOverTime
		m_HasDamageOverTime = m_BulletSection.AsBool( "bHasDamageOverTime", m_HasDamageOverTime );

		// fOverTimeDamageDuration
		m_OverTimeDamageDuration = m_BulletSection.AsFloat( "fOverTimeDamageDuration", m_OverTimeDamageDuration );

		// eOverTimeDamageType
		Utils.Converters.StringToEnum(m_BulletSection.AsString("eOverTimeDamageType"), out m_OverTimeDamageType );

		// bCanPenetrate
		m_BulletSection.TryAsBool( "bCanPenetrate", out m_CanPenetrate );

		// fVelocity
		m_Velocity = m_BulletSection.AsFloat( "fVelocity", m_Velocity );

		// fRange
		m_Range = m_BulletSection.AsFloat( "fRange", MAX_BULLET_DISTANCE );
	}

	public		abstract	void	Setup( Entity whoRef, Weapon weaponRef );

	public		virtual		void	OverrideDamages( float NewMinDamage, float NewMaxDamage ) { }


	public		abstract	void	SetActive( bool state );
	protected	abstract	void	Update();	

	/** Shhot bullet using asigned motion type */
	public		abstract	void	Shoot(Vector3 position, Vector3 direction, float? velocity);

	/** Use physic to shoot immediately */
	public		abstract	void	ShootInstant(Vector3 position, Vector3 direction, float? maxDistance);

	/** Launch the bullet with fixed velocity and distance check */
	public		abstract	void	ShootDirect(Vector3 position, Vector3 direction, float? velocity);

	/** Launch the bullet with physic applied */
	public		abstract	void	ShootParabolic(Vector3 position, Vector3 direction, float? velocity);

	//
	protected	virtual void	OnCollisionEnter( Collision collision )
	{
		ContactPoint contact = collision.GetContact( 0 );
		OnCollisionDetailed( contact.point, contact.normal, collision.collider );
	}

	protected	virtual void	OnTriggerEnter( Collider other )
	{
		Vector3 point = other.ClosestPoint( transform.position );
		Vector3 normal = ( other.transform.position - point ).normalized;
		OnCollisionDetailed( point, normal, other );
	}

	protected	abstract	void	OnCollisionDetailed( in Vector3 point, in Vector3 normal, in Collider otherCollider );
														
}

