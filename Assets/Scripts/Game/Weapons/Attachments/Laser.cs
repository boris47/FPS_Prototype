using UnityEngine;
using System.Collections;

public interface ILaser : IWeaponAttachment
{

}

[System.Serializable]
public class Laser : WeaponAttachment, ILaser
{
	[SerializeField]
	protected		float				m_ScaleFactor		= 0.03f;

	[SerializeField]
	protected		Color				m_Color				= Color.red;

	[SerializeField]
	protected		float				m_LaserLength		= 100f;
	public			float				LaserLength
	{
		get { return this.m_LaserLength; }
		set { this.m_LaserLength = value; }
	}

	[SerializeField, ReadOnly]
	protected		bool				m_HasHit			= false;
	public			bool				HasHit
	{
		get { return this.m_HasHit; }
	}

	protected		RaycastHit			m_RayCastHit		= default( RaycastHit );
	protected		RaycastHit			m_DefaultRaycastHit	= default( RaycastHit );
	public			RaycastHit			RayCastHit
	{
		get { return this.m_RayCastHit; }
	}

	protected		Transform			m_LaserTransform	= null;
	protected		Vector3				m_LocalScale		= new Vector3();
	protected		Renderer			m_Renderer			= null;


	//////////////////////////////////////////////////////////////////////////
	protected void Awake()
	{
		this.m_IsUsable &= this.transform.childCount > 0;
		if (this.m_IsUsable )
		{
			this.m_LaserTransform = this.transform.GetChild( 0 );
		}

		this.m_IsUsable &= this.transform.SearchComponent( ref this.m_Renderer, ESearchContext.CHILDREN );
		if (this.m_IsUsable )
		{
			this.m_Renderer.material.color = this.m_Color;
		}

		this.enabled = this.m_IsUsable;
	}


	private void OnEnable()
	{
		this.SetActive( true );
	}

	private void OnDisable()
	{
		this.SetActive( false );
	}


	//////////////////////////////////////////////////////////////////////////
	protected override void OnActivate()
	{
		if (this.m_IsUsable == false )
			return;

		this.m_LaserTransform.gameObject.SetActive( true );

		GameManager.UpdateEvents.OnFrame += this.InternalUpdate;
	}


	//////////////////////////////////////////////////////////////////////////
	protected override void OnDeactivated()
	{
		if (this.m_IsUsable == false )
			return;

		this.m_LaserTransform.gameObject.SetActive( false );

		if ( GameManager.UpdateEvents.IsNotNull() )
		{
			GameManager.UpdateEvents.OnFrame -= this.InternalUpdate;
		}
	}
	
	
	//////////////////////////////////////////////////////////////////////////
	protected void InternalUpdate( float DeltaTime )
	{
		if (this.m_IsUsable == false || this.m_IsAttached == false )
			return;

		// Save cpu
		if ( Time.frameCount % 15 == 0 )
			return;

		this.m_RayCastHit = this.m_DefaultRaycastHit;

		this.m_HasHit = Physics.Raycast( this.transform.position, this.transform.forward, out this.m_RayCastHit, this.m_LaserLength );//, Utils.LayersHelper.Layers_AllButOne( "Bullets" ) );

		float currentLength = this.HasHit ? this.m_RayCastHit.distance : this.m_LaserLength;

		 //if the additional decimal isn't added then the beam position glitches
		float beamPosition = currentLength * ( 0.5f + 0.0001f );

		this.m_LocalScale.Set(this.m_ScaleFactor, this.m_ScaleFactor, currentLength );
		this.m_LaserTransform.localScale		= this.m_LocalScale;
		this.m_LaserTransform.localPosition	= Vector3.forward * beamPosition;
	}

}
