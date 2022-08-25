using UnityEngine;
using UnityEngine.InputSystem;

namespace Entities.Player.Components
{
	public class PlayerConfigurationSwim : ConfigurationBase
	{
		[SerializeField]
		public				float					GravityMult						= 1f;

		[SerializeField, Range(0.1f, 10f)]
		public				float					BaseSwimSpeed					= 1f;

		[SerializeField, Range(1f, 10f)]
		public				float					MoveAcceleration				= 5f;

		[SerializeField, Range(1f, 10f)]
		public				float					MoveDeceleration				= 5f;
		
		[SerializeField, Range(1f, 10f)]
		public				float					SprintMult						= 1f;

		[SerializeField, Range(0.1f, 10f)]
		public				float					ForwardSwimMult					= 1f;
		
		[SerializeField, Range(0.1f, 10f)]
		public				float					BackwardSwimMult				= 1f;

		[SerializeField, Range(0.1f, 10f)]
		public				float					LateralSwimMult					= 1f;

		[SerializeField, Range(0.1f, 10f)]
		public				float					VerticalSwimMult				= 1f;


		[Header("Actions")]
		[SerializeField]
		public				InputActionReference	MoveAction						= null;
		[SerializeField]
		public				InputActionReference	SprintAction					= null;
		[SerializeField]
		public				InputActionReference	SwimUpAction					= null;
		[SerializeField]
		public				InputActionReference	SwimDownAction					= null;
	}
}
