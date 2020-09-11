
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


//////////////////////////////////////////////////////////////////////////
// WPN_WeaponModule_Zoom
public class WPN_WeaponModule_Zoom : WPN_BaseModule, IWPN_UtilityModule {

	protected	Vector3				m_ZoomOffset			= Vector3.zero;
	protected	float				m_ZoomFactor			= 2.0f;
	protected	float				m_ZoomingTime			= 1.0f;
	protected	float				m_ZoomSensitivity		= 1.0f;

	protected	Image				m_ZoomFrame				= null;
	protected	Scope				m_Scope					= null;


	public	virtual	float			ZoomSensitivity
	{
		get { return this.m_ZoomSensitivity; }
	}


	//////////////////////////////////////////////////////////////////////////
	public	override	bool	OnAttach			( IWeapon w, EWeaponSlots slot )
	{
		string moduleSectionName = this.GetType().FullName;
		this.m_WeaponRef = w;
		if ( GlobalManager.Configs.GetSection( moduleSectionName, ref this.m_ModuleSection ) == false )			// Get Module Section
			return false;

		if (this.InternalSetup(this.m_ModuleSection ) == false )
			return false;

		return true;
	}

	public override void OnDetach()
	{
		
	}


	//////////////////////////////////////////////////////////////////////////
	protected	override	bool	InternalSetup( Database.Section moduleSection )
	{
		Vector3 zoomOffset		= Vector3.zero;
		moduleSection.bAsVec3( "ZoomOffset", ref zoomOffset, Vector3.zero );
		float zoomFactor		= moduleSection.AsFloat( "ZoomFactor", this.m_ZoomFactor );
		float zoomingTime		= moduleSection.AsFloat( "ZoomingTime", this.m_ZoomFactor );
		float zoomSensitivity	= moduleSection.AsFloat( "ZoomSensitivity", this.m_ZoomFactor );
		string FramePath		= moduleSection.AsString( "FramePath", "" );
		string ScopePath		= moduleSection.AsString( "ScopePath", "" );

		this.m_ZoomOffset			= zoomOffset;
		this.m_ZoomFactor			= zoomFactor;
		this.m_ZoomingTime			= zoomingTime;
		this.m_ZoomSensitivity		= zoomSensitivity;

		// Image frame
		if ( FramePath.Length > 0 )
		{
			ResourceManager.LoadedData<GameObject> imageData = new ResourceManager.LoadedData<GameObject>();
			void onLoadSuccess(GameObject resource)
			{
				Transform parent = UIManager.InGame.transform;
				if ( resource && resource.transform.HasComponent<Image>() )
				{
					this.m_ZoomFrame = Instantiate( resource, parent: parent ).GetComponent<Image>();

				}
			}
			ResourceManager.LoadResourceAsync( FramePath, imageData, onLoadSuccess );

		}

		// Scope Prefab
		if ( ScopePath.Length > 0 )
		{
			Transform opticSpot = null;
			bool bHasSpot = this.transform.SearchChildWithName( "OpticSpot", ref opticSpot );
			if ( bHasSpot )
			{
				ResourceManager.LoadedData<GameObject> ScopeObject = new ResourceManager.LoadedData<GameObject>();
				void onLoadSuccess(GameObject resource)
				{
					if ( resource.transform.HasComponent<Scope>() )
					{
						this.m_Scope = Instantiate( resource, opticSpot ).GetComponent<Scope>();
						this.m_Scope.transform.localPosition = Vector3.zero;
						this.m_Scope.transform.localRotation = Quaternion.identity;
					}
				}
				ResourceManager.LoadResourceAsync( ScopePath, ScopeObject, onLoadSuccess );
			}
		}

		return true;
	}


	//		MODIFIERS
	//////////////////////////////////////////////////////////////////////////
	public override void ApplyModifier( Database.Section modifier )
	{
		// Do actions here

		base.ApplyModifier( modifier );
	}


	public	override	void	ResetBaseConfiguration()
	{
		// Do actions here

		base.ResetBaseConfiguration();
	}

	public	override	void	RemoveModifier( Database.Section modifier )
	{
		// Do Actions here

		base.RemoveModifier( modifier );
	}





	//////////////////////////////////////////////////////////////////////////
	public	override	bool	OnSave			( StreamUnit streamUnit )
	{
//		streamUnit.SetInternal( "zoomFactor",					m_ZoomFactor );
//		streamUnit.SetInternal( "ZoomingTime",					m_ZoomingTime );
//		streamUnit.SetInternal( "zoomSensitivity",				m_ZoomSensitivity );
		return true;
	}


	//////////////////////////////////////////////////////////////////////////
	public	override	bool	OnLoad			( StreamUnit streamUnit )
	{
//		m_ZoomFactor				= streamUnit.GetAsFloat( "ZoomFactor" );
//		m_ZoomingTime				= streamUnit.GetAsFloat( "ZoomingTime" );
//		m_ZoomSensitivity			= streamUnit.GetAsFloat( "ZoomSensitivity" );
		return true;
	}

	public	override	bool	CanChangeWeapon	() {  return true; }
	public	override	bool	CanBeUsed		() {  return Player.Instance.IsRunning == false; }
	public	override	void	OnWeaponChange	() { }
	public	override	bool	NeedReload		() { return false; }
	public	override	void	OnAfterReload	() { }
	public	override	void	InternalUpdate	( float DeltaTime ) { }


/*
public	struct ZoomWeaponData {
	public	float				ZoomFactor;
	public	float				ZoomTime;
	public	float				ZoomSensitivity;
}
*/
	

	/// <summary> Zoom toggle </summary>
	public override		void	OnStart()
	{
		if ( WeaponManager.Instance.IsZoomed )
		{
			WeaponManager.Instance.ZoomOut();
		}
		else
		{
			WeaponManager.Instance.ZoomIn(this.m_WeaponRef.ZoomOffset, this.m_WeaponRef.ZoomFactor, this.m_WeaponRef.ZoomingTime, this.m_WeaponRef.ZoomSensitivity, null );
		}
	}


	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (this.m_ZoomFrame )
		{
			Destroy(this.m_ZoomFrame.gameObject );
		}

		if (this.m_Scope )
		{
			Destroy(this.m_Scope.gameObject );
		}
	}

}





//////////////////////////////////////////////////////////////////////////
// WPN_WeaponModule_OpticZoom
public class WPN_WeaponModule_OpticZoom : WPN_WeaponModule_Zoom {

	public override void OnStart()
	{
		if ( WeaponManager.Instance.IsZoomed )
		{
			WeaponManager.Instance.ZoomOut();
		}
		else
		{
			WeaponManager.Instance.ZoomIn(this.m_ZoomOffset, this.m_ZoomFactor, this.m_ZoomingTime, this.m_ZoomSensitivity, this.m_ZoomFrame );
		}
	}

}
