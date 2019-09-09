
// C# example.
using UnityEditor;
using System.Collections.Generic;

public class PhysicsActions 
{
	const string MENU_LABEL = "Physics";

	[MenuItem( MENU_LABEL + "/SimulateOnlySelectedWithRaycasts" )]
	private	static	void	PhysicsActions_SimulateOnlySelectedWithRaycasts()
	{

		// TODO
		/// - Collect all objects that need of physic simulation ( Pheraps just the selected ones in editor?? )
		/// - Add RigidBody if none is found
		/// - Execute simulation
		/// - Remove RigidBody if was added
		/// 

		
		UnityEngine.Transform[] transforms = UnityEditor.Selection.GetTransforms(SelectionMode.ExcludePrefab | SelectionMode.Editable | SelectionMode.OnlyUserModifiable );

		if ( transforms.Length == 0 ) return;
//		UnityEngine.Physics.autoSimulation = false;
		{
			for ( int i = 0; i < transforms.Length; i++ )
			{
				UnityEngine.Transform t = transforms[i];

				UnityEngine.Collider collider = null;
				if ( t.SearchComponent( ref collider, SearchContext.LOCAL ) )
				{
					UnityEngine.Rigidbody rigidBody = null;
					bool bHasRigidbody = t.SearchComponent( ref rigidBody, SearchContext.LOCAL );
					if ( bHasRigidbody )
					{
						if ( rigidBody.constraints == UnityEngine.RigidbodyConstraints.FreezePosition ||
							rigidBody.constraints == UnityEngine.RigidbodyConstraints.FreezePositionY )
							continue;
					}

					float halfHeight = collider.bounds.extents.y;
					UnityEngine.Vector3 origin = t.position + UnityEngine.Vector3.down * halfHeight;
					UnityEngine.Vector3 direction = UnityEngine.Vector3.down;

					UnityEngine.RaycastHit hitInfo;
					if ( UnityEngine.Physics.Raycast( origin: origin, direction: direction, hitInfo: out hitInfo ) )
					{
						t.position = hitInfo.point + UnityEngine.Vector3.up * halfHeight;
					}

				}
			}


		}
//		UnityEngine.Physics.autoSimulation = true;
	}
}