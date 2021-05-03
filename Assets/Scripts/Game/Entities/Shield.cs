
using UnityEngine;

public enum EShieldContext
{
	NONE,
	ENTITY, WEAPON, MESH
}

[RequireComponent(typeof(Collider))]
public class Shield : MonoBehaviour//, IStreamableByEvents
{
	// Delegate definition
	public	delegate	void	ShieldHitEvent(Vector3 startPosition, Entity whoRef, WeaponBase weaponRef, EDamageType damageType, float damage, bool canPenetrate = false);

	// Internal Event
	protected		ShieldHitEvent	m_ShieldHitEvent		= delegate { };

	/// <summary> Event called when shield is hitted </summary>
	public		event ShieldHitEvent					OnHit
	{
		add		{ if ( value.IsNotNull() ) m_ShieldHitEvent += value; }
		remove	{ if ( value.IsNotNull() ) m_ShieldHitEvent -= value; }
	}

	[SerializeField]
	protected		bool		m_IsUnbreakable			= false;
	[SerializeField]
	private		EShieldContext	m_Context				= EShieldContext.NONE;
	[SerializeField]
	private		float			m_CurrentStatus			= 100f;
	[SerializeField]
	private		float			m_StartStatus			= 100f;
	[SerializeField]
	private		bool			m_bCanRegenerate		= true;
	[SerializeField]
	private		float			m_RegenerationSpeed		= 1.0f;

	public		EShieldContext	Context					=> m_Context;
	public		Collider		Collider				=> m_Collider;
	public		float			StartStatus				=> m_StartStatus;
	public		float			Status					=> m_CurrentStatus;
	public		bool			IsUnbreakable			=> m_IsUnbreakable;

	private		Collider		m_Collider				= null;
	private		Renderer		m_Renderer				= null;


	//////////////////////////////////////////////////////////////////////////
	private void Awake()
	{
		Utils.Base.TrySearchComponent(gameObject, ESearchContext.LOCAL, out m_Renderer);
		Utils.Base.TrySearchComponent(gameObject, ESearchContext.LOCAL, out m_Collider);

		CustomAssertions.IsNotNull(m_Renderer);
		CustomAssertions.IsNotNull(m_Collider);

		// First assignment
		ResetDelegate();
	}


	//////////////////////////////////////////////////////////////////////////
	private void ResetDelegate()
	{
		m_ShieldHitEvent = TakeDamage;
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnEnable()
	{
		m_Renderer.enabled = true;
		m_Collider.enabled = true;

		CustomAssertions.IsNotNull(GameManager.UpdateEvents);

		GameManager.UpdateEvents.OnFrame += OnFrame;
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnDisable()
	{
		m_Renderer.enabled = false;
		m_Collider.enabled = false;

		if (GameManager.UpdateEvents.IsNotNull())
		{
			GameManager.UpdateEvents.OnFrame -= OnFrame;
		}
	}

	/*
	//////////////////////////////////////////////////////////////////////////
	public bool OnSave(StreamData streamData, ref StreamUnit streamUnit)
	{
		bool bResult = streamData.TryGetUnit(gameObject, out streamUnit);
		if (bResult)
		{
			streamUnit.SetInternal("CurrentStatus", m_CurrentStatus);
		}
		else
		{
			enabled = false;
			ResetDelegate();
		}
		return bResult;
	}


	//////////////////////////////////////////////////////////////////////////
	public bool OnLoad(StreamData streamData, ref StreamUnit streamUnit)
	{
		bool bResult = streamData.TryGetUnit(gameObject, out streamUnit);
		if (bResult)
		{
			m_CurrentStatus = streamUnit.GetAsFloat("CurrentStatus");

			bool bIsActive = m_CurrentStatus > 0.0f;
			m_Renderer.enabled = bIsActive;
			m_Collider.enabled = bIsActive;
		}
		else
		{
			enabled = false;
			ResetDelegate();
			return false;
		}
		return bResult;
	}
	*/

	//////////////////////////////////////////////////////////////////////////
	public void Setup(float StartStatus, EShieldContext Context, bool IsUnbreakable = false)
	{
		m_StartStatus	= StartStatus;
		m_Context		= Context;
		m_IsUnbreakable = IsUnbreakable;
	}


	//////////////////////////////////////////////////////////////////////////
	public void SetRegenerationStatus(bool newStatus)
	{
		m_bCanRegenerate = true;
	}


	//////////////////////////////////////////////////////////////////////////
	public void SetRegenerationSpeed(float newSpeed)
	{
		m_RegenerationSpeed = Mathf.Max(0f, newSpeed);
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnFrame(float deltaTime)
	{
		if (m_bCanRegenerate)
		{
			m_CurrentStatus += Mathf.Clamp(m_CurrentStatus +  (m_RegenerationSpeed * deltaTime), 0f, m_StartStatus);
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public void OnReset()
	{
		m_Renderer.enabled = true;
		m_Collider.enabled = true;
		m_CurrentStatus = m_StartStatus;
	}


	//////////////////////////////////////////////////////////////////////////
	protected void TakeDamage(Vector3 startPosition, Entity whoRef, WeaponBase weaponRef, EDamageType damageType, float damage, bool canPenetrate)
	{
		if (!m_IsUnbreakable)
		{
			m_CurrentStatus -= damage;
			if (m_CurrentStatus <= 0.0f)
			{
				enabled = false;
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public void OnHittedDetails(GameObject collidingObject)
	{
		if (Utils.Base.TrySearchComponent(collidingObject, ESearchContext.LOCAL_AND_CHILDREN, out Bullet bullet))
		{
			var Interface = bullet as IBullet;
			m_ShieldHitEvent(Interface.StartPosition, Interface.WhoRef, Interface.Weapon, Interface.DamageType, Interface.Damage, Interface.CanPenetrate);
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnCollisionEnter(Collision collision)
	{
		if (Utils.Base.TrySearchComponent(collision.gameObject, ESearchContext.LOCAL_AND_CHILDREN, out Bullet bullet))
		{
			var Interface = bullet as IBullet;
			m_ShieldHitEvent(Interface.StartPosition, Interface.WhoRef, Interface.Weapon, Interface.DamageType, Interface.Damage, Interface.CanPenetrate);
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnTriggerEnter(Collider other)
	{
		if (Utils.Base.TrySearchComponent(other.gameObject, ESearchContext.LOCAL_AND_CHILDREN, out Bullet bullet))
		{
			var Interface = bullet as IBullet;
			m_ShieldHitEvent(Interface.StartPosition, Interface.WhoRef, Interface.Weapon, Interface.DamageType, Interface.Damage, Interface.CanPenetrate);
		}
	}
}
