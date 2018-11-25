using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteInEditMode] [RequireComponent( typeof( MeshFilter ), typeof( MeshRenderer ) )]
public class IterableVolume : MonoBehaviour {

	[SerializeField, Range( 0.01f, Mathf.Infinity )]
	private		float			m_StepSizeX		= 1.0f;

	[SerializeField, Range( 0.01f, Mathf.Infinity )]
	private		float			m_StepSizeY		= 1.0f;

	[SerializeField, Range( 0.01f, Mathf.Infinity )]
	private		float			m_StepSizeZ		= 1.0f;

	[SerializeField]
	private		bool			m_VerticalAlso	= false;


	public		float			StepSizeX
	{
		get { return m_StepSizeX; }
		set { m_StepSizeX = Mathf.Max( 0.01f, value ); }
	}

	public		float			StepSizeY
	{
		get { return m_StepSizeY; }
		set { m_StepSizeY = Mathf.Max( 0.01f, value ); }
	}

	public		float			StepSizeZ
	{
		get { return m_StepSizeZ; }
		set { m_StepSizeZ = Mathf.Max( 0.01f, value ); }
	}


	private		MeshRenderer	m_MeshRenderer	= null;
	private		MeshFilter		m_MeshFilter	= null;


	//////////////////////////////////////////////////////////////////////////
	private void Awake()
	{
		EnsureComponents();
	}

	//////////////////////////////////////////////////////////////////////////
	private void    Start()
	{
		m_MeshRenderer.enabled = false;

		GameObject go = GameObject.CreatePrimitive( PrimitiveType.Cube );
		{
			m_MeshFilter.mesh = go.GetComponent<MeshFilter>().sharedMesh;
			m_MeshRenderer.sharedMaterial = go.GetComponent<MeshRenderer>().sharedMaterial;
		}
		if ( Application.isEditor && Application.isPlaying == false )
		{
			DestroyImmediate( go );
		}
		else
		{
			Destroy( go );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private	void	EnsureComponents()
	{
		MeshRenderer renderer = null;
		if ( ( renderer = GetComponent<MeshRenderer>() ) == null )
		{
			renderer = gameObject.AddComponent<MeshRenderer>();
		}

		m_MeshRenderer = renderer;

		MeshFilter filter = null;
		if ( ( filter = GetComponent<MeshFilter>() ) == null )
		{
			filter = gameObject.AddComponent<MeshFilter>();
		}

		m_MeshFilter = filter;
	}


	//////////////////////////////////////////////////////////////////////////
	public	bool	IterateOver( System.Action<Vector3> OnPosition )
	{
		if ( OnPosition == null )
			return false;

		float extentsX = -transform.lossyScale.x / 2.0f;
		float extentsY = -transform.lossyScale.y / 2.0f;
		float extentsZ = -transform.lossyScale.z / 2.0f;

		float currentStepX = m_StepSizeX * transform.lossyScale.x;
		float currentStepY = m_StepSizeY * ( m_VerticalAlso ? transform.lossyScale.y : 0.0f );
		float currentStepZ = m_StepSizeZ * transform.lossyScale.z;

		Vector3 position = Vector3.zero;

		while ( true )
		{
			float currentX = extentsX + currentStepX;
			float currentY = extentsY + currentStepY;
			float currentZ = extentsZ + currentStepZ;

			position.Set( currentX, currentY, currentZ );

			OnPosition( transform.position + transform.rotation * position );

			currentStepX -= m_StepSizeX;
			if ( currentStepX <= 0.0f )
			{
				currentStepZ -= m_StepSizeZ;
				if ( currentStepZ <= 0.0f )
				{
					if ( m_VerticalAlso == true )
					{
						currentStepY -= m_StepSizeY;
						if ( currentStepY <= 0.0f )
						{
							break;
						}
						currentStepZ = transform.lossyScale.z;
					}
					else
					{
						break;
					}
				}
				currentStepX = transform.lossyScale.x;
			}
		}
		return true;
	}

	private void OnDrawGizmosSelected()
	{
		IterateOver( ( Vector3 p ) => Gizmos.DrawSphere( p, 0.5f ) );
	}

}