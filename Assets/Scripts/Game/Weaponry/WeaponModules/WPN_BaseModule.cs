using System.Collections.Generic;
using UnityEngine;


public interface IWPN_UtilityModule
{

}


//////////////////////////////////////////////////////////////////////////
/// <summary> Abstract base class for weapon modules </summary>
public abstract partial class WPN_BaseModule : MonoBehaviour
{
	[System.Serializable]
	private class WeaponModuleData
	{
		public void AssignFrom(WeaponModuleData other)
		{
			
		}
	}
	private						WeaponModuleData				m_WeaponModuleData			= new WeaponModuleData();

	protected					Database.Section				m_ModuleSection				= new Database.Section( "WPN_BaseModule" );
	protected					IWeapon							m_WeaponRef					= null;
	protected					EWeaponSlots					m_ModuleSlot				= EWeaponSlots.NONE;
	protected					GameObject						m_FireModeContainer			= null;

	public		virtual			Database.Section				ModuleSection				=> m_ModuleSection;

	
	//////////////////////////////////////////////////////////////////////////
	protected virtual void Awake()
	{
		string moduleSectionName = GetType().FullName;
		CustomAssertions.IsTrue(GlobalManager.Configs.TryGetSection(moduleSectionName, out m_ModuleSection));

		InitializeAttachments();
	}

	//////////////////////////////////////////////////////////////////////////
	/// <summary> Initialize everything about this module </summary>
	public abstract void OnAttach(IWeapon w, EWeaponSlots slot);

	//////////////////////////////////////////////////////////////////////////
	/// <summary> Unload and clean everything about this module </summary>
	public abstract void OnDetach();

	//////////////////////////////////////////////////////////////////////////
	protected abstract bool InternalSetup(Database.Section moduleSection);


	//////////////////////////////////////////////////////////////////////////
	public static bool GetRules(Database.Section moduleSection, out string[] allowedBullets)
	{
		return moduleSection.TryGetMultiAsArray("AllowedBullets", out allowedBullets);
	}


	//////////////////////////////////////////////////////////////////////////
	public bool CanAssignBullet(string bulletName)
	{
		bool result = true;
		if (result &= GetRules(m_ModuleSection, out string[] allowedBullets))
		{
			result &= System.Array.IndexOf(allowedBullets, bulletName) > -1;
		}
		return result;
	}


	//////////////////////////////////////////////////////////////////////////
	protected virtual void OnEnable()
	{
		if (CustomAssertions.IsNotNull(GameManager.UpdateEvents))
		{
			GameManager.UpdateEvents.OnFrame += OnFrame;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	protected virtual void OnDisable()
	{
		if (GameManager.UpdateEvents.IsNotNull())
		{
			GameManager.UpdateEvents.OnFrame -= OnFrame;
		}
	}

	//////////////////////////////////////////////////////////////////////////
	protected	abstract	void	OnFrame(float DeltaTime);

	public		abstract	bool	OnSave			( StreamUnit streamUnit );
	public		abstract	bool	OnLoad			( StreamUnit streamUnit );

	public		abstract	bool	CanChangeWeapon	();
	public		abstract	bool	CanBeUsed		();
	public		abstract	void	OnWeaponChange	();

	public		abstract	bool	NeedReload		();
	public		abstract	void	OnAfterReload	();

	//
	public		virtual		void	OnStart		()	{ }
	public		virtual		void	OnUpdate	()	{ }
	public		virtual		void	OnEnd		()	{ }
}
