using UnityEngine;

public class OnHitEventGrabbedHandler : MonoBehaviour
{
	[SerializeField]
	private			Interactions_Base		m_InteractionsBase				= null;


	private void OnCollisionEnter(Collision collision)
	{
		if (collision.transform.HasComponent<Bullet>())
		{
			m_InteractionsBase.DropGrabbedObject();
			Destroy(this);
		}
	}

	internal void Setup(Interactions_Base interactions_Base)
	{
		m_InteractionsBase = interactions_Base;
	}
}
