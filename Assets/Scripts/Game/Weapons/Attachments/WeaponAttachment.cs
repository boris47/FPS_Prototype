using UnityEngine;
using System.Collections;

public interface IWeaponAttachment {

	bool			IsActive { get; }
	bool			IsAttached { get; }

	void			OnActivate();
	void			OnDeactivated();
	void			SetActive( bool state );

	void			OnAttached();
	void			OnRemoved();

}

public abstract class WeaponAttachment : MonoBehaviour {

	protected	bool			m_IsActive		= false;
	protected	bool			m_IsAttached	= false;
	protected	bool			m_IsUsable		= false;

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

	public void OnAttached()
	{
		m_IsAttached = true;
	}
	public void OnRemoved()
	{
		m_IsAttached = false;
	}
}
