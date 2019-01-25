
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IWPN_FireModule {

	FireModes				FireMode						{ get; }

	float					Damage							{ get; }
	uint					Magazine						{ get; }
	uint					MagazineCapacity				{ get; }
	float					ShotDelay						{ get; }

	float					CamDeviation					{ get; }
	float					FireDispersion					{ get; }

	bool					NeedRecharge					{ get; }

	void					OnLoad( uint magazine );
}


//////////////////////////////////////////////////////////////////////////
// WPN_BaseModule ( Abstract )
/// <summary> Abstract base class for weapon modules </summary>
[System.Serializable]
public abstract class WPN_BaseModule : MonoBehaviour {
	
	[SerializeField]
	public		float			m_ZoomSensitivity			= 1f;
	public			float		ZoomSensitivity
	{
		get { return m_ZoomSensitivity; }
	}

	public	abstract	void	Setup( IWeapon w );

	public	abstract	bool	CanChangeWeapon	();
	public	abstract	bool	CanBeUsed();
	public	abstract	void	OnWeaponChange	();

	public	abstract	void	OnAfterReload();

	public	abstract	void	InternalUpdate();

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

	public	override	void	Setup( IWeapon w )
	{ }

	public	override	bool	CanChangeWeapon	() {  return true; }
	public	override	bool	CanBeUsed		() {  return true; }
	public	override	void	OnWeaponChange	() { }
	public	override	void	InternalUpdate	() { }
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

	// Weapon reference
	[SerializeField]
	private		IWeapon								m_Weapon					= null;
	protected	IWeapon								Weapon
	{
		get { return m_Weapon; }
	}

	[SerializeField]
	protected	Transform							m_FirePoint					= null;
	[SerializeField]
	protected	uint								m_Magazine					= 0;
	[SerializeField]
	protected	uint								m_BaseMagazineCapacity		= 1;
	[SerializeField]
	protected	float								m_BaseDamage				= 0.0f;
	[SerializeField]
	protected	float								m_BaseShotDelay				= 1.0f;

	[SerializeField]
	protected	float								m_BaseCamDeviation			= 0.02f;
	[SerializeField]
	protected	float								m_BaseFireDispersion		= 0.05f;

	[SerializeField]
	protected	float								m_FireDelay					= 0.5f;

	// INTERFACE START
	public abstract	FireModes						FireMode					{ get; }
	public			Vector3							FirePointPosition			{ get { return m_FirePoint.position; } } // TODO Assign m_FirePoint
	public			Quaternion						FirePointRotation			{ get { return m_FirePoint.rotation; } }
	public			uint							Magazine					{ get { return m_Magazine; } }				// TODO apply multipliers
	public			uint							MagazineCapacity			{ get { return m_BaseMagazineCapacity; } }
	public			float							Damage						{ get { return m_BaseDamage; } }
	public			float							ShotDelay					{ get { return m_BaseShotDelay; } }

	public			float							CamDeviation				{ get { return m_BaseCamDeviation; } }
	public			float							FireDispersion				{ get { return m_BaseFireDispersion; } }

	public			bool							NeedRecharge				{
		get {
			return m_Magazine == 0 || m_Magazine < m_BaseMagazineCapacity;
		}
	}
	// INTERFACE END

	protected		GameObjectsPool<Bullet>			m_PoolBullets				= null;	// TODO Create pool of bullets

	protected		bool							m_Initialized				= false;

	protected		ICustomAudioSource				m_AudioSourceFire			= null; // TODO Create audio

	[SerializeField]
	protected		Database.Section				m_ModuleSection				= null;


	// CONTRUCTOR
	public		override	void	Setup( IWeapon w )
	{
		string moduleSectionName = this.GetType().FullName;

		if ( w != null && GameManager.Configs.bGetSection( moduleSectionName, ref m_ModuleSection ) )
		{
			m_Weapon = w;

			m_FirePoint					= m_Weapon.Transform.Find( "FirePoint" );

			m_BaseMagazineCapacity		= m_ModuleSection.AsInt( "BaseMagazineCapacity", m_BaseMagazineCapacity );
			m_BaseDamage				= m_ModuleSection.AsFloat( "BaseDamage", m_BaseDamage );
			m_BaseShotDelay				= m_ModuleSection.AsFloat( "BaseShotDelay", m_BaseShotDelay );
			m_BaseCamDeviation			= m_ModuleSection.AsFloat( "BaseCamDeviation", m_BaseCamDeviation );
			m_BaseFireDispersion		= m_ModuleSection.AsFloat( "BaseFireDispersion", m_BaseFireDispersion );
			m_BaseShotDelay				= m_ModuleSection.AsFloat( "FireDelay", m_BaseShotDelay );
			
			m_Magazine = m_BaseMagazineCapacity;

			// Bullet pool
			if ( m_ModuleSection.HasKey( "Bullet" ) )
			{
//				Resources.FindObjectsOfTypeAll TODO check that the bullet specified is present into resources
				string bulletName = m_ModuleSection.AsString( "Bullet" );
				GameObject bulletGO = Resources.Load<GameObject>( "Prefabs/Bullets/" + bulletName );
				m_PoolBullets = new GameObjectsPool<Bullet>
				(
					model			: bulletGO,
					size			: m_BaseMagazineCapacity,
					containerName	: moduleSectionName + "_BulletsPool_" + m_Weapon.Transform.name,
					permanent		: true,
					actionOnObject	: ( Bullet o ) =>
					{
						o.SetActive( false );
						o.Setup
						(
							canPenetrate: false,
							whoRef: Player.Instance,
							weaponRef: m_Weapon as Weapon,
							damageMin: -1.0f,
							damageMax: m_BaseDamage
						);
						Player.Instance.DisableCollisionsWith( o.Collider );
					}
				);
			}

			m_Initialized = true;
		}
	}

	//
	public		override	void	OnAfterReload()
	{
		m_Magazine = m_BaseMagazineCapacity;
	}

	// ON LOAD
	public		virtual		void	OnLoad( uint magazine )
	{
		m_Magazine = magazine;
	}

	// CAN SHOOT
	public	override		bool	CanBeUsed()
	{
		return m_Magazine > 0;
	}
	//
	protected	abstract	float	GetFireDispersion();
	//
	protected	abstract	float	GetCamDeviation();

	// SHOOT
	protected	virtual		void	Shoot( float moduleFireDispersion, float moduleCamDeviation )
	{
		m_FireDelay = m_BaseShotDelay;

		m_Magazine --;

		// TODO muzzle flash
//		EffectManager.Instance.PlayEffect( EffectType.MUZZLE, m_FirePoint.position, m_FirePoint.forward, 1 );
//		EffectManager.Instance.PlayEffect( EffectType.SMOKE, m_FirePoint.position, m_FirePoint.forward, 1 );

		// BULLET
		IBullet bullet = m_PoolBullets.GetComponent();
		
		float finalDispersion =  
			m_BaseFireDispersion * moduleFireDispersion // TODO calculate fire dispersion
			* bullet.RecoilMult;

		finalDispersion	*= Player.Instance.IsCrouched			? 0.50f : 1.00f;
		finalDispersion	*= Player.Instance.IsMoving				? 1.50f : 1.00f;
		finalDispersion	*= Player.Instance.IsRunning			? 2.00f : 1.00f;
		finalDispersion	*= WeaponManager.Instance.IsZoomed		? 0.80f : 1.00f;
		// SHOOT
		bullet.Shoot( position: m_FirePoint.position, direction: m_FirePoint.forward );

///		m_AudioSourceFire.Play();

		// CAM DEVIATION
		float camDeviation = m_BaseCamDeviation * moduleCamDeviation;
		CameraControl.Instance.ApplyDeviation( camDeviation );

		// CAM DISPERSION
		CameraControl.Instance.ApplyDispersion( finalDispersion );

		// UI ELEMENTS
		UI.Instance.InGame.UpdateUI();
	}


	// DESTRUCTOR
	~WPN_FireModule()
	{
		m_PoolBullets.Destroy();
	}

}
