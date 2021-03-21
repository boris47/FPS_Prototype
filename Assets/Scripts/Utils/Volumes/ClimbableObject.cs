
using UnityEngine;

public class ClimbableObject : MonoBehaviour
{
	[SerializeField]
	private		Collider					m_TriggerCollider			= null;

	[SerializeField]
	private		Collider					m_PhysicCollider			= null;

	[SerializeField, ReadOnly]
	private		Entity						m_Entity					= null;


	/////////////////////////////////////////////////////////////////
	private void Awake()
	{
		if (CustomAssertions.IsNotNull(m_TriggerCollider))
		{
			m_TriggerCollider.enabled = false;
			m_TriggerCollider.isTrigger = true;
		}

		if (CustomAssertions.IsNotNull(m_PhysicCollider))
		{
			m_PhysicCollider.enabled = true;
			m_PhysicCollider.isTrigger = false;
		}

		/*
		if (CustomAssertions.IsNotNull(m_PhysicCollider))
		{
			m_PhysicCollider.material = new PhysicMaterial()
			{
				dynamicFriction = 0f,
				staticFriction = 0f,
			};
		}
		*/
	}


	/////////////////////////////////////////////////////////////////
	private void OnEnable()
	{
		m_TriggerCollider.enabled = true;


	}


	/////////////////////////////////////////////////////////////////
	private void OnDisable()
	{
		m_TriggerCollider.enabled = false;
	}


	/////////////////////////////////////////////////////////////////
	private void OnTriggerEnter(Collider other)
	{
		if (!m_Entity)
		{
			if (other.transform.TrySearchComponent(ESearchContext.LOCAL_AND_PARENTS, out Entity entity))
			{
				entity.Motion.SetMotionType(EEntityMotionType.CLIMB, this);
				m_Entity = entity;
			}
		}
	}


	/////////////////////////////////////////////////////////////////
	private void OnTriggerExit(Collider other)
	{
		if (m_Entity)
		{
			if (other.transform.TrySearchComponent(ESearchContext.LOCAL_AND_PARENTS, out Entity entity) && entity.Id == m_Entity.Id)
			{
				entity.Motion.SetMotionType(EEntityMotionType.GROUNDED);
				m_Entity = null;
			}
		}
	}
}
