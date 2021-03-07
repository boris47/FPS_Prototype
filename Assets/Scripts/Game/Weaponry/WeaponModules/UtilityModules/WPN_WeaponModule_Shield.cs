
using UnityEngine;

public class WPN_WeaponModule_Shield : WPN_BaseModule, IWPN_UtilityModule
{
	protected	float				m_ShieldLife					= 1.0f;

	protected	Rigidbody			m_RigidBody						= null;
	protected	Shield				m_Shield						= null;
	protected	GameObject			m_ShieldGO						= null;

	protected	float				m_TimeToWaitBeforeRestore		= 0.0f;
	protected	float				m_RestorationSpeed				= 5.0f;


	//////////////////////////////////////////////////////////////////////////
	public	override	bool	OnAttach			( IWeapon w, EWeaponSlots slot )
	{
		m_WeaponRef = w;

		string moduleSectionName = GetType().FullName;
		if (!GlobalManager.Configs.TryGetSection( moduleSectionName, out m_ModuleSection ))
		{
			return false;
		}

		m_ShieldLife = m_ModuleSection.AsFloat("BaseShieldLife", 50f);

		if (m_ModuleSection.TryAsString("Module_Prefab", out string modulePrefabPath))
		{
			GameObject modulePrefab = Resources.Load<GameObject>(modulePrefabPath);
			if ( modulePrefab )
			{
				m_ShieldGO = Instantiate<GameObject>(modulePrefab, transform);
				m_ShieldGO.transform.localPosition = Vector3.zero;
				m_ShieldGO.transform.localRotation = Quaternion.identity;

				m_RigidBody	= m_ShieldGO.GetComponentInChildren<Rigidbody>();
				m_Shield	= m_ShieldGO.GetComponentInChildren<Shield>();

				UnityEngine.Assertions.Assert.IsNotNull(m_RigidBody);
				UnityEngine.Assertions.Assert.IsNotNull(m_Shield);

				m_Shield.Setup(m_ShieldLife, EShieldContext.WEAPON);
				m_Shield.OnHit += OnShieldHit;
				m_Shield.enabled = false;
			}
		}

		return InternalSetup(m_ModuleSection);
	}

	public override void OnDetach()
	{
		
	}


	//////////////////////////////////////////////////////////////////////////
	protected	override	bool	InternalSetup( Database.Section moduleSection )
	{
		return true;
	}


	//		MODIFIERS
	//////////////////////////////////////////////////////////////////////////
	public override void ApplyModifier( Database.Section modifier )
	{
		// Do actions here

		base.ApplyModifier( modifier );
	}


	public	override	void	ResetBaseConfiguration()
	{
		// Do actions here

		base.ResetBaseConfiguration();
	}

	public	override	void	RemoveModifier( Database.Section modifier )
	{
		// Do Actions here

		base.RemoveModifier( modifier );
	}


		//////////////////////////////////////////////////////////////////////////
	public	override	bool	OnSave			(StreamUnit streamUnit)
	{
		streamUnit.SetInternal("TimeToWaitBeforeRestore", m_TimeToWaitBeforeRestore);
		streamUnit.SetInternal("RestorationSpeed", m_RestorationSpeed);
		return true;
	}


	//////////////////////////////////////////////////////////////////////////
	public	override	bool	OnLoad			(StreamUnit streamUnit)
	{
		m_TimeToWaitBeforeRestore = streamUnit.GetAsFloat("TimeToWaitBeforeRestore");
		m_RestorationSpeed = streamUnit.GetAsFloat("RestorationSpeed");
		return true;
	}
	
	public	override	bool	CanChangeWeapon	() {  return true; }
	public	override	bool	CanBeUsed		() {  return true; }
	public	override	void	OnWeaponChange	() { }
	public	override	bool	NeedReload		() { return false; }
	public	override	void	OnAfterReload	() { }


	//////////////////////////////////////////////////////////////////////////
	protected	override	void	InternalUpdate	( float deltaTime )
	{
		if (m_TimeToWaitBeforeRestore > 0.0f )
		{
			m_TimeToWaitBeforeRestore -= deltaTime;
			return;
		}

		if (!m_Shield.enabled)
		{
			m_Shield.SetRegenerationStatus(true);
			m_Shield.SetRegenerationSpeed(m_RestorationSpeed);
			m_Shield.enabled = true;
		}
		if (!m_RigidBody.detectCollisions)
		{
			m_RigidBody.detectCollisions = true;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private	void		OnShieldHit(Vector3 startPosition, Entity whoRef, Weapon weaponRef, EDamageType damageType, float damage, bool canPenetrate = false)
	{
		if (m_Shield.Status <= 0.0f )
		{
			m_TimeToWaitBeforeRestore = 3.0f;
			m_Shield.enabled = false;
			m_RigidBody.detectCollisions = false;
		}

		TimersManager.Instance.AddTimerScaled(3f, () =>
		{
			m_Shield.SetRegenerationStatus(true);
			m_Shield.SetRegenerationSpeed(m_RestorationSpeed);

			m_Shield.enabled = true;
			m_RigidBody.detectCollisions = true;
		});
	}


	//////////////////////////////////////////////////////////////////////////
	public override void OnEnd()
	{
		m_Shield.enabled = false;
		m_RigidBody.detectCollisions = false;
	}


	//////////////////////////////////////////////////////////////////////////
	protected void OnDestroy()
	{
		Destroy(m_ShieldGO);
	}
}
