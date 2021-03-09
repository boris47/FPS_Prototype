
using UnityEditor;
using UnityEngine;

public class PhysicsActions 
{
	const string MENU_LABEL = "Physics";
	
	// TODO
	/// - Collect all objects that need of physic simulation ( Pheraps just the selected ones in editor?? )
	/// - Add RigidBody if none is found
	/// - Execute simulation
	/// - Remove RigidBody if was added
	/// 

	[MenuItem( MENU_LABEL + "/SimulateOnlySelectedWithRaycasts" )]
	private	static	void	PhysicsActions_SimulateOnlySelectedWithRaycasts()
	{
		SimulateOnlySelectedWithRaycasts(Space.World);
	}

	[MenuItem(MENU_LABEL + "/SimulateOnlySelectedWithRaycastsLocalUp")]
	private static void PhysicsActions_SimulateOnlySelectedWithRaycastsLocalUp()
	{
		SimulateOnlySelectedWithRaycasts(Space.Self);
	}

	private static void SimulateOnlySelectedWithRaycasts(Space space)
	{
		Transform[] transforms = UnityEditor.Selection.GetTransforms(SelectionMode.ExcludePrefab | SelectionMode.Editable | SelectionMode.OnlyUserModifiable);
		foreach(Transform t in transforms)
		{
			Vector3 up = space == Space.World ? Vector3.up : t.up;
			if (t.TryGetComponent(out Collider collider))
			{
				if (t.TryGetComponent(out Rigidbody rigidBody))
				{
					if (rigidBody.constraints == RigidbodyConstraints.FreezePosition || rigidBody.constraints == RigidbodyConstraints.FreezePositionY)
						continue;
				}

				float halfHeight = collider.bounds.extents.y;
				Vector3 origin = t.position + (-up * halfHeight);
				Vector3 direction = -up;
				if (Physics.Raycast(origin: origin, direction: direction, hitInfo: out RaycastHit hitInfo))
				{
					t.position = hitInfo.point + (up * halfHeight) + (up * 0.001f)/*Always leave a small space in order to avoid undesired collisons*/;
				}
			}
			else
			{
				Vector3 origin = t.position;
				Vector3 direction = -up;
				if (Physics.Raycast(origin: origin, direction: direction, hitInfo: out RaycastHit hitInfo))
				{
					t.position = hitInfo.point + (up * 0.001f)/*Always leave a small space in order to avoid undesired collisons*/;
				}
			}
		}
	}

	[MenuItem( MENU_LABEL + "/SimulatePhysisStep" )]
	private static void PhysicsActions_SimulatePhysisStep()
	{
		Physics.autoSimulation = false;
		Physics.Simulate( 1 );
		Physics.autoSimulation = true;
	}
}
