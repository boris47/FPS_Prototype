using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class EntityCollisionDetector : MonoBehaviour
{
	public	delegate	void		OnTriggerEventDel      (Collider obstacle);
	private static		List<EntityCollisionDetector>	s_Detectors						= new List<EntityCollisionDetector>();


	[Tooltip("If true RequestForCurrentState() evaluate as true if collider has value, otherwise false")]
	[SerializeField]
	private				bool							m_PositiveResult				= false;

	[SerializeField]
	private				Collider						m_Collider						= null;

	[SerializeField, ReadOnly]
	private				Rigidbody						m_Rigidbody						= null;

	[SerializeField, ReadOnly]
	private				Collider						m_CurrentCollider				= null;


	//--------------------
	private event		OnTriggerEventDel				m_OnTriggerEvent				= delegate { };

	/// <summary> Called every frame! </summary>
	public		event	OnTriggerEventDel				OnTriggerEvent
	{
		add		{ if (value.IsNotNull()) m_OnTriggerEvent += value; }
		remove	{ if (value.IsNotNull()) m_OnTriggerEvent -= value; }
	}

	public				bool							PositiveResult
	{
		get => m_PositiveResult;
		set => m_PositiveResult = value;
	}
	public				Collider						CurrentCollider					=> m_CurrentCollider;


	public float Height
	{
		get => m_Collider is CapsuleCollider capsule ? capsule.height : 0f;
		set
		{
			if (m_Collider is CapsuleCollider capsule)
			{
				capsule.height = value;
			}
		}
	}

	public float Radius
	{
		get => m_Collider is CapsuleCollider capsule ? capsule.height : m_Collider is SphereCollider sphere ? sphere.radius : 0f;
		set
		{
			if (m_Collider is CapsuleCollider capsule)
			{
				capsule.radius = value;
			}
			else
			{
				if (m_Collider is SphereCollider sphere)
				{
					sphere.radius = value;
				}
			}
		}
	}

	public	 Vector3 Center
	{
		get => m_Collider is CapsuleCollider capsule ? capsule.center : m_Collider is SphereCollider sphere ? sphere.center : Vector3.zero;
		set
		{
			if (m_Collider is CapsuleCollider capsule)
			{
				capsule.center = value;
			}
			else
			{
				if (m_Collider is SphereCollider sphere)
				{
					sphere.center = value;
				}
			}
		}
	}
	

	//////////////////////////////////////////////////////////////////////////
	private void Awake()
	{
		if ((m_Rigidbody.IsNotNull() || Utils.CustomAssertions.IsTrue(gameObject.TryGetComponent(out m_Rigidbody))) && (m_Collider.IsNotNull() || Utils.CustomAssertions.IsTrue(gameObject.TryGetComponent(out m_Collider))))
		{
			// Configure rigidbody
			m_Rigidbody.useGravity = false;
			m_Rigidbody.mass = float.Epsilon;
			m_Rigidbody.constraints = RigidbodyConstraints.FreezeAll;

			// Configure Collider
			m_Collider.isTrigger = true;

			bool bCurrentlyEnabled = m_Collider.enabled;
			m_Collider.enabled = false;

			foreach (Collider parentCollider in transform.root.GetComponentsInChildren<Collider>(includeInactive: true))
			{
				Physics.IgnoreCollision(m_Collider, parentCollider, ignore: true);
			}

			m_Collider.enabled = bCurrentlyEnabled;
			s_Detectors.Add(this);
		}
	}

	//////////////////////////////////////////////////////////////////////////
	public static void UpdateDetectors()
	{
		foreach (EntityCollisionDetector detector in s_Detectors)
		{
			foreach (Collider parentCollider in detector.transform.root.GetComponentsInChildren<Collider>(includeInactive: true))
			{
				Physics.IgnoreCollision(detector.m_Collider, parentCollider, ignore: true);

				// Invalidate Current collider, maybe should be keept, it's too early to know this atm
				detector.m_CurrentCollider = null;
			}
		}
	}

	//////////////////////////////////////////////////////////////////////////
	private void OnDestroy()
	{
		s_Detectors.Remove(this);
	}

	//////////////////////////////////////////////////////////////////////////
	private void OnValidate()
	{
		if (gameObject.TryGetComponent(out m_Collider))
		{
			m_Collider.isTrigger = true;
		}

		if (gameObject.TryGetComponent(out m_Rigidbody))
		{
			m_Rigidbody.mass = float.Epsilon;
			m_Rigidbody.constraints = RigidbodyConstraints.FreezeAll;
		}
	}

	//////////////////////////////////////////////////////////////////////////
	private void UpdateState()
	{
		m_OnTriggerEvent(m_CurrentCollider);
	}

	//////////////////////////////////////////////////////////////////////////
	public bool RequestForCurrentState()
	{
		if (m_PositiveResult)
		{
			return m_CurrentCollider.IsNotNull();
		}
		else
		{
			return m_CurrentCollider == null;
		}
	}
	

	//////////////////////////////////////////////////////////////////////////
	private void OnTriggerStay(Collider other)
	{
		if (!other.isTrigger)
		{
			m_CurrentCollider = other;
			UpdateState();
		}
	}

	//////////////////////////////////////////////////////////////////////////
	private void OnTriggerExit(Collider other)
	{
		if (!other.isTrigger)
		{
			m_CurrentCollider = null;
			UpdateState();
		}
	}

	//////////////////////////////////////////////////////////////////////////
	private void OnDrawGizmos()
	{
		if (m_Collider.IsNotNull())
		{
	//		Gizmos.DrawSphere(m_Collider.transform.position + m_Collider.center, m_Collider.radius);
		}
	}
}
