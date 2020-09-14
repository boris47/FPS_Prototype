
using UnityEngine;
using System.Collections;


public class WPN_BaseWeaponAttachmentEmpty : WPN_BaseWeaponAttachment
{
	public override bool Configure(in Database.Section attachmentSection) => true;

	protected override void OnActivate() { }
	protected override void OnDeactivated() { }
}
