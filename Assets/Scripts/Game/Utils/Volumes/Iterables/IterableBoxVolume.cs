using UnityEngine;

public class IterableBoxVolume : IterableVolume
{
	[SerializeField, Min(1f)]
	private		uint			m_CountOnX		= 1u;

	[SerializeField, Min(1f)]
	private		uint			m_CountOnY		= 1u;

	[SerializeField, Min(1f)]
	private		uint			m_CountOnZ		= 1u;


	public		uint			CountOnX
	{
		get => m_CountOnX;
		set => m_CountOnX = (uint)Mathf.Max(value, 1u);
	}

	public		float			CountOnY
	{
		get => m_CountOnY;
		set => m_CountOnY = (uint)Mathf.Max(value, 1u);
	}

	public		float			CountOnZ
	{
		get => m_CountOnZ;
		set => m_CountOnZ = (uint)Mathf.Max(value, 1u);
	}

	//////////////////////////////////////////////////////////////////////////
	public override bool IsPointInside(in Vector3 InPoint)
	{
		return Utils.Math.IsPointInsideBox(InPoint, transform.position, transform.rotation, transform.lossyScale);
	}

	//////////////////////////////////////////////////////////////////////////
	public override void IterateOver(System.Action<Vector3, Vector3> OnPosition, in float InMargin = 0f, 
		in float? InCountOnX = null, in float? InCountOnY = null, in float? InCountOnZ = null)
	{
		if (OnPosition.IsNotNull())
		{
			Vector3 lossyScale = transform.lossyScale;

			float stepX = (lossyScale.x + InMargin) / (InCountOnX ?? m_CountOnX);
			float stepY = (lossyScale.y + InMargin) / (InCountOnY ?? m_CountOnY);
			float stepZ = (lossyScale.z + InMargin) / (InCountOnZ ?? m_CountOnZ);

			float halfExtentX = ((lossyScale.x - InMargin - InMargin) * 0.5f) - (stepX  * 0.5f);
			float halfExtentY = ((lossyScale.y - InMargin - InMargin) * 0.5f) - (stepY  * 0.5f);
			float halfExtentZ = ((lossyScale.z - InMargin - InMargin) * 0.5f) - (stepZ * 0.5f);

			Vector3 currentPosition = new Vector3(-halfExtentX, -halfExtentY, -halfExtentZ);
			Vector3 size = new Vector3(stepX, stepY, stepZ);
			while (true)
			{
				OnPosition(transform.position + (transform.rotation * currentPosition), size);

				if ((currentPosition.x += stepX) > halfExtentX)
				{
					if ((currentPosition.z += stepY) > halfExtentZ)
					{
						if ((currentPosition.y += stepZ) > halfExtentY)
						{
							break;
						}
						currentPosition.z = -halfExtentZ;
					}

					currentPosition.x = -halfExtentX;
				}
			}
		}
	}
	/*
	//////////////////////////////////////////////////////////////////////////
	private void OnDrawGizmosSelected()
	{
		const float sphereRadius = 0.07f;
		IterateOver((p, s) => Gizmos.DrawSphere(p, sphereRadius));
	}
	*/
}
