using UnityEngine;

public class IterableSphereVolume : IterableVolume
{
	[SerializeField, Range(0.25f, 5f)]
	private		float			m_Radius		= 1.0f;

	[SerializeField, Range(0.25f, 5f)]
	private		float			m_StepSize		= 1.0f;


	public		float			StepSize
	{
		get => m_StepSize;
		set => m_StepSize = Mathf.Max( 0.01f, value );
	}


	//////////////////////////////////////////////////////////////////////////
	public override bool IterateOver(System.Action<Vector3> OnPosition)
	{
		if ( OnPosition == null )
			return false;

		Vector3 lossyScale = transform.lossyScale;
		Vector3 position = Vector3.zero;

		float halfExtentX = lossyScale.x * 0.5f;
		float halfExtentY = lossyScale.y * 0.5f;
		float halfExtentZ = lossyScale.z * 0.5f;

		position = new Vector3(-halfExtentX, -halfExtentY, -halfExtentZ);
		while (true)
		{
			Vector3 lineEnd = transform.position + (transform.rotation * position);
			Utils.Math.LineSphereIntersection(transform.position, m_Radius, transform.position, lineEnd, out Vector3 newPosition);

			OnPosition(newPosition);

			if ((position.x += m_StepSize) > halfExtentX)
			{
				if ((position.z += m_StepSize) > halfExtentZ)
				{
					break;
				}

				position.x = -halfExtentX;
			}
		}
		return true;
	}
}
