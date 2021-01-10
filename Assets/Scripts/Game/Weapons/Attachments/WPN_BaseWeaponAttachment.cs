using UnityEngine;
using System.Collections;

public interface IWeaponAttachment
{
	bool			IsActive { get; }
	bool			IsAttached { get; }

	void			SetActive(bool state);

	bool			Configure(in Database.Section attachmentSection, in IWeapon weaponRef);
	void			OnAttach();
	void			OnDetach();
}

public abstract class WPN_BaseWeaponAttachment : MonoBehaviour, IWeaponAttachment, IModifiable
{
	protected	bool			m_IsActive		= false;
	protected	bool			m_IsAttached	= false;
	protected	bool			m_IsUsable		= true;
	protected	IWeapon			m_WeaponRef		= null;


	protected abstract void OnActivate();
	protected abstract void OnDeactivated();

	// IModifiable
	public		virtual		void	ApplyModifier			( Database.Section modifier )	{ }
	public		virtual		void	ResetBaseConfiguration	()								{ }
	public		virtual		void	RemoveModifier			( Database.Section modifier )	{ }
	

	// IWeaponAttachment
	public bool IsActive						=> m_IsActive;
	public bool IsAttached						=> m_IsAttached;
	public bool Configure(in Database.Section attachmentSection, in IWeapon weaponRef)
	{
		m_WeaponRef = weaponRef;
		return ConfigureInternal( attachmentSection );
	}
	public abstract bool ConfigureInternal(in Database.Section attachmentSection);


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
