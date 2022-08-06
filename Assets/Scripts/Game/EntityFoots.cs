using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class EntityFoots : MonoBehaviour
{
	/*
	public	delegate	void		OnGroundedChangeEvent      (bool newState);

	[SerializeField, ReadOnly]
	private				SphereCollider					m_Collider					= null;

	[SerializeField, ReadOnly]
	private				bool							m_CurrentState				= false;

	private	event		OnGroundedChangeEvent			m_OnGroundedChange			= delegate { };

	public		event	OnGroundedChangeEvent			OnEvent_GroundedChanged
	{
		add		{ if (value.IsNotNull()) m_OnGroundedChange += value; }
		remove	{ if (value.IsNotNull()) m_OnGroundedChange -= value; }
	}

	private Collider[] m_ToIgnoreColliders = new Collider[0];

	//////////////////////////////////////////////////////////////////////////
	private void Awake()
	{
		if (m_Collider.IsNotNull() || Utils.CustomAssertions.IsTrue(gameObject.TryGetComponent(out m_Collider)))
		{
			m_Collider.isTrigger = true;

			m_ToIgnoreColliders = transform.root.GetComponentsInChildren<Collider>(includeInactive: true).Where(c => !c.isTrigger).ToArray();
		}
	}

	//////////////////////////////////////////////////////////////////////////
	private void OnValidate()
	{
		gameObject.TryGetComponent(out m_Collider);
	}

	//////////////////////////////////////////////////////////////////////////
	public bool RequestForCurrentState() => m_CurrentState;


	//////////////////////////////////////////////////////////////////////////
	private void Update()
	{
		bool newState = false;

		if (Physics.OverlapSphere(m_Collider.transform.position, m_Collider.radius, Physics.AllLayers, QueryTriggerInteraction.Ignore).Except(m_ToIgnoreColliders).Count() > 0)
		{
			newState = true;
		}

		if (m_CurrentState != newState)
		{
			m_CurrentState = newState;
			m_OnGroundedChange(m_CurrentState);
		}
	}
	*/

	
	public	delegate	void		OnGroundedChangeEvent      (bool newState);

	[SerializeField, ReadOnly]
	private				SphereCollider					m_Collider					= null;

	[SerializeField, ReadOnly]
	private				Collider						m_CurrentCollider			= null;


	private	event		OnGroundedChangeEvent			m_OnGroundedChange			= delegate { };

	public		event	OnGroundedChangeEvent			OnEvent_GroundedChanged
	{
		add		{ if (value.IsNotNull()) m_OnGroundedChange += value; }
		remove	{ if (value.IsNotNull()) m_OnGroundedChange -= value; }
	}
	
	//////////////////////////////////////////////////////////////////////////
	private void Awake()
	{
		if (m_Collider.IsNotNull() || Utils.CustomAssertions.IsTrue(gameObject.TryGetComponent(out m_Collider)))
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
		m_OnGroundedChange(m_CurrentCollider.IsNotNull());
	}

	//////////////////////////////////////////////////////////////////////////
	public bool RequestForCurrentState() => m_CurrentCollider.IsNotNull();
	
	//////////////////////////////////////////////////////////////////////////
	private void OnTriggerEnter(Collider other)
	{

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
		Gizmos.DrawSphere(m_Collider.transform.position, m_Collider.radius);
	}
}
