using UnityEngine;
using System.Collections;

[System.Serializable]
public class Laser : WeaponAttachment {

	[SerializeField]
	protected		float				m_ScaleFactor		= 0.03f;

	[SerializeField]
	protected		Color				m_Color				= Color.red;

	[SerializeField]
	protected		float				m_LaserLength		= 100f;
	public			float				LaserLength
	{
		get { return m_LaserLength; }
		set { m_LaserLength = value; }
	}

	[SerializeField, ReadOnly]
	protected		bool				m_HasHit			= false;
	public			bool				HasHit
	{
		get { return m_HasHit; }
	}
/*
	[SerializeField]
	private		LayerMask			m_LayerMaskToExclude = default( LayerMask );
	// Layer mask to exclude during raycast
	public		LayerMask			LayerMaskToExclude
	{
		get { return m_LayerMaskToExclude; }
		set { m_LayerMaskToExclude = value; }
	}
*/
	protected		RaycastHit			m_RayCastHit		= default( RaycastHit );
	protected		RaycastHit			m_DefaultRaycastHit	= default( RaycastHit );
	public			RaycastHit			RayCastHit
	{
		get { return m_RayCastHit; }
	}

	protected		Transform			m_LaserTransform	= null;
	protected		Vector3				m_LocalScale		= new Vector3();
	protected		Renderer			m_Renderer			= null;
	protected		bool				m_CanBeUsed			= true;

	//////////////////////////////////////////////////////////////////////////
	// Awake
	protected void Awake()
	{
		m_CanBeUsed &= transform.childCount > 0;
		if ( m_CanBeUsed )
		{
			m_LaserTransform = transform.GetChild( 0 );
		}

		m_CanBeUsed &= transform.SearchComponent( ref m_Renderer, SearchContext.CHILDREN );
		if ( m_CanBeUsed )
		{
			m_Renderer.material.color = m_Color;
		}

		enabled = m_CanBeUsed;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnEnable
	protected void OnEnable()
	{
		if ( m_CanBeUsed == false )
			return;

		m_LaserTransform.gameObject.SetActive( true );

		GameManager.UpdateEvents.OnFrame += InternalUpdate;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnDisable
	protected void OnDisable()
	{
		if ( m_CanBeUsed == false )
			return;

		m_LaserTransform.gameObject.SetActive( false );

		GameManager.UpdateEvents.OnFrame -= InternalUpdate;
	}

	
	//////////////////////////////////////////////////////////////////////////
	// Update
	protected void InternalUpdate( float DeltaTime )
	{
		if ( m_CanBeUsed == false )
			return;

		// Save cpu
		if ( Time.frameCount % 15 == 0 )
			return;

		m_RayCastHit = m_DefaultRaycastHit;

		m_HasHit = Physics.Raycast( transform.position, transform.forward, out m_RayCastHit, m_LaserLength, Utils.Base.LayersAllButOne( "Bullets" ) );

		float currentLength = HasHit ? m_RayCastHit.distance : m_LaserLength;

		 //if the additional decimal isn't added then the beam position glitches
		float beamPosition = currentLength * ( 0.5f + 0.0001f );

		m_LocalScale.Set( m_ScaleFactor, m_ScaleFactor, currentLength );
		m_LaserTransform.localScale		= m_LocalScale;
		m_LaserTransform.localPosition	= Vector3.forward * beamPosition;
	}
	
}
