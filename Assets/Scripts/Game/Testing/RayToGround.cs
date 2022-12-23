using UnityEngine;


public class RayToGround : MonoBehaviour
{
#if UNITY_EDITOR
	private void OnDrawGizmos()
	{
		if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit outHit, Mathf.Infinity, Physics.AllLayers, QueryTriggerInteraction.Ignore))
		{
			Gizmos.DrawLine(transform.position, outHit.point);
		}
	}
#endif
}
