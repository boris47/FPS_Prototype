
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum EBulletMotionType
{
	INSTANT,
	DIRECT,
	PARABOLIC
}


public interface IBullet {

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
	void				Shoot						( Vector3 position, Vector3 direction, float velocity = 0f ); // TODO Compute modifiers
}




[RequireComponent( typeof( Rigidbody ), typeof( Collider ), typeof( Renderer ) )]
public abstract class Bullet : MonoBehaviour, IBullet {
	
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



	protected		Rigidbody			m_RigidBody				= null;
	protected		Collider			m_Collider				= null;
	protected		Entity				m_WhoRef				= null;
	protected		Weapon				m_Weapon				= null;
	protected		Object				m_BulletEffect			= null;

	// INTERFACE START
					EBulletMotionType	IBullet.MotionType				{	get { return this.m_BulletMotionType; }	}
					EDamageType			IBullet.DamageType				{	get { return this.m_DamageType; }	}
					float				IBullet.Damage					{	get { return this.m_Damage;}		}
					bool				IBullet.HasDamageOverTime		{	get { return this.m_HasDamageOverTime; }	}
					float				IBullet.OverTimeDamageDuration	{	get { return this.m_OverTimeDamageDuration; }		}
					EDamageType			IBullet.OverTimeDamageType		{	get { return this.m_OverTimeDamageType; }		}
					bool				IBullet.CanPenetrate			{	get { return this.m_CanPenetrate; }	}
					float				IBullet.Velocity				{	get { return this.m_Velocity; }		}

					Entity				IBullet.WhoRef					{	get { return this.m_WhoRef; }		}
					Weapon				IBullet.Weapon					{	get { return this.m_Weapon; }		}
					Object				IBullet.Effect					{	get { return this.m_BulletEffect; }	}
					float				IBullet.RecoilMult				{	get { return this.m_RecoilMult; }	}
					Vector3				IBullet.StartPosition			{	get { return this.m_StartPosition; }	}
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
		string sectionName = this.GetType().Name;
		if ( m_BulletsSections.TryGetValue( sectionName, out this.m_BulletSection ) == false )
		{
			GlobalManager.Configs.GetSection( sectionName, ref this.m_BulletSection );
			m_BulletsSections[sectionName] = this.m_BulletSection;
		}

		CoroutinesManager.Start(this.SetupBulletCO() );

		this.m_RigidBody	= this.GetComponent<Rigidbody>();
		this.m_Collider	= this.GetComponent<Collider>();
		this.m_Renderer	= this.GetComponent<Renderer>();
	}


	
	//////////////////////////////////////////////////////////////////////////
	// SetupBulletCO ( Virtual )
	protected virtual IEnumerator SetupBulletCO()
	{
		yield return null;

		this.m_RigidBody.interpolation				= RigidbodyInterpolation.Interpolate;
		this.m_RigidBody.collisionDetectionMode		= CollisionDetectionMode.ContinuousDynamic;
		this.m_RigidBody.maxAngularVelocity			= 0f;

		yield return null;

		// MotionType
		Utils.Converters.StringToEnum(this.m_BulletSection.AsString("eBulletMotionType"), ref this.m_BulletMotionType );

		// DamageType
		Utils.Converters.StringToEnum(this.m_BulletSection.AsString("eDamageType"), ref this.m_DamageType );

		// fDamage
		this.m_Damage = this.m_BulletSection.AsFloat( "fDamage", this.m_Damage );

		// bHasDamageOverTime
		this.m_HasDamageOverTime = this.m_BulletSection.AsBool( "bHasDamageOverTime", this.m_HasDamageOverTime );

		// fOverTimeDamageDuration
		this.m_OverTimeDamageDuration = this.m_BulletSection.AsFloat( "fOverTimeDamageDuration", this.m_OverTimeDamageDuration );

		// eOverTimeDamageType
		Utils.Converters.StringToEnum(this.m_BulletSection.AsString("eOverTimeDamageType"), ref this.m_OverTimeDamageType );

		// bCanPenetrate
		this.m_BulletSection.bAsBool( "bCanPenetrate", ref this.m_CanPenetrate );

		// fVelocity
		this.m_Velocity = this.m_BulletSection.AsFloat( "fVelocity", this.m_Velocity );

		// fRange
		this.m_Range = this.m_BulletSection.AsFloat( "fRange", this.m_Range );
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

