
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IWPN_FireModule {

	FireModes				FireMode						{ get; }

	float					Damage							{ get; }
	uint					Magazine						{ get; }
	uint					MagazineCapacity				{ get; }

	float					CamDeviation					{ get; }
	float					FireDispersion					{ get; }

	bool					NeedReload						();
	bool					ChangeFireMode					( string FireMode );
	bool					ChangeFireMode				<T>	();
}


//////////////////////////////////////////////////////////////////////////
// WPN_BaseModule ( Abstract )
/// <summary> Abstract base class for weapon modules </summary>
[System.Serializable]
public abstract class WPN_BaseModule : MonoBehaviour {

	public	abstract	bool	Setup			( IWeapon w );

	//////////////////////////////////////////////////////////////////////////
	protected	abstract	bool	InternalSetup( Database.Section moduleSection );


	//		MODIFIERS
	//////////////////////////////////////////////////////////////////////////
	public		virtual		void	ApplyModifier( Database.Section modifier )
	{ }

	public		virtual		void	RestoreModuleProperties()
	{ }

	public		virtual		void	RemoveModifier( Database.Section modifier )
	{ }

	public	abstract	bool	OnSave			( StreamUnit streamUnit );
	public	abstract	bool	OnLoad			( StreamUnit streamUnit );

	public	abstract	bool	CanChangeWeapon	();
	public	abstract	bool	CanBeUsed		();
	public	abstract	void	OnWeaponChange	();

	public	abstract	bool	NeedReload		();
	public	abstract	void	OnAfterReload	();

	public	abstract	void	InternalUpdate( float DeltaTime );

	//
	public	virtual		void	OnStart		()	{ }
	public	virtual		void	OnUpdate	()	{ }
	public	virtual		void	OnEnd		()	{ }

}


//////////////////////////////////////////////////////////////////////////
// WPN_BaseModuleEmpty
/// <summary> Concrete class for empty weapon modules </summary>
[System.Serializable]
public class WPN_BaseModuleEmpty : WPN_BaseModule {

	public	override	bool	Setup			( IWeapon w ) { return true; }
	protected	override	bool	InternalSetup( Database.Section moduleSection ) { return true; }

	public	override	bool	OnSave			( StreamUnit streamUnit ) { return true; }
	public	override	bool	OnLoad			( StreamUnit streamUnit ) {	return true; }

	public	override	bool	CanChangeWeapon	() {  return true; }
	public	override	bool	CanBeUsed		() {  return true; }
	public	override	void	OnWeaponChange	() { }
	public	override	void	InternalUpdate	( float DeltaTime ) { }
	public	override	bool	NeedReload		() { return false; }
	public	override	void	OnAfterReload	() { }

	//
	public	override	void	OnStart		()	{ }
	public	override	void	OnUpdate	()	{ }
	public	override	void	OnEnd		()	{ }

}


//////////////////////////////////////////////////////////////////////////
// WPN_FireModule  ( Abstract )
/// <summary> Abstract base class for fire modules </summary>
[System.Serializable]
public abstract class WPN_FireModule : WPN_BaseModule, IWPN_FireModule {

	private	static		AudioCollection							m_ModuleSounds				= null;

	[SerializeField]	protected	Transform					m_FirePoint					= null;
	[SerializeField]	protected	uint						m_Magazine					= 0;
	[SerializeField]	protected	uint						m_MagazineCapacity			= 1;
	[SerializeField]	protected	float						m_Damage					= 0.0f;
	[SerializeField]	protected	float						m_ShotDelay					= 0.5f;
	[SerializeField]	protected	float						m_CamDeviation				= 0.02f;
	[SerializeField]	protected	float						m_FireDispersion			= 0.05f;
	[SerializeField]	protected	Database.Section			m_ModuleSection				= null;
	[SerializeField]	protected	WPN_FireMode_Base			m_WpnFireMode				= new WPN_FireMode_Empty(null);

	// INTERFACE START
	public abstract	FireModes									FireMode					{ get; }
	public			Vector3										FirePointPosition			{ get { return m_FirePoint.position; } } // TODO Assign m_FirePoint
	public			Quaternion									FirePointRotation			{ get { return m_FirePoint.rotation; } }
	public			uint										Magazine					{ get { return m_Magazine; } }
	public			uint										MagazineCapacity			{ get { return m_MagazineCapacity; } }
	public			float										Damage						{ get { return m_Damage; } }

	public			float										CamDeviation				{ get { return m_CamDeviation; } }
	public			float										FireDispersion				{ get { return m_FireDispersion; } }
	// INTERFACE END

	protected		GameObjectsPool<Bullet>						m_PoolBullets				= null;
	protected		bool										m_Initialized				= false;
	protected		CustomAudioSource							m_AudioSourceFire			= null; // TODO Create audio
	protected		IWeapon										m_WeaponRef					= null;
	protected		List<Database.Section>						m_Modifiers					= new List<Database.Section>();


	//		SETUP
	//////////////////////////////////////////////////////////////////////////
	public		override	bool	Setup( IWeapon wpn )
	{
		if ( m_Initialized )
			return false;

		string moduleSectionName = this.GetType().FullName;

		// TRY RECOVER MODULE SECTION
		if ( GameManager.Configs.bGetSection( moduleSectionName, ref m_ModuleSection ) == false )			// Get Module Section
			return false;

		// GET FIRE MODE SECTION NAME
		string weaponFireModeSectionName = null;
		if ( m_ModuleSection.bAsString( "FireMode", ref weaponFireModeSectionName ) == false )
			return false;

		// LOAD FIRE MODE
		weaponFireModeSectionName = "WPN_FireMode_" + weaponFireModeSectionName;
		if ( TryLoadFireMode( weaponFireModeSectionName, ref m_WpnFireMode ) == false )
			return false;

		// Get variables from file
		m_ShotDelay				= m_ModuleSection.AsFloat( "BaseShotDelay", m_ShotDelay );
		m_MagazineCapacity		= m_ModuleSection.AsInt( "BaseMagazineCapacity", m_MagazineCapacity );
		m_Damage				= m_ModuleSection.AsFloat( "BaseDamage", m_Damage );
		m_CamDeviation			= m_ModuleSection.AsFloat( "BaseCamDeviation", m_CamDeviation );
		m_FireDispersion		= m_ModuleSection.AsFloat( "BaseFireDispersion", m_FireDispersion );

		// Create Fire Mode
		m_WpnFireMode.Setup( m_ShotDelay, Shoot );

		// Assign internals
		m_Magazine					= m_MagazineCapacity;
		m_FirePoint					= wpn.Transform.Find( "FirePoint" );
		m_WeaponRef					= wpn;

		if ( InternalSetup( m_ModuleSection ) == false )
			return false;

		#region to remove
		/*
		// APPLY MODIFIERS
		string[] modifiers = null;
		if ( m_ModuleSection.bGetMultiAsArray( "Modifiers", ref modifiers ) )
		{
			foreach( string modifierSectionName in modifiers )
			{
				Database.Section modifierSection = null;
				if ( GameManager.Configs.bGetSection( modifierSectionName, ref modifierSection ) )
				{
					ApplyModifier( modifierSection );
				}
			}
		}
		*/
		#endregion


		// MODULE CONTAINER
		string containerID = moduleSectionName;
		GameObject container = transform.Find(containerID) != null ? transform.Find(containerID).gameObject : null;
		if ( container )
		{
			Destroy(container);
		}

		container = new GameObject( containerID );
		container.transform.SetParent( transform );

		// AUDIO
		{
			if ( m_ModuleSounds == null )
			{
				const string fireSoundCollectionPath = "Scriptables/WeaponsFireSound";
				m_ModuleSounds = Resources.Load<AudioCollection>( fireSoundCollectionPath );
			}

			string fireSound = null;
			if ( m_ModuleSection.bAsString( "FireSound", ref fireSound ) )
			{

				AudioSource source = container.GetComponent<AudioSource>();
				if ( source == null )
				{
					source = container.AddComponent<AudioSource>();
				}
				{
					source.playOnAwake = false;
					if ( source.clip = System.Array.Find( m_ModuleSounds.AudioClips, s => s.name == fireSound ) )
					{
						DynamicCustomAudioSource audioSource = container.GetComponent<DynamicCustomAudioSource>();
						if ( audioSource == null )
						{
							audioSource = container.AddComponent<DynamicCustomAudioSource>();
						}
						audioSource.enabled = true;

						audioSource.Setup( source );
						m_AudioSourceFire = audioSource;
					}
				}
			}
		}


		if ( m_PoolBullets != null )
		{
			m_PoolBullets.Destroy();
			m_PoolBullets = null;
		}

		// BULLET POOL
		string bulletObjectName = m_ModuleSection.AsString( "Bullet", "InvalidBulletResource");
		GameObject bulletGO = null;
		if ( ( bulletGO = Resources.Load<GameObject>( "Prefabs/Bullets/" + bulletObjectName ) ) != null ) 
		{
			m_PoolBullets = new GameObjectsPool<Bullet>
			(
				model			: bulletGO,
				size			: m_MagazineCapacity,
				containerName	: moduleSectionName + "_BulletsPool_" + wpn.Transform.name,
				actionOnObject	: ActionOnBullet
			);
		}

		m_Initialized = true;
		return true;
	}


	//////////////////////////////////////////////////////////////////////////
	protected				void	ActionOnBullet( Bullet bullet )
	{
		bullet.SetActive( false );
		bullet.Setup
		(
			canPenetrate: false,
			whoRef: Player.Instance,
			weaponRef: m_WeaponRef as Weapon,
			damageMin: -1.0f,
			damageMax: Damage
		);
		Player.Instance.DisableCollisionsWith( bullet.Collider );
	}


	//		MODIFIERS
	//////////////////////////////////////////////////////////////////////////
	public		override	void	ApplyModifier( Database.Section modifier )
	{
		base.ApplyModifier( modifier );

		float MultMagazineCapacity			= modifier.AsFloat( "MultMagazineCapacity",			1.0f );
		float MultDamage					= modifier.AsFloat( "MultDamage",					1.0f );
		float MultShotDelay					= modifier.AsFloat( "MultShotDelay",				1.0f );
		float MultCamDeviation				= modifier.AsFloat( "MultCamDeviation",				1.0f );
		float MultFireDispersion			= modifier.AsFloat( "MultFireDispersion",			1.0f );
	
		// MAGAZINE
		m_MagazineCapacity				= (uint)( (float)m_MagazineCapacity * MultMagazineCapacity );
		m_PoolBullets.Resize( m_Magazine = m_MagazineCapacity );

		// DAMAGE
		m_Damage						= m_Damage * MultDamage;
		m_PoolBullets.ExecuteActionOnObjectr( ActionOnBullet );


		// FIRE MODE
		string newFireModeSecName = null;
		if ( modifier.bAsString( "FireMode", ref newFireModeSecName ) )
		{
			ChangeFireMode( newFireModeSecName );
		}

		m_ShotDelay						= m_ShotDelay * MultShotDelay;
		m_WpnFireMode.Setup( m_ShotDelay, Shoot );

		// DEVIATION AND DISPERSION
		m_CamDeviation					= m_CamDeviation * MultCamDeviation;
		m_FireDispersion				= m_FireDispersion * MultFireDispersion;

		// BULLET
		string bulletObjectName = null;
		if ( modifier.bAsString( "Bullet", ref bulletObjectName ) )
		{
			GameObject bulletGO = null;
			if ( ( bulletGO = Resources.Load<GameObject>( "Prefabs/Bullets/" + bulletObjectName ) ) != null )
			{
				m_PoolBullets.Convert( bulletGO, ActionOnBullet );
			}
		}

		m_Modifiers.Add( modifier );
	}


	//////////////////////////////////////////////////////////////////////////
	public		override	void	RestoreModuleProperties()
	{
		Setup( m_WeaponRef );
	}


	//////////////////////////////////////////////////////////////////////////
	public		override	void	RemoveModifier( Database.Section modifier )
	{
		if ( m_Modifiers.Contains( modifier ) )
		{
			m_Modifiers.Remove( modifier );

			RestoreModuleProperties();

			foreach( Database.Section mod in m_Modifiers )
			{
				ApplyModifier( mod );
			}
		}
	}


	//		FIREMODE
	//////////////////////////////////////////////////////////////////////////
	public					bool	ChangeFireMode( string FireMode )
	{
		FireMode = "WPN_FireMode_" + FireMode;
		return TryLoadFireMode( FireMode, ref m_WpnFireMode );
	}


	//////////////////////////////////////////////////////////////////////////
	public					bool	ChangeFireMode<T>()
	{
		return TryLoadFireMode( typeof(T).Name, ref m_WpnFireMode );
	}


	//////////////////////////////////////////////////////////////////////////
	private	static			bool	TryLoadFireMode( string weaponFireModeSectionName, ref WPN_FireMode_Base fireMode )
	{
		System.Type type = System.Type.GetType( weaponFireModeSectionName.Trim() );
		if ( type == null )
		{
			Debug.Log( "WPN_FireModule:Setting invalid weapon fire mode \"" + weaponFireModeSectionName + "\"" );
			return false;
		}
			
		// Check module type as child of WPN_BaseModule
		if ( type.IsSubclassOf( typeof( WPN_FireMode_Base ) ) == false )
		{
			Debug.Log( "WPN_FireModule:Class Requested is not a supported weapon fire mode, \"" + weaponFireModeSectionName + "\"" );
			return false;
		}

		Database.Section section = null;
		if ( GameManager.Configs.bGetSection( weaponFireModeSectionName, ref section ) == false )
		{
			Debug.Log( "WPN_FireModule: CAnnot find section for fire mode \"" + weaponFireModeSectionName + "\"" );
			return false;
		}

		object[] arguments = new object[1]
		{
			section
		};
		fireMode = System.Activator.CreateInstance( type, arguments ) as WPN_FireMode_Base;
		return true;
	}


	//		DISPERSION
	//////////////////////////////////////////////////////////////////////////
	protected	virtual		float	GetFireDispersion()
	{
		return m_FireDispersion;
	}


	//////////////////////////////////////////////////////////////////////////
	protected	virtual		float	GetCamDeviation()
	{
		return m_CamDeviation;
	}


	//		SHOOT ACTION
	//////////////////////////////////////////////////////////////////////////
	protected	abstract	void	Shoot( float moduleFireDispersion, float moduleCamDeviation );


	// DESTRUCTOR
	~WPN_FireModule()
	{
		if ( m_PoolBullets != null )
		{
			m_PoolBullets.Destroy();
		}
	}

}
