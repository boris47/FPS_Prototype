
using System.Collections.Generic;
using UnityEngine;

/// <summary> Abstract base class for fire modules </summary>
public abstract partial class WPN_BaseModule: IAttachments<WPN_ModuleAttachmentBase>
{
	private static Dictionary<System.Type, Database.Section>	m_AttachmentsCache				= new Dictionary<System.Type, Database.Section>();

	[Header("Module Attachments")]
	[SerializeField]
	protected		Transform									m_AttachmentRoot				= null;

	[SerializeField, ReadOnly]
	private			List<WPN_ModuleAttachmentBase>				m_AttachmentsList				= new List<WPN_ModuleAttachmentBase>();

	[SerializeField, ReadOnly]
	protected			string[]								m_DefaultAttachments			= null;
	[SerializeField, ReadOnly]
	protected			string[]								m_AllowedAttachments			= null;

	protected		bool										m_AreAttachmentsAllowed			{ get; private set; } = false;

	public			IAttachments<WPN_ModuleAttachmentBase>		Attachments						{ get; private set; } = null;

	//////////////////////////////////////////////////////////////////////////
	private static WPN_ModuleAttachmentBase FindAttachmentByType(in List<WPN_ModuleAttachmentBase> attachmentsList, System.Type requestedAttachmentType)
	{
		return attachmentsList.Find(a =>
		{
			System.Type currentAttachmentType = a.GetType();
			return requestedAttachmentType == currentAttachmentType || currentAttachmentType.IsSubclassOf(requestedAttachmentType);
		});
	}

	//////////////////////////////////////////////////////////////////////////
	T IAttachments<WPN_ModuleAttachmentBase>.AddAttachment<T>() => AddAttachmentInternal(typeof(T)) as T;

	//////////////////////////////////////////////////////////////////////////
	bool IAttachments<WPN_ModuleAttachmentBase>.HasAttachment<T>() => FindAttachmentByType(m_AttachmentsList, typeof(T));

	//////////////////////////////////////////////////////////////////////////
	T IAttachments<WPN_ModuleAttachmentBase>.GetAttachment<T>() => FindAttachmentByType(m_AttachmentsList, typeof(T)) as T;

	//////////////////////////////////////////////////////////////////////////
	void IAttachments<WPN_ModuleAttachmentBase>.RemoveAttachment<T>() => Object.Destroy(FindAttachmentByType(m_AttachmentsList, typeof(T))?.gameObject);

	//////////////////////////////////////////////////////////////////////////
	void IAttachments<WPN_ModuleAttachmentBase>.ToggleAttachment<T>()
	{
	//	WPN_ModuleAttachmentBase attachmentFound = FindAttachmentByType(m_AttachmentsList, typeof(T));
	//	if (attachmentFound)
	//	{
	//		attachmentFound.SetActive(!attachmentFound.IsActive);
	//	}
	}

	//////////////////////////////////////////////////////////////////////////
	void IAttachments<WPN_ModuleAttachmentBase>.ActivateAttachment<T>() { }// => FindAttachmentByType(m_AttachmentsList, typeof(T))?.SetActive(true);

	//////////////////////////////////////////////////////////////////////////
	void IAttachments<WPN_ModuleAttachmentBase>.DeactivateAttachment<T>() { }// => FindAttachmentByType(m_AttachmentsList, typeof(T))?.SetActive(false);

	//////////////////////////////////////////////////////////////////////////
	void IAttachments<WPN_ModuleAttachmentBase>.ActivateAllAttachments() { }// => m_AttachmentsList.ForEach(a => { if (!a.IsActive) a.SetActive(true); });

	//////////////////////////////////////////////////////////////////////////
	void IAttachments<WPN_ModuleAttachmentBase>.DeactivateAllAttachments() { }// => m_AttachmentsList.ForEach(a => { if (a.IsActive) a.SetActive(false); });

	//////////////////////////////////////////////////////////////////////////
	void IAttachments<WPN_ModuleAttachmentBase>.RemoveAllAttachments() => m_AttachmentsList.ForEach(a => Object.Destroy(a.gameObject));

	//////////////////////////////////////////////////////////////////////////
	void IAttachments<WPN_ModuleAttachmentBase>.ResetAttachments() { }// => ResetAttachmentsInternal();






	//////////////////////////////////////////////////////////////////////////
	WPN_ModuleAttachmentBase IAttachments<WPN_ModuleAttachmentBase>.AddAttachment(System.Type type) => AddAttachmentInternal(type);

	//////////////////////////////////////////////////////////////////////////
	bool IAttachments<WPN_ModuleAttachmentBase>.HasAttachment(System.Type type) => FindAttachmentByType(m_AttachmentsList, type);

	//////////////////////////////////////////////////////////////////////////
	WPN_ModuleAttachmentBase IAttachments<WPN_ModuleAttachmentBase>.GetAttachment(System.Type type) => FindAttachmentByType(m_AttachmentsList, type);

	//////////////////////////////////////////////////////////////////////////
	void IAttachments<WPN_ModuleAttachmentBase>.RemoveAttachment(System.Type type) => Object.Destroy(FindAttachmentByType(m_AttachmentsList, type)?.gameObject);

	//////////////////////////////////////////////////////////////////////////
	void IAttachments<WPN_ModuleAttachmentBase>.ToggleAttachment(System.Type type)
	{
	//	WPN_ModuleAttachmentBase attachmentFound = FindAttachmentByType(m_AttachmentsList, type);
	//	if (attachmentFound)
	//	{
	//		attachmentFound.SetActive(!attachmentFound.IsActive);
	//	}
	}

	//////////////////////////////////////////////////////////////////////////
	void IAttachments<WPN_ModuleAttachmentBase>.ActivateAttachment(System.Type type) { }// => FindAttachmentByType(m_AttachmentsList, type)?.SetActive(true);

	//////////////////////////////////////////////////////////////////////////
	void IAttachments<WPN_ModuleAttachmentBase>.DeactivateAttachment(System.Type type) { }// => FindAttachmentByType(m_AttachmentsList, type)?.SetActive(false);


	//////////////////////////////////////////////////////////////////////////
	private void InitializeAttachments()
	{
		Attachments = this as IAttachments<WPN_ModuleAttachmentBase>;

		// Is the transform where the attachment will be located is defined
		if (m_AreAttachmentsAllowed = m_AttachmentRoot.IsNotNull())
		{
			if (m_ModuleSection.TryGetMultiAsArray("AllowedAttachments", out m_AllowedAttachments))
			{
				// Get the array of children's name of the attachment root
				//string[] allowedByPrefabSlots = m_AttachmentRoot.Cast<Transform>().Select(child => child.name).ToArray();

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

				if (m_ModuleSection.TryGetMultiAsArray("DefaultAttachments", out m_DefaultAttachments))
				{
					foreach (string defaultAttachment in m_DefaultAttachments)
					{
						string err = $" Module {m_ModuleSection.GetSectionName()}, ";
						CustomAssertions.IsTrue(System.Array.IndexOf(m_AllowedAttachments, defaultAttachment) >= 0, err, this);
					}
					ResetAttachmentsInternal();
				}
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private WPN_ModuleAttachmentBase AddAttachmentInternal(System.Type attachmentType)
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
				CustomAssertions.IsTrue(false, $"Module {m_ModuleSection.GetSectionName()}:Cannot find slot name in attachment {attachmentType.FullName}");
			}

			string err = $"Module {m_ModuleSection.GetSectionName()}: Attachment {attachmentType.FullName} require missing slot {slotName}";
			CustomAssertions.IsNotNull(attachmentSlot = m_AttachmentRoot.Find(slotName), err, this);
		}

		// Check attachment type as child of WPN_ModuleAttachmentBase
		{
			string err = $"Trying to instantiate a non attachment class: {attachmentType.FullName}";
			CustomAssertions.IsTrue(attachmentType.IsSubclassOf(typeof(WPN_ModuleAttachmentBase)), err, this);
		}

		// Check the requested type is allowed to this Module
		{
			string err = $"Module {m_ModuleSection.GetSectionName()}: Trying to add not allowed attachment {attachmentType.FullName}";
			CustomAssertions.IsTrue(System.Array.IndexOf(m_AllowedAttachments, attachmentType.FullName) >= 0, err, this);
		}

		WPN_ModuleAttachmentBase wpnAttachment = null;
		if (attachmentSection.TryAsString("Attachment_Prefab", out string attachmentPrefabPath))
		{
			if (ResourceManager.LoadResourceSync(attachmentPrefabPath, out GameObject attachmentPrefab))
			{
				GameObject attachmentInstance = Instantiate<GameObject>(attachmentPrefab, attachmentSlot);
				attachmentInstance.transform.localPosition = Vector3.zero;
				attachmentInstance.transform.localRotation = Quaternion.identity;

				string err = $"Attachment prefab {attachmentPrefabPath} have any {typeof(WPN_ModuleAttachmentBase).FullName} script";
				CustomAssertions.IsTrue(attachmentInstance.TryGetComponent(out wpnAttachment), err, this);
			}
		}
		else
		{
			wpnAttachment = attachmentSlot.gameObject.AddComponent(attachmentType) as WPN_ModuleAttachmentBase;
		}

		CustomAssertions.IsNotNull(wpnAttachment, $"Module {m_ModuleSection.GetSectionName()}: Attachment {attachmentType.FullName} is null", this);

		wpnAttachment.OnAttach(this);

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
