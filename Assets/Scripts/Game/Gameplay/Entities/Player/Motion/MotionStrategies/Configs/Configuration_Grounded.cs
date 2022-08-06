using UnityEngine;
using UnityEngine.InputSystem;

namespace Entities.Player.Components
{
	public class Configuration_Grounded : ConfigurationBase
	{
		public	const		float					k_CharacterControllerHeight		= 2f;
		public	const		float					k_HeadHeight					= 0.565f;

		[Header("Configs")]
		[Tooltip("Move speed in meters/second")][Range(0.001f, 5f)]
		public				float					MoveSpeed						= 1f;
		
		[Range(1f, 5f)]
		public				float					MoveAccelleration				= 1f;

		[Range(1f, 5f)]
		public				float					MoveDecelleration				= 1f;

		[Tooltip("Sprint multiplier to apply at movement")][Range(1f, 10f)]
		public				float					SprintSpeedMult					= 2f;

		[Tooltip("Crouch multiplier to apply at movement")][Range(.1f, 1f)]
		public				float					CrouchSpeedMult					= 0.4f;

		[Tooltip("")][Range(.1f, k_CharacterControllerHeight)]
		public				float					CrouchedHeight					= k_CharacterControllerHeight * .5f;
		
		[Tooltip("")][Min(0.1f)]
		public				float					CrouchTransitionSeconds			= 0.7f;

		[Tooltip("The height the player can jump")]
		public				float					JumpHeight						= 1.2f;

		[Space]
		[Header("Actions")]
		[SerializeField]
		public				InputActionReference	MoveAction						= null;
		[SerializeField]
		public				InputActionReference	SprintAction					= null;
		[SerializeField]
		public				InputActionReference	CrouchAction					= null;
		[SerializeField]
		public				InputActionReference	JumpAction						= null;


		public				float					HeadHeight						=> k_HeadHeight;
		public				float					CharacterControllerHeight		=> k_CharacterControllerHeight;
	}
}
