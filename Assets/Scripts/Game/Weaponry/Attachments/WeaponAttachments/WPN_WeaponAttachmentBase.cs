using UnityEngine;

public interface IWeaponAttachment
{
	bool			IsActive { get; }
	bool			IsAttached { get; }

	void			SetActive(bool state);

	bool			Configure(in Database.Section attachmentSection, in IWeapon weaponRef);
	void			OnAttach();
	void			OnDetach();
}

public abstract partial class WPN_WeaponAttachmentBase : MonoBehaviour, IWeaponAttachment
{
	[SerializeField, ReadOnly]
	protected	bool			m_IsActive		= false;
	[SerializeField, ReadOnly]
	protected	bool			m_IsAttached	= false;
	[SerializeField, ReadOnly]
	protected	bool			m_IsUsable		= true;

	protected	IWeapon			m_WeaponRef		= null;

	public bool IsActive						=> m_IsActive;
	public bool IsAttached						=> m_IsAttached;

	//////////////////////////////////////////////////////////////////////////
	protected abstract void OnActivate();

	//////////////////////////////////////////////////////////////////////////
	protected abstract void OnDeactivated();

	//////////////////////////////////////////////////////////////////////////
	public bool Configure(in Database.Section attachmentSection, in IWeapon weaponRef)
	{
		m_WeaponRef = weaponRef;
		return ConfigureInternal( attachmentSection );
	}

	//////////////////////////////////////////////////////////////////////////
	protected abstract bool ConfigureInternal(in Database.Section attachmentSection);


	//////////////////////////////////////////////////////////////////////////
	public	void	SetActive( bool state )
	{
		if (m_IsUsable == false || m_IsAttached == false || ( state == m_IsActive ) )
			return;

		m_IsActive = state;

		if (m_IsActive == true )
		{
			OnActivate();
		}
		else
		{
			OnDeactivated();
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public void OnAttach()
	{
		m_IsAttached = true;
	}


	//////////////////////////////////////////////////////////////////////////
	public void OnDetach()
	{
		m_IsAttached = false;
	}
}
