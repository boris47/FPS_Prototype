
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

	[MenuItem( MENU_LABEL + "/SimulateOnlySelectedWithRaycastsDown" )]
	private	static	void	PhysicsActions_SimulateOnlySelectedWithRaycasts()
	{
		SimulateOnlySelectedWithRaycasts(Space.World, Vector3.down);
	}

	[MenuItem(MENU_LABEL + "/SimulateOnlySelectedWithRaycastsDown_Locally")]
	private static void PhysicsActions_SimulateOnlySelectedWithRaycastsLocalUp()
	{
		SimulateOnlySelectedWithRaycasts(Space.Self, Vector3.down);
	}

	[MenuItem(MENU_LABEL + "/SimulateOnlySelectedWithRaycastsForward_Locally")]
	private static void PhysicsActions_SimulateOnlySelectedWithRaycastsLocalForward()
	{
		SimulateOnlySelectedWithRaycasts(Space.Self, Vector3.forward);
	}

	private static void SimulateOnlySelectedWithRaycasts(Space space, Vector3 direction)
	{
		Transform[] transforms = UnityEditor.Selection.GetTransforms(SelectionMode.ExcludePrefab | SelectionMode.Editable | SelectionMode.OnlyUserModifiable);
		foreach(Transform t in transforms)
		{
			Quaternion rotation = t.rotation;
			Vector3 up = space == Space.World ? -direction : -rotation.GetVector(direction);

			if (t.TryGetComponent(out Collider collider))
			{
				if (t.TryGetComponent(out Rigidbody rigidBody))
				{
					if (rigidBody.constraints == RigidbodyConstraints.FreezePosition || rigidBody.constraints == RigidbodyConstraints.FreezePositionY)
						continue;
				}

				float halfHeight = collider.bounds.extents.y;
				Vector3 origin = t.position + (-up * halfHeight);
				if (Physics.Raycast(origin: origin, direction: direction, hitInfo: out RaycastHit hitInfo))
				{
					t.position = hitInfo.point + (up * halfHeight) + (up * 0.001f)/*Always leave a small space in order to avoid undesired collisons*/;
				}
			}
			else
			{
				Vector3 origin = t.position;
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
