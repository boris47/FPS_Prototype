using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Platform : MonoBehaviour
{
	[SerializeField]
	private			UnityEvent				m_OnEnter;
	[SerializeField]
	private			UnityEvent				m_OnExit;


	//////////////////////////////////////////////////////////////////////////
	private void Awake()
	{
		bool bGoodInitialization = Utils.CustomAssertions.IsTrue(gameObject.TrySearchComponent(ESearchContext.LOCAL, out Collider _, c => !c.isTrigger), this, $"Platform {name} has no non trigger collider");
		if (bGoodInitialization &= Utils.CustomAssertions.IsTrue(gameObject.TrySearchComponent(ESearchContext.LOCAL, out Collider triggerCollider, c => c.isTrigger), this, $"Platform {name} has no trigger collider"))
		{
			foreach(RaycastHit hit in Physics.BoxCastAll(triggerCollider.bounds.center, triggerCollider.bounds.extents, transform.up, transform.rotation, -1, Physics.AllLayers, QueryTriggerInteraction.Ignore))
			{
				if (hit.transform.parent == null)
				{
					hit.transform.SetParent(transform, worldPositionStays: true);
				}
			}
			Debug.DrawLine(triggerCollider.bounds.center, Vector3.up, Color.blue, 10f); 
		}
		enabled = bGoodInitialization & Utils.CustomAssertions.IsTrue(gameObject.TryGetComponent(out Rigidbody _), this, $"Platform {name} has no rigidbody");
	}

	/*
	//////////////////////////////////////////////////////////////////////////
	private void OnCollisionEnter(Collision collision)
	{
		if (collision.rigidbody)
		{
			collision.rigidbody.isKinematic = true;
		//	FixedJoint fixedJoint = gameObject.AddComponent<FixedJoint>();
		//	fixedJoint.connectedBody = collision.rigidbody;

		//	fixedJoint.breakForce = float.PositiveInfinity;
		//	fixedJoint.breakTorque = float.PositiveInfinity;
		}
		else
		{
			if (!(Vector3.Dot(collision.contacts[0].normal, Vector3.down) > 0.5)) return;

			if (collision.transform.parent == null)
			{
				collision.transform.SetParent(transform, worldPositionStays: true);
			}
		}
		m_OnEnter?.Invoke();
	}

	//////////////////////////////////////////////////////////////////////////
	private void OnCollisionExit(Collision collision)
	{
		if (collision.rigidbody)
		{
			collision.rigidbody.isKinematic = false;
		//	GetComponent<FixedJoint>()?.Destroy();
		}
		else
		{
			if (collision.transform.parent == transform)
			{
				collision.transform.SetParent(null);
			}
		}
		m_OnExit?.Invoke();
	}
	*/

	private readonly Dictionary<Collider, Transform> m_MappedChildren = new Dictionary<Collider, Transform>();

	
	private void OnTriggerEnter(Collider other)
	{
		// only when player is on top of the platform
		//	if (!(Vector3.Dot(collision.contacts[0].normal, Vector3.down) > 0.5)) return;

		if (!other.isTrigger)
		{
			m_MappedChildren.Add(other, other.transform.transform.parent);
			other.transform.SetParent(transform, worldPositionStays: true);
		}
	}
	
	private void OnTriggerExit(Collider other)
	{
		if (m_MappedChildren.TryGetValue(other, out Transform parent))
		{
			other.transform.SetParent(parent);
			m_MappedChildren.Remove(other);
		}
	}
	
}
