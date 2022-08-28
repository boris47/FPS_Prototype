
using UnityEngine;

namespace Entities.Player.Components
{
	public class PlayerGrabInteractor : PlayerUseInteractor
	{
		private class RigidBodyStoredData
		{
			private					Rigidbody						m_Rigidbody							= null;
			private					float							m_GrabbedObjectMass					= 0f;
			private					bool							m_GrabbedObjectUseGravity			= false;
			private					RigidbodyInterpolation			m_RigidbodyInterpolation			= RigidbodyInterpolation.None;

			public void Set(Rigidbody rb)
			{
				m_Rigidbody = rb;
				m_GrabbedObjectMass = rb.mass;
				m_GrabbedObjectUseGravity = rb.useGravity;
				m_RigidbodyInterpolation = rb.interpolation;
			}

			public void Reset()
			{
				m_Rigidbody.mass = m_GrabbedObjectMass;
				m_Rigidbody.useGravity = m_GrabbedObjectUseGravity;
				m_Rigidbody.interpolation = m_RigidbodyInterpolation;
			}
		}

		[SerializeField, ReadOnly]
		private					GameObject						m_GrabPoint							= null;

		[SerializeField, ReadOnly]
		private					GrabInteractable				m_CurrentGrabbed					= null;

		public override			uint							Priority							=> 0u;

		private readonly		RigidBodyStoredData				m_RigidBodyStoredData				= new RigidBodyStoredData();


		//////////////////////////////////////////////////////////////////////////
		protected override void Awake()
		{
			base.Awake();

			m_GrabPoint = new GameObject("GrabPoint");
			m_GrabPoint.transform.SetParent(m_Owner.Head);
			m_GrabPoint.transform.localPosition = Vector3.zero;
			m_GrabPoint.transform.localRotation = Quaternion.identity;
			m_GrabPoint.transform.Translate(0f, 0f, Owner.Configs.UseDistance);
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnDestroy()
		{
			base.OnDestroy();

			m_GrabPoint.Destroy();
		}


		// IInteractor START
		//////////////////////////////////////////////////////////////////
		public override bool IsCurrentlyInteracting()
		{
			return m_CurrentGrabbed.IsNotNull();
		}
		//////////////////////////////////////////////////////////////////
		public override bool CanInteractWith(Interactable interactable)
		{
			return (interactable is GrabInteractable grabbable) && grabbable.CanInteract(m_Owner) && (grabbable.transform.position - Owner.Head.position).sqrMagnitude < Owner.Configs.UseDistanceSqr;
		}
		//////////////////////////////////////////////////////////////////
		public override void Interact(Interactable interactable)
		{
			if (Utils.CustomAssertions.IsValidCast(interactable, out GrabInteractable grabbable))
			{
				interactable.OnInteraction(m_Owner);

				m_GrabPoint.transform.localPosition = Vector3.forward * Vector3.Distance(Owner.Head.position, grabbable.transform.position);

				Rigidbody rb = grabbable.Rigidbody;
				rb.velocity = Vector3.zero;
				m_RigidBodyStoredData.Set(rb);

				m_CurrentGrabbed = grabbable;

				OnInteractorFoundInternal(grabbable);
			}
		}
		//////////////////////////////////////////////////////////////////
		public override void StopInteraction()
		{
			if (m_CurrentGrabbed.IsNotNull())
			{
				m_GrabPoint.transform.localPosition = Vector3.forward * Owner.Configs.UseDistance;

				m_RigidBodyStoredData.Reset();

			//	if (m_CurrentGrabbed.transform.TryGetComponent(out OnHitEventGrabbedHandler eventHandler))
			//	{
			//		Destroy(eventHandler);
			//	}

				m_GrabPoint.transform.Translate(0f, 0f, Owner.Configs.UseDistance);

				OnInteractorLostInternal(m_CurrentGrabbed);
				m_CurrentGrabbed = null;
			}
		}
		// IInteractor END

		//////////////////////////////////////////////////////////////////////////
		private void Update()
		{
			if (m_CurrentGrabbed.IsNotNull())
			{
				float distance = (m_CurrentGrabbed.transform.position - m_GrabPoint.transform.position).sqrMagnitude;
				if (distance > Owner.Configs.UseDistanceSqr + 0.1f)
				{
					StopInteraction();
				}
				else
				{
					Rigidbody rb = m_CurrentGrabbed.Rigidbody;
					rb.MoveRotation(m_Owner.Head.rotation);
					rb.angularVelocity = Vector3.zero;
					rb.velocity = (m_GrabPoint.transform.position - m_CurrentGrabbed.transform.position) / (Time.deltaTime * 4f);
				}
			}
		}
	}
}
