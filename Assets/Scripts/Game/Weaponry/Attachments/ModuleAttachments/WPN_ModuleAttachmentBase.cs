using UnityEngine;

public abstract class WPN_ModuleAttachmentBase : MonoBehaviour
{
	[SerializeField, ReadOnly]
	protected			WPN_BaseModule		m_WeaponModule							= null;

	[SerializeField, ReadOnly]
	protected			string				m_Attachment_PrefabPath					= string.Empty;

	[SerializeField, ReadOnly]
	protected			GameObject			m_AttachmentInstance					= null;

	[SerializeField, ReadOnly]
	protected			string				m_AttachmentSlotName					= string.Empty;


	public				string				AttachmentSlotName						=> m_AttachmentSlotName;

	//////////////////////////////////////////////////////////////////////////
	protected virtual void Awake()
	{
		string sectionName = GetType().Name;
		if (CustomAssertions.IsTrue(GlobalManager.Configs.TryGetSection(sectionName, out Database.Section attachmentSection)))
		{
			if (CustomAssertions.IsTrue(attachmentSection.TryAsString("Attachment_Prefab", out m_Attachment_PrefabPath)))
			{
				if (!string.IsNullOrEmpty(m_Attachment_PrefabPath))
				{
					void OnResourceLoaded(GameObject resource)
					{
						m_AttachmentInstance = Instantiate(resource, transform);
						m_AttachmentInstance.transform.localPosition = Vector3.zero;
						m_AttachmentInstance.transform.localRotation = Quaternion.identity;
					}
					ResourceManager.LoadResourceAsync<GameObject>(m_Attachment_PrefabPath, OnResourceLoaded);
				}
			}

			CustomAssertions.IsTrue(attachmentSection.TryAsString("SlotName", out m_AttachmentSlotName));
		}
	}


	//////////////////////////////////////////////////////////////////////////
	protected virtual void OnDestroy()
	{
		OnDetach();
	}

	//////////////////////////////////////////////////////////////////////////
	protected abstract void OnAttachInternal();


	//////////////////////////////////////////////////////////////////////////
	protected abstract void OnDetachInternal();


	//////////////////////////////////////////////////////////////////////////
	public void OnAttach(WPN_BaseModule weaponModule)
	{
		m_WeaponModule = weaponModule;
		OnAttachInternal();
	}


	//////////////////////////////////////////////////////////////////////////
	public void OnDetach()
	{
		OnDetachInternal();
	}
}
