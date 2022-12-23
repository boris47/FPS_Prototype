
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Entities.Player.Components
{
	[RequireComponent(typeof(CharacterController))]
	[Configurable(nameof(m_Configs), "Player/MotionStrategies/" + nameof(MotionStrategyGrounded))]
	public class MotionStrategyGroundedController : MotionStrategyBase
	{
		private enum EModifiers
		{
			SPRINT, CROUCH
		}

		[SerializeField, ReadOnly]
		private				ConfigurationGrounded			m_Configs							= null;

		[SerializeField, ReadOnly]
		private				CharacterController				m_CharacterController				= null;

		[SerializeField, ReadOnly]
		private				Transform						m_Head								= null;

		[SerializeField, ReadOnly]
		private				EntityCollisionDetector			m_EntityAboveCollisionDetector		= null;

		[SerializeField, ReadOnly]
		private				EntityCollisionDetector			m_HeadAboveCollisionDetector		= null;

		[SerializeField, ReadOnly]
		private				Vector3							m_CurrentWorldVelocity				= Vector3.zero;

		[SerializeField, ReadOnly]
		private				Vector3							m_CurrentLocalVelocity				= Vector3.zero;


		[Header("Debug")]
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
	//	private RaycastHit m_LastGroundHit = default;
		private bool m_CanJump = true;
		private bool m_JumpRequested = false;
		private	float m_CurrentCrouchTransition01Value = 0f;
		private bool m_CrouchedRequested = false;
		private bool m_CrouchExitRequested = false;
		private bool m_HasObstacleAbove = false;
		private bool m_HasObstacleAboveHead = false;
		private ControllerColliderHit m_LastControllerColliderHit = null;


		//////////////////////////////////////////////////////////////////////////
		protected override void Awake()
		{
			base.Awake();

			enabled &= Utils.CustomAssertions.IsTrue(this.TryGetConfiguration(out m_Configs));

			if (enabled &= Utils.CustomAssertions.IsTrue(gameObject.TryGetComponent(out m_CharacterController)))
			{
				m_CharacterController.enabled = true;

				//Ref: https://docs.unity3d.com/Manual/class-CharacterController.html

				// This will offset the Capsule Collider in world space, and won’t affect how the Character pivots.
				m_CharacterController.center = Vector3.zero;

				// The Character’s Capsule Collider height. Changing this will scale the collider along the Y axis in both positive and negative directions.
				m_CharacterController.height = m_Configs.CharacterHeight;

				// Length of the Capsule Collider’s radius. This is essentially the width of the collider.
				m_CharacterController.radius = m_Configs.CharacterRadius;

				// Determines whether other rigidbodies or character controllers collide with this
				// character controller (by default this is always enabled).
				m_CharacterController.detectCollisions = true;

				// Enables or disables overlap recovery. Enables or disables overlap recovery. Used
				// to depenetrate character controllers from static objects when an overlap is detected.
				m_CharacterController.enableOverlapRecovery = true;

				// Limits the collider to only climb slopes that are less steep (in degrees) than the indicated value.
				m_CharacterController.slopeLimit = m_Configs.SlopeLimit;

				// The character will step up a stair only if it is closer to the ground than the indicated value.
				// This should not be greater than the Character Controller’s height or it will generate an error.
				m_CharacterController.stepOffset = 0.3f; // Default

				// Two colliders can penetrate each other as deep as their Skin Width. Larger Skin Widths reduce jitter.
				// Low Skin Width can cause the character to get stuck.
				// A good setting is to make this value 10% of the Radius.
				m_CharacterController.skinWidth = m_Configs.CharacterRadius * 0.1f;

				// If the character tries to move below the indicated value, it will not move at all.
				// This can be used to reduce jitter. In most situations this value should be left at 0.
				m_CharacterController.minMoveDistance = 0f;

			}

			if (enabled &= Utils.CustomAssertions.IsTrue(transform.TrySearchComponentByChildName("Head", out m_Head)))
			{
				m_Head.localPosition = Vector3.up * m_Configs.HeadHeight;

			}
			
			if (enabled &= Utils.CustomAssertions.IsTrue(transform.TrySearchComponentByChildName("JumpAboveChecker", out m_EntityAboveCollisionDetector)))
			{

			}

			if (enabled &= Utils.CustomAssertions.IsTrue(transform.TrySearchComponentByChildName("CrouchAboveChecker", out m_HeadAboveCollisionDetector)))
			{
				m_HeadAboveCollisionDetector.Height = m_Configs.CharacterHeight - m_Configs.CrouchedHeight;
				m_HeadAboveCollisionDetector.Radius = m_CharacterController.radius - 0.01f;
				m_HeadAboveCollisionDetector.transform.localPosition = (m_Configs.CharacterHeight - m_Configs.CrouchedHeight) * 0.5f * Vector3.up;
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

				m_CanJump = m_EntityAboveCollisionDetector.RequestForCurrentState();
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

			InputHandler.UnRegisterCallbacks(this, m_Configs.MoveAction);
			InputHandler.UnRegisterCallbacks(this, m_Configs.SprintAction);
			InputHandler.UnRegisterCallbacks(this, m_Configs.CrouchAction);
			InputHandler.UnRegisterCallbacks(this, m_Configs.JumpAction);

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
		private void OnMoveActionUpdate(Vector2 input) => m_CurrentMoveInputVector.Set(input.x, input.y);
		private void OnJumpRequest() => m_JumpRequested = m_CanJump;
		private void OnCrouchStart() => m_CrouchedRequested = true;
		private void OnCrouchContinue() => m_CrouchedRequested = true;
		private void OnCrouchEnd() => m_CrouchExitRequested = true;
		private void OnSprintStart() => m_SpeedModifiers[EModifiers.SPRINT] = m_Configs.SprintSpeedMult;
		private void OnSprintContinue() => m_SpeedModifiers[EModifiers.SPRINT] = m_Configs.SprintSpeedMult;
		private void OnSprintEnd() => m_SpeedModifiers[EModifiers.SPRINT] = 1f;
		private void OnAboveObstacle(Collider obstacle) => m_CanJump = !(m_HasObstacleAbove = obstacle.IsNotNull());
		private void OnAboveHeadObstacle(Collider obstacle) => m_HasObstacleAboveHead = obstacle.IsNotNull();


		//////////////////////////////////////////////////////////////////////////
		private void OnCharacterController(CustomCharacterController.CollisionInfo collisionInfo)
		{
			CustomCharacterController controller = collisionInfo.controller;

			if (controller == null || controller.collisionFlags == CollisionFlags.Below || controller.collisionFlags == CollisionFlags.Above)
			{
				return;
			}

			Rigidbody rigidbody = collisionInfo.rigidbody;
			if (rigidbody.IsNotNull())
			{
				rigidbody.AddForceAtPosition(m_CurrentWorldVelocity, collisionInfo.point, ForceMode.Impulse);
			}
		}

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

			m_CurrentCrouchTransition01Value += Mathf.Pow(m_Configs.CrouchTransitionSeconds, -1f) * deltaTime * Utils.Math.BoolToMinusOneOsPlusOne(m_CrouchedRequested);

			m_CurrentCrouchTransition01Value = Mathf.Clamp01(m_CurrentCrouchTransition01Value);

			m_SpeedModifiers[EModifiers.CROUCH] = Utils.Math.ScaleBetween(m_CurrentCrouchTransition01Value, 0f, 1f, 1f, m_Configs.CrouchSpeedMult);

			m_IsCrouched = Mathf.Approximately(m_CurrentCrouchTransition01Value, 1f);

			// We want to avoid transform scaling because children and colliders and all is scaled also and it can really easy lead to problems

			float ratio = m_Configs.CrouchedHeight / m_Configs.CharacterHeight;
			m_CharacterController.height = Mathf.Lerp(m_Configs.CharacterHeight, m_Configs.CrouchedHeight, m_CurrentCrouchTransition01Value);
			m_CharacterController.center = Mathf.Lerp(0f, ratio, m_CurrentCrouchTransition01Value) * Vector3.down;
		
			// Update head local position
			{
				Vector3 headLocalPosition = m_Head.localPosition;
				headLocalPosition.y = m_CharacterController.height - (m_Configs.CharacterHeight - m_Configs.HeadHeight);
				m_Head.localPosition = headLocalPosition;
			}
		}

		/*
		//////////////////////////////////////////////////////////////////////////
		private void FixedUpdate2()
		{
			m_IsGrounded = Physics.SphereCast
			(
				origin: transform.position,
				radius: m_CharacterController.radius * 0.9f,
				direction: -transform.up,
				hitInfo: out m_LastGroundHit,
				maxDistance: (m_CharacterController.height * 0.5f) - m_CharacterController.radius + 0.2f,
				layerMask: Physics.AllLayers,
				queryTriggerInteraction: QueryTriggerInteraction.Ignore
			);
		}
		*/

		//////////////////////////////////////////////////////////////////////////
		private void UpdateSlidingState()
		{
			if (m_IsGrounded && m_LastControllerColliderHit.IsNotNull())
			{
				float slopeAngle = Vector3.Angle(Vector3.up, m_LastControllerColliderHit.normal);
				if (Utils.Math.IsBetweenOrEqualValues(slopeAngle, m_Configs.SlopeLimit, m_Configs.MaxSlopeLimitAngle))
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
		[SerializeField, ReadOnly]
		private float m_SlidingDownSlopeTime = 0f;

		//////////////////////////////////////////////////////////////////////////
		private void Update()
		{
			float deltaTime = Time.deltaTime;
			float gravity = Physics.gravity.y;

			UpdateCrouchState(deltaTime);

			UpdateSlidingState();
		
			if (m_IsSliding && m_LastControllerColliderHit.IsNotNull())
			{
					Vector3 normal = m_LastControllerColliderHit.normal;
					float slopeAngle = Vector3.Angle(Vector3.up, normal);
				
					m_SlidingDownSlopeTime += deltaTime;
				//
				//	// Speed increases as slope angle increases
				//	float slideSpeedScale = Mathf.Clamp01(slopeAngle / m_Configs.MaxSlopeLimitAngle);
				//
				//
				//	// Apply gravity and slide along the obstacle
				//	float verticalVelocity = gravity * slideSpeedScale * m_SlidingDownSlopeTime;
				//
				//	// Distance to push away from slopes when sliding down them.
				//	const float k_PushAwayFromSlopeDistance = 0.001f;
				//
				//	// Push slightly away from the slope
				//	Vector3 push = new Vector3(normal.x, 0.0f, normal.z).normalized * k_PushAwayFromSlopeDistance;
				//	Vector3 moveVector = new Vector3(push.x, verticalVelocity * deltaTime, push.z);
				//
				//	m_CurrentWorldVelocity = moveVector;
				//	Debug.DrawRay(transform.position, m_CurrentWorldVelocity);

				/*
				bool slopeIsSteep = slopeAngle > m_Configs.SlopeLimit && slopeAngle < m_Configs.MaxSlopeLimit && Vector3.Dot(m_CurrentWorldVelocity, normal) < 0.0f;

				if (slopeIsSteep && normal.y > 0.0f)
				{
					// Do not move up the slope
					normal.y = 0.0f;
					normal.Normalize();
				}

				// Vector to slide along the obstacle
				Vector3 project = Vector3.Cross(normal, Vector3.Cross(m_CurrentWorldVelocity, normal));

				if (slopeIsSteep && project.y > 0.0f)
				{
					// Do not move up the slope
					project.y = 0.0f;
				}

				project.Normalize();
				*/

				/*
				Vector3 moveDirection = new Vector3(normal.x, -normal.y, normal.z);
				Vector3.OrthoNormalize(ref normal, ref moveDirection);
				m_CurrentWorldVelocity = moveDirection * m_SlidingDownSlopeTime;
				*/

				// Speed increases as slope angle increases
				float slideSpeedScale = Mathf.Clamp01(slopeAngle / m_Configs.MaxSlopeLimitAngle);
				Vector3 right = Vector3.Cross(Vector3.up, normal);
				Vector3 slopeDirection = Vector3.Cross(right, normal);
			//	float slopeAffinity = 1f - ((Vector3.Angle(slopeDirection, m_CurrentWorldVelocity) + float.Epsilon) / 180f);
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
			if (m_CanJump && m_IsGrounded && m_JumpRequested && !m_IsSliding)
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
