
using UnityEngine;


public abstract class UI_BaseCrosshair : UI_Base, IStateDefiner
{
	protected			bool								m_IsInitialized						= false;
						bool								IStateDefiner.IsInitialized			=> m_IsInitialized;

	[SerializeField]
	private				float								m_MinValue							= 0.0f;

	[SerializeField]
	private				float								m_CurrentValue						= 0.0f;
	protected			float								CurrentValue						=> m_CurrentValue;

	[SerializeField]
	protected			float								m_EffectMult						= 1f;


	[SerializeField]
	private				uint								m_RefCount							= 0u;


	//////////////////////////////////////////////////////////////////////////
	public virtual void PreInit()
	{

	}

	//////////////////////////////////////////////////////////////////////////
	public virtual	void Initialize()
	{
		if (!m_IsInitialized)
		{
			if (CustomAssertions.IsTrue(GlobalManager.Configs.TryGetSection(GetType().FullName, out Database.Section section)))
			{
				m_EffectMult = section.AsFloat("EffectMult", m_EffectMult);
			}
			m_IsInitialized = true;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public virtual void ReInit()
	{

	}


	//////////////////////////////////////////////////////////////////////////
	public virtual bool Finalize()
	{
		return m_IsInitialized;
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnEnable()
	{
		CustomAssertions.IsTrue(m_IsInitialized);

		if (CustomAssertions.IsNotNull(GameManager.UpdateEvents))
		{
			GameManager.UpdateEvents.OnFrame += OnFrame;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnDisable()
	{
		if (GameManager.UpdateEvents.IsNotNull())
		{
			GameManager.UpdateEvents.OnFrame -= OnFrame;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public void AddRef()
	{
		++m_RefCount;
		if (m_RefCount > 0)
		{
			gameObject.SetActive(true);
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public void RemoveRef()
	{
		--m_RefCount;
		if (m_RefCount <= 0)
		{
			gameObject.SetActive(false);
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public void SetMin(float min)
	{
		m_MinValue = min;
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnFrame(float deltaTime)
	{
		var wpnPivot = WeaponPivot.Instance;
		m_CurrentValue = m_MinValue + wpnPivot.Dispersion.sqrMagnitude + wpnPivot.Deviation.sqrMagnitude;
		InternalUpdate();
	}

	protected abstract void InternalUpdate();

}
