
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

	void				Setup						( Entity whoRef, Weapon weaponRef );
	void				OverrideDamages				( float NewMinDamage, float NewMaxDamage );
	void				SetActive					( bool state );
	void				Shoot						( Vector3 position, Vector3 direction, float velocity = 0f ); // TODO Compute modifiers
}




[RequireComponent( typeof( Rigidbody ), typeof( Collider ), typeof( Renderer ) )]
public abstract class Bullet : MonoBehaviour, IBullet {
	
	[SerializeField, ReadOnly]
	protected		BulletMotionType	m_BulletMotionType		= BulletMotionType.DIRECT;

	[SerializeField, ReadOnly]
	protected		DamageType			m_DamageType			= DamageType.BALLISTIC;

	[SerializeField, ReadOnly]
	protected		float				m_DamageMin				= 0f;

	[SerializeField, ReadOnly]
	protected		float				m_DamageMax				= 0f;

	[SerializeField, ReadOnly]
	protected		bool				m_HasDamageOverTime		= false;

	[SerializeField, ReadOnly]
	protected		float				m_OverTimeDamageDuration = 5.0f;

	[SerializeField, ReadOnly]
	protected		DamageType			m_OverTimeDamageType	= DamageType.NONE;

	[SerializeField, ReadOnly]
	protected		float				m_Velocity				= 15f;


	[Space(), Header("Prefab Data")]

	[SerializeField]
	protected		float				m_Range					= 30f;

	[SerializeField, Range( 0.5f, 3f )]
	protected		float				m_RecoilMult			= 1f;

	[SerializeField]
	protected		bool				m_CanPenetrate			= false;



	protected		Rigidbody			m_RigidBody				= null;
	protected		Collider			m_Collider				= null;
	protected		Entity				m_WhoRef				= null;
	protected		Weapon				m_Weapon				= null;
	protected		Object				m_BulletEffect			= null;

	// INTERFACE START
					BulletMotionType	IBullet.MotionType				{	get { return m_BulletMotionType; }	}
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

	protected		Vector3				m_StartPosition			= Vector3.zero;
	protected		Vector3				m_RigidBodyVelocity		= Vector3.zero;
	protected		Database.Section	m_BulletSection			= null;


	private static Dictionary<string, Database.Section> m_BulletsSections = new Dictionary<string, Database.Section>();




	//////////////////////////////////////////////////////////////////////////
	// Awake ( Virtual )
	protected	virtual		void	Awake()
	{
		string sectionName = GetType().Name;
		if ( m_BulletsSections.TryGetValue( sectionName, out m_BulletSection ) == false )
		{
			GlobalManager.Configs.bGetSection( sectionName, ref m_BulletSection );
			m_BulletsSections[sectionName] = m_BulletSection;
		}

		CoroutinesManager.Start( SetupBulletCO() );

		m_RigidBody	= GetComponent<Rigidbody>();
		m_Collider	= GetComponent<Collider>();
		m_Renderer	= GetComponent<Renderer>();
	}


	
	//////////////////////////////////////////////////////////////////////////
	// SetupBulletCO ( Virtual )
	protected virtual System.Collections.IEnumerator SetupBulletCO()
	{
		yield return null;

		m_RigidBody.interpolation				= RigidbodyInterpolation.Interpolate;
		m_RigidBody.collisionDetectionMode		= CollisionDetectionMode.ContinuousDynamic;
		m_RigidBody.maxAngularVelocity			= 0f;

		yield return null;

		// MotionType
		Utils.Converters.StringToEnum( m_BulletSection.AsString("eBulletMotionType"), ref m_BulletMotionType );

		// DamageType
		Utils.Converters.StringToEnum( m_BulletSection.AsString("eDamageType"), ref m_DamageType );

		// fDamageMin
		m_DamageMin = m_BulletSection.AsFloat( "fDamageMin", m_DamageMin );

		// fDamageMax
		m_DamageMax = m_BulletSection.AsFloat( "fDamageMax", m_DamageMax );

		// bHasDamageOverTime
		m_HasDamageOverTime = m_BulletSection.AsBool( "bHasDamageOverTime", m_HasDamageOverTime );

		// fOverTimeDamageDuration
		m_OverTimeDamageDuration = m_BulletSection.AsFloat( "fOverTimeDamageDuration", m_OverTimeDamageDuration );

		// eOverTimeDamageType
		Utils.Converters.StringToEnum( m_BulletSection.AsString("eOverTimeDamageType"), ref m_OverTimeDamageType );

		// bCanPenetrate
		m_BulletSection.bAsBool( "bCanPenetrate", ref m_CanPenetrate );

		// fVelocity
		m_Velocity = m_BulletSection.AsFloat( "fVelocity", m_Velocity );

		// fRange
		m_Range = m_BulletSection.AsFloat( "fRange", m_Range );
	}


	//////////////////////////////////////////////////////////////////////////
	// Setup ( Abstract )
	/// <summary> For Bullet Setup </summary>
	public		abstract	void	Setup( Entity whoRef, Weapon weaponRef );


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

