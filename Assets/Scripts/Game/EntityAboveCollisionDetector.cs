using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class EntityAboveCollisionDetector : MonoBehaviour
{
	public	delegate	void		OnAboveObstacleEvent      (Collider obstacle);

	[SerializeField]
	private				SphereCollider					m_Collider					= null;

	[SerializeField, ReadOnly]
	private				Collider						m_CurrentCollider			= null;


	private	event		OnAboveObstacleEvent			m_OnAboveObstacleEvent			= delegate { };

	public		event	OnAboveObstacleEvent			OnAboveObstacle
	{
		add		{ if (value.IsNotNull()) m_OnAboveObstacleEvent += value; }
		remove	{ if (value.IsNotNull()) m_OnAboveObstacleEvent -= value; }
	}
	
	//////////////////////////////////////////////////////////////////////////
	private void Awake()
	{
		if (Utils.CustomAssertions.IsNotNull(m_Collider))
		{
			m_Collider.isTrigger = true;

			if (Utils.CustomAssertions.IsNotNull(transform.parent))
			{
				foreach (Collider parentCollider in transform.parent.GetComponents<Collider>())
				{
					Physics.IgnoreCollision(m_Collider, parentCollider, ignore: true);
				}
			}
		}
	}

	//////////////////////////////////////////////////////////////////////////
	private void OnValidate()
	{
		gameObject.TryGetComponent(out m_Collider);
	}

	//////////////////////////////////////////////////////////////////////////
	private void UpdateState()
	{
		m_OnAboveObstacleEvent(m_CurrentCollider);
	}

	//////////////////////////////////////////////////////////////////////////
	public bool RequestForCurrentState() => !m_CurrentCollider.IsNotNull();
	

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
