using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Entities
{
	/// <summary>
	/// A PlayerController is the interface between the Player class and the human player controlling it. The PlayerController essentially represents the human player's will.<br />
	/// The PlayerController decides what to do and then issues commands to the Player.
	/// </summary>
	[RequireComponent(typeof(PlayerEntity))]
	public class PlayerController : EntityController
	{
		protected override void OnPossess(in Entity entity)
		{
			
		}

		protected override void OnUnPossess(in Entity entity)
		{

		}
	}
}
