using UnityEngine;


[RequireComponent(typeof(Collider))]
public class PickupableItem : MonoBehaviour
{

	[SerializeField]
	protected	string					m_PickUpSectionName		= string.Empty;

	[SerializeField]
	protected	Texture2D				m_Texture				= null;

	[SerializeField, ReadOnly]
	protected	Collider				m_Collider				= null;


	private		Database.Section		m_ItemSection			= null;
	private		bool					m_Initialized			= true;


	//////////////////////////////////////////////////////////////////////////
	private void OnEnable()
	{
		if (GlobalManager.Configs.TryGetSection(m_PickUpSectionName, out m_ItemSection) && gameObject.TryGetComponent(out m_Collider) && m_Collider.isTrigger)
		{
			m_Initialized = true;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public bool SetPickupSectionName(string PickupSectionName)
	{
		bool bIsSectionFound = GlobalManager.Configs.TryGetSection(m_PickUpSectionName, out Database.Section pickupableSection);
		if (bIsSectionFound)
		{
			m_PickUpSectionName = PickupSectionName;
		}
		return bIsSectionFound;
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnTriggerEnter(Collider other)
	{
		if (m_Initialized && Utils.Base.TrySearchComponent(other.gameObject, ESearchContext.LOCAL_AND_PARENTS, out Entity entity))
		{
			entity.Inventory.AddInventoryItem(m_PickUpSectionName);
			enabled = false;
			Destroy(gameObject);
		}

		if (m_Initialized == false)
		{
			enabled = false;
		}
	}

}