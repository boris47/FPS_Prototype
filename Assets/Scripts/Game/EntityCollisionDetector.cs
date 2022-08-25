using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class EntityCollisionDetector : MonoBehaviour
{
	public	delegate	void		OnAboveObstacleEvent      (Collider obstacle);

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
	private event		OnAboveObstacleEvent			m_OnAboveObstacleEvent			= delegate { };

	public		event	OnAboveObstacleEvent			OnAboveObstacle
	{
		add		{ if (value.IsNotNull()) m_OnAboveObstacleEvent += value; }
		remove	{ if (value.IsNotNull()) m_OnAboveObstacleEvent -= value; }
	}

	public bool PositiveResult { get => PositiveResult; set => m_PositiveResult = value; }


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
		}
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
		m_OnAboveObstacleEvent(m_CurrentCollider);
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
