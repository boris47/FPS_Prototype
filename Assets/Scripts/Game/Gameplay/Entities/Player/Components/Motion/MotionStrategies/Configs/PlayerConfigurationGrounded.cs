using UnityEngine;
using UnityEngine.InputSystem;

namespace Entities.Player.Components
{
	public class PlayerConfigurationGrounded : ConfigurationBase
	{
		[Header("Configs")]
		[Tooltip("Move speed in meters/second")][Range(0.001f, 5f)]
		public				float					MoveSpeed						= 1f;

		public				float					MoveAcceleration				= 5f;
	
		public				float					MoveDeceleration				= 5f;

		[Tooltip("Sprint multiplier to apply at movement")][Range(1f, 10f)]
		public				float					SprintSpeedMult					= 2f;

		[Tooltip("Crouch multiplier to apply at movement")][Range(.1f, 1f)]
		public				float					CrouchSpeedMult					= 0.4f;

		[Tooltip("")][Range(.1f, PlayerConfiguration.k_CharacterHeight)]
		public				float					CrouchedHeight					= PlayerConfiguration.k_CharacterHeight * .5f;
		
		[Tooltip("")][Min(0.1f)]
		public				float					CrouchTransitionSeconds			= 0.7f;

		[Tooltip("The height the player can jump")]
		public				float					JumpHeight						= 1.2f;

		[Min(0f)]
		public				float					AirborneDrag					= 0f;

		[Range(0f, 1f)]
		public				float					AirControlMult					= 0f;

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
	}
}
