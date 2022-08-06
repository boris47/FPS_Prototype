using UnityEngine;

public class IterableBoxVolume : IterableVolume
{
	[SerializeField, Range(0.25f, 5f)]
	private		float			m_StepSizeX		= 1.0f;

	[SerializeField, Range(0.25f, 5f)]
	private		float			m_StepSizeY		= 1.0f;

	[SerializeField, Range(0.25f, 5f)]
	private		float			m_StepSizeZ		= 1.0f;

	[SerializeField]
	private		bool			m_VerticalAlso	= false;


	public		float			StepSizeX
	{
		get => m_StepSizeX;
		set => m_StepSizeX = Mathf.Max( 0.01f, value );
	}

	public		float			StepSizeY
	{
		get => m_StepSizeY;
		set => m_StepSizeY = Mathf.Max( 0.01f, value );
	}

	public		float			StepSizeZ
	{
		get => m_StepSizeZ;
		set => m_StepSizeZ = Mathf.Max( 0.01f, value );
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
			OnPosition(transform.position + (transform.rotation * position));

			if ((position.x += m_StepSizeX) > halfExtentX)
			{
				if ((position.z += m_StepSizeZ) > halfExtentZ)
				{
					if (m_VerticalAlso)
					{
						if ((position.y += m_StepSizeY) > halfExtentY)
						{
							break;
						}
						position.z = -halfExtentZ;
					}
					else
					{
						break;
					}
				}

				position.x = -halfExtentX;
			}
		}
		return true;
	}
}
