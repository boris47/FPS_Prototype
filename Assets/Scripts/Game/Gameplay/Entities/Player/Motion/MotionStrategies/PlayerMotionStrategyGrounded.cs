
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Entities.Player.Components
{
	[RequireComponent(typeof(CharacterController))]
	[Configurable(nameof(m_Configs), "Player/MotionStrategies/" + nameof(PlayerMotionStrategyGrounded))]
	public class PlayerMotionStrategyGrounded : PlayerMotionStrategyBase
	{
		private enum EModifiers
		{
			SPRINT, CROUCH
		}

		[SerializeField, ReadOnly]
		private				PlayerConfigurationGrounded		m_Configs							= null;

		[SerializeField, ReadOnly]
		private				CharacterController				m_CharacterController				= null;

		[SerializeField, ReadOnly]
		private				Transform						m_Head								= null;

		[SerializeField, ReadOnly]
		private				EntityCollisionDetector			m_EntityAboveCollisionDetector		= null;

		[SerializeField, ReadOnly]
		private				EntityCollisionDetector			m_HeadAboveCollisionDetector		= null;


		[Header("Debug")]
		[SerializeField, ReadOnly]
		private				Vector3							m_CurrentWorldVelocity				= Vector3.zero;

		[SerializeField, ReadOnly]
		private				Vector3							m_CurrentLocalVelocity				= Vector3.zero;

		[SerializeField, ReadOnly]
		private				bool							m_IsCrouched						= false;

		[SerializeField, ReadOnly]
		private				bool							m_IsGrounded						= false;

		[SerializeField, ReadOnly]
		private				bool							m_IsSliding							= false;


		//--------------------
		private	readonly Dictionary<EModifiers, float> m_SpeedModifiers = new Dictionary<EModifiers, float>()
		{
			{EModifiers.SPRINT, 1f },
			{EModifiers.CROUCH, 1f }
		};
		private Vector2 m_CurrentMoveInputVector = Vector2.zero;
		private CollisionFlags m_CollisionFlags = CollisionFlags.None;
		private ControllerColliderHit m_LastControllerColliderHit = null;

		private bool m_JumpRequested = false;
		private	float m_CurrentCrouchTransition01Value = 0f;
		private bool m_CrouchedRequested = false;
		private bool m_CrouchExitRequested = false;
		private bool m_HasObstacleAbove = false;
		private bool m_HasObstacleAboveHead = false;
		private float m_SlidingDownSlopeTime = 0f;


		//////////////////////////////////////////////////////////////////////////
		protected override void Awake()
		{
			base.Awake();

			enabled &= Utils.CustomAssertions.IsTrue(this.TryGetConfiguration(out m_Configs));

			m_CharacterController = m_Controller.Player.CharacterController;
			m_Head = m_Owner.Head;

			PlayerConfiguration playerConfigs = m_Controller.Player.PlayerConfiguration;

			// Create an entity aAbove collision detector
			{
				GameObject go = new GameObject("JumpAboveChecker", typeof(Rigidbody), typeof(CapsuleCollider));
				go.transform.SetParent(transform);
				go.transform.SetSiblingIndex(0);
				m_EntityAboveCollisionDetector = go.AddComponent<EntityCollisionDetector>();
				{
					float radius = m_CharacterController.radius - 0.01f;
					m_EntityAboveCollisionDetector.Height = playerConfigs.CharacterHeight * 0.75f;
					m_EntityAboveCollisionDetector.Radius = radius;
					m_EntityAboveCollisionDetector.PositiveResult = true;
					m_EntityAboveCollisionDetector.transform.localPosition = playerConfigs.CharacterHeight * 0.5f * Vector3.up;
				}
			}

			// Create an head above collision detector
			{
				GameObject go = new GameObject("CrouchAboveChecker", typeof(Rigidbody), typeof(CapsuleCollider));
				go.transform.SetParent(transform);
				go.transform.SetSiblingIndex(1);
				m_HeadAboveCollisionDetector = go.AddComponent<EntityCollisionDetector>();
				{
					m_HeadAboveCollisionDetector.Height = playerConfigs.CharacterHeight - m_Configs.CrouchedHeight;
					m_HeadAboveCollisionDetector.Radius = m_CharacterController.radius - 0.01f;
					m_HeadAboveCollisionDetector.PositiveResult = true;
					m_HeadAboveCollisionDetector.transform.localPosition = (playerConfigs.CharacterHeight - m_Configs.CrouchedHeight) * 0.5f * Vector3.up;
				}
			}
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

			if (Utils.CustomAssertions.IsNotNull(m_EntityAboveCollisionDetector))
			{
				m_EntityAboveCollisionDetector.OnAboveObstacle += OnAboveObstacle;

				m_HasObstacleAbove = m_EntityAboveCollisionDetector.RequestForCurrentState();
			}

			if (Utils.CustomAssertions.IsNotNull(m_HeadAboveCollisionDetector))
			{
				m_HeadAboveCollisionDetector.OnAboveObstacle += OnAboveHeadObstacle;

				m_HasObstacleAboveHead = m_HeadAboveCollisionDetector.RequestForCurrentState();
			}
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnDisable()
		{
			base.OnDisable();

			InputHandler.UnRegisterAllCallbacks(this);

			if (Utils.CustomAssertions.IsNotNull(m_EntityAboveCollisionDetector))
			{
				m_EntityAboveCollisionDetector.OnAboveObstacle -= OnAboveObstacle;
			}

			if (Utils.CustomAssertions.IsNotNull(m_HeadAboveCollisionDetector))
			{
				m_HeadAboveCollisionDetector.OnAboveObstacle -= OnAboveHeadObstacle;
			}
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnDestroy()
		{
			base.OnDestroy();

			if (m_EntityAboveCollisionDetector.IsNotNull())
			{
				m_EntityAboveCollisionDetector.Destroy();
			}

			if (m_HeadAboveCollisionDetector.IsNotNull())
			{
				m_HeadAboveCollisionDetector.Destroy();
			}
		}

		//////////////////////////////////////////////////////////////////////////
		public override PlayerMotionTransitionSnapshot CreateSnapshot()
		{
			return new PlayerMotionTransitionSnapshot()
			{
				CurrentVelocity = m_CurrentWorldVelocity
			};
		}

		//////////////////////////////////////////////////////////////////////////
		public override void PorcessSnappshot(PlayerMotionTransitionSnapshot InSnapShot)
		{

		}

		//////////////////////////////////////////////////////////////////////////
		private void OnMoveActionUpdate(Vector2 input) => m_CurrentMoveInputVector.Set(input.x, input.y);
		private void OnJumpRequest() => m_JumpRequested = true;
		private void OnCrouchStart() => m_CrouchedRequested = true;
		private void OnCrouchContinue() => m_CrouchedRequested = true;
		private void OnCrouchEnd() => m_CrouchExitRequested = true;
		private void OnSprintStart() => m_SpeedModifiers[EModifiers.SPRINT] = m_Configs.SprintSpeedMult;
		private void OnSprintContinue() => m_SpeedModifiers[EModifiers.SPRINT] = m_Configs.SprintSpeedMult;
		private void OnSprintEnd() => m_SpeedModifiers[EModifiers.SPRINT] = 1f;
		private void OnAboveObstacle(Collider obstacle) => m_HasObstacleAbove = obstacle.IsNotNull();
		private void OnAboveHeadObstacle(Collider obstacle) => m_HasObstacleAboveHead = obstacle.IsNotNull();

		//////////////////////////////////////////////////////////////////////////
		private void UpdateCrouchState(float deltaTime)
		{
			if (m_CrouchExitRequested && m_IsCrouched)
			{
				if (m_HasObstacleAboveHead)
				{
					m_CrouchedRequested = true;
				}
				else
				{
					m_CrouchedRequested = false;
					m_CrouchExitRequested = false;
				}
			}

			PlayerConfiguration playerConfigs = m_Controller.Player.PlayerConfiguration;

			m_CurrentCrouchTransition01Value += Mathf.Pow(m_Configs.CrouchTransitionSeconds, -1f) * deltaTime * Utils.Math.BoolToMinusOneOsPlusOne(m_CrouchedRequested);

			m_CurrentCrouchTransition01Value = Mathf.Clamp01(m_CurrentCrouchTransition01Value);

			m_SpeedModifiers[EModifiers.CROUCH] = Utils.Math.ScaleBetween(m_CurrentCrouchTransition01Value, 0f, 1f, 1f, m_Configs.CrouchSpeedMult);

			m_IsCrouched = Mathf.Approximately(m_CurrentCrouchTransition01Value, 1f);

			// We want to avoid transform scaling because children and colliders and all is scaled also and it can really easy lead to problems

			float ratio = m_Configs.CrouchedHeight / playerConfigs.CharacterHeight;
			m_CharacterController.height = Mathf.Lerp(playerConfigs.CharacterHeight, m_Configs.CrouchedHeight, m_CurrentCrouchTransition01Value);
			m_CharacterController.center = Mathf.Lerp(0f, ratio, m_CurrentCrouchTransition01Value) * Vector3.down;
		
			// Update head local position
			{
				Vector3 headLocalPosition = m_Head.localPosition;
				headLocalPosition.y = m_CharacterController.height - (playerConfigs.CharacterHeight - playerConfigs.HeadHeight);
				m_Head.localPosition = headLocalPosition;
			}
		}

		//////////////////////////////////////////////////////////////////////////
		private void UpdateSlidingState()
		{
			if (m_IsGrounded && m_LastControllerColliderHit.IsNotNull())
			{
				PlayerConfiguration playerConfigs = m_Controller.Player.PlayerConfiguration;
				float slopeAngle = Vector3.Angle(Vector3.up, m_LastControllerColliderHit.normal);
				if (Utils.Math.IsBetweenOrEqualValues(slopeAngle, playerConfigs.SlopeLimit, playerConfigs.MaxSlopeLimitAngle))
				{
					if (!m_IsSliding)
					{
						// Slide start
						m_IsSliding = true;
					}
				}
				else
				{
					if (m_IsSliding)
					{
						m_IsSliding = false;
						m_CurrentWorldVelocity.y = 0f;
						m_SlidingDownSlopeTime = 0f;
						// slide end
					}
				}
			}
			else
			{
				if (m_IsSliding)
				{
					m_IsSliding = false;
					// slide end
				}
		
				m_IsSliding = false;
				m_SlidingDownSlopeTime = 0f;
			}
		}

		//////////////////////////////////////////////////////////////////////////
		private void Update()
		{
			float deltaTime = Time.deltaTime;
			float gravity = Physics.gravity.y;

			UpdateCrouchState(deltaTime);

			UpdateSlidingState();
		
			if (m_IsSliding && m_LastControllerColliderHit.IsNotNull())
			{
				PlayerConfiguration playerConfigs = m_Controller.Player.PlayerConfiguration;
				Vector3 normal = m_LastControllerColliderHit.normal;
				float slopeAngle = Vector3.Angle(Vector3.up, normal);
				
				m_SlidingDownSlopeTime += deltaTime;

				// Speed increases as slope angle increases
				float slideSpeedScale = Mathf.Clamp01(slopeAngle / playerConfigs.MaxSlopeLimitAngle);
				Vector3 right = Vector3.Cross(Vector3.up, normal);
				Vector3 slopeDirection = Vector3.Cross(right, normal);
				m_CurrentWorldVelocity = m_SlidingDownSlopeTime * slideSpeedScale * Mathf.Abs(gravity) * slopeDirection;
			}
			else
			{
				// Use user input
				m_CurrentMoveInputVector.Normalize();
				m_CurrentLocalVelocity = transform.InverseTransformDirection(m_CurrentWorldVelocity);
				if (m_IsGrounded)
				{
					if (m_CurrentMoveInputVector.sqrMagnitude > 0f)
					{
						float moveSpeed = m_Configs.MoveSpeed * m_SpeedModifiers.Values.Aggregate(1f, (float f1, float f2) => f1 * f2);

						float tX = (Mathf.Sign(m_CurrentLocalVelocity.x) == Mathf.Sign(m_CurrentMoveInputVector.x)) ? m_Configs.MoveAcceleration : m_Configs.MoveDeceleration;
						float tY = (Mathf.Sign(m_CurrentLocalVelocity.z) == Mathf.Sign(m_CurrentMoveInputVector.y)) ? m_Configs.MoveAcceleration : m_Configs.MoveDeceleration;

						m_CurrentLocalVelocity.x = Mathf.MoveTowards(m_CurrentLocalVelocity.x, moveSpeed * m_CurrentMoveInputVector.x, tX * deltaTime);
						m_CurrentLocalVelocity.z = Mathf.MoveTowards(m_CurrentLocalVelocity.z, moveSpeed * m_CurrentMoveInputVector.y, tY * deltaTime);
					}
					else
					{
						m_CurrentLocalVelocity.x = Mathf.MoveTowards(m_CurrentLocalVelocity.x, 0f, m_Configs.MoveDeceleration * deltaTime);
						m_CurrentLocalVelocity.z = Mathf.MoveTowards(m_CurrentLocalVelocity.z, 0f, m_Configs.MoveDeceleration * deltaTime);
					}
				}
				else
				{
					float moveSpeed = m_Configs.MoveSpeed * m_SpeedModifiers.Values.Aggregate(1f, (float f1, float f2) => f1 * f2);
					Utils.Math.ClampResult(ref m_CurrentLocalVelocity.x, m_CurrentLocalVelocity.x + (moveSpeed * m_Configs.AirControlMult * deltaTime * m_CurrentMoveInputVector.x), -moveSpeed, moveSpeed);
					Utils.Math.ClampResult(ref m_CurrentLocalVelocity.z, m_CurrentLocalVelocity.z + (moveSpeed * m_Configs.AirControlMult * deltaTime * m_CurrentMoveInputVector.y), -moveSpeed, moveSpeed);
				}
				m_CurrentWorldVelocity = transform.TransformDirection(m_CurrentLocalVelocity);
			}

			// Gravity
			{
				if (m_IsGrounded && !m_IsSliding)
				{
					const float k_GroundedGravity = -3f;
					m_CurrentWorldVelocity.y = k_GroundedGravity;
				}
				else
				{
					m_CurrentWorldVelocity.y += gravity * deltaTime;
				}
			}

			// Jump request
			if (!m_HasObstacleAbove && m_IsGrounded && m_JumpRequested && !m_IsSliding)
			{
				m_CurrentWorldVelocity.y = Mathf.Sqrt(m_Configs.JumpHeight * -2f * gravity);
			}

			m_CollisionFlags = m_CharacterController.Move(m_CurrentWorldVelocity * deltaTime);

			m_IsGrounded = m_CollisionFlags.IsOrContains(CollisionFlags.Below);

			// Consume input
			m_JumpRequested = false;
			m_CurrentMoveInputVector.Set(0f, 0f);
		}


		//////////////////////////////////////////////////////////////////////////
		private void OnControllerColliderHit(ControllerColliderHit controllerColliderHit)
		{
			if (m_CollisionFlags.IsOrContains(CollisionFlags.Sides))
			{
				Rigidbody rigidbody = controllerColliderHit.collider.attachedRigidbody;
				if (rigidbody.IsNotNull())
				{
					if (!rigidbody.isKinematic)
					{
						Vector3 pushDir = new Vector3(m_CurrentWorldVelocity.x, 0f, m_CurrentWorldVelocity.z);
						rigidbody.AddForceAtPosition(pushDir, controllerColliderHit.point, ForceMode.Impulse);
					}
				}
			}

			if (m_CollisionFlags.IsOrContains(CollisionFlags.Below))
			{
				m_LastControllerColliderHit = controllerColliderHit;
			}
		}

	}
}
