
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
	float				RecoilMult					{ get; }
	Vector3				StartPosition				{ get; }

	void				Setup						(in Entity whoRef, in Weapon weaponRef);
	void				OverrideDamage				(in float newDamage);

	void				Shoot						(in Vector3 position, in Vector3 direction, in float? velocity, in float? impactForceMultiplier);
}


[RequireComponent(typeof(Rigidbody), typeof(Collider), typeof(Renderer))]
public abstract class Bullet : MonoBehaviour, IBullet
{
	public	const		float									MAX_BULLET_DISTANCE				= 900.0f;

	[SerializeField, ReadOnly]
	protected			EBulletMotionType						m_BulletMotionType				= EBulletMotionType.DIRECT;

	[SerializeField, ReadOnly]
	protected			EDamageType								m_DamageType					= EDamageType.BALLISTIC;

	[SerializeField, ReadOnly]
	protected			float									m_Damage						= 0f;

	[SerializeField, ReadOnly]
	protected			bool									m_HasDamageOverTime				= false;

	[SerializeField, ReadOnly]
	protected			float									m_OverTimeDamageDuration		= 5.0f;

	[SerializeField, ReadOnly]
	protected			EDamageType								m_OverTimeDamageType			= EDamageType.NONE;

	[SerializeField, ReadOnly]
	protected			float									m_Velocity						= 15f;

	[Space(), Header("Prefab Data")]

	[SerializeField]
	protected			float									m_Range							= 30f;

	[SerializeField, Range( 0.5f, 3f )]
	protected			float									m_RecoilMult					= 1f;

	[SerializeField]
	protected			bool									m_CanPenetrate					= false;

	// INTERFACE START
						EBulletMotionType						IBullet.MotionType				=> m_BulletMotionType;
						EDamageType								IBullet.DamageType				=> m_DamageType;
						float									IBullet.Damage					=> m_Damage;
						bool									IBullet.HasDamageOverTime		=> m_HasDamageOverTime;
						float									IBullet.OverTimeDamageDuration	=> m_OverTimeDamageDuration;
						EDamageType								IBullet.OverTimeDamageType		=> m_OverTimeDamageType;
						bool									IBullet.CanPenetrate			=> m_CanPenetrate;
						float									IBullet.Velocity				=> m_Velocity;

						Entity									IBullet.WhoRef					=> m_WhoRef;
						Weapon									IBullet.Weapon					=> m_Weapon;
						float									IBullet.RecoilMult				=> m_RecoilMult;
						Vector3									IBullet.StartPosition			=> m_StartPosition;
	// INTERFACE END

	protected			Rigidbody								m_RigidBody						= null;
	protected			Collider								m_Collider						= null;
	protected			Entity									m_WhoRef						= null;
	protected			Weapon									m_Weapon						= null;

	protected			Renderer								m_Renderer						= null;
	protected			Vector3									m_StartPosition					= Vector3.zero;
	protected			Vector3									m_LastPosition					= Vector3.zero;
	protected			float									m_SqrDistanceTravelled			= 0;
	protected			Vector3									m_RigidBodyVelocity				= Vector3.zero;
	protected			float									m_ImpactForceMultiplier			= 1f;
	protected			Database.Section						m_BulletSection					= null;


	private	static		Dictionary<string, Database.Section>	m_BulletsSections				= new Dictionary<string, Database.Section>();
	private	static		Dictionary<string, GameObject>			m_BulletsCache					= new Dictionary<string, GameObject>();


	//////////////////////////////////////////////////////////////////////////
	public static void GetBulletModel(string bulletSectionName, out GameObject model)
	{
		if (!m_BulletsCache.TryGetValue(bulletSectionName, out model))
		{
			CustomAssertions.IsTrue(ResourceManager.LoadResourceSync($"Prefabs/Bullets/{bulletSectionName}", out model));
			m_BulletsCache[bulletSectionName] = model;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	protected virtual void Awake()
	{
		string sectionName = GetType().Name;
		if (!m_BulletsSections.TryGetValue(sectionName, out m_BulletSection))
		{
			CustomAssertions.IsTrue(GlobalManager.Configs.TryGetSection(sectionName, out m_BulletSection));
			m_BulletsSections[sectionName] = m_BulletSection;
		}

		CustomAssertions.IsTrue(TryGetComponent(out m_RigidBody));
		CustomAssertions.IsTrue(TryGetComponent(out m_Collider));
		CustomAssertions.IsTrue(TryGetComponent(out m_Renderer));

		SetupBullet();

		// Bullets are meant to be used by pool, so activated a right time
		if (gameObject.activeSelf)
		{
			gameObject.SetActive(false);
		}
		
		// Ensure bullet script as enabled
		enabled = true;
	}


	//////////////////////////////////////////////////////////////////////////
	protected virtual void OnEnable()
	{
		if (CustomAssertions.IsNotNull(GameManager.UpdateEvents))
		{
			GameManager.UpdateEvents.OnFrame += OnUpdate;
			GameManager.UpdateEvents.OnLateFrame += OnLateFrame;
			GameManager.UpdateEvents.OnPhysicFrame += OnPhysicFrame;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	protected virtual void OnDisable()
	{
		if (GameManager.UpdateEvents.IsNotNull())
		{
			GameManager.UpdateEvents.OnPhysicFrame -= OnPhysicFrame;
			GameManager.UpdateEvents.OnLateFrame -= OnLateFrame;
			GameManager.UpdateEvents.OnFrame -= OnUpdate;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	protected virtual void SetupBullet()
	{
	//	m_RigidBody.interpolation			= RigidbodyInterpolation.None;
		m_RigidBody.collisionDetectionMode	= CollisionDetectionMode.ContinuousDynamic;
		m_RigidBody.maxAngularVelocity		= 0.0001f;
		m_RigidBody.drag					= 0f;
		m_RigidBody.angularDrag				= 0f;

		// MotionType
		Utils.Converters.StringToEnum(m_BulletSection.AsString("eBulletMotionType"), out m_BulletMotionType);

		// DamageType
		Utils.Converters.StringToEnum(m_BulletSection.AsString("eDamageType"), out m_DamageType);

		// fDamage
		m_Damage = m_BulletSection.AsFloat("fDamage", m_Damage);

		// bHasDamageOverTime
		m_HasDamageOverTime = m_BulletSection.AsBool("bHasDamageOverTime", m_HasDamageOverTime);

		// fOverTimeDamageDuration
		m_OverTimeDamageDuration = m_BulletSection.AsFloat("fOverTimeDamageDuration", m_OverTimeDamageDuration);

		// eOverTimeDamageType
		Utils.Converters.StringToEnum(m_BulletSection.AsString("eOverTimeDamageType"), out m_OverTimeDamageType);

		// bCanPenetrate
		m_BulletSection.TryAsBool("bCanPenetrate", out m_CanPenetrate);

		// fVelocity
		m_Velocity = m_BulletSection.AsFloat("fVelocity", m_Velocity);

		// fRange
		m_Range = m_BulletSection.AsFloat("fRange", MAX_BULLET_DISTANCE);
	}


	//////////////////////////////////////////////////////////////////////////
	protected virtual void OnTriggerEnter(Collider other)
	{
		Vector3 point = other.ClosestPoint(transform.position);
		Vector3 normal = (other.transform.position - point).normalized;
		OnCollisionDetailed(point, normal, other);
	}


	//////////////////////////////////////////////////////////////////////////
	protected virtual void OnCollisionEnter(Collision collision)
	{
		ContactPoint contact = collision.GetContact(0);
		OnCollisionDetailed(contact.point, contact.normal, collision.collider);
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Use physic to shoot immediately </summary>
	protected void ShootInstant(in Vector3 position, in Vector3 direction, in float maxDistance, in float impactForceMultiplier)
	{
		EffectsManager.Instance.PlayEffect(EffectsManager.EEffecs.MUZZLE, position, direction, 0, 0.1f);

		if (m_RigidBody.SweepTest(direction, out RaycastHit hit, maxDistance, QueryTriggerInteraction.Ignore))
		{
			OnCollisionDetailed(hit.point, hit.normal, hit.collider);
		}
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Launch the bullet with fixed velocity </summary>
	protected void ShootDirect(in Vector3 position, in Vector3 direction, in float maxDistance, in float impactForceMultiplier)
	{
		EffectsManager.Instance.PlayEffect(EffectsManager.EEffecs.MUZZLE, position, direction, 0, 0.1f);

		if (m_RigidBody.SweepTest(direction, out RaycastHit hit, maxDistance, QueryTriggerInteraction.Ignore))
		{
			OnCollisionDetailed(hit.point, hit.normal, hit.collider);
		}
		else
		{
			transform.up = direction;
			m_RigidBody.velocity = m_RigidBodyVelocity;
			m_RigidBody.useGravity = false;
			gameObject.SetActive(true);
		}
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Launch the bullet with physic applied </summary>
	protected void ShootParabolic(in Vector3 position, in Vector3 direction, in float maxDistance, in float impactForceMultiplier)
	{
		EffectsManager.Instance.PlayEffect(EffectsManager.EEffecs.MUZZLE, position, direction, 0, 0.1f);

		if (m_RigidBody.SweepTest(direction, out RaycastHit hit, maxDistance, QueryTriggerInteraction.Ignore))
		{
			OnCollisionDetailed(hit.point, hit.normal, hit.collider);
		}
		else
		{
			transform.up = direction;
			m_RigidBody.velocity = m_RigidBodyVelocity;
			m_RigidBody.useGravity = true;
			gameObject.SetActive(true);
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnUpdate(float deltaTime)
	{
		m_SqrDistanceTravelled += (transform.position - m_LastPosition).sqrMagnitude;
		m_LastPosition = transform.position;

		if (m_BulletMotionType == EBulletMotionType.PARABOLIC)
		{
			transform.up = m_RigidBody.velocity.normalized;
		}

		if (gameObject.EveryFrames(12))
		{
			if (m_RigidBody.SweepTest(transform.up, out RaycastHit hit, m_RigidBody.velocity.magnitude))
			{
				OnCollisionDetailed(hit.point, hit.normal, hit.collider);
			}
			else
			{
				if (m_SqrDistanceTravelled > m_Range * m_Range)
				{
					OnEndTravel();
				}
			}
		}

		OnFrame(deltaTime);
	}


	//////////////////////////////////////////////////////////////////////////
	protected abstract void OnEndTravel();


	//////////////////////////////////////////////////////////////////////////
	protected virtual void OnFrame(float deltaTime)
	{

	}


	//////////////////////////////////////////////////////////////////////////
	protected virtual void OnLateFrame(float deltaTime)
	{

	}


	//////////////////////////////////////////////////////////////////////////
	protected virtual void OnPhysicFrame(float fixedDeltaTime)
	{

	}


	//////////////////////////////////////////////////////////////////////////
	protected virtual void OnCollisionDetailed(in Vector3 point, in Vector3 normal, in Collider otherCollider)
	{
		EffectsManager.EEffecs effectToPlay = EffectsManager.EEffecs.ENTITY_ON_HIT;

		Debug.DrawRay(m_StartPosition, point - m_StartPosition, Color.red, 50f);

		if (Utils.Base.TrySearchComponent(otherCollider.gameObject, ESearchContext.LOCAL_AND_PARENTS, out Entity entity))
		{
			entity.OnHittedDetails(m_StartPosition, m_WhoRef, m_DamageType, 0, m_CanPenetrate);
		}
		else if (Utils.Base.TrySearchComponent(otherCollider.gameObject, ESearchContext.LOCAL_AND_CHILDREN, out Shield shield))
		{
			shield.OnHittedDetails(gameObject);
		}
		else
		{
			effectToPlay = EffectsManager.EEffecs.AMBIENT_ON_HIT;
			otherCollider.attachedRigidbody?.AddForceAtPosition(m_RigidBodyVelocity * m_ImpactForceMultiplier, point, ForceMode.Impulse);
		}

		EffectsManager.Instance.PlayEffect(effectToPlay, point, normal, 3);
		gameObject.SetActive(false);
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Setup this bullet </summary>
	public virtual void Setup(in Entity whoRef, in Weapon weaponRef)
	{
		m_WhoRef = whoRef;
		m_Weapon = weaponRef;

		if (m_WhoRef)
		{
			whoRef.SetCollisionStateWith(m_Collider, state: false);
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public virtual void OverrideDamage(in float newDamage)
	{
		m_Damage = newDamage;
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Shoot bullet using assigned motion type </summary>
	public virtual void Shoot(in Vector3 position, in Vector3 direction, in float? velocity, in float? impactForceMultiplier)
	{
		float finalVelocity = (velocity ?? m_Velocity);
		float finalImpactForceMultiplier = impactForceMultiplier ?? m_ImpactForceMultiplier;
		transform.position = position;

		m_RigidBodyVelocity = direction * finalVelocity;
		m_StartPosition = m_LastPosition = position;

		switch (m_BulletMotionType)
		{
			case EBulletMotionType.INSTANT:		ShootInstant(position, direction, finalVelocity, finalImpactForceMultiplier);		break;
			case EBulletMotionType.DIRECT:		ShootDirect(position, direction, finalVelocity, finalImpactForceMultiplier);		break;
			case EBulletMotionType.PARABOLIC:	ShootParabolic(position, direction, finalVelocity, finalImpactForceMultiplier);		break;
		}
	}
}

