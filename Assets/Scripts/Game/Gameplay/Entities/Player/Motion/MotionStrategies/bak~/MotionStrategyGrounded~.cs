
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Entities.Player.Components
{
	[Configurable(nameof(m_Configs), "Player/MotionStrategies/" + nameof(MotionStrategyGrounded))]
	public class MotionStrategyGrounded : MotionStrategyBase
	{
		private enum EModifiers
		{
			SPRINT, CROUCH
		}

		[SerializeField, ReadOnly]
		private				ConfigurationGrounded			m_Configs							= null;

		[SerializeField, ReadOnly]
		private				Rigidbody						m_Rigidbody							= null;

		[SerializeField, ReadOnly]
		private				CapsuleCollider					m_CharacterCollider					= null;

		[SerializeField, ReadOnly]
		private				Transform						m_Head								= null;

		[SerializeField, ReadOnly]
		private				EntityCollisionDetector			m_EntityAboveCollisionDetector		= null;

		[SerializeField, ReadOnly]
		private				EntityCollisionDetector			m_HeadAboveCollisionDetector		= null;

		[SerializeField, ReadOnly]
		private				Vector3							m_CurrentLocalVelocity				= Vector3.zero;

		[SerializeField, ReadOnly]
		private				Vector3							m_CurrentWorldVelocity				= Vector3.zero;

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
		private RaycastHit m_LastGroundHit = default;
		private bool m_CanJump = true;
		private bool m_JumpRequested = false;
		private	float m_CurrentCrouchTransition01Value = 0f;
		private bool m_CrouchedRequested = false;
		private bool m_CrouchExitRequested = false;
		private bool m_HasObstacleAbove = false;
		private bool m_HasObstacleAboveHead = false;


		//////////////////////////////////////////////////////////////////////////
		protected override void Awake()
		{
			base.Awake();

			enabled &= Utils.CustomAssertions.IsTrue(this.TryGetConfiguration(out m_Configs));

			if (enabled &= Utils.CustomAssertions.IsTrue(gameObject.TryGetComponent(out m_Rigidbody)))
			{

			}

			if (enabled &= Utils.CustomAssertions.IsTrue(gameObject.TryGetComponent(out m_CharacterCollider)))
			{
				m_CharacterCollider.height = m_Configs.CharacterHeight;
				m_CharacterCollider.enabled = true;
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
				m_HeadAboveCollisionDetector.Radius = m_CharacterCollider.radius - 0.01f;
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

			m_CharacterCollider.height = Mathf.Lerp(m_Configs.CharacterHeight, m_Configs.CrouchedHeight, m_CurrentCrouchTransition01Value);

			float ratio = m_Configs.CrouchedHeight / m_Configs.CharacterHeight;
			m_CharacterCollider.center = Mathf.Lerp(0f, ratio, m_CurrentCrouchTransition01Value) * Vector3.down;

			// Update head local position
			{
				Vector3 headLocalPosition = m_Head.localPosition;
				headLocalPosition.y = m_CharacterCollider.height - (m_Configs.CharacterHeight - m_Configs.HeadHeight);
				m_Head.localPosition = headLocalPosition;
			}
		}

		public float anglee;
		//////////////////////////////////////////////////////////////////////////
		private void UpdateSliding()
		{
			if (m_IsGrounded)
			{
				if ((anglee = Vector3.Angle(transform.up, m_LastGroundHit.normal)) >= m_Configs.SlopeLimit)
				{
					if (!m_IsSliding)
					{
						// Slide start
						m_IsSliding = true;
					}
					else // sliding
					{
				//		Debug.Log("Appling force on slide");
				//		Vector3 right = Vector3.Cross(transform.up, m_LastGroundHit.normal);
				//		Vector3 slopeDirection = Vector3.Cross(right, m_LastGroundHit.normal);
				//		m_Rigidbody.AddForce(slopeDirection, ForceMode.Force);
					}
				}
				else
				{
					if (m_IsSliding)
					{
						m_IsSliding = false;
						Vector3 localVelocity = transform.InverseTransformDirection(m_Rigidbody.velocity);
						{
							localVelocity.y = 0f;
						}
						m_Rigidbody.velocity = transform.TransformDirection(localVelocity);
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
			}
		}

		//////////////////////////////////////////////////////////////////////////
		private void FixedUpdate()
		{
			m_IsGrounded = Physics.SphereCast
			(
				origin: transform.position,
				radius: m_CharacterCollider.radius * 0.9f,
				direction: -transform.up,
				hitInfo: out m_LastGroundHit,
				maxDistance: (m_CharacterCollider.height * 0.5f) - m_CharacterCollider.radius + 0.2f,
				layerMask: Physics.AllLayers,
				queryTriggerInteraction: QueryTriggerInteraction.Ignore
			);

			if (m_IsGrounded)
			{
				// Ref: https://github.com/Unity-Technologies/Standard-Assets-Characters/blob/master/Assets/_Standard%20Assets/Characters/Scripts/Physics/OpenCharacterController.cs#L1473
				// Raycast returns a more accurate normal than SphereCast/CapsuleCast
				Physics.Raycast
				(
					origin: transform.position,
					direction: m_LastGroundHit.point - transform.position,
					hitInfo: out m_LastGroundHit,
					maxDistance: float.MaxValue,
					layerMask: Physics.AllLayers,
					queryTriggerInteraction: QueryTriggerInteraction.Ignore
				);
			}
			
			UpdateSliding();
		}

		//////////////////////////////////////////////////////////////////////////
		private void Update()
		{
			float deltaTime = Time.deltaTime;
			float gravity = Physics.gravity.y;

			UpdateCrouchState(deltaTime);

			/*
			float angle = Vector3.Angle(transform.up, m_LastGroundHit.normal);
			bool scendendo = angle > 0f + float.Epsilon;
			bool salendo = angle < 0f - float.Epsilon;
			*/
			// Vector to slide along the obstacle


		//	Vector3 right = Vector3.Cross(transform.up, m_LastGroundHit.normal);
		//	Vector3 slopeDirection = Vector3.Cross(right, m_LastGroundHit.normal);
		//	float angle = Vector3.Angle(transform.forward, slopeDirection);
		//	float angle2 = Vector3.Angle(transform.up, slopeDirection);

		//	slopeAffinity = 1f - ((Vector3.Angle(slopeDirection, m_Rigidbody.velocity) + float.Epsilon) / 180f);

		//	slopeAffinity = ((Vector3.SignedAngle(transform.forward, slopeDirection, transform.up) + float.Epsilon) / 90f);
			
			if (m_IsGrounded && !m_IsSliding)
			{
				float moveSpeed = m_Configs.MoveSpeed * m_SpeedModifiers.Aggregate(1f, (float f1, KeyValuePair<EModifiers, float> f2) => f1 * f2.Value);

				m_CurrentLocalVelocity = transform.InverseTransformDirection(m_Rigidbody.velocity);
				{
					if (m_CurrentMoveInputVector.sqrMagnitude > 0f)
					{
						m_CurrentMoveInputVector.Normalize();

						m_CurrentLocalVelocity.x = Mathf.MoveTowards(m_CurrentLocalVelocity.x, moveSpeed * m_CurrentMoveInputVector.x, m_Configs.MoveAcceleration * deltaTime);
						m_CurrentLocalVelocity.z = Mathf.MoveTowards(m_CurrentLocalVelocity.z, moveSpeed * m_CurrentMoveInputVector.y, m_Configs.MoveAcceleration * deltaTime);
					}
					else
					{
						m_CurrentLocalVelocity.x = Mathf.MoveTowards(m_CurrentLocalVelocity.x, 0f, m_Configs.MoveDeceleration * deltaTime);
						m_CurrentLocalVelocity.z = Mathf.MoveTowards(m_CurrentLocalVelocity.z, 0f, m_Configs.MoveDeceleration * deltaTime);
					}
				}

				m_CurrentWorldVelocity = transform.TransformDirection(m_CurrentLocalVelocity);
				//Debug.DrawRay(transform.position, m_CurrentWorldVelocity);
				m_Rigidbody.velocity = m_CurrentWorldVelocity;
			}

			// Jump request
			if (m_CanJump && m_IsGrounded && !m_IsSliding && m_JumpRequested)
			{
				float force = Mathf.Sqrt(m_Configs.JumpHeight * -2f * gravity);
			//	m_CurrentWorldVelocity.y = force;
				m_Rigidbody.AddRelativeForce(force * Vector3.up, ForceMode.VelocityChange);
			}

			// Consume input
			m_JumpRequested = false;
			m_CurrentMoveInputVector.Set(0f, 0f);
		}
	public float slopeAffinity;
	}
}
