using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class EntityFoots : MonoBehaviour
{

	public	delegate	void		OnGroundedChangeEvent      (Collider maybeCollider);

	[SerializeField, ReadOnly]
	private				SphereCollider					m_Collider					= null;

	[SerializeField, ReadOnly]
	private				Collider						m_CurrentCollider			= null;


	private	event		OnGroundedChangeEvent			m_OnGroundedChange			= delegate { };

	public		event	OnGroundedChangeEvent			OnGroundedChanged
	{
		add		{ if (value.IsNotNull()) m_OnGroundedChange += value; }
		remove	{ if (value.IsNotNull()) m_OnGroundedChange -= value; }
	}
	public Collider CurrentCollider => m_CurrentCollider;
	

	//////////////////////////////////////////////////////////////////////////
	private void Awake()
	{
		if (m_Collider.IsNotNull() || Utils.CustomAssertions.IsTrue(gameObject.TryGetComponent(out m_Collider), this))
		{
			m_Collider.isTrigger = true;

			if (Utils.CustomAssertions.IsNotNull(transform.parent, this))
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
	private void OnTriggerEnter(Collider other)
	{
		if (!other.isTrigger)
		{
			m_OnGroundedChange(m_CurrentCollider = other);
		}
	}

//	//////////////////////////////////////////////////////////////////////////
//	private void OnTriggerStay(Collider other)
//	{
//		if (!other.isTrigger)
//		{
//			m_CurrentCollider = other;
//			UpdateState();
//		}
//	}

	//////////////////////////////////////////////////////////////////////////
	private void OnTriggerExit(Collider other)
	{
		// If is not a trigger and is the current then set null otherwise another collider is stored and we must keep it's reference
		if (!other.isTrigger && m_CurrentCollider == other)
		{
			m_OnGroundedChange(m_CurrentCollider = null);
		}
	}
	
	//////////////////////////////////////////////////////////////////////////
	private void OnDrawGizmos()
	{
		Gizmos.DrawSphere(m_Collider.transform.position, m_Collider.radius);
	}
}
