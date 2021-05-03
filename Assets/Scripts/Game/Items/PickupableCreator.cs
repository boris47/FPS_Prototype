using UnityEngine;

public	static	class	PickupableItemSpawner {

	//////////////////////////////////////////////////////////////////////////
	public	static	bool	Spawn<T>( string PickupSectionName, Vector3 Position, Quaternion Rotation ) where T : Collider
	{
		GameObject go = new GameObject();

		// Collider
		Collider collider = go.AddComponent<T>();
		collider.isTrigger = true;

		// Pickupable component
		PickupableItem pickupable = go.AddComponent<PickupableItem>();
		if ( pickupable.SetPickupSectionName( PickupSectionName ) == false )
		{
			Object.Destroy( go );
			return false;
		}

		// Name ( Editor )
		go.name = PickupSectionName;

		return true;
	}


	//////////////////////////////////////////////////////////////////////////
	public	static	bool	Destroy( GameObject pickup )
	{
		if ( pickup && pickup.transform.HasComponent<PickupableItem>() )
		{
			Object.Destroy( pickup );
			return true;
		}
		return false;
	}
}
