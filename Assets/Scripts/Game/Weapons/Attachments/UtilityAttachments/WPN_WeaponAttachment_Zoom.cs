
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

	public	virtual	float			ZoomSensitivityMultiplier	=> this.m_ZoomSensitivityMultiplier;
	

	//////////////////////////////////////////////////////////////////////////
	protected	void	Awake()
	{
		this.m_IsUsable = true;
	}


	//////////////////////////////////////////////////////////////////////////
	public override bool Configure(in Database.Section attachmentSection)
	{
		this.m_ZoomOffset					= attachmentSection.AsVec3( "ZoomOffset", Vector3.zero );
		this.m_ZoomFactor					= attachmentSection.AsFloat( "ZoomFactor", this.m_ZoomFactor );
		this.m_ZoomingTime					= attachmentSection.AsFloat( "ZoomingTime", this.m_ZoomingTime );
		this.m_ZoomSensitivityMultiplier	= attachmentSection.AsFloat( "ZoomSensitivityMultiplier", this.m_ZoomSensitivityMultiplier );
		this.m_Attachment_PrefabPath		= attachmentSection.AsString( "Attachment_Prefab", null );
		
		if ( !string.IsNullOrEmpty(this.m_Attachment_PrefabPath) )
		{
			void onLoadSuccess(GameObject resource)
			{
				this.m_ScopeModel = Instantiate( resource, this.transform );
				this.m_ScopeModel.transform.localPosition = Vector3.zero;
				this.m_ScopeModel.transform.localRotation = Quaternion.identity;
				this.m_ZoomFrame = this.m_ScopeModel.transform.GetComponentInChildren<Image>(includeInactive: true);
			}
			ResourceManager.LoadResourceAsync<GameObject>( ResourcePath: this.m_Attachment_PrefabPath, LoadedResource: null, OnResourceLoaded: onLoadSuccess );
			
		}
		return true;
	}


	//////////////////////////////////////////////////////////////////////////
	protected sealed override void OnActivate()
	{
		if (this.m_IsUsable == false || this.m_IsAttached == false || WeaponManager.Instance.IsChangingZoom )
			return;

		if (!WeaponManager.Instance.IsZoomed && !WeaponManager.Instance.IsChangingWeapon)
		{
			this.m_ZoomFrame.transform.SetParent(UIManager.InGame.transform);
			this.OnActivateInternal();
		}

		this.m_IsActive = true;
	}


	//////////////////////////////////////////////////////////////////////////
	protected sealed override void OnDeactivated()
	{
		if (this.m_IsUsable == false || this.m_IsAttached == false || WeaponManager.Instance.IsChangingZoom )
			return;

		if (WeaponManager.Instance.IsZoomed && !WeaponManager.Instance.IsChangingWeapon)
		{
			this.m_ZoomFrame.transform.SetParent( null );
			this.OnDeactivatedInternal();
		}

		this.m_IsActive = false;
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