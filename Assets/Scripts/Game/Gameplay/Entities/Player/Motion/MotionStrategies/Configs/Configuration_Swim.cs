using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Entities.Player.Components
{
	public class Configuration_Swim : ConfigurationBase
	{
		[Header("Configs")]
		[Tooltip("Move speed in meters/second")]
		[Range(0.001f, 5f)]
		public				float					MoveSpeed					= 1f;

		[Tooltip("Sprint multiplier to apply at movement")]
		public				float					SprintMult					= 2f;

		[Tooltip("The height the player can jump")]
		public				float					JumpHeight					= 1.2f;

		[Space]
		[Header("Actions")]
		[SerializeField]
		public				InputActionReference	MoveAction					= null;
		[SerializeField]
		public				InputActionReference	SprintAction				= null;
		[SerializeField]
		public				InputActionReference	JumpAction					= null;
	}
}
