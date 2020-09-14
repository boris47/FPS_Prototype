
using System.Linq;
using System.Collections.Generic;
using UnityEngine;


public interface IAttachments
{
	bool					HasAttachment<T>				() where T : WPN_BaseWeaponAttachment, new();
	T						AddAttachment<T>				() where T : WPN_BaseWeaponAttachment, new();
	T						GetAttachment<T>				() where T : WPN_BaseWeaponAttachment, new();
	void					RemoveAttachment<T>				() where T : WPN_BaseWeaponAttachment, new();
	void					ToggleAttachment<T>				() where T : WPN_BaseWeaponAttachment, new();
	void					ActivateAttachment<T>			() where T : WPN_BaseWeaponAttachment, new();
	void					DeactivateAttachment<T>			() where T : WPN_BaseWeaponAttachment, new();
	void					ActivateAllAttachments			();
	void					DeactivateAllAttachments		();
	void					RemoveAllAttachments			();
	void					ResetAttachments				();
}

public abstract partial class Weapon : IAttachments
{
	private static Dictionary<System.Type, Database.Section> m_AttachmentsCache = new Dictionary<System.Type, Database.Section>();

	public			IAttachments							Attachments						{ get; private set; } = null;

	protected		bool									m_AreAttachmentsAllowed			{ get; private set; } = false;

	[Header( "WeaponAttachments" )]
	protected		Transform								m_AttachmentRoot				= null;

	[SerializeField, ReadOnly]
	private			List<WPN_BaseWeaponAttachment>			m_AttachmentsList				= new List<WPN_BaseWeaponAttachment>();

	[SerializeField, ReadOnly]
	private			string[]								m_AllowedAttachments			= null;

	[SerializeField, ReadOnly]
	private			string[]								m_DefaultAttachments			= null;




	//////////////////////////////////////////////////////////////////////////
	private static WPN_BaseWeaponAttachment FindAttachmentByType(in List<WPN_BaseWeaponAttachment> attachmentsList, System.Type requestedAttachmentType)
	{
		return attachmentsList.Find( currentAttachment =>
		{
			System.Type currentAttachmentType = currentAttachment.GetType();
			return requestedAttachmentType == currentAttachmentType || currentAttachmentType.IsSubclassOf( requestedAttachmentType );
		} );
	}


	//////////////////////////////////////////////////////////////////////////
	bool					IAttachments.HasAttachment<T>()
	{
		return FindAttachmentByType( this.m_AttachmentsList, typeof( T ) );
	}

	//////////////////////////////////////////////////////////////////////////
	T						IAttachments.AddAttachment<T>()
	{
		return this.AddAttachmentInternal( typeof( T ) ) as T;
	}

	//////////////////////////////////////////////////////////////////////////
	T						IAttachments.GetAttachment<T>()
	{
		return FindAttachmentByType( this.m_AttachmentsList, typeof( T ) ) as T;
	}

	//////////////////////////////////////////////////////////////////////////
	void					IAttachments.RemoveAttachment<T>()
	{
		WPN_BaseWeaponAttachment attachmentFound = FindAttachmentByType( this.m_AttachmentsList, typeof( T ) );
		if (attachmentFound)
		{
			Object.Destroy( attachmentFound.gameObject );
		}
	}

	//////////////////////////////////////////////////////////////////////////
	void					IAttachments.ToggleAttachment<T>()
	{
		WPN_BaseWeaponAttachment attachmentFound = FindAttachmentByType( this.m_AttachmentsList, typeof( T ) );
		if (attachmentFound)
		{
			attachmentFound.SetActive( !attachmentFound.IsActive );
		}
	}

	//////////////////////////////////////////////////////////////////////////
	void					IAttachments.ActivateAttachment<T>()
	{
		WPN_BaseWeaponAttachment attachmentFound = FindAttachmentByType( this.m_AttachmentsList, typeof( T ) );
		if (attachmentFound)
		{
			attachmentFound.SetActive( true );
		}
	}

	//////////////////////////////////////////////////////////////////////////
	void					IAttachments.DeactivateAttachment<T>()
	{
		WPN_BaseWeaponAttachment attachmentFound = FindAttachmentByType( this.m_AttachmentsList, typeof( T ) );
		if (attachmentFound)
		{
			attachmentFound.SetActive( false );
		}
	}

	//////////////////////////////////////////////////////////////////////////
	void					IAttachments.ActivateAllAttachments()
	{
		this.m_AttachmentsList.ForEach( attachment =>
		{
			if ( !attachment.IsActive ) attachment.SetActive( true );
		} );
	}

	//////////////////////////////////////////////////////////////////////////
	void					IAttachments.DeactivateAllAttachments()
	{
		this.m_AttachmentsList.ForEach( attachment =>
		{
			if ( attachment.IsActive ) attachment.SetActive( false );
		} );
	}

	//////////////////////////////////////////////////////////////////////////
	void					IAttachments.RemoveAllAttachments()
	{
		this.m_AttachmentsList.ForEach( attachment => Object.Destroy( attachment.gameObject ) );
	}

	//////////////////////////////////////////////////////////////////////////
	void					IAttachments.ResetAttachments()
	{
		this.ResetAttachmentsInternal();
	}




	//----------------------------------------------------------------------//
	//----------------------------------------------------------------------//
	//----------------------------------------------------------------------//


	//////////////////////////////////////////////////////////////////////////
	public bool InitializeAttachments()
	{
		this.Attachments = this as IAttachments;

		if (this.m_AreAttachmentsAllowed = this.transform.SearchComponentInChild( "AllowedAttachments", ref this.m_AttachmentRoot ))
		{
			if (this.m_WpnSection.bGetMultiAsArray( "AllowedAttachments", ref this.m_AllowedAttachments ))
			{
				string[] allowedByPrefabSlots = this.m_AttachmentRoot.Cast<Transform>().Select( child => child.name ).ToArray();
//				foreach(string allowedAttachmentBySection in this.m_AllowedAttachments )
				{
//					string err = $"Weapon:InitializeAttachments: {this.m_WpnBaseSectionName} allows attachment '{allowedAttachmentBySection}' by section but prefab does not!";
//					Debug.Assert( allowedByPrefabSlots.Contains( allowedAttachmentBySection ), err, this.gameObject );
				}

				if (allowedByPrefabSlots.Length < this.m_AllowedAttachments.Length)
				{
					string[] notAllowedbyPrefabAttachments = this.m_AllowedAttachments.Where( attachmentName => !allowedByPrefabSlots.Contains( attachmentName ) ).ToArray();
					string err = $"Weapon:InitializeAttachments: {this.m_WpnBaseSectionName} allows less attachments as required by section, missing attachment slots:\n- {string.Join( "\n- ", notAllowedbyPrefabAttachments )}";
					Debug.Assert( false, err, this.gameObject );
				}

//				foreach(string allowedByPrefabSlot in allowedByPrefabSlots)
				{
//					string err = $"Weapon:InitializeAttachments: {this.m_WpnBaseSectionName} does not allow section allowed attachment '{allowedByPrefabSlot}'";
//					Debug.Assert( this.m_AllowedAttachments.Contains(allowedByPrefabSlot), err, this.gameObject );
				}

				if (this.m_WpnSection.bGetMultiAsArray("DefaultAttachments", ref this.m_DefaultAttachments ))
				{
					foreach(string defaultAttachment in this.m_DefaultAttachments )
					{
						string err = $"Weapon:InitializeAttachments: Weapon {this.m_WpnBaseSectionName}, ";
						Debug.Assert( this.m_AllowedAttachments.Contains( defaultAttachment ), err, this );
					}
					this.ResetAttachmentsInternal();
				}
			}
		}

		return true;
	}


	//////////////////////////////////////////////////////////////////////////
	private IWeaponAttachment AddAttachmentInternal(System.Type attachmentType)
	{
		if ( !m_AttachmentsCache.TryGetValue( attachmentType, out Database.Section attachmentSection ) )
		{
			string sectionName = attachmentType.Name;
			GlobalManager.Configs.GetSection( sectionName, ref attachmentSection );
			m_AttachmentsCache.Add( attachmentType, attachmentSection );
		}

		// Verify slot availability
		string slotName = null;
		{
			if ( !attachmentSection.bAsString("SlotName", ref slotName ) )
			{
				Debug.Assert( false, $"Weapon.Attachment:AddAttachmentInternal: Weapon {this.m_WpnBaseSectionName}:Cannot find slot name in attachment {attachmentType.FullName}" );
			}

			string err = $"Weapon.Attachment:AddAttachmentInternal: Weapon {this.m_WpnBaseSectionName}: Attachment {attachmentType.FullName} require missing slot {slotName}";
			Debug.Assert( this.m_AttachmentRoot.Find( slotName ), err, this );
		}

		// Check attachment type as child of WPN_WeaponAttachment
		{
			string err = $"Weapon.Attachment:AddAttachmentInternal: Trying to instantiate a non attachment class: {attachmentType.FullName}";
			Debug.Assert( attachmentType.IsSubclassOf( typeof( WPN_BaseWeaponAttachment ) ), err, this );
		}

		// Check the requested type is allowed to this weapon
		{
			string err = $"Weapon.Attachment:AddAttachmentInternal: Weapon {this.m_WpnBaseSectionName}: Trying to add not allowed attachment {attachmentType.FullName}";
			Debug.Assert( this.m_AllowedAttachments.Contains( attachmentType.FullName ), err, this );
		}

		WPN_BaseWeaponAttachment wpnAttachment = null;

		string attachmentPrefabPath = null;
		if (attachmentSection.bAsString( "Attachment_Prefab", ref attachmentPrefabPath ) )
		{
			GameObject attachmentPrefab = Resources.Load( attachmentPrefabPath ) as GameObject;
			if ( attachmentPrefab )
			{
				GameObject attachmentInstance = Instantiate<GameObject>( attachmentPrefab, this.m_AttachmentRoot.Find( slotName ) );
				attachmentInstance.transform.localPosition = Vector3.zero;
				attachmentInstance.transform.localRotation = Quaternion.identity;

				string err = $"Weapon.Attachment:AddAttachmentInternal: Attachment prefab {attachmentPrefabPath} have any {typeof(WPN_BaseWeaponAttachment).FullName} script";
				Debug.Assert( attachmentInstance.TryGetComponent( out wpnAttachment ), err, this );
			}
		}
		else
		{
			wpnAttachment = this.m_AttachmentRoot.gameObject.AddComponent( attachmentType ) as WPN_BaseWeaponAttachment;
		}

		// Final Check
		{
			string err = $"Weapon.Attachment:AddAttachmentInternal: Weapon {this.m_WpnBaseSectionName}: Attachment {attachmentType.FullName} is null";
			Debug.Assert( wpnAttachment, err, this );
		}

		// On success attach the component
		bool bConfiguredSuccessfully = wpnAttachment.Configure(attachmentSection);
		if ( bConfiguredSuccessfully == true )
		{
			wpnAttachment.OnAttach();
			this.m_AttachmentsList.Add( wpnAttachment );
		}
		// On Fail remove the component
		else
		{
			Object.Destroy( wpnAttachment.gameObject );
			Debug.LogError( $"Weapon.Attachments::AddAttachmentInternal: Weapon: {this.m_WpnBaseSectionName}: Attachment \"{attachmentType.ToString()}\" has failed the attach" );
		}
		return bConfiguredSuccessfully ? wpnAttachment : null;
	}


	//////////////////////////////////////////////////////////////////////////
	private void ResetAttachmentsInternal()
	{
		foreach(string defaultAttachment in this.m_DefaultAttachments)
		{
			this.AddAttachmentInternal(System.Type.GetType(defaultAttachment));
		}
	}
}