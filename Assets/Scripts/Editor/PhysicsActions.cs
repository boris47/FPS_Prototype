
using UnityEditor;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine;

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
				if ( t.TrySearchComponent(ESearchContext.LOCAL, out UnityEngine.Collider collider) )
				{
					if (t.TrySearchComponent(ESearchContext.LOCAL, out UnityEngine.Rigidbody rigidBody))
					{
						if ( rigidBody.constraints == UnityEngine.RigidbodyConstraints.FreezePosition ||
							rigidBody.constraints == UnityEngine.RigidbodyConstraints.FreezePositionY )
							continue;
					}

					float halfHeight = collider.bounds.extents.y;
					UnityEngine.Vector3 origin = t.position + (UnityEngine.Vector3.down * halfHeight);
					UnityEngine.Vector3 direction = UnityEngine.Vector3.down;

					if (UnityEngine.Physics.Raycast(origin: origin, direction: direction, hitInfo: out UnityEngine.RaycastHit hitInfo))
					{
						t.position = hitInfo.point + (UnityEngine.Vector3.up * halfHeight);
					}

				}
			}


		}
//		UnityEngine.Physics.autoSimulation = true;
	}

	[MenuItem( MENU_LABEL + "/SimulatePhysisStep" )]
	private static void PhysicsActions_SimulatePhysisStep()
	{
		Physics.autoSimulation = false;
		Physics.Simulate( 1 );
		Physics.autoSimulation = true;
	}
}
