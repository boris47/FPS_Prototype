
using UnityEngine;

public class DodgeTarget : MonoBehaviour {

	private static	LayerMask			m_Layer_Default				= 0;	// Default
	private	static	LayerMask			m_LayerIgnoreRaycast		= 2;	// Ignore Raycast
	private	static	ColorsCollection	m_ColorsCollection			= null;

	[SerializeField]
	private			AnimationCurve		m_DodgeTimeScaleCurve		= AnimationCurve.Linear( 0f, 1f, 1f, 1f );
	public			AnimationCurve		DodgeTimeScaleCurve
	{
		get { return this.m_DodgeTimeScaleCurve; }
	}

	[SerializeField]
	private			bool				m_HasTimeScaleCurveOverride	= false;
	public			bool				HasTimeScaleCurveOverride
	{
		get { return this.m_HasTimeScaleCurveOverride; }
	}

		
	private			Renderer			m_Renderer					= null;
	private			Transform			m_TextWorldSpace			= null;
	private			bool				m_IsActive					= true;



	////////////////////////////////////////////////////////////////////////////////////////
	// Awake
	private void	Awake()
	{
		// Get components and other refs
		this.m_Renderer				= this.GetComponent<Renderer>();
		this.m_TextWorldSpace		= this.transform.GetChild( 0 );
		this.m_TextWorldSpace.gameObject.SetActive( false );

		// Load colors collection
		if ( m_ColorsCollection == null )
		{
			m_ColorsCollection = Resources.Load<ColorsCollection>( "Scriptables/DodgeTargetColors" );
			if ( m_ColorsCollection == null )
			{
				print( "Cannot load \"Scriptables/DodgeTargetColors\"" );
				this.enabled = false;
				return;
			}

			if ( m_ColorsCollection.Colors == null || m_ColorsCollection.Colors.Length < 4 )
			{
				print( "Colors number is less than 4" );
				this.enabled = false;
				return;
			}
		}

		// If has not a custom time curve return
		if (this.m_HasTimeScaleCurveOverride == false )
			return;

		// PARSE CURVE IN ORDER TO GET MIN VALUE AND SET A CORRECT COLOR TO RENDER MATERIAL
#region CURVE PARSING
		float minValue = 1f;
		for ( float i = 0f; i < 1f; i += 0.05f )
		{
			float currentValue = this.m_DodgeTimeScaleCurve.Evaluate ( i );
			if ( currentValue < minValue )
			{
				minValue = currentValue;
			}
		}
		minValue = Mathf.Clamp( minValue, 0.05f, 1f );

		if ( minValue >= 0.7f )
		{
			this.m_Renderer.material.color = m_ColorsCollection.Colors[ 0 ];
			return;
		}

		if ( minValue < 0.7f && minValue >= 0.5f )
		{
			this.m_Renderer.material.color = m_ColorsCollection.Colors[ 1 ];
			return;
		}

		if ( minValue < 0.5f && minValue >= 0.25f )
		{
			this.m_Renderer.material.color = m_ColorsCollection.Colors[ 2 ];
			return;
		}

		if ( minValue < 0.25f && minValue >= 0f )
		{
			this.m_Renderer.material.color = m_ColorsCollection.Colors[ 3 ];
			return;
		}
#endregion
}


	////////////////////////////////////////////////////////////////////////////////////////
	// OnTargetReached
	public	void	OnTargetReached()
	{
		this.gameObject.layer = m_LayerIgnoreRaycast;
	}


	////////////////////////////////////////////////////////////////////////////////////////
	// OnReset
	public	void	OnReset()
	{
		this.m_Renderer.enabled = true;
		this.gameObject.layer = m_Layer_Default;
		this.m_IsActive = true;
	}


	////////////////////////////////////////////////////////////////////////////////////////
	// ShowText
	public	void	ShowText()
	{
		this.m_TextWorldSpace.gameObject.SetActive( true );
	}


	////////////////////////////////////////////////////////////////////////////////////////
	// HideText
	public	void	HideText()
	{
		this.m_TextWorldSpace.gameObject.SetActive( false );
	}


	////////////////////////////////////////////////////////////////////////////////////////
	// Enable
	public	void	Enable()
	{
		this.m_Renderer.enabled = true;
		this.gameObject.layer = m_Layer_Default;
		this.m_IsActive = true;
	}


	////////////////////////////////////////////////////////////////////////////////////////
	// Disable
	public	void	Disable()
	{
		this.m_Renderer.enabled = false;
		this.gameObject.layer = m_LayerIgnoreRaycast;
		this.m_IsActive = false;
	}


	////////////////////////////////////////////////////////////////////////////////////////
	// Update
	private void	Update()
	{
		if (this.m_IsActive == true )
		{
			Vector3 lookPoint = CameraControl.Instance.MainCamera.transform.position;
			lookPoint.y = this.m_TextWorldSpace.position.y;
			this.m_TextWorldSpace.LookAt( lookPoint );
		}
	}


	////////////////////////////////////////////////////////////////////////////////////////
	// OnTriggerEnter
	private void	OnTriggerEnter( Collider other )
	{
		string sName = other.gameObject.name;
		if ( sName[0] == 'P' && sName[2] == 'A' && sName[3] == 'T' ) // Player Near/Far Area Trigger
		{
			if ( sName[1] == 'N' )	// Near
			{
				this.Disable();
			}
			else					// Far
			{
				this.Enable();
			}
		}
	}


	////////////////////////////////////////////////////////////////////////////////////////
	// OnTriggerExit
	private void	OnTriggerExit( Collider other )
	{
		string sName = other.gameObject.name;
		if ( sName[0] == 'P' && sName[2] == 'A' && sName[3] == 'T' ) // Player Near/Far Area Trigger
		{
			if ( sName[1] == 'N' )	// Near
			{
				this.OnReset();
			}
			else					// Far
			{
				this.HideText();
				this.Disable();
			}
		}
	}
	
}