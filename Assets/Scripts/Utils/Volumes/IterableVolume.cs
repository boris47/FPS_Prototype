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
		get { return this.m_StepSizeX; }
		set { this.m_StepSizeX = Mathf.Max( 0.01f, value ); }
	}

	public		float			StepSizeY
	{
		get { return this.m_StepSizeY; }
		set { this.m_StepSizeY = Mathf.Max( 0.01f, value ); }
	}

	public		float			StepSizeZ
	{
		get { return this.m_StepSizeZ; }
		set { this.m_StepSizeZ = Mathf.Max( 0.01f, value ); }
	}


	private		MeshFilter		m_MeshFilter	= null;
	private		MeshRenderer	m_MeshRenderer	= null;


	//////////////////////////////////////////////////////////////////////////
	private void Awake()
	{
		// EnsureComponents

		// Mesh Filter
		MeshFilter filter = null;
		if ( ( filter = this.GetComponent<MeshFilter>() ) == null )
		{
			filter = this.gameObject.AddComponent<MeshFilter>();
		}
		this.m_MeshFilter = filter;

		// mesh renderer
		MeshRenderer renderer = null;
		if ( ( renderer = this.GetComponent<MeshRenderer>() ) == null )
		{
			renderer = this.gameObject.AddComponent<MeshRenderer>();
		}
		this.m_MeshRenderer = renderer;
	}

	//////////////////////////////////////////////////////////////////////////
	private void    Start()
	{
		this.m_MeshRenderer.enabled = false;

		GameObject go = GameObject.CreatePrimitive( PrimitiveType.Cube );
		{
			this.m_MeshFilter.mesh = go.GetComponent<MeshFilter>().sharedMesh;
			this.m_MeshRenderer.sharedMaterial = go.GetComponent<MeshRenderer>().sharedMaterial;
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
	public	bool	IterateOver( System.Action<Vector3> OnPosition )
	{
		if ( OnPosition == null )
			return false;

		float extentsX = -this.transform.lossyScale.x / 2.0f;
		float extentsY = -this.transform.lossyScale.y / 2.0f;
		float extentsZ = -this.transform.lossyScale.z / 2.0f;

		float currentStepX = this.m_StepSizeX * this.transform.lossyScale.x - this.m_StepSizeX * 0.5f;
		float currentStepZ = this.m_StepSizeZ * this.transform.lossyScale.z - this.m_StepSizeZ * 0.5f;
		float currentStepY = this.m_StepSizeY * this.transform.lossyScale.y - this.m_StepSizeY * 0.5f;

		Vector3 position = Vector3.zero;

		while ( true )
		{
			float currentX = extentsX + currentStepX;
			float currentY = extentsY + currentStepY;
			float currentZ = extentsZ + currentStepZ;

			position.Set( currentX, currentY, currentZ );

			OnPosition(this.transform.position + this.transform.rotation * position );

			currentStepX -= this.m_StepSizeX;
			if ( currentStepX <= 0.0f )
			{
				currentStepZ -= this.m_StepSizeZ;
				if ( currentStepZ <= 0.0f )
				{
					if (this.m_VerticalAlso == true )
					{
						currentStepY -= this.m_StepSizeY;
						if ( currentStepY <= 0.0f )
						{
							break;
						}
						currentStepZ = this.transform.lossyScale.z - this.m_StepSizeZ *0.5f;
					}
					else
					{
						break;
					}
				}
				currentStepX = this.transform.lossyScale.x - this.m_StepSizeX *0.5f;
			}
		}
		return true;
	}

	private void OnDrawGizmosSelected()
	{
		this.IterateOver( ( Vector3 p ) => Gizmos.DrawSphere( p, 0.25f ) );
	}

}