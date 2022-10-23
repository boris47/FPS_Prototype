using System;
using UnityEngine.UIElements;

namespace Entities.AI.Components.Behaviours.Editor
{
	public class BehaviourTreeDropdownMenuAction : DropdownMenuAction
	{
		public BehaviourTreeDropdownMenuAction(
			string actionName,
			Action<DropdownMenuAction> actionCallback,
			Func<DropdownMenuAction, Status> actionStatusCallback,
			object userData = null
		) : base(actionName, actionCallback, actionStatusCallback, userData) {
		}

		public BehaviourTreeDropdownMenuAction(
			string actionName,
			Action<DropdownMenuAction> actionCallback
		) : this(actionName, actionCallback, (e) => DropdownMenuAction.Status.Normal, null) {
		}
	}
}
