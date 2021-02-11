
using UnityEngine;
using System.Collections;


public abstract class UI_BaseCrosshair : MonoBehaviour, IStateDefiner
{
	[SerializeField]
	private		float	m_MinValue			= 0.0f;

	[SerializeField]
	private		float	m_CurrentValue		= 0.0f;
	protected	float	CurrentValue		=> m_CurrentValue;

	[SerializeField]
	protected	float	m_EffectMult		= 1f;


	[SerializeField]
	private		uint	m_RefCount			= 0u;


	public void AddRef()
	{
		++m_RefCount;
		if ( m_RefCount >= 0 )
		{
			gameObject.SetActive( true );
		}
	}

	public void RemoveRef()
	{
		--m_RefCount;
		if ( m_RefCount <= 0 )
		{
			if (gameObject != null)
			{
				gameObject.SetActive( false );
			}
		}
	}

	// IStateDefiner START
	public			string		StateName		=> name;
	public			bool		IsInitialized	{ get; protected set; } = false;

	//-------------------------------------
	public			void		PreInit			() { m_EffectMult = 1.0f; }
	//-------------------------------------
	public virtual	IEnumerator Initialize		()
	{
		if ( IsInitialized &= GlobalManager.Configs.TryGetSection( GetType().FullName, out Database.Section section ) )
		{
			m_EffectMult = section.AsFloat( "EffectMult", m_EffectMult );
		}
		yield return null;
	}
	//-------------------------------------
	public virtual	IEnumerator	ReInit			() => null;
	
	//-------------------------------------
	public virtual	bool		Finalize		() => true;
	// IStateDefiner END
	

	//////////////////////////////////////////////////////////////////////////
	public void SetMin(float min)
	{
		m_MinValue = min;
	}


	//////////////////////////////////////////////////////////////////////////
	private void Update()
	{
		if ( !IsInitialized ) return;

		IWeapon currentWeapon = WeaponManager.Instance.CurrentWeapon;
		m_CurrentValue = m_MinValue + currentWeapon.Dispersion.sqrMagnitude + currentWeapon.Deviation.sqrMagnitude;
		InternalUpdate();
	}

	protected abstract void InternalUpdate();

}
