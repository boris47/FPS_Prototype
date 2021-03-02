
using UnityEngine;

[System.Serializable]
public partial class Player : Human
{
	[Header("Player Properties")]

	private	const	float			BODY_DRAG						= 8f;

	private	static	Player			m_Instance						= null;
	public	static	Player			Instance						=> m_Instance;
	public	static	IEntity			Entity							= null;

	private		GameObject			m_GrabPoint						= null;
	[SerializeField, ReadOnly]
	private		Interactable		m_Interactable					= null;
	[SerializeField, ReadOnly]
	private		Grabbable			m_CurrentGrabbed				= null;

	[System.NonSerialized]
	protected	float				m_GrabbedObjectMass				= 0f;

	[System.NonSerialized]
	protected	bool				m_GrabbedObjectUseGravity		= false;
	protected	bool				m_CanGrabObjects				= true;

//	private		Vector3				m_Move							= Vector3.zero;

	private		RaycastHit			m_RaycastHit					= default;

	[SerializeField]
	private		bool				m_HasRaycasthit					= false;

	private		Collider			m_PlayerNearAreaTrigger			= null;
	public		Collider			PlayerNearAreaTrigger			=> m_PlayerNearAreaTrigger;

	private		Collider			m_PlayerFarAreaTrigger			= null;
	public		Collider			PlayerFarAreaTrigger			=> m_PlayerFarAreaTrigger;

	protected	override EEntityType			m_EntityType => EEntityType.ACTOR;

	//////////////////////////////////////////////////////////////////////////
	protected	override	void	Awake()
	{
		// Singleton
		if (m_Instance.IsNotNull())
		{
			Destroy(gameObject);
			return;
		}
		m_Instance = this;
		DontDestroyOnLoad(this);

		base.Awake();

		Entity = this as IEntity;
		transform.TrySearchComponentByChildName("PNAT", out m_PlayerNearAreaTrigger);
		transform.TrySearchComponentByChildName("PFAT", out m_PlayerFarAreaTrigger);

		// Player Components
		{
			// Foots
			UnityEngine.Assertions.Assert.IsTrue
			(
				Utils.Base.TrySearchComponent(gameObject, ESearchContext.LOCAL_AND_CHILDREN, out m_Foots),
				$"We need foots !!"
			);
			if (m_Foots.IsNotNull())
			{
				DisableCollisionsWith(m_Foots.Collider);
			}
		}

		// Player Data
		{
			// Walking
	//		m_SectionRef.AsMultiValue("Walk",		1, 2, 3, out m_WalkSpeed,	out m_WalkJumpCoef,		out m_WalkStamina);

			// Running
	//		m_SectionRef.AsMultiValue("Run",		1, 2, 3, out m_RunSpeed,	out m_RunJumpCoef,		out m_RunStamina);

			// Crouched
	//		m_SectionRef.AsMultiValue("Crouch",		1, 2, 3, out m_CrouchSpeed, out m_CrouchJumpCoef,	out m_CrouchStamina);

			m_FallDistanceThreshold		= m_SectionRef.AsFloat( "FallDistanceThreshold", m_FallDistanceThreshold);

			// Climbing
	//		m_ClimbSpeed				= m_SectionRef.AsFloat( "Climb", m_ClimbSpeed);

			// Jumping
			{
				m_SectionRef.AsMultiValue("Jump", 1, 2, out m_JumpForce, out m_JumpStamina);
			}

			// Stamina
			{
				m_StaminaRestore		= m_SectionRef.AsFloat("StaminaRestore", 0.0f);
				m_StaminaRunMin			= m_SectionRef.AsFloat("StaminaRunMin",  0.3f);
				m_StaminaJumpMin		= m_SectionRef.AsFloat("StaminaJumpMin", 0.4f);
			}
		}

		// Create grab point gameobject
		m_GrabPoint = new GameObject("GrabPoint");
		m_GrabPoint.transform.SetParent(m_HeadTransform);
		m_GrabPoint.transform.localPosition = Vector3.zero;
		m_GrabPoint.transform.localRotation = Quaternion.identity;
		m_GrabPoint.transform.Translate(0f, 0f, m_UseDistance);

		m_Health			= m_SectionRef.AsFloat("Health", 100.0f);
		m_RigidBody.mass	= m_SectionRef.AsFloat("phMass", 80.0f);
		m_RigidBody.maxAngularVelocity = 0f;
		m_RigidBody.useGravity = false;
		m_Stamina			= 1.0f;
		GroundSpeedModifier = 1.0f;
		IsGrounded			= false;
		m_IsActive			= true;
	}


	//////////////////////////////////////////////////////////////////////////
	protected	override	void	Start()
	{
		FPSEntityCamera.Instance.SetViewPoint(m_HeadTransform, transform); // the entity entity is rotating so we use that transform

		IsGrounded = false;
	}

	/*
	// TODO
	private void OnTransformParentChanged()
	{
		if(transform.parent.IsNotNull())
		{
			SetMotionType(EMotionType.Platform);
		}
		else
		{
			SetMotionType(EMotionType.Walking);
		}
	}
	// TODO
	*/

	//////////////////////////////////////////////////////////////////////////
	protected override void OnEnable()
	{
		base.OnEnable();

		SetMotionType(EMotionType.WALK); // Default
	}


	//////////////////////////////////////////////////////////////////////////
	protected override void OnDisable()
	{
		SetMotionType(EMotionType.NONE); // also Unbind bindings

		base.OnDisable();
	}


	//////////////////////////////////////////////////////////////////////////
	public					void	DisableCollisionsWith( in Collider collider, in bool bAlsoTriggerCollider = true )
	{
		if (bAlsoTriggerCollider)
		{
			Physics.IgnoreCollision( collider, m_TriggerCollider, ignore: true );
		}
		Physics.IgnoreCollision( collider, m_PhysicCollider, ignore: true );
		Physics.IgnoreCollision( collider, m_PlayerNearAreaTrigger, ignore: true );
		Physics.IgnoreCollision( collider, m_PlayerFarAreaTrigger, ignore: true );
		Physics.IgnoreCollision( collider, m_Foots.Collider, ignore: true );
	}


	//////////////////////////////////////////////////////////////////////////
	public		override	bool	CanTrigger()
	{
		if ( base.CanTrigger() == false )
			return false;
		return true;
	}


	//////////////////////////////////////////////////////////////////////////
	public					void	DropEntityDragged()
	{
		if (m_CurrentGrabbed.IsNotNull())
		{
			m_GrabPoint.transform.localPosition = Vector3.forward * m_UseDistance;

			Rigidbody rb		= m_CurrentGrabbed.Interactable.RigidBody;
			rb.useGravity		= m_GrabbedObjectUseGravity;
			rb.mass				= m_GrabbedObjectMass;

			if (m_CurrentGrabbed.Transform.TryGetComponent(out OnHitEventGrabbedHandler eventHandler))
			{
				Destroy( eventHandler );
			}

			m_CurrentGrabbed.Interactable.OnRetroInteraction();

			Physics.IgnoreCollision(m_PhysicCollider, m_CurrentGrabbed.Interactable.Collider, ignore: false);

			m_CurrentGrabbed			= null;
			m_CanGrabObjects	= true;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private					void	MoveGrabbedObject(float fixedDeltaTime)
	{
		if (m_IsActive && m_CurrentGrabbed)
		{
			float distance = (m_CurrentGrabbed.Transform.position - m_GrabPoint.transform.position ).sqrMagnitude;
			if ( distance > (m_UseDistance * m_UseDistance) + 0.1f )
			{
				DropEntityDragged();
				return;
			}

			Rigidbody rb = m_CurrentGrabbed.Interactable.RigidBody;
			rb.rotation = FPSEntityCamera.Instance.transform.rotation;
			rb.angularVelocity = Vector3.zero;
			rb.velocity = (m_GrabPoint.transform.position - m_CurrentGrabbed.Transform.position ) / ( fixedDeltaTime * 4f );
		//	* ( 1.0f - Vector3.Angle( transform.forward, CameraControl.Instance.transform.forward ) / CameraControl.CLAMP_MAX_X_AXIS );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private					void	CheckForInteraction(bool hasHit)
	{
		//// skip if no target
		//if ( hasHit == false )
		//{
		//	m_Interactable = null;
		//	return;
		//}

		//// skip if currently grabbing an object
		//if ( m_GrabbedObject != null )
		//	return;

		//// skip if currently dask is active
		//if ( m_IsDodging == true )
		//	return;

		//bool m_HasComponent = false;
		//// Distance check
		//if ( m_RaycastHit.distance <= m_UseDistance )
		//{
		//	m_HasComponent = Utils.Base.TrySearchComponent( m_RaycastHit.transform.gameObject,ref m_Interactable, SearchContext.LOCAL );
		//}

		//// ACTION INTERACT
		//if ( InputManager.Inputs.Use )
		//{
		//	if ( m_HasComponent && m_Interactable.CanInteract )
		//	{
		//		m_Interactable.OnInteraction();
		//	}
		//}
	}


	//////////////////////////////////////////////////////////////////////////
	private					bool	InteractionPredicate()
	{
		bool bResult = m_HasRaycasthit; // Player must be pointing something
		bResult &= m_CurrentGrabbed == null; // hand must be free;
		bResult &= m_RaycastHit.distance <= m_UseDistance; // Pointed object must be in use distance range
		if (bResult &= m_Interactable.IsNotNull()) // There must be an interactable component on it
		{
			bResult &= m_Interactable.CanInteract; // The interactable can be interacted to
		}
		return bResult;
	}


	//////////////////////////////////////////////////////////////////////////
	private					void	InteractionAction()
	{
	//	m_Interactable.OnInteraction();

		if (m_Interactable.HasRetroInteraction && m_Interactable.HasInteracted)
		{
			m_Interactable.OnRetroInteraction();
		}
		else
		{
			m_Interactable.OnInteraction();
		}

	}


	//////////////////////////////////////////////////////////////////////////
	private					bool	GrabPredicate()
	{
		if (m_CurrentGrabbed.IsNotNull())
		{
			// An object is already grabbed, so in order to allow the player to drop the
			// object predicate must return always true
			return true;
		}

		bool bResult = m_HasRaycasthit; // Player must be pointing something
		bResult &= m_RaycastHit.distance <= m_UseDistance; // Pointed object must be in use distance range
		if (bResult &= m_Interactable.IsNotNull()) // There must be an grabbable component on it
		{
			bResult &= m_Interactable.CanInteract; // The interactable can be interacted to
			bResult &= m_Interactable.transform.HasComponent<Grabbable>();
		}
		return bResult;
	}


	//////////////////////////////////////////////////////////////////////////
	private					void	GrabAction()
	{
		if (m_CurrentGrabbed.IsNotNull())
		{
			DropEntityDragged();
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

		Physics.IgnoreCollision(m_PhysicCollider, m_CurrentGrabbed.Collider, ignore: true);

		m_CurrentGrabbed.gameObject.GetOrAddIfNotFound<OnHitEventGrabbedHandler>();
	}


	//////////////////////////////////////////////////////////////////////////
	private					void	CheckForGrab( bool hasHit )
	{
		//// skip grab evaluation if dodging
		//if ( m_IsDodging == true )
		//	return;

		//// ACTION RELEASE
		//if ( InputManager.Inputs.Use )
		//{
		//	if ( m_GrabbedObject != null )
		//	{
		//		DropEntityDragged();
		//		return;
		//	}
		//}

		//// Skip if cannot grab objects
		//if ( m_CanGrabObjects == false )
		//	return;

		//// skip if no target
		//if ( hasHit == false )
		//	return;

		//	// GRAB ACTION
		//	Grabbable grabbable = null;
		//	bool m_HasComponent = Utils.Base.TrySearchComponent( m_RaycastHit.transform.gameObject, ref grabbable, SearchContext.LOCAL );
		//	if ( InputManager.Inputs.Use && m_HasComponent && grabbable.CanInteract )
		//	{
		//		m_GrabbedObject = grabbable;
		//		m_GrabPoint.transform.localPosition = Vector3.forward * Vector3.Distance( transform.position, grabbable.transform.position );

		//		Rigidbody rb				= grabbable.RigidBody;
		//		m_GrabbedObjectMass			= rb.mass;			rb.mass				= 1f;
		//		m_GrabbedObjectUseGravity	= rb.useGravity;	rb.useGravity		= false;
		//		rb.velocity					= Vector3.zero;		rb.interpolation	= RigidbodyInterpolation.Interpolate;
		//		m_CanGrabObjects			= false;

		//		Physics.IgnoreCollision( m_PhysicCollider, grabbable.Collider, ignore: true );
		//	}
	}

}
