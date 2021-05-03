using UnityEngine;
using UnityEngine.UI;

public class WPN_WeaponAttachment_Zoom_Base : WPN_WeaponAttachmentBase
{
	[SerializeField]
	protected			 float				m_ZoomFactor						= 2.0f;
	[SerializeField]
	protected			 float				m_ZoomingTime						= 1.0f;
	[SerializeField]
	protected			 float				m_ZoomSensitivityMultiplier			= 1.0f;
	[SerializeField, ReadOnly]
	protected			 string				m_FramePath							= string.Empty;
	[SerializeField, ReadOnly]
	protected			 string				m_Attachment_PrefabPath				= string.Empty;
	[SerializeField, ReadOnly]
	protected			Image				m_ZoomFrame							= null;
	[SerializeField, ReadOnly]
	protected			GameObject			m_AttachmentGO						= null;

	protected			Vector3				m_ZoomOffset						= Vector3.zero;

	public		virtual	float				ZoomSensitivityMultiplier			=> m_ZoomSensitivityMultiplier;


	//////////////////////////////////////////////////////////////////////////
	protected void Awake()
	{
		m_IsUsable = true;
	}


	//////////////////////////////////////////////////////////////////////////
	protected override bool ConfigureInternal(in Database.Section attachmentSection)
	{
		CustomAssertions.IsTrue(attachmentSection.TryAsVec3("ZoomOffset", out m_ZoomOffset));
		CustomAssertions.IsTrue(attachmentSection.TryAsFloat("ZoomFactor", out m_ZoomFactor));
		CustomAssertions.IsTrue(attachmentSection.TryAsFloat("ZoomingTime", out m_ZoomingTime));
		CustomAssertions.IsTrue(attachmentSection.TryAsFloat("ZoomSensitivityMultiplier", out m_ZoomSensitivityMultiplier));

		m_FramePath = attachmentSection.AsString("ZoomFrame", null);
		m_Attachment_PrefabPath = attachmentSection.AsString("Attachment_Prefab", null);
	
		if (!string.IsNullOrEmpty(m_Attachment_PrefabPath))
		{
			void OnResourceLoaded(GameObject resource)
			{
				m_AttachmentGO = Instantiate(resource, transform);
				m_AttachmentGO.transform.localPosition = Vector3.zero;
				m_AttachmentGO.transform.localRotation = Quaternion.identity;
				m_ZoomFrame = m_AttachmentGO.transform.GetComponentInChildren<Image>(includeInactive: true);
			}
			ResourceManager.LoadResourceAsync<GameObject>(m_Attachment_PrefabPath, OnResourceLoaded);
		}
		return true;
	}


	//////////////////////////////////////////////////////////////////////////
	protected sealed override void OnActivate()
	{
		if (!m_IsUsable || !m_IsAttached || WeaponManager.Instance.IsChangingZoom || m_WeaponRef.WeaponSubState == EWeaponSubState.RELOADING)
			return;

		if (!WeaponManager.Instance.IsZoomed && !WeaponManager.Instance.IsChangingWeapon)
		{
			m_ZoomFrame?.transform.SetParent(UIManager.InGame.transform);
			OnActivateInternal();
		}

		m_IsActive = true;
	}


	//////////////////////////////////////////////////////////////////////////
	protected sealed override void OnDeactivated()
	{
		if (!m_IsUsable || !m_IsAttached || WeaponManager.Instance.IsChangingZoom || m_WeaponRef.WeaponSubState == EWeaponSubState.RELOADING)
			return;

		if (WeaponManager.Instance.IsZoomed && !WeaponManager.Instance.IsChangingWeapon)
		{
			m_ZoomFrame?.transform.SetParent(null);
			OnDeactivatedInternal();
		}

		m_IsActive = false;
	}


	//////////////////////////////////////////////////////////////////////////
	protected virtual void OnActivateInternal()
	{
		IWeapon weapon = WeaponManager.Instance.CurrentWeapon;
		WeaponManager.Instance.ZoomIn(weapon.ZoomOffset, weapon.ZoomFactor, weapon.ZoomingTime, weapon.ZoomSensitivity, null);
	}


	//////////////////////////////////////////////////////////////////////////
	protected virtual void OnDeactivatedInternal()
	{
		WeaponManager.Instance.ZoomOut();
	}

}
