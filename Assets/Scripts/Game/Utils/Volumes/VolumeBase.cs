using UnityEngine;

public abstract class VolumeBase : MonoBehaviour
{
	//////////////////////////////////////////////////////////////////////////
	public abstract bool IsPointInside(in Vector3 InPoint);
}
