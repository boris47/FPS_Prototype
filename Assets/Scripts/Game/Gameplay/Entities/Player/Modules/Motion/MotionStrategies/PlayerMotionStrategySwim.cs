
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Entities.Player.Components
{
	[Configurable(nameof(m_Configs), "Player/MotionStrategies/" + nameof(PlayerMotionStrategySwim))]
	public class PlayerMotionStrategySwim : PlayerMotionStrategyBase, IConfigurable<SwimVolume>
	{
		private enum EModifiers
		{
			FORWARD_BACKWARD, LATERAL, VERTICAL
		}

		[SerializeField, ReadOnly]
		private				PlayerConfigurationSwim			m_Configs							= null;

		[SerializeField, ReadOnly]
		private				CharacterController				m_CharacterController				= null;

		[SerializeField, ReadOnly]
		private				Transform						m_Head								= null;

		[SerializeField, ReadOnly]
		private				SwimVolume						m_SwimVolume						= null;

		[Header("Debug")]
		[SerializeField, ReadOnly]
		private				Vector3							m_CurrentWorldVelocity				= Vector3.zero;

		[SerializeField, ReadOnly]
		private				Vector3							m_CurrentLocalVelocity				= Vector3.zero;


		//--------------------
		private Vector3 m_CurrentLocalMoveInputVector = Vector3.zero;
		/*private UDictionary<EModifiers, float> m_Directions = new UDictionary<EModifiers, float>()
		{
			{EModifiers.FORWARD_BACKWARD,  0f },
			{EModifiers.LATERAL,  0f },
			{EModifiers.VERTICAL, 0f },
		};
		*/
		private bool m_SprintRequested = false;
		private CollisionFlags m_CollisionFlags = CollisionFlags.None;


		//////////////////////////////////////////////////////////////////////////
		protected override void Awake()
		{
			base.Awake();

			enabled &= Utils.CustomAssertions.IsTrue(this.TryGetConfiguration(out m_Configs));

			m_CharacterController = m_Controller.Player.CharacterController;
			m_Head = m_Owner.Head;

		//	PlayerConfiguration playerConfigs = m_Controller.Player.PlayerConfiguration;
		}

		

		//////////////////////////////////////////////////////////////////////////
		public void Configure(SwimVolume InSwimVolume)
		{
			m_SwimVolume = InSwimVolume;
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnEnable()
		{
			base.OnEnable();

			InputHandler.RegisterAxis2DCallback(this, m_Configs.MoveAction, OnMoveActionUpdate, InTryReadRaw: false);
			InputHandler.RegisterButtonCallbacks(this, m_Configs.SprintAction, OnSprintStart, null, OnSprintEnd);
			InputHandler.RegisterButtonCallbacks(this, m_Configs.SwimUpAction, OnSwimLocalUpStart, OnSwimLocalUpContinue, OnSwimLocalUpEnd);
			InputHandler.RegisterButtonCallbacks(this, m_Configs.SwimDownAction, SwimLocalDownStart, SwimLocalDownContinue, SwimLocalDownEnd);
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnDisable()
		{
			base.OnDisable();

			InputHandler.UnRegisterAllCallbacks(this);
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnDestroy()
		{
			base.OnDestroy();
		}

		//////////////////////////////////////////////////////////////////////////
		private void OnMoveActionUpdate(float deltaTime, Vector2 input)
		{
			input.Normalize();

			m_CurrentLocalMoveInputVector.x = input.x;
			m_CurrentLocalMoveInputVector.z = input.y;
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
		private void OnSprintStart() => m_SprintRequested = true;
		private void OnSprintEnd() => m_SprintRequested = false;
		private void OnSwimLocalUpStart() => m_CurrentLocalMoveInputVector.y += 1f;
		private void OnSwimLocalUpContinue(float deltaTime) => m_CurrentLocalMoveInputVector.y = Mathf.Max(m_CurrentLocalMoveInputVector.y + 1f, 1f);
		private void OnSwimLocalUpEnd() => m_CurrentLocalMoveInputVector.y -= 1f;
		private void SwimLocalDownStart() => m_CurrentLocalMoveInputVector.y -= 1f;
		private void SwimLocalDownContinue(float deltaTime) => m_CurrentLocalMoveInputVector.y = Mathf.Min(m_CurrentLocalMoveInputVector.y - 1f, -1f);
		private void SwimLocalDownEnd() => m_CurrentLocalMoveInputVector.y += 1f;


		//////////////////////////////////////////////////////////////////////////
		private void Update()
		{
			float deltaTime = Time.deltaTime;
		//	float gravity = Physics.gravity.y;

			m_CurrentLocalMoveInputVector.x *= m_Configs.LateralSwimMult;

			m_CurrentLocalMoveInputVector.y *= m_Configs.VerticalSwimMult;

			// Different velocity for forward and backward
			m_CurrentLocalMoveInputVector.z *= m_CurrentLocalMoveInputVector.z > 0f ? m_Configs.ForwardSwimMult : m_Configs.BackwardSwimMult;

			// Apply sprint multiplier if requested
			if (m_SprintRequested)
			{
				m_CurrentLocalMoveInputVector.z *= m_Configs.SprintMult;
			}

			//  use head to apply relative movement
			m_CurrentLocalVelocity = m_Head.InverseTransformDirection(m_CurrentWorldVelocity);
			{
				float tX = (Mathf.Sign(m_CurrentLocalVelocity.x) == Mathf.Sign(m_CurrentLocalMoveInputVector.x)) ? m_Configs.MoveAcceleration : m_Configs.MoveDeceleration;
				float tY = (Mathf.Sign(m_CurrentWorldVelocity.y) == Mathf.Sign(m_CurrentLocalMoveInputVector.y)) ? m_Configs.MoveAcceleration : m_Configs.MoveDeceleration;
				float tZ = (Mathf.Sign(m_CurrentLocalVelocity.z) == Mathf.Sign(m_CurrentLocalMoveInputVector.z)) ? m_Configs.MoveAcceleration : m_Configs.MoveDeceleration;

				m_CurrentLocalVelocity.x = Mathf.MoveTowards(m_CurrentLocalVelocity.x, m_CurrentLocalMoveInputVector.x * m_Configs.BaseSwimSpeed, tX * deltaTime);
				m_CurrentLocalVelocity.y = Mathf.MoveTowards(m_CurrentLocalVelocity.y, m_CurrentLocalMoveInputVector.y * m_Configs.BaseSwimSpeed, tY * deltaTime);
				m_CurrentLocalVelocity.z = Mathf.MoveTowards(m_CurrentLocalVelocity.z, m_CurrentLocalMoveInputVector.z * m_Configs.BaseSwimSpeed, tZ * deltaTime);
			}
			m_CurrentWorldVelocity = m_Head.TransformDirection(m_CurrentLocalVelocity);

			// TODO Apply gravity and floating force

			m_CollisionFlags = m_CharacterController.Move(m_CurrentWorldVelocity * deltaTime);

			// Consume input
			m_CurrentLocalMoveInputVector.Set(0f, 0f, 0f);
		}
	}
}
