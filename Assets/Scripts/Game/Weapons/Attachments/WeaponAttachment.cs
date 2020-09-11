using UnityEngine;
using System.Collections;

public interface IWeaponAttachment
{
	bool			IsActive { get; }
	bool			IsAttached { get; }

	void			SetActive( bool state );

	void			OnAttached();
	void			OnRemoved();
}

public abstract class WeaponAttachment : MonoBehaviour
{
	protected	bool			m_IsActive		= false;
	protected	bool			m_IsAttached	= false;
	protected	bool			m_IsUsable		= true;

	public bool IsActive
	{
		get { return this.m_IsActive; }
	}

	public bool IsAttached
	{
		get { return this.m_IsAttached; }
	}

	protected abstract void OnActivate();
	protected abstract void OnDeactivated();

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


	public void OnAttached()
	{
		this.m_IsAttached = true;
	}
	public void OnRemoved()
	{
		this.m_IsAttached = false;
	}
}
