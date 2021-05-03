
using System.Collections.Generic;
using UnityEngine;



public partial interface IWeapon: IAttachments<WPN_WeaponAttachmentBase>
{
	IAttachments<WPN_WeaponAttachmentBase> Attachments { get; }
}

public abstract partial class WeaponBase
{
	private static Dictionary<System.Type, Database.Section>	m_AttachmentsCache				= new Dictionary<System.Type, Database.Section>();

	[Header( "Weapon Attachments" )]

	[SerializeField]
	protected		Transform									m_AttachmentRoot				= null;

	[SerializeField, ReadOnly]
	private			List<WPN_WeaponAttachmentBase>				m_AttachmentsList				= new List<WPN_WeaponAttachmentBase>();

	[SerializeField, ReadOnly]
	private			string[]									m_AllowedAttachments			= null;

	[SerializeField, ReadOnly]
	private			string[]									m_DefaultAttachments			= null;

	protected		bool										m_AreAttachmentsAllowed			{ get; private set; } = false;


	public			IAttachments<WPN_WeaponAttachmentBase>		Attachments						{ get; private set; } = null;


	//////////////////////////////////////////////////////////////////////////
	private static WPN_WeaponAttachmentBase FindAttachmentByType(in List<WPN_WeaponAttachmentBase> attachmentsList, System.Type requestedAttachmentType)
	{
		return attachmentsList.Find(a =>
		{
			System.Type currentAttachmentType = a.GetType();
			return requestedAttachmentType == currentAttachmentType || currentAttachmentType.IsSubclassOf(requestedAttachmentType);
		});
	}

	//////////////////////////////////////////////////////////////////////////
	T IAttachments<WPN_WeaponAttachmentBase>.AddAttachment<T>() => AddAttachmentInternal(typeof(T)) as T;

	//////////////////////////////////////////////////////////////////////////
	bool IAttachments<WPN_WeaponAttachmentBase>.HasAttachment<T>() => FindAttachmentByType(m_AttachmentsList, typeof(T));

	//////////////////////////////////////////////////////////////////////////
	T IAttachments<WPN_WeaponAttachmentBase>.GetAttachment<T>() => FindAttachmentByType(m_AttachmentsList, typeof(T)) as T;

	//////////////////////////////////////////////////////////////////////////
	void IAttachments<WPN_WeaponAttachmentBase>.RemoveAttachment<T>() => Object.Destroy(FindAttachmentByType(m_AttachmentsList, typeof(T))?.gameObject);

	//////////////////////////////////////////////////////////////////////////
	void IAttachments<WPN_WeaponAttachmentBase>.ToggleAttachment<T>()
	{
		WPN_WeaponAttachmentBase attachmentFound = FindAttachmentByType(m_AttachmentsList, typeof(T));
		if (attachmentFound)
		{
			attachmentFound.SetActive(!attachmentFound.IsActive);
		}
	}

	//////////////////////////////////////////////////////////////////////////
	void IAttachments<WPN_WeaponAttachmentBase>.ActivateAttachment<T>() => FindAttachmentByType(m_AttachmentsList, typeof(T))?.SetActive(true);

	//////////////////////////////////////////////////////////////////////////
	void IAttachments<WPN_WeaponAttachmentBase>.DeactivateAttachment<T>() => FindAttachmentByType(m_AttachmentsList, typeof(T))?.SetActive(false);

	//////////////////////////////////////////////////////////////////////////
	void IAttachments<WPN_WeaponAttachmentBase>.ActivateAllAttachments() => m_AttachmentsList.ForEach(a => { if (!a.IsActive) a.SetActive(true); });

	//////////////////////////////////////////////////////////////////////////
	void IAttachments<WPN_WeaponAttachmentBase>.DeactivateAllAttachments() => m_AttachmentsList.ForEach(a => { if (a.IsActive) a.SetActive(false); });

	//////////////////////////////////////////////////////////////////////////
	void IAttachments<WPN_WeaponAttachmentBase>.RemoveAllAttachments() => m_AttachmentsList.ForEach(a => Object.Destroy(a.gameObject));

	//////////////////////////////////////////////////////////////////////////
	void IAttachments<WPN_WeaponAttachmentBase>.ResetAttachments() => ResetAttachmentsInternal();






	//////////////////////////////////////////////////////////////////////////
	WPN_WeaponAttachmentBase IAttachments<WPN_WeaponAttachmentBase>.AddAttachment(System.Type type) => AddAttachmentInternal(type);

	//////////////////////////////////////////////////////////////////////////
	bool IAttachments<WPN_WeaponAttachmentBase>.HasAttachment(System.Type type) => FindAttachmentByType(m_AttachmentsList, type);

	//////////////////////////////////////////////////////////////////////////
	WPN_WeaponAttachmentBase IAttachments<WPN_WeaponAttachmentBase>.GetAttachment(System.Type type) => FindAttachmentByType(m_AttachmentsList, type);

	//////////////////////////////////////////////////////////////////////////
	void IAttachments<WPN_WeaponAttachmentBase>.RemoveAttachment(System.Type type) => Object.Destroy(FindAttachmentByType(m_AttachmentsList, type)?.gameObject);

	//////////////////////////////////////////////////////////////////////////
	void IAttachments<WPN_WeaponAttachmentBase>.ToggleAttachment(System.Type type)
	{
		WPN_WeaponAttachmentBase attachmentFound = FindAttachmentByType(m_AttachmentsList, type);
		if (attachmentFound)
		{
			attachmentFound.SetActive(!attachmentFound.IsActive);
		}
	}

	//////////////////////////////////////////////////////////////////////////
	void IAttachments<WPN_WeaponAttachmentBase>.ActivateAttachment(System.Type type) => FindAttachmentByType(m_AttachmentsList, type)?.SetActive(true);

	//////////////////////////////////////////////////////////////////////////
	void IAttachments<WPN_WeaponAttachmentBase>.DeactivateAttachment(System.Type type) => FindAttachmentByType(m_AttachmentsList, type)?.SetActive(false);




	//----------------------------------------------------------------------//
	//----------------------------------------------------------------------//
	//----------------------------------------------------------------------//


	//////////////////////////////////////////////////////////////////////////
	public void InitializeAttachments()
	{
		Attachments = this as IAttachments<WPN_WeaponAttachmentBase>;

		// Weapon
		//		-> AllowedAttachments
		//				-> FlashLight
		//				-> LaserPointer
		//				-> Zoom
		//

		// Is the transform where the attachment will be located is defined
		if (m_AreAttachmentsAllowed = m_AttachmentRoot.IsNotNull())
		{
			if (m_WpnSection.TryGetMultiAsArray("AllowedAttachments", out m_AllowedAttachments))
			{
				// Get the array of children's name of the attachment root
			//	string[] allowedByPrefabSlots = m_AttachmentRoot.Cast<Transform>().Select(child => child.name).ToArray();

				// Verity that names of these children are included in allowed attachment in weapon section
				//foreach (string allowedAttachmentBySection in m_AllowedAttachments)
				//{
				//	string err = $"{m_WpnBaseSectionName} allows attachment '{allowedAttachmentBySection}' by section but prefab does not!";
				//	CustomAssertions.IsTrue(allowedByPrefabSlots.Contains(allowedAttachmentBySection), err, gameObject);
				//}
			
				// Check if the number of allowed modules is less than allowed ones
				//if (allowedByPrefabSlots.Length < m_AllowedAttachments.Length)
				//{
				//	string[] notAllowedbyPrefabAttachments = m_AllowedAttachments.Where(attachmentName => !allowedByPrefabSlots.Contains(attachmentName)).ToArray();
				//	string err = $"{m_WpnBaseSectionName} allows less attachments as required by section, missing attachment slots:\n- {string.Join("\n- ", notAllowedbyPrefabAttachments)}";
				//	CustomAssertions.IsTrue(false, err, gameObject);
				//}

				// Verify that allowed attachements list contains 
				//foreach (string allowedByPrefabSlot in allowedByPrefabSlots)
				//{
				//	string err = $"{m_WpnBaseSectionName} does not allow section allowed attachment '{allowedByPrefabSlot}'";
				//	CustomAssertions.IsTrue(m_AllowedAttachments.Contains(allowedByPrefabSlot), err, gameObject);
				//}

				if (m_WpnSection.TryGetMultiAsArray("DefaultAttachments", out m_DefaultAttachments))
				{
					foreach (string defaultAttachment in m_DefaultAttachments)
					{
						string err = $"Weapon {m_WpnBaseSectionName}, ";
						CustomAssertions.IsTrue(System.Array.IndexOf(m_AllowedAttachments, defaultAttachment) >= 0, err, this);
					}
					ResetAttachmentsInternal();
				}
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private WPN_WeaponAttachmentBase AddAttachmentInternal(System.Type attachmentType)
	{
		CustomAssertions.IsTrue(m_AreAttachmentsAllowed);

		// Update the cache
		if (!m_AttachmentsCache.TryGetValue(attachmentType, out Database.Section attachmentSection))
		{
			string sectionName = attachmentType.Name;
			if (CustomAssertions.IsTrue(GlobalManager.Configs.TryGetSection(sectionName, out attachmentSection)))
			{
				m_AttachmentsCache.Add(attachmentType, attachmentSection);
			}
		}

		// Verify slot availability
		Transform attachmentSlot = null;
		{
			if (!attachmentSection.TryAsString("SlotName", out string slotName))
			{
				CustomAssertions.IsTrue(false, $"Weapon {m_WpnBaseSectionName}:Cannot find slot name in attachment {attachmentType.FullName}");
			}

			string err = $"Weapon {m_WpnBaseSectionName}: Attachment {attachmentType.FullName} require missing slot {slotName}";
			CustomAssertions.IsNotNull(attachmentSlot = m_AttachmentRoot.Find(slotName), err, this);
		}

		// Check attachment type as child of WPN_WeaponAttachment
		{
			string err = $"Trying to instantiate a non attachment class: {attachmentType.FullName}";
			CustomAssertions.IsTrue(attachmentType.IsSubclassOf(typeof(WPN_WeaponAttachmentBase)), err, this);
		}

		// Check the requested type is allowed to this weapon
		{
			string err = $"Weapon {m_WpnBaseSectionName}: Trying to add not allowed attachment {attachmentType.FullName}";
			CustomAssertions.IsTrue(System.Array.IndexOf(m_AllowedAttachments, attachmentType.FullName) >= 0, err, this);
		}

		WPN_WeaponAttachmentBase wpnAttachment = null;
		if (attachmentSection.TryAsString("Attachment_Prefab", out string attachmentPrefabPath))
		{
			if (ResourceManager.LoadResourceSync(attachmentPrefabPath, out GameObject attachmentPrefab))
			{
				GameObject attachmentInstance = Instantiate<GameObject>(attachmentPrefab, attachmentSlot);
				attachmentInstance.transform.localPosition = Vector3.zero;
				attachmentInstance.transform.localRotation = Quaternion.identity;

				string err = $"Attachment prefab {attachmentPrefabPath} have any {typeof(WPN_WeaponAttachmentBase).FullName} script";
				CustomAssertions.IsTrue(attachmentInstance.TryGetComponent(out wpnAttachment), err, this);
			}
		}
		else
		{
			wpnAttachment = attachmentSlot.gameObject.AddComponent(attachmentType) as WPN_WeaponAttachmentBase;
		}

		CustomAssertions.IsNotNull(wpnAttachment, $"Weapon {m_WpnBaseSectionName}: Attachment {attachmentType.FullName} is null", this);

		CustomAssertions.IsTrue(wpnAttachment.Configure(attachmentSection, this));

		wpnAttachment.OnAttach();
		m_AttachmentsList.Add(wpnAttachment);
		return wpnAttachment;
	}


	//////////////////////////////////////////////////////////////////////////
	private void ResetAttachmentsInternal()
	{
		foreach (string defaultAttachment in m_DefaultAttachments)
		{
			AddAttachmentInternal(System.Type.GetType(defaultAttachment));
		}
	}
}