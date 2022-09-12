using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Platform : MonoBehaviour
{
	[SerializeField]
	private			UnityEvent				m_OnEnter							= null;
	[SerializeField]
	private			UnityEvent				m_OnExit							= null;


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

	//////////////////////////////////////////////////////////////////////////
	private void OnTriggerEnter(Collider other)
	{
		if (!other.isTrigger)
		{
			if (other.TryGetComponent(out Entities.IPalatformAttachable attachable))
			{
				attachable.AttachTo(transform);
			}
		}
	}
	
	//////////////////////////////////////////////////////////////////////////
	private void OnTriggerExit(Collider other)
	{
		if (other.TryGetComponent(out Entities.IPalatformAttachable softParentable))
		{
			softParentable.Detach();
		}
	}
	
}
