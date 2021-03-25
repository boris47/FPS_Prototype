
using System.Linq;
using System.Collections.Generic;
using UnityEngine;


public interface IAttachments
{
	T						AddAttachment<T>				() where T : WPN_BaseWeaponAttachment, new();
	bool					HasAttachment<T>				() where T : WPN_BaseWeaponAttachment, new();
	T						GetAttachment<T>				() where T : WPN_BaseWeaponAttachment, new();
	void					RemoveAttachment<T>				() where T : WPN_BaseWeaponAttachment, new();
	void					ToggleAttachment<T>				() where T : WPN_BaseWeaponAttachment, new();
	void					ActivateAttachment<T>			() where T : WPN_BaseWeaponAttachment, new();
	void					DeactivateAttachment<T>			() where T : WPN_BaseWeaponAttachment, new();

	IWeaponAttachment		AddAttachment					(System.Type type);
	bool					HasAttachment					(System.Type type);
	IWeaponAttachment		GetAttachment					(System.Type type);
	void					RemoveAttachment				(System.Type type);
	void					ToggleAttachment				(System.Type type);
	void					ActivateAttachment				(System.Type type);
	void					DeactivateAttachment			(System.Type type);

	void					ActivateAllAttachments			();
	void					DeactivateAllAttachments		();
	void					RemoveAllAttachments			();
	void					ResetAttachments				();
}

public abstract partial class Weapon : IAttachments
{
	private static Dictionary<System.Type, Database.Section> m_AttachmentsCache				= new Dictionary<System.Type, Database.Section>();

	[Header( "WeaponAttachments" )]
	
	protected		Transform								m_AttachmentRoot				= null;

	[SerializeField, ReadOnly]
	private			List<WPN_BaseWeaponAttachment>			m_AttachmentsList				= new List<WPN_BaseWeaponAttachment>();

	[SerializeField, ReadOnly]
	private			string[]								m_AllowedAttachments			= null;

	[SerializeField, ReadOnly]
	private			string[]								m_DefaultAttachments			= null;

	protected		bool									m_AreAttachmentsAllowed			{ get; private set; } = false;

	public			IAttachments							Attachments						{ get; private set; } = null;


	//////////////////////////////////////////////////////////////////////////
	private static WPN_BaseWeaponAttachment FindAttachmentByType(in List<WPN_BaseWeaponAttachment> attachmentsList, System.Type requestedAttachmentType)
	{
		return attachmentsList.Find(currentAttachment =>
		{
			System.Type currentAttachmentType = currentAttachment.GetType();
			return requestedAttachmentType == currentAttachmentType || currentAttachmentType.IsSubclassOf(requestedAttachmentType);
		});
	}

	//////////////////////////////////////////////////////////////////////////
	T IAttachments.AddAttachment<T>() => AddAttachmentInternal(typeof(T)) as T;

	//////////////////////////////////////////////////////////////////////////
	bool IAttachments.HasAttachment<T>() => FindAttachmentByType(m_AttachmentsList, typeof(T));

	//////////////////////////////////////////////////////////////////////////
	T IAttachments.GetAttachment<T>() => FindAttachmentByType(m_AttachmentsList, typeof(T)) as T;

	//////////////////////////////////////////////////////////////////////////
	void IAttachments.RemoveAttachment<T>() => Object.Destroy(FindAttachmentByType(m_AttachmentsList, typeof(T))?.gameObject);

	//////////////////////////////////////////////////////////////////////////
	void IAttachments.ToggleAttachment<T>()
	{
		WPN_BaseWeaponAttachment attachmentFound = FindAttachmentByType(m_AttachmentsList, typeof(T));
		if (attachmentFound)
		{
			attachmentFound.SetActive(!attachmentFound.IsActive);
		}
	}

	//////////////////////////////////////////////////////////////////////////
	void IAttachments.ActivateAttachment<T>() => FindAttachmentByType(m_AttachmentsList, typeof(T))?.SetActive(true);

	//////////////////////////////////////////////////////////////////////////
	void IAttachments.DeactivateAttachment<T>() => FindAttachmentByType(m_AttachmentsList, typeof(T))?.SetActive(false);

	//////////////////////////////////////////////////////////////////////////
	void IAttachments.ActivateAllAttachments() => m_AttachmentsList.ForEach(attachment => { if (!attachment.IsActive) attachment.SetActive(true); });

	//////////////////////////////////////////////////////////////////////////
	void IAttachments.DeactivateAllAttachments() => m_AttachmentsList.ForEach(attachment => { if (attachment.IsActive) attachment.SetActive(false); });

	//////////////////////////////////////////////////////////////////////////
	void IAttachments.RemoveAllAttachments() => m_AttachmentsList.ForEach(attachment => Object.Destroy(attachment.gameObject));

	//////////////////////////////////////////////////////////////////////////
	void IAttachments.ResetAttachments() => ResetAttachmentsInternal();






	//////////////////////////////////////////////////////////////////////////
	IWeaponAttachment IAttachments.AddAttachment(System.Type type) => AddAttachmentInternal(type);

	//////////////////////////////////////////////////////////////////////////
	bool IAttachments.HasAttachment(System.Type type) => FindAttachmentByType(m_AttachmentsList, type);

	//////////////////////////////////////////////////////////////////////////
	IWeaponAttachment IAttachments.GetAttachment(System.Type type) => FindAttachmentByType(m_AttachmentsList, type);

	//////////////////////////////////////////////////////////////////////////
	void IAttachments.RemoveAttachment(System.Type type) => Object.Destroy(FindAttachmentByType(m_AttachmentsList, type)?.gameObject);

	//////////////////////////////////////////////////////////////////////////
	void IAttachments.ToggleAttachment(System.Type type)
	{
		WPN_BaseWeaponAttachment attachmentFound = FindAttachmentByType(m_AttachmentsList, type);
		if (attachmentFound)
		{
			attachmentFound.SetActive(!attachmentFound.IsActive);
		}
	}

	//////////////////////////////////////////////////////////////////////////
	void IAttachments.ActivateAttachment(System.Type type) => FindAttachmentByType(m_AttachmentsList, type)?.SetActive(true);

	//////////////////////////////////////////////////////////////////////////
	void IAttachments.DeactivateAttachment(System.Type type) => FindAttachmentByType(m_AttachmentsList, type)?.SetActive(false);




	//----------------------------------------------------------------------//
	//----------------------------------------------------------------------//
	//----------------------------------------------------------------------//


	//////////////////////////////////////////////////////////////////////////
	public bool InitializeAttachments()
	{
		Attachments = this as IAttachments;

		// Weapon
		//		-> AllowedAttachments
		//				-> FlashLight
		//				-> LaserPointer
		//				-> Zoom
		//

		// Is the transform where the attachment will be located
		if (m_AreAttachmentsAllowed = transform.TrySearchComponentByChildName("AllowedAttachments", out m_AttachmentRoot))
		{
			if (m_WpnSection.TryGetMultiAsArray("AllowedAttachments", out m_AllowedAttachments))
			{
				// Get the array of children's name of the attachment root
				string[] allowedByPrefabSlots = m_AttachmentRoot.Cast<Transform>().Select(child => child.name).ToArray();

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
						CustomAssertions.IsTrue(m_AllowedAttachments.Contains(defaultAttachment), err, this);
					}
					ResetAttachmentsInternal();
				}
			}
		}

		return true;
	}


	//////////////////////////////////////////////////////////////////////////
	private IWeaponAttachment AddAttachmentInternal(System.Type attachmentType)
	{
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
			CustomAssertions.IsNotNull(m_AttachmentRoot.Find(slotName), err, this);
		}

		// Check attachment type as child of WPN_WeaponAttachment
		{
			string err = $"Trying to instantiate a non attachment class: {attachmentType.FullName}";
			CustomAssertions.IsTrue(attachmentType.IsSubclassOf(typeof(WPN_BaseWeaponAttachment)), err, this);
		}

		// Check the requested type is allowed to this weapon
		{
			string err = $"Weapon {m_WpnBaseSectionName}: Trying to add not allowed attachment {attachmentType.FullName}";
			CustomAssertions.IsTrue(m_AllowedAttachments.Contains(attachmentType.FullName), err, this);
		}

		WPN_BaseWeaponAttachment wpnAttachment = null;
		if (attachmentSection.TryAsString("Attachment_Prefab", out string attachmentPrefabPath))
		{
			if (ResourceManager.LoadResourceSync(attachmentPrefabPath, out GameObject attachmentPrefab))
			{
				GameObject attachmentInstance = Instantiate<GameObject>(attachmentPrefab, attachmentSlot);
				attachmentInstance.transform.localPosition = Vector3.zero;
				attachmentInstance.transform.localRotation = Quaternion.identity;

				string err = $"Attachment prefab {attachmentPrefabPath} have any {typeof(WPN_BaseWeaponAttachment).FullName} script";
				CustomAssertions.IsTrue(attachmentInstance.TryGetComponent(out wpnAttachment), err, this);
			}
		}
		else
		{
			wpnAttachment = m_AttachmentRoot.gameObject.AddComponent(attachmentType) as WPN_BaseWeaponAttachment;
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