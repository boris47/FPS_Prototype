
using UnityEngine;
using System.Collections.Generic;

public abstract partial class Entity {
	

	public	static	Entity	GetBestTarget( ref List<Entity> entities, Vector3 currentPosition )
	{
		float distance		= float.MaxValue;
		Entity bestResult	= null;
		RaycastHit hit;
		foreach( Entity entity in entities )
		{
			Vector3 direction = ( entity.transform.position - currentPosition );
			Debug.DrawLine
			(
				currentPosition, 
				entity.transform.position
			);

			if ( Physics.Raycast( currentPosition, direction, out hit ) )
			{
				if ( hit.transform != entity.transform )
					continue;

				float currentDistance = direction.sqrMagnitude;
				if ( currentDistance < distance )
				{
					distance = currentDistance;
					bestResult = entity;
				}
			}
		}
		return bestResult;
	}

}