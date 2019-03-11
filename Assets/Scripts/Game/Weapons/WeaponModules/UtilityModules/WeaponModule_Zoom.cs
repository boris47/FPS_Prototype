
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
		if ( GameManager.Configs.bGetSection( moduleSectionName, ref m_ModuleSection ) == false )			// Get Module Section
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
		string FramePath		= moduleSection.AsString( "FramePath", null );
		string ScopePath		= moduleSection.AsString( "ScopePath", null );

		m_ZoomOffset			= zoomOffset;
		m_ZoomFactor			= zoomFactor;
		m_ZoomingTime			= zoomingTime;
		m_ZoomSensitivity		= zoomSensitivity;
		
		if ( FramePath.IsNotNull() )
		{
			GameObject go = Resources.Load<GameObject>( FramePath );
			if ( go )
			{
				m_ZoomFrame = Instantiate( go, UI.Instance.InGame.transform ).GetComponent<Image>();
			}
		}

		if ( ScopePath.IsNotNull() )
		{
			GameObject go = Resources.Load<GameObject>( ScopePath );
			if ( go )
			{
				if ( go.transform.HasComponent<Scope>() )
				{
					Transform receiver = transform.Find( "car15_reciever" );

					m_Scope = Instantiate( go, receiver ).GetComponent<Scope>();
//					m_Scope.transform.localPosition = new Vector3( -0.0869f, -0.008f, -0.0281f );
//					m_Scope.transform.localRotation = Quaternion.Euler( -90f, -90f, 0.0f );
				}
			}
		}
		
		/*
		ResourceManager.LoadData<GameObject> imageData = new ResourceManager.LoadData<GameObject>();
		System.Action<GameObject> onLoadSuccess = delegate( GameObject t )
		{
			m_ZoomFrame = t.GetComponent<Image>();
		};
		ResourceManager.LoadResourceAsync( FramePath, imageData, onLoadSuccess );
		*/

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
