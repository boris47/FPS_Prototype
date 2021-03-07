using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnHitEventGrabbedHandler : MonoBehaviour {

	private void OnCollisionEnter( Collision collision )
	{
		if ( collision.transform.HasComponent<Bullet>() )
		{
			Player.Instance.Interactions.DropGrabbedObject();
			Destroy(this);
		}
	}
}
