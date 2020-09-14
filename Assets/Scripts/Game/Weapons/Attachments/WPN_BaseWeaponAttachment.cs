using UnityEngine;
using System.Collections;

public interface IWeaponAttachment
{
	bool			IsActive { get; }
	bool			IsAttached { get; }

	void			SetActive(bool state);

	bool			Configure(in Database.Section attachmentSection);
	void			OnAttach();
	void			OnDetach();
}

public abstract class WPN_BaseWeaponAttachment : MonoBehaviour, IWeaponAttachment, IModifiable
{
	protected	bool			m_IsActive		= false;
	protected	bool			m_IsAttached	= false;
	protected	bool			m_IsUsable		= true;


	protected abstract void OnActivate();
	protected abstract void OnDeactivated();

	// IModifiable
	public		virtual		void	ApplyModifier			( Database.Section modifier )	{ }
	public		virtual		void	ResetBaseConfiguration	()								{ }
	public		virtual		void	RemoveModifier			( Database.Section modifier )	{ }
	

	// IWeaponAttachment
	public bool IsActive						=> this.m_IsActive;
	public bool IsAttached						=> this.m_IsAttached;
	public abstract bool Configure(in Database.Section attachmentSection);


	//////////////////////////////////////////////////////////////////////////
	public	void	SetActive( bool state )
	{
		if (this.m_IsUsable == false || this.m_IsAttached == false || ( state == this.m_IsActive ) )
			return;

		this.m_IsActive = state;

		if (this.m_IsActive == true )
		{
			this.OnActivate();
		}
		else
		{
			this.OnDeactivated();
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public void OnAttach()
	{
		this.m_IsAttached = true;
	}


	//////////////////////////////////////////////////////////////////////////
	public void OnDetach()
	{
		this.m_IsAttached = false;
	}
}
