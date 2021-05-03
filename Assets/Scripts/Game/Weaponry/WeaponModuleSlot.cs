using UnityEngine;


public class WeaponModuleSlot : MonoBehaviour
{
	[SerializeField]
	private			EWeaponSlots			m_Slot						= EWeaponSlots.PRIMARY;

	[SerializeField, ReadOnly]
	private			WPN_BaseModule			m_WeaponModule				= null;

	public			EWeaponSlots			Slot						=> m_Slot;
	public			WPN_BaseModule			WeaponModule				=> m_WeaponModule;


	//////////////////////////////////////////////////////////////////////////
	private static (bool[], int) GetModuleRules(Database.Section moduleSection)
	{
		bool[] allowedSlots = default;
		int maxCount = moduleSection.AsInt("MaxModuleCount", 1);

		// By default, if not present th key "AllowedSlots", all slot are allowed
		int[] localAllowedSlots = new int[2] { 1, 2 };
		if (moduleSection.TryGetMultiAsArray<int>("AllowedSlots", out localAllowedSlots))
		{
			// slot - 1 because in config file user use number 1, 2 and 3 but array starts from Zero
			allowedSlots[0] = System.Array.Exists(localAllowedSlots, slot => slot - 1 == (int)EWeaponSlots.PRIMARY);
			allowedSlots[1] = System.Array.Exists(localAllowedSlots, slot => slot - 1 == (int)EWeaponSlots.SECONDARY);
		}
		return (allowedSlots, maxCount);
	}

	//////////////////////////////////////////////////////////////////////////
	public bool CanAssignModule(Database.Section moduleSection, string[] alreadyAssignedModules = null)
	{
		(bool[] allowedSlots, int maxCount) = GetModuleRules(moduleSection);

		// Is this slot is allowed for module
		bool result = allowedSlots[(int)m_Slot];

		// Is this module max count less the maximum allowed
		if (alreadyAssignedModules.IsNotNull())
		{
			int counter = 0;
			string name = moduleSection.GetSectionName();
			System.Array.ForEach(alreadyAssignedModules, m => { if (m == name) counter++; });
			result &= !(counter > maxCount);
		}
		return result;
	}

	//////////////////////////////////////////////////////////////////////////
	public void SetModule<T>(IWeapon wpn) where T : WPN_BaseModule
	{
		SetModule(wpn, typeof(T));
	}

	//////////////////////////////////////////////////////////////////////////
	public void SetModule(IWeapon wpn, Database.Section moduleSection)
	{
		CustomAssertions.IsNotNull(moduleSection);
		SetModule(wpn, System.Type.GetType(moduleSection.GetSectionName()));
	}

	//////////////////////////////////////////////////////////////////////////
	public void SetModule(IWeapon wpn, System.Type type)
	{
		// Check type value
		CustomAssertions.IsNotNull(type, $"{wpn.Section.GetSectionName()}, Slot:{WeaponBase.GetModuleSlotName(m_Slot)}, Setting invalid weapon module");

		// Check module type as child of WPN_BaseModule
		CustomAssertions.IsTrue(type.IsSubclassOf(typeof(WPN_BaseModule)), $"{wpn.Section.GetSectionName()}, Slot:{WeaponBase.GetModuleSlotName(m_Slot)}, Class Requested is not a supported weapon module, \"{type.ToString()}\"");

		if (m_WeaponModule.IsNotNull())
		{
			if (m_WeaponModule.GetType() == type)
			{
				// the module is already mounted
				return;
			}
			else // Different one, detach and destroy
			{
				m_WeaponModule.OnDetach();
				Object.Destroy(m_WeaponModule);
			}
		}

		// Load module
		if (CustomAssertions.IsTrue(GlobalManager.Configs.TryGetSection(type.Name, out Database.Section moduleSection)))
		{
			if (moduleSection.TryAsString("Module_Prefab", out string prefabPath))
			{
				if (CustomAssertions.IsTrue(ResourceManager.LoadResourceSync(prefabPath, out GameObject moduleGO)))
				{
					GameObject instanceGO = Object.Instantiate(moduleGO, transform);
					if (CustomAssertions.IsTrue(instanceGO.TryGetComponent(type, out Component weaponModule)))
					{
						m_WeaponModule = weaponModule as WPN_BaseModule;
						m_WeaponModule.transform.localPosition = Vector3.zero;
						m_WeaponModule.transform.localRotation = Quaternion.identity;
						m_WeaponModule.OnAttach(wpn, m_Slot);
					}
					else
					{
						Object.Destroy(instanceGO);
						Resources.UnloadAsset(moduleGO);
					}
				}
			}
		}
	}

	//////////////////////////////////////////////////////////////////////////
	public void RemoveModule()
	{
		m_WeaponModule.OnDetach();
		Object.Destroy(m_WeaponModule);
		m_WeaponModule = null;
	}
}
