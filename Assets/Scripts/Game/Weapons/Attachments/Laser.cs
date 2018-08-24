using UnityEngine;
using System.Collections;

public class Laser : WeaponAttachment {

	[SerializeField]
	private		float				m_ScaleFactor		= 0.03f;

	[SerializeField]
	private		Color				m_Color				= Color.red;

	[SerializeField]
	private		float				m_LaserLength		= 100f;
	public		float				LaserLength
	{
		get { return m_LaserLength; }
		set { m_LaserLength = value; }
	}

	[SerializeField, ReadOnly]
	private		bool				m_HasHit			= false;
	public		bool				HasHit
	{
		get { return m_HasHit; }
	}

	private		RaycastHit			m_RayCastHit		= default( RaycastHit );
	private		RaycastHit			m_DefaultRaycastHit	= default( RaycastHit );
	public		RaycastHit			RayCastHit
	{
		get { return m_RayCastHit; }
	}
	private		Transform			m_LaserTransform	= null;

	private		Vector3				m_LocalScale		= new Vector3();

	private		Renderer			m_Renderer			= null;

	//////////////////////////////////////////////////////////////////////////
	// Awake
	private void Awake()
	{
		m_LaserTransform = transform.GetChild( 0 );

		m_Renderer = GetComponentInChildren<Renderer>();
		m_Renderer.material.color = m_Color;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnDisable
	private void OnEnable()
	{
		m_LaserTransform.gameObject.SetActive( true );
	}


	//////////////////////////////////////////////////////////////////////////
	// OnDisable
	private void OnDisable()
	{
		m_LaserTransform.gameObject.SetActive( false );
	}

	
	//////////////////////////////////////////////////////////////////////////
	// Update
	private void Update()
	{
		// Save cpu
		if ( Time.frameCount % 5 == 0 )
			return;

		m_HasHit = Physics.Raycast( transform.position, transform.forward, out m_RayCastHit, m_LaserLength );

		float	currentLength = m_LaserLength;
		if ( HasHit )
			currentLength = m_RayCastHit.distance;
		else
			m_RayCastHit = m_DefaultRaycastHit;

		 //if the additional decimal isn't added then the beam position glitches
		float beamPosition = currentLength / ( 2f + 0.0001f );

		m_LocalScale.Set( m_ScaleFactor, m_ScaleFactor, currentLength );
		m_LaserTransform.localScale		= m_LocalScale;
		m_LaserTransform.localPosition	= Vector3.forward * beamPosition;
	}
	
}
