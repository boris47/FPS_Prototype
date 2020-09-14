using UnityEngine;
using System.Collections;

[System.Serializable]
public class SimpleLaser : MonoBehaviour
{
	[SerializeField]
	protected		float				m_ScaleFactor		= 0.03f;

	[SerializeField]
	protected		Color				m_Color				= Color.red;

	[SerializeField]
	protected		float				m_LaserLength		= 100f;

	protected		Transform			m_LaserTransform	= null;
	protected		Renderer			m_Renderer			= null;
	protected		Vector3				m_LocalScale		= new Vector3();

	private void Awake()
	{
		this.m_LaserTransform = this.transform.GetChild( 0 );

		if (this.enabled = this.transform.SearchComponent( ref this.m_Renderer, ESearchContext.CHILDREN ))
		{
			this.m_Renderer.material.color = this.m_Color;
		}
	}


	private void Update()
	{
		// Save cpu
		if ( Time.frameCount % 15 == 0 )
			return;

		bool bHasHit = Physics.Raycast( this.transform.position, this.transform.forward, out RaycastHit rayCastHit, this.m_LaserLength, Utils.LayersHelper.Layers_AllButOne( "Bullets" ) );


		float currentLength = bHasHit ? rayCastHit.distance : this.m_LaserLength;

		 //if the additional decimal isn't added then the beam position glitches
		float beamPosition = currentLength * ( 0.5f + 0.0001f );

		this.m_LocalScale.Set(this.m_ScaleFactor, this.m_ScaleFactor, currentLength );

		this.m_LaserTransform.localScale		= this.m_LocalScale;
		this.m_LaserTransform.localPosition	= Vector3.forward * beamPosition;
	}
}
