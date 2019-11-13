using UnityEngine;
using System.Collections;

public interface IWeaponAttachment {

	bool			IsActive { get; }
	bool			IsAttached { get; }

	void			SetActive( bool state );

	void			OnAttached();
	void			OnRemoved();

}

public abstract class WeaponAttachment : MonoBehaviour {

	protected	bool			m_IsActive		= false;
	protected	bool			m_IsAttached	= false;
	protected	bool			m_IsUsable		= true;

	public bool IsActive
	{
		get { return m_IsActive; }
	}

	public bool IsAttached
	{
		get { return m_IsAttached; }
	}

	protected abstract void OnActivate();
	protected abstract void OnDeactivated();

	//////////////////////////////////////////////////////////////////////////
	public	void	SetActive( bool state )
	{
		if ( m_IsUsable == false || m_IsAttached == false || ( state == m_IsActive ) )
			return;

		m_IsActive = state;

		if ( m_IsActive == true )
		{
			OnActivate();
		}
		else
		{
			OnDeactivated();
		}
	}


	public void OnAttached()
	{
		m_IsAttached = true;
	}
	public void OnRemoved()
	{
		m_IsAttached = false;
	}
}
