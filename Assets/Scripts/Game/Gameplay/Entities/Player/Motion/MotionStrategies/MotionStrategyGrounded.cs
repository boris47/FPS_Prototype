
using UnityEngine;

namespace Entities.Player.Components
{
//	[RequireComponent(typeof(Rigidbody))]
	[Configurable(nameof(m_Configs), "Player/MotionStrategies/" + nameof(MotionStrategyGrounded))]
	public class MotionStrategyGrounded : MotionStrategyBase
	{
		[SerializeField, ReadOnly]
		private				Configuration_Grounded			m_Configs					= null;

		[SerializeField, ReadOnly]
		private				Rigidbody						m_Rigidbody					= null;

		[SerializeField, ReadOnly]
		private				EntityFoots						m_Foots						= null;


		public override		bool							IsMotionConditionValid		=> m_IsGrounded;


		//--------------------
		private Vector2 m_CurrentMoveInputVector = Vector2.zero;
		private float m_CurrentMovementMultiplier = 1f;
		private bool m_JumpRequested = false;
		private bool m_IsGrounded = false;

		//////////////////////////////////////////////////////////////////////////
		protected override void Awake()
		{
			base.Awake();

			Utils.CustomAssertions.IsTrue(this.TryGetConfiguration(out m_Configs));

			// Find needed components
			Utils.CustomAssertions.IsTrue(TryGetComponent(out m_Rigidbody));
			
			Utils.CustomAssertions.IsTrue(gameObject.TrySearchComponent(ESearchContext.LOCAL_AND_CHILDREN, out m_Foots));
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnEnable()
		{
			base.OnEnable();

			// Register grounded listener
			m_Foots.OnGroundedChanged += OnGroundedChanged;

			m_IsGrounded = m_Foots.RequestForCurrentState();

			InputHandler.RegisterAxis2DCallback(this, m_Configs.MoveAction, OnMoveActionUpdate, InTryReadRaw: false);
			InputHandler.RegisterButtonCallbacks(this, m_Configs.JumpAction, OnJumpRequest, null, null);
			InputHandler.RegisterButtonCallbacks(this, m_Configs.SprintAction, OnSprintStart, null, OnSprintEnd);
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnDisable()
		{
			base.OnDisable();

			InputHandler.UnRegisterCallbacks(this, m_Configs.MoveAction);
			InputHandler.UnRegisterCallbacks(this, m_Configs.JumpAction);
			InputHandler.UnRegisterCallbacks(this, m_Configs.SprintAction);

			if (Utils.CustomAssertions.IsNotNull(m_Foots))
			{
				m_Foots.OnGroundedChanged -= OnGroundedChanged;
			}
		}

		//////////////////////////////////////////////////////////////////////////
		private void OnMoveActionUpdate(Vector2 input) => m_CurrentMoveInputVector.Set(input.x, input.y);
		private void OnJumpRequest() => m_JumpRequested = true;
		private void OnSprintStart() => m_CurrentMovementMultiplier += m_Configs.SprintSpeedMult;
		private void OnSprintEnd() => m_CurrentMovementMultiplier -= m_Configs.SprintSpeedMult;
		private void OnGroundedChanged(bool newState) => m_IsGrounded = newState;

		
		//////////////////////////////////////////////////////////////////////////
		private void Update()
		{
			m_Rigidbody.drag = m_IsGrounded ? 7f : 0f;
			if (m_IsGrounded)
			{
				if (m_CurrentMoveInputVector.sqrMagnitude > 0f)
				{
					m_CurrentMoveInputVector.Normalize();

					Vector3 localVelocity = m_Rigidbody.transform.InverseTransformDirection(m_Rigidbody.velocity);
					{
						// Forward
						localVelocity.z = m_CurrentMoveInputVector.y * m_Configs.MoveSpeed * m_CurrentMovementMultiplier;
						// Right
						localVelocity.x = m_CurrentMoveInputVector.x * m_Configs.MoveSpeed * m_CurrentMovementMultiplier;

						// Consume input
						m_CurrentMoveInputVector.Set(0f, 0f);
					}
					m_Rigidbody.velocity = m_Rigidbody.transform.TransformDirection(localVelocity);

				}

				// Jump
				if (m_JumpRequested)
				{
					// the square root of H * -2 * G = how much velocity needed to reach desired height
					m_Rigidbody.AddForce(Vector3.up * (Mathf.Sqrt(m_Configs.JumpHeight * -2f * Physics.gravity.y) - m_Rigidbody.velocity.y), ForceMode.VelocityChange);
				}
			}
			else // airborne
			{

			}
			// Always consume input
			m_JumpRequested = false;
		}
	}
}
