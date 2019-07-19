
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
		get { return m_ZoomSensitivity; }
	}


	//////////////////////////////////////////////////////////////////////////
	public	override	bool	Setup			( IWeapon w, WeaponSlots slot )
	{
		string moduleSectionName = this.GetType().FullName;
		m_WeaponRef = w;
		if ( GlobalManager.Configs.bGetSection( moduleSectionName, ref m_ModuleSection ) == false )			// Get Module Section
			return false;

		if ( InternalSetup( m_ModuleSection ) == false )
			return false;

		return true;
	}


	//////////////////////////////////////////////////////////////////////////
	protected	override	bool	InternalSetup( Database.Section moduleSection )
	{
		Vector3 zoomOffset		= Vector3.zero;
		moduleSection.bAsVec3( "ZoomOffset", ref zoomOffset, Vector3.zero );
		float zoomFactor		= moduleSection.AsFloat( "ZoomFactor",		m_ZoomFactor );
		float zoomingTime		= moduleSection.AsFloat( "ZoomingTime",		m_ZoomFactor );
		float zoomSensitivity	= moduleSection.AsFloat( "ZoomSensitivity",	m_ZoomFactor );
		string FramePath		= moduleSection.AsString( "FramePath", "" );
		string ScopePath		= moduleSection.AsString( "ScopePath", "" );

		m_ZoomOffset			= zoomOffset;
		m_ZoomFactor			= zoomFactor;
		m_ZoomingTime			= zoomingTime;
		m_ZoomSensitivity		= zoomSensitivity;
		
		// Image frame
		if ( FramePath.Length > 0 )
		{
			ResourceManager.LoadedData<GameObject> imageData = new ResourceManager.LoadedData<GameObject>();
			System.Action<GameObject> onLoadSuccess = delegate( GameObject t )
			{
				Transform parent = UIManager.InGame.transform;
				if ( t && t.transform.HasComponent<Image>() )
				{
					m_ZoomFrame = Instantiate( t, parent: parent ).GetComponent<Image>();

				}
			};
			ResourceManager.LoadResourceAsync( FramePath, imageData, onLoadSuccess );

		}

		// Scope Prefab
		if ( ScopePath.Length > 0 )
		{
			Transform opticSpot = null;
			bool bHasSpot = transform.SearchChildWithName( "OpticSpot", ref opticSpot );
			if ( bHasSpot )
			{
				ResourceManager.LoadedData<GameObject> ScopeObject = new ResourceManager.LoadedData<GameObject>();
				System.Action<GameObject> onLoadSuccess = delegate( GameObject t )
				{
					if ( t.transform.HasComponent<Scope>() )
					{
						m_Scope = Instantiate( t, opticSpot ).GetComponent<Scope>();
						m_Scope.transform.localPosition = Vector3.zero;
						m_Scope.transform.localRotation = Quaternion.identity;
					}
				};
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
			WeaponManager.Instance.ZoomIn( m_WeaponRef.ZoomOffset, m_WeaponRef.ZoomFactor, m_WeaponRef.ZoomingTime, m_WeaponRef.ZoomSensitivity, null );
		}
	}


	private void OnDestroy()
	{
		if ( m_ZoomFrame )
		{
			Destroy( m_ZoomFrame.gameObject );
		}

		if ( m_Scope )
		{
			Destroy( m_Scope.gameObject );
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
			WeaponManager.Instance.ZoomIn( m_ZoomOffset, m_ZoomFactor, m_ZoomingTime, m_ZoomSensitivity, m_ZoomFrame );
		}
	}

}
