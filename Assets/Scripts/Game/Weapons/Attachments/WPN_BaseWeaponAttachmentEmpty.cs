
using UnityEngine;
using System.Collections;


public class WPN_BaseWeaponAttachmentEmpty : WPN_BaseWeaponAttachment
{
	public override bool ConfigureInternal(in Database.Section attachmentSection) => true;

	protected override void OnActivate() { }
	protected override void OnDeactivated() { }
}
