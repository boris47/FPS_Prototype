
using System.Collections.Generic;
using UnityEngine;


public enum BulletMotionType {
	INSTANT,
	DIRECT,
	PARABOLIC
}


public interface IBullet {

	BulletMotionType	MotionType					{ get; }
	DamageType			DamageType					{ get; }
	float				DamageMin					{ get; }
	float				DamageMax					{ get; }
	bool				HasDamageOverTime			{ get; }
	float				OverTimeDamageDuration		{ get; }
	DamageType			OverTimeDamageType			{ get; }
	bool				CanPenetrate				{ get; }
	float				Velocity					{ get; }


	Entity				WhoRef						{ get; }
	Weapon				Weapon						{ get; }
	Object				Effect						{ get; }
	float				DamageRandom				{ get; }
	float				RecoilMult					{ get; }
	Vector3				StartPosition				{ get; }

	void				Setup						( bool canPenetrate, Entity whoRef, Weapon weaponRef );
	void				OverrideDamages				( float NewMinDamage, float NewMaxDamage );
	void				SetActive					( bool state );
	void				Shoot						( Vector3 position, Vector3 direction, float velocity = 0f ); // TODO Compute modifiers
}



public interface IBulletBallistic {
}


public interface IExplosive {
	bool		BlowOnHit							{ get; }
	float		BlastRadius							{ get; }
	float		BlastDamage							{ get; }
	bool		AttachOnHit							{ get; }
	float		ExplosionDelay						{ get; }


	void		ForceExplosion						();
}


[RequireComponent( typeof( Rigidbody ), typeof( Collider ), typeof( Renderer ) )]
public abstract class Bullet : MonoBehaviour, IBullet {
	
	[SerializeField]
	protected		BulletMotionType	m_MotionType			= BulletMotionType.DIRECT;

	[SerializeField]
	protected		DamageType			m_DamageType			= DamageType.BALLISTIC;

	[SerializeField, ReadOnly]
	protected		float				m_DamageMin				= 0f;

	[SerializeField, ReadOnly]
	protected		float				m_DamageMax				= 0f;

	[SerializeField, ReadOnly]
	protected		bool				m_HasDamageOverTime		= false;

	[SerializeField, ReadOnly]
	protected		float				m_OverTimeDamageDuration = 5.0f;

	[SerializeField]
	protected		DamageType			m_OverTimeDamageType	= DamageType.NONE;

	[SerializeField]
	protected		float				m_Velocity				= 15f;


	[Space(), Header("Prefab Data")]

	[SerializeField]
	protected		float				m_Range					= 30f;

	[SerializeField, Range( 0.5f, 3f )]
	protected		float				m_RecoilMult			= 1f;

	[SerializeField, ReadOnly]
	protected		bool				m_CanPenetrate			= false;




	protected		Rigidbody			m_RigidBody				= null;
	protected		Collider			m_Collider				= null;
	protected		Entity				m_WhoRef				= null;
	protected		Weapon				m_Weapon				= null;
	protected		Object				m_BulletEffect			= null;

	// INTERFACE START
					BulletMotionType	IBullet.MotionType				{	get { return m_MotionType; }	}
					DamageType			IBullet.DamageType				{	get { return m_DamageType; }	}
					float				IBullet.DamageMin				{	get { return m_DamageMin; }		}
					float				IBullet.DamageMax				{	get { return m_DamageMax;}		}
					bool				IBullet.HasDamageOverTime		{	get { return m_HasDamageOverTime; }	}
					float				IBullet.OverTimeDamageDuration	{	get { return m_DamageMax;}		}
					DamageType			IBullet.OverTimeDamageType		{	get { return m_OverTimeDamageType; }		}
					bool				IBullet.CanPenetrate			{	get { return m_CanPenetrate; }	}
					float				IBullet.Velocity				{	get { return m_Velocity; }		}

					Entity				IBullet.WhoRef					{	get { return m_WhoRef; }		}
					Weapon				IBullet.Weapon					{	get { return m_Weapon; }		}
					Object				IBullet.Effect					{	get { return m_BulletEffect; }	}
					float				IBullet.DamageRandom			{	get { return UnityEngine.Random.Range( m_DamageMin, m_DamageMax ); } }
					float				IBullet.RecoilMult				{	get { return m_RecoilMult; }	}
					Vector3				IBullet.StartPosition			{	get { return m_StartPosition; }	}
	// INTERFACE END

	protected		Renderer			m_Renderer				= null;
	protected		IBullet				m_Instance				= null;

	protected		Vector3				m_StartPosition			= Vector3.zero;
	protected		Vector3				m_RigidBodyVelocity		= Vector3.zero;

	private static Dictionary<string, Database.Section> m_BulletsSections = new Dictionary<string, Database.Section>();

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

		string sectionName = GetType().Name;
		Database.Section bulletSection = null;
		if ( m_BulletsSections.TryGetValue( sectionName, out bulletSection ) == false )
		{
			GlobalManager.Configs.bGetSection( sectionName, ref bulletSection );
			m_BulletsSections[sectionName] = bulletSection;
		}

		// MotionType
		Utils.Converters.StringToEnum( bulletSection.AsString("eMotionType"), ref m_MotionType );

		// DamageType
		Utils.Converters.StringToEnum( bulletSection.AsString("eDamageType"), ref m_DamageType );

		// fDamageMin
		m_DamageMin = bulletSection.AsFloat( "fDamageMin", m_DamageMin );

		// fDamageMax
		m_DamageMax = bulletSection.AsFloat( "fDamageMax", m_DamageMax );

		// bHasDamageOverTime
		m_HasDamageOverTime = bulletSection.AsBool( "bHasDamageOverTime", m_HasDamageOverTime );

		// fOverTimeDamageDuration
		m_OverTimeDamageDuration = bulletSection.AsFloat( "fOverTimeDamageDuration", m_OverTimeDamageDuration );

		// eOverTimeDamageType
		Utils.Converters.StringToEnum( bulletSection.AsString("eOverTimeDamageType"), ref m_OverTimeDamageType );

		// bCanPenetrate
		bulletSection.bAsBool( "bCanPenetrate", ref m_CanPenetrate );

		// fVelocity
		m_Velocity = bulletSection.AsFloat( "fVelocity", m_Velocity );
	}


	//////////////////////////////////////////////////////////////////////////
	// Setup ( Abstract )
	/// <summary> Allow concrete class to read specific data </summary>
	protected	abstract	void	ReadInternals( Database.Section section );


	//////////////////////////////////////////////////////////////////////////
	// Setup ( Abstract )
	/// <summary> For Bullet Setup </summary>
	public		abstract	void	Setup( bool canPenetrate, Entity whoRef, Weapon weaponRef );


	//////////////////////////////////////////////////////////////////////////
	// OverrideDamages ( Abstract )
	public		/*abstract*/	void	OverrideDamages( float NewMinDamage, float NewMaxDamage ) { }


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

