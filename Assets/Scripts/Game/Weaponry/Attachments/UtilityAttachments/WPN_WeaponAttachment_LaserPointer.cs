using UnityEngine;
using System.Collections;

[System.Serializable]
public class WPN_WeaponAttachment_LaserPointer : WPN_BaseWeaponAttachment
{
	[SerializeField]
	protected		float				m_ScaleFactor		= 0.03f;

	[SerializeField]
	protected		Color				m_Color				= Color.red;

	[SerializeField]
	protected		float				m_LaserLength		= 100f;
	public			float				LaserLength
	{
		get => m_LaserLength;
		set => m_LaserLength = value;
	}

	[SerializeField, ReadOnly]
	protected		bool				m_HasHit			= false;
	public			bool				HasHit				=> m_HasHit;

	protected static RaycastHit			m_DefaultRaycastHit	= default( RaycastHit );
	protected		RaycastHit			m_RayCastHit		= default( RaycastHit );
	public			RaycastHit			RayCastHit			=> m_RayCastHit;


	protected		Transform			m_LaserTransform	= null;
	protected		Vector3				m_LocalScale		= new Vector3();
	protected		Renderer			m_Renderer			= null;


	//////////////////////////////////////////////////////////////////////////
	protected void Awake()
	{
		m_IsUsable &= transform.childCount > 0;
		if (m_IsUsable )
		{
			m_LaserTransform = transform.GetChild( 0 );
		}

		m_IsUsable &= transform.TrySearchComponent(ESearchContext.LOCAL_AND_CHILDREN, out m_Renderer );
		if (m_IsUsable )
		{
			m_Renderer.material.color = m_Color;
		}

		m_LaserTransform.gameObject.SetActive( false );
		enabled = m_IsUsable;
	}

	//////////////////////////////////////////////////////////////////////////
	public override bool ConfigureInternal(in Database.Section attachmentSection)
	{
		return true;
	}

	//////////////////////////////////////////////////////////////////////////
	private void OnEnable()
	{
		SetActive( true );
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnDisable()
	{
		SetActive( false );
	}


	//////////////////////////////////////////////////////////////////////////
	protected override void OnActivate()
	{
		if (m_IsUsable == false || m_IsAttached == false )
			return;

		m_LaserTransform.gameObject.SetActive( true );

		GameManager.UpdateEvents.OnFrame += InternalUpdate;
	}


	//////////////////////////////////////////////////////////////////////////
	protected override void OnDeactivated()
	{
		if (m_IsUsable == false || m_IsAttached == false )
			return;

		m_LaserTransform.gameObject.SetActive( false );

		if ( GameManager.UpdateEvents.IsNotNull() )
		{
			GameManager.UpdateEvents.OnFrame -= InternalUpdate;
		}
	}
	
	
	//////////////////////////////////////////////////////////////////////////
	protected void InternalUpdate( float DeltaTime )
	{
		if (m_IsUsable == false || m_IsAttached == false )
			return;

		// Save cpu
		if ( Time.frameCount % 15 == 0 )
			return;

		m_RayCastHit = m_DefaultRaycastHit;

		m_HasHit = Physics.Raycast( transform.position, transform.forward, out m_RayCastHit, m_LaserLength, Utils.LayersHelper.Layers_AllButOne( "Bullets" ) );

		float currentLength = HasHit ? m_RayCastHit.distance : m_LaserLength;

		 //if the additional decimal isn't added then the beam position glitches
		float beamPosition = currentLength * ( 0.5f + 0.0001f );

		m_LocalScale.Set(m_ScaleFactor, m_ScaleFactor, currentLength );
		m_LaserTransform.localScale		= m_LocalScale;
		m_LaserTransform.localPosition	= Vector3.forward * beamPosition;
	}
}
