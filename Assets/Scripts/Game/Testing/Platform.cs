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
		bool bGoodInitialization = Utils.CustomAssertions.IsTrue(gameObject.TrySearchComponent(ESearchContext.LOCAL, out Collider _, c => c.isTrigger), $"Platform {name} has no trigger collider");
		bGoodInitialization &= Utils.CustomAssertions.IsTrue(gameObject.TrySearchComponent(ESearchContext.LOCAL, out Collider _, c => !c.isTrigger), $"Platform {name} has no non trigger collider");
		enabled = bGoodInitialization & Utils.CustomAssertions.IsTrue(gameObject.TryGetComponent(out Rigidbody _), $"Platform {name} has no rigidbody");
	}

	//////////////////////////////////////////////////////////////////////////
	private void OnTriggerEnter(Collider other)
	{
		if (other.transform.parent == null)
		{
			other.transform.SetParent(transform, worldPositionStays: true);
			m_OnEnter?.Invoke();
		}
	}

	//////////////////////////////////////////////////////////////////////////
	private void OnTriggerExit(Collider other)
	{
		if (other.transform.parent == transform)
		{
			other.transform.SetParent(null);
			m_OnExit?.Invoke();
		}
	}
}
