using UnityEngine;

public interface IEntityComponent_Interactions
{
	bool IsHoldingObject { get; }

	void GrabObject(GameObject obj);

	void DropGrabbedObject();
}

public abstract class Interactions_Base : EntityComponent, IEntityComponent_Interactions
{
	[SerializeField]
	protected	float						m_UseDistance					= 1f;
	[SerializeField, ReadOnly]
	protected	Interactable				m_Interactable					= null;
	[System.NonSerialized]
	protected	GameObject					m_GrabPoint						= null;
	[SerializeField, ReadOnly]
	protected	Grabbable					m_CurrentGrabbed				= null;
	[System.NonSerialized]
	protected	float						m_GrabbedObjectMass				= 0f;
	[System.NonSerialized]
	protected	bool						m_GrabbedObjectUseGravity		= false;
	[SerializeField, ReadOnly]
	protected	bool						m_CanGrabObjects				= true;

	[SerializeField, ReadOnly]
	protected	RaycastHit					m_RaycastHit					= default;
	[SerializeField, ReadOnly]
	protected	bool						m_HasRaycastHit					= false;

	public		bool						IsHoldingObject					{ get => m_CurrentGrabbed.IsNotNull(); }


	//////////////////////////////////////////////////////////////////////////
	protected override void Enable()
	{
		base.Enable();

		UnityEngine.Assertions.Assert.IsNotNull(GameManager.UpdateEvents);

		GameManager.UpdateEvents.OnFrame += OnFrame;

		m_GrabPoint = new GameObject("GrabPoint");
		m_GrabPoint.transform.SetParent(m_Entity.Head);
		m_GrabPoint.transform.localPosition = Vector3.zero;
		m_GrabPoint.transform.localRotation = Quaternion.identity;
		m_GrabPoint.transform.Translate(0f, 0f, m_UseDistance);
	}

	//////////////////////////////////////////////////////////////////////////
	protected override void Disable()
	{
		base.Disable();

		Destroy(m_GrabPoint);

		if (GameManager.UpdateEvents.IsNotNull())
		{
			GameManager.UpdateEvents.OnFrame -= OnFrame;
		}
	}

	//////////////////////////////////////////////////////////////////////////
	public override void OnLoad(StreamUnit streamUnit)
	{
		base.OnLoad(streamUnit);

		DropGrabbedObject();

		m_Interactable = null;
		m_RaycastHit = default;
	}

	//////////////////////////////////////////////////////////////////////////
	private void OnFrame(float deltaTime)
	{
		UpdateGrabbedObjectPosition(deltaTime);
	}


	//////////////////////////////////////////////////////////////////////////
	public bool InteractionPredicate()
	{
		bool bResult = m_HasRaycastHit; // entity must be pointing something
		bResult &= m_CurrentGrabbed == null; // hand must be free;
		bResult &= m_RaycastHit.distance <= m_UseDistance; // Pointed object must be in use distance range
		if (bResult &= m_Interactable.IsNotNull()) // There must be an interactable component on it
		{
			bResult &= m_Interactable.CanInteract; // The interactable can be interacted to
		}
		return bResult;
	}

	//////////////////////////////////////////////////////////////////////////
	public void InteractionAction()
	{
		//	m_Interactable.OnInteraction();

		if (m_Interactable.HasRetroInteraction && m_Interactable.HasBeenInteracted)
		{
			m_Interactable.OnRetroInteraction();
		}
		else
		{
			m_Interactable.OnInteraction();
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public bool GrabPredicate()
	{
		if (m_CurrentGrabbed.IsNotNull())
		{
			// An object is already grabbed, so in order to allow the entity to drop the
			// object predicate must return always true
			return true;
		}

		bool bResult = m_HasRaycastHit; // entity must be pointing something
		bResult &= m_RaycastHit.distance <= m_UseDistance; // Pointed object must be in use distance range
		if (bResult &= m_Interactable.IsNotNull()) // There must be an grabbable component on it
		{
			bResult &= m_Interactable.CanInteract; // The interactable can be interacted to
			bResult &= m_Interactable.transform.HasComponent<Grabbable>();
		}
		return bResult;
	}

	//////////////////////////////////////////////////////////////////////////
	public virtual void GrabObject(GameObject obj)
	{
		if (m_CurrentGrabbed.IsNotNull())
		{
			DropGrabbedObject();
			return;
		}

		UnityEngine.Assertions.Assert.IsNotNull(m_Interactable);

		Grabbable grabbable = m_Interactable.GetComponent<Grabbable>();
		UnityEngine.Assertions.Assert.IsNotNull(grabbable);

		m_CurrentGrabbed = grabbable;

		// GRAB ACTION
		m_CurrentGrabbed.OnInteraction();
		m_GrabPoint.transform.localPosition = Vector3.forward * Vector3.Distance(transform.position, m_CurrentGrabbed.transform.position);

		Rigidbody rb = m_CurrentGrabbed.RigidBody;
		m_GrabbedObjectMass = rb.mass; rb.mass = 1f;
		m_GrabbedObjectUseGravity = rb.useGravity; rb.useGravity = false;
		rb.velocity = Vector3.zero; rb.interpolation = RigidbodyInterpolation.Interpolate;
		m_CanGrabObjects = false;

		Physics.IgnoreCollision(m_Entity.PhysicCollider, m_CurrentGrabbed.Collider, ignore: true);

		m_CurrentGrabbed.gameObject.GetOrAddIfNotFound<OnHitEventGrabbedHandler>();
	}

	//////////////////////////////////////////////////////////////////////////
	public void UpdateGrabbedObjectPosition(float deltaTime)
	{
		if (m_CurrentGrabbed)
		{
			float distance = (m_CurrentGrabbed.Transform.position - m_GrabPoint.transform.position).sqrMagnitude;
			if (distance > (m_UseDistance * m_UseDistance) + 0.1f)
			{
				DropGrabbedObject();
				return;
			}

			Rigidbody rb = m_CurrentGrabbed.Interactable.RigidBody;
			rb.rotation = m_Entity.Head.rotation;
			rb.angularVelocity = Vector3.zero;
			rb.velocity = (m_GrabPoint.transform.position - m_CurrentGrabbed.Transform.position) / (deltaTime * 4f);
		}
	}

	//////////////////////////////////////////////////////////////////////////
	public virtual void DropGrabbedObject()
	{
		if (m_CurrentGrabbed.IsNotNull())
		{
			m_GrabPoint.transform.localPosition = Vector3.forward * m_UseDistance;

			Rigidbody rb		= m_CurrentGrabbed.RigidBody;
			rb.useGravity		= m_GrabbedObjectUseGravity;
			rb.mass				= m_GrabbedObjectMass;

			if (m_CurrentGrabbed.transform.TryGetComponent(out OnHitEventGrabbedHandler eventHandler))
			{
				Destroy( eventHandler );
			}

			m_CurrentGrabbed.OnRetroInteraction();

			Physics.IgnoreCollision(m_Entity.PhysicCollider, m_CurrentGrabbed.Collider, ignore: false);

			m_CurrentGrabbed	= null;
			m_CanGrabObjects	= true;
		}
	}
}

public class EntityComponentContainer_Interactions<T> : EntityComponentContainer where T : Interactions_Base, new()
{
	public override System.Type type { get; } = typeof(T);
}
