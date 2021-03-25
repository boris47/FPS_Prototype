﻿

using UnityEngine;
using UnityEngine.UI;


//////////////////////////////////////////////////////////////////////////
public class WPN_WeaponModule_Zoom : WPN_BaseModule, IWPN_UtilityModule
{
	protected				Vector3									m_ZoomOffset				= Vector3.zero;
	protected				float									m_ZoomFactor				= 2.0f;
	protected				float									m_ZoomingTime				= 1.0f;
	protected				float									m_ZoomSensitivity			= 1.0f;

	protected				Image									m_ZoomFrame					= null;
	protected				WPN_WeaponAttachment_Zoom				m_Scope						= null;


	public		virtual		float									ZoomSensitivity				=> m_ZoomSensitivity;


	//////////////////////////////////////////////////////////////////////////
	public	override	bool	OnAttach			( IWeapon w, EWeaponSlots slot )
	{
		string moduleSectionName = GetType().FullName;
		m_WeaponRef = w;
		if (!GlobalManager.Configs.TryGetSection( moduleSectionName, out m_ModuleSection ))			// Get Module Section
			return false;

		if (InternalSetup(m_ModuleSection ) == false )
			return false;

		m_WeaponRef.Attachments.AddAttachment(System.Type.GetType(moduleSectionName));

		return true;
	}

	public override void OnDetach()
	{
		
	}


	//////////////////////////////////////////////////////////////////////////
	protected	override	bool	InternalSetup( Database.Section moduleSection )
	{
		Vector3 zoomOffset		= Vector3.zero;
		moduleSection.TryAsVec3( "ZoomOffset", out zoomOffset, Vector3.zero );
		float zoomFactor		= moduleSection.AsFloat( "ZoomFactor", m_ZoomFactor );
		float zoomingTime		= moduleSection.AsFloat( "ZoomingTime", m_ZoomFactor );
		float zoomSensitivity	= moduleSection.AsFloat( "ZoomSensitivity", m_ZoomFactor );
		string FramePath		= moduleSection.AsString( "FramePath", "" );

		m_ZoomOffset			= zoomOffset;
		m_ZoomFactor			= zoomFactor;
		m_ZoomingTime			= zoomingTime;
		m_ZoomSensitivity		= zoomSensitivity;

		// Image frame
		if (!string.IsNullOrEmpty(FramePath))
		{
			void onLoadSuccess(GameObject resource)
			{
				Transform parent = UIManager.InGame.transform;
				if (resource && resource.transform.HasComponent<Image>())
				{
					m_ZoomFrame = Instantiate(resource, parent: parent).GetComponent<Image>();

				}
			}
			ResourceManager.LoadResourceAsync<GameObject>(FramePath, onLoadSuccess);
		}

		return true;
	}


	//////////////////////////////////////////////////////////////////////////
	public override void ApplyModifier(Database.Section modifier)
	{
		// Do actions here

		base.ApplyModifier(modifier);
	}


	//////////////////////////////////////////////////////////////////////////
	public override void ResetBaseConfiguration()
	{
		// Do actions here

		base.ResetBaseConfiguration();
	}


	//////////////////////////////////////////////////////////////////////////
	public override void RemoveModifier(Database.Section modifier)
	{
		// Do Actions here

		base.RemoveModifier(modifier);
	}


	//////////////////////////////////////////////////////////////////////////
	public override bool OnSave(StreamUnit streamUnit)
	{
		streamUnit.SetInternal("zoomFactor", m_ZoomFactor);
		streamUnit.SetInternal("ZoomingTime", m_ZoomingTime);
		streamUnit.SetInternal("zoomSensitivity", m_ZoomSensitivity);
		return true;
	}


	//////////////////////////////////////////////////////////////////////////
	public override bool OnLoad(StreamUnit streamUnit)
	{
		m_ZoomFactor = streamUnit.GetAsFloat("ZoomFactor");
		m_ZoomingTime = streamUnit.GetAsFloat("ZoomingTime");
		m_ZoomSensitivity = streamUnit.GetAsFloat("ZoomSensitivity");
		return true;
	}

	public	override	bool	CanChangeWeapon	() {  return true; }
	public	override	bool	CanBeUsed		() {  return !Player.Instance.Motion.MotionStrategy.States.IsRunning; }
	public	override	void	OnWeaponChange	() { }
	public	override	bool	NeedReload		() { return false; }
	public	override	void	OnAfterReload	() { }
	protected	override	void	InternalUpdate	( float DeltaTime ) { }


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Zoom toggle </summary>
	public override void OnStart()
	{
		if (WeaponManager.Instance.IsZoomed)
		{
			WeaponManager.Instance.ZoomOut();
		}
		else
		{
			WeaponManager.Instance.ZoomIn(m_WeaponRef.ZoomOffset, m_WeaponRef.ZoomFactor, m_WeaponRef.ZoomingTime, m_WeaponRef.ZoomSensitivity, null);
		}
	}

	//////////////////////////////////////////////////////////////////////////
	private void OnDestroy()
	{
		if (m_ZoomFrame)
		{
			Destroy(m_ZoomFrame.gameObject);
		}

		if (m_Scope)
		{
			Destroy(m_Scope.gameObject);
		}
	}
}





//////////////////////////////////////////////////////////////////////////
public class WPN_WeaponModule_OpticZoom : WPN_WeaponModule_Zoom
{
	public override void OnStart()
	{
		if (WeaponManager.Instance.IsZoomed)
		{
			WeaponManager.Instance.ZoomOut();
		}
		else
		{
			WeaponManager.Instance.ZoomIn(m_ZoomOffset, m_ZoomFactor, m_ZoomingTime, m_ZoomSensitivity, m_ZoomFrame);
		}
	}

}

