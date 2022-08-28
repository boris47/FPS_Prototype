
using UnityEngine;

namespace Entities.Player
{
	using Components;

	[RequireComponent(typeof(CharacterController))]
	[RequireComponent(typeof(PlayerController))]
	[RequireComponent(typeof(PlayerMotionManager))]
	[RequireComponent(typeof(PlayerInteractionManager))]
	[Configurable(nameof(m_Configs), "Player/" + nameof(Configs))]
	public partial class PlayerEntity : Entity
	{
		public new				PlayerController				Controller							=> m_Controller as PlayerController;


		[SerializeField, ReadOnly]
		private					PlayerConfiguration				m_Configs							= null;

		[SerializeField, ReadOnly]
		private					CharacterController				m_CharacterController				= null;

		[SerializeField, ReadOnly]
		private					PlayerMotionManager				m_PlayerMotionManager				= null;

		[SerializeField, ReadOnly]
		private					PlayerInteractionManager		m_PlayerInteractionManager			= null;

		//--------------------
		public					PlayerConfiguration				Configs								=> m_Configs;
		public					CharacterController				CharacterController					=> m_CharacterController;
		public					PlayerMotionManager				PlayerMotionManager					=> m_PlayerMotionManager;
		private					PlayerInteractionManager		PlayerInteractionManager			=> m_PlayerInteractionManager;


		//////////////////////////////////////////////////////////////////////////
		protected override void Awake()
		{
			base.Awake();

			Utils.CustomAssertions.IsTrue(this.TryGetConfiguration(out m_Configs));

			if (Utils.CustomAssertions.IsTrue(gameObject.TryGetIfNotAssigned(ref m_CharacterController)))
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

			if (Utils.CustomAssertions.IsTrue(gameObject.TryGetIfNotAssigned(ref m_PlayerMotionManager)))
			{

			}

			if (Head.IsNotNull())
			{
				Head.localPosition = Vector3.up * m_Configs.HeadHeight;
			}
		}

		//////////////////////////////////////////////////////////////////////////
		protected sealed override Transform GetHead()
		{
			Transform outValue = null;
			{
				Utils.CustomAssertions.IsTrue(transform.TrySearchComponentByChildName("Head", out outValue));
			}
			return outValue;
		}

		//////////////////////////////////////////////////////////////////////////
		protected sealed override Transform GetBody()
		{
			return transform;
		}

		//////////////////////////////////////////////////////////////////////////
		protected sealed override Transform GetTargetable()
		{
			return transform;
		}

		//////////////////////////////////////////////////////////////////////////
		protected sealed override Collider GetPrimaryCollider()
		{
			return m_CharacterController;
		}
	}
}
