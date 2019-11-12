using UnityEngine;
using System.Collections;

public interface IWeaponAttachment {

	WPN_BaseModule	WpnModule { get; }
	bool			IsActive { get; }
	bool			IsAttached { get; }

	void			OnActivate();
	void			OnDeactivated();
	void			SetActive( bool state );

	void			OnAttached( WPN_BaseModule wpnModule );
	void			OnRemoved();

}

public abstract class WeaponAttachment : MonoBehaviour, IWeaponAttachment {

	protected	WPN_BaseModule	m_WpnModule		= null;
	protected	bool			m_IsActive		= false;
	protected	bool			m_IsAttached	= false;
	protected	bool			m_IsUsable		= false;

	public WPN_BaseModule WpnModule
	{
		get { return m_WpnModule; }
	}

	public bool IsActive
	{
		get { return m_IsActive; }
	}

	public bool IsAttached
	{
		get { return m_IsAttached; }
	}

	public abstract void OnActivate();
	public abstract void SetActive( bool state );
	public abstract void OnDeactivated();

	public void OnAttached( WPN_BaseModule wpnModule )
	{
		m_WpnModule = wpnModule;
		m_IsAttached = true;
	}
	public void OnRemoved()
	{
		m_WpnModule = null;
		m_IsAttached = false;
	}
}
