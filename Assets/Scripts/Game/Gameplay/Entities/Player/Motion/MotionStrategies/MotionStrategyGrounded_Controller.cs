
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Entities.Player.Components
{
	[Configurable(nameof(m_Configs), "Player/MotionStrategies/" + nameof(MotionStrategyGrounded))]
	public class MotionStrategyGrounded_Controller : MotionStrategyBase
	{
		private enum EModifiers
		{
			SPRINT, CROUCH
		}

		[SerializeField, ReadOnly]
		private				Configuration_Grounded			m_Configs					= null;

		[SerializeField, ReadOnly]
		private				CharacterController				m_CharacterController		= null;

		[SerializeField, ReadOnly]
		private				EntityFoots						m_Foots						= null;

		[SerializeField, ReadOnly]
		private				Transform						m_Head						= null;

		[SerializeField, ReadOnly]
		private				EntityAboveCollisionDetector	m_EntityAboveCollisionDetector = null;

		[SerializeField, ReadOnly]
		private				Vector3							m_CurrentVelocity			= Vector3.zero;


		[Header("Debug")]
		[SerializeField, ReadOnly]
		private				bool							m_CurrentCrouched			= false;

		[SerializeField, ReadOnly]
		private				bool							m_IsGrounded				= false;

		public override		bool							IsMotionConditionValid		=> m_IsGrounded;


		//--------------------
		private	readonly Dictionary<EModifiers, float> m_SpeedModifiers = new Dictionary<EModifiers, float>()
		{
			{EModifiers.SPRINT, 1f },
			{EModifiers.CROUCH, 1f }
		};
		private Vector2 m_CurrentMoveInputVector = Vector2.zero;
		private	float m_CurrentCrouchTransition01Value = 0f;
		private bool m_CanJump = true;
		private bool m_JumpRequested = false;
		private bool m_CrouchedRequested = false;


		//////////////////////////////////////////////////////////////////////////
		protected override void Awake()
		{
			base.Awake();

			enabled &= Utils.CustomAssertions.IsTrue(this.TryGetConfiguration(out m_Configs));

			if (enabled &= Utils.CustomAssertions.IsTrue(gameObject.TryGetComponent(out m_CharacterController)))
			{
				m_CharacterController.height = m_Configs.CharacterControllerHeight;
			}

			if (enabled &= Utils.CustomAssertions.IsTrue(transform.TrySearchComponentByChildName("Head", out m_Head)))
			{
				m_Head.localPosition = Vector3.up * m_Configs.HeadHeight;
			}

			enabled &= Utils.CustomAssertions.IsTrue(gameObject.TrySearchComponent(ESearchContext.LOCAL_AND_CHILDREN, out m_Foots));

			enabled &= Utils.CustomAssertions.IsTrue(gameObject.TrySearchComponent(ESearchContext.LOCAL_AND_CHILDREN, out m_EntityAboveCollisionDetector));
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnEnable()
		{
			base.OnEnable();

			InputHandler.RegisterAxis2DCallback(this, m_Configs.MoveAction, OnMoveActionUpdate, InTryReadRaw: false);

			// TODO: Handle Toggling and holding feature
			InputHandler.RegisterButtonCallbacks(this, m_Configs.SprintAction, OnSprintStart, OnSprintContinue, OnSprintEnd);
			InputHandler.RegisterButtonCallbacks(this, m_Configs.CrouchAction, OnCrouchStart, OnCrouchContinue, OnCrouchEnd);
			InputHandler.RegisterButtonCallbacks(this, m_Configs.JumpAction, OnJumpRequest, null, null);

			// Register grounded listener
			if (Utils.CustomAssertions.IsNotNull(m_Foots))
			{
				m_Foots.OnGroundedChanged += OnGroundedChanged;

				m_IsGrounded = m_Foots.RequestForCurrentState();
			}

			if (Utils.CustomAssertions.IsNotNull(m_EntityAboveCollisionDetector))
			{
				m_EntityAboveCollisionDetector.OnAboveObstacle += OnAboveObstacle;

				m_CanJump = m_EntityAboveCollisionDetector.RequestForCurrentState();
			}
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnDisable()
		{
			base.OnDisable();

			InputHandler.UnRegisterCallbacks(this, m_Configs.MoveAction);
			InputHandler.UnRegisterCallbacks(this, m_Configs.SprintAction);
			InputHandler.UnRegisterCallbacks(this, m_Configs.CrouchAction);
			InputHandler.UnRegisterCallbacks(this, m_Configs.JumpAction);

			if (Utils.CustomAssertions.IsNotNull(m_Foots))
			{
				m_Foots.OnGroundedChanged -= OnGroundedChanged;
			}

			if (Utils.CustomAssertions.IsNotNull(m_EntityAboveCollisionDetector))
			{
				m_EntityAboveCollisionDetector.OnAboveObstacle -= OnAboveObstacle;
			}
		}

		//////////////////////////////////////////////////////////////////////////
		private void OnMoveActionUpdate(Vector2 input) => m_CurrentMoveInputVector.Set(input.x, input.y);
		private void OnJumpRequest() => m_JumpRequested = m_CanJump;
		private void OnCrouchStart() => m_CrouchedRequested = true;
		private void OnCrouchContinue() => m_CrouchedRequested = true;
		private void OnCrouchEnd() => m_CrouchedRequested = false;
		private void OnSprintStart() => m_SpeedModifiers[EModifiers.SPRINT] = m_Configs.SprintSpeedMult;
		private void OnSprintContinue() => m_SpeedModifiers[EModifiers.SPRINT] = m_Configs.SprintSpeedMult;
		private void OnSprintEnd() => m_SpeedModifiers[EModifiers.SPRINT] = 1f;
		private void OnGroundedChanged(bool newState) => m_IsGrounded = newState;
		private void OnAboveObstacle(Collider obstacle)
		{
			m_CanJump = !obstacle;
		}


		//////////////////////////////////////////////////////////////////////////
		private void UpdateCrouchState(float deltaTime)
		{
			m_CurrentCrouchTransition01Value += Mathf.Pow(m_Configs.CrouchTransitionSeconds, -1f) * deltaTime * Utils.Math.BoolToMinusOneOsPlusOne(m_CrouchedRequested);

			m_CurrentCrouchTransition01Value = Mathf.Clamp01(m_CurrentCrouchTransition01Value);

			m_SpeedModifiers[EModifiers.CROUCH] = Utils.Math.ScaleBetween(m_CurrentCrouchTransition01Value, 0f, 1f, 1f, m_Configs.CrouchSpeedMult);

			m_CurrentCrouched = Mathf.Approximately(m_CurrentCrouchTransition01Value, 1f);

			// We want to avoid transform scaling because children and colliders and all is scaled also and it can really easy lead to problems

			m_CharacterController.height = Mathf.Lerp(m_Configs.CharacterControllerHeight, m_Configs.CrouchedHeight, m_CurrentCrouchTransition01Value);

			float ratio = m_Configs.CrouchedHeight / m_Configs.CharacterControllerHeight;
			m_CharacterController.center = Mathf.Lerp(0f, ratio, m_CurrentCrouchTransition01Value) * Vector3.down;
		}


		//////////////////////////////////////////////////////////////////////////
		private void UpdateHeadPosition(float deltaTime)
		{
			Vector3 headLocalPosition = m_Head.localPosition;
			headLocalPosition.y = m_CharacterController.height - (m_Configs.CharacterControllerHeight - m_Configs.HeadHeight);
			m_Head.localPosition = headLocalPosition;
		}


		//////////////////////////////////////////////////////////////////////////
		private void Update()
		{
			float deltaTime = Time.deltaTime;
			float gravity = Physics.gravity.y;

			UpdateCrouchState(deltaTime);

			UpdateHeadPosition(deltaTime);

			m_CurrentVelocity.y += gravity * deltaTime;

			if (m_IsGrounded)
			{
				// Prevent getting negative potential
				m_CurrentVelocity.y = Mathf.Max(m_CurrentVelocity.y, 0f);

				m_CurrentVelocity = transform.InverseTransformDirection(m_CurrentVelocity);
				{
					m_CurrentMoveInputVector.Normalize();

					float movementMultiplier = m_SpeedModifiers.Values.Aggregate(1f, (float f1, float f2) => f1 * f2);

					float accelleration = m_CurrentMoveInputVector.sqrMagnitude > 0f ? m_Configs.MoveAccelleration : m_Configs.MoveDecelleration;

					m_CurrentVelocity.x = Mathf.MoveTowards(m_CurrentVelocity.x, m_Configs.MoveSpeed * movementMultiplier * m_CurrentMoveInputVector.x, accelleration * deltaTime);
					m_CurrentVelocity.z = Mathf.MoveTowards(m_CurrentVelocity.z, m_Configs.MoveSpeed * movementMultiplier * m_CurrentMoveInputVector.y, accelleration * deltaTime);
				}
				m_CurrentVelocity = transform.TransformDirection(m_CurrentVelocity);

				if (m_JumpRequested)
				{
					m_CurrentVelocity.y = Mathf.Sqrt(m_Configs.JumpHeight * -2f * gravity);
				}
			}

			m_CharacterController.Move(m_CurrentVelocity * deltaTime);

			// Consume input
			m_JumpRequested = false;
			m_CurrentMoveInputVector.Set(0f, 0f);
		}
	}
}
