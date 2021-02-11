
using UnityEngine;
using UnityEngine.UI;

public class WPN_WeaponAttachment_Zoom : WPN_BaseWeaponAttachment
{
	protected	Vector3				m_ZoomOffset				= Vector3.zero;
	protected	float				m_ZoomFactor				= 2.0f;
	protected	float				m_ZoomingTime				= 1.0f;
	protected	float				m_ZoomSensitivityMultiplier	= 1.0f;
	protected	string				m_FramePath					= string.Empty;
	protected	string				m_Attachment_PrefabPath		= string.Empty;

	protected	Image				m_ZoomFrame					= null;
	protected	GameObject			m_ScopeModel				= null;

	public	virtual	float			ZoomSensitivityMultiplier	=> m_ZoomSensitivityMultiplier;
	

	//////////////////////////////////////////////////////////////////////////
	protected	void	Awake()
	{
		m_IsUsable = true;
	}


	//////////////////////////////////////////////////////////////////////////
	public override bool ConfigureInternal(in Database.Section attachmentSection)
	{
		m_ZoomOffset					= attachmentSection.AsVec3( "ZoomOffset", Vector3.zero );
		m_ZoomFactor					= attachmentSection.AsFloat( "ZoomFactor", m_ZoomFactor );
		m_ZoomingTime					= attachmentSection.AsFloat( "ZoomingTime", m_ZoomingTime );
		m_ZoomSensitivityMultiplier	= attachmentSection.AsFloat( "ZoomSensitivityMultiplier", m_ZoomSensitivityMultiplier );
		m_Attachment_PrefabPath		= attachmentSection.AsString( "Attachment_Prefab", null );
		
		if ( !string.IsNullOrEmpty(m_Attachment_PrefabPath) )
		{
			void onLoadSuccess(GameObject resource)
			{
				m_ScopeModel = Instantiate( resource, transform );
				m_ScopeModel.transform.localPosition = Vector3.zero;
				m_ScopeModel.transform.localRotation = Quaternion.identity;
				m_ZoomFrame = m_ScopeModel.transform.GetComponentInChildren<Image>(includeInactive: true);
			}
			ResourceManager.LoadResourceAsync<GameObject>( ResourcePath: m_Attachment_PrefabPath, LoadedResource: null, OnResourceLoaded: onLoadSuccess );
		}
		return true;
	}


	//////////////////////////////////////////////////////////////////////////
	protected sealed override void OnActivate()
	{
		if (m_IsUsable == false || m_IsAttached == false || WeaponManager.Instance.IsChangingZoom || m_WeaponRef.WeaponSubState == EWeaponSubState.RELOADING )
			return;

		if (!WeaponManager.Instance.IsZoomed && !WeaponManager.Instance.IsChangingWeapon)
		{
			m_ZoomFrame.transform.SetParent(UIManager.InGame.transform);
			OnActivateInternal();
		}

		m_IsActive = true;
	}


	//////////////////////////////////////////////////////////////////////////
	protected sealed override void OnDeactivated()
	{
		if (m_IsUsable == false || m_IsAttached == false || WeaponManager.Instance.IsChangingZoom || m_WeaponRef.WeaponSubState == EWeaponSubState.RELOADING )
			return;

		if (WeaponManager.Instance.IsZoomed && !WeaponManager.Instance.IsChangingWeapon)
		{
			m_ZoomFrame.transform.SetParent( null );
			OnDeactivatedInternal();
		}

		m_IsActive = false;
	}


	//////////////////////////////////////////////////////////////////////////
	protected virtual void OnActivateInternal()
	{
		IWeapon weapon = WeaponManager.Instance.CurrentWeapon;
		WeaponManager.Instance.ZoomIn(weapon.ZoomOffset, weapon.ZoomFactor, weapon.ZoomingTime, weapon.ZoomSensitivity, null );
	}


	//////////////////////////////////////////////////////////////////////////
	protected virtual void OnDeactivatedInternal()
	{
		WeaponManager.Instance.ZoomOut();
	}
	
}