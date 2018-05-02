
using UnityEngine;

public class DashTarget : MonoBehaviour {

	private static	LayerMask			m_Layer_Default				= 0;	// Default
	private	static	LayerMask			m_LayerIgnoreRaycast		= 2;	// Ignore Raycast
	private	static	ColorsCollection	m_ColorsCollection			= null;

	[SerializeField]
	private			AnimationCurve		m_DashTimeScaleCurve		= AnimationCurve.Linear( 0f, 1f, 1f, 1f );
	public			AnimationCurve		DashTimeScaleCurve
	{
		get { return m_DashTimeScaleCurve; }
	}

	[SerializeField]
	private			bool				m_HasTimeScaleCurveOverride	= false;
	public			bool				HasTimeScaleCurveOverride
	{
		get { return m_HasTimeScaleCurveOverride; }
	}

		
	private			Renderer			m_Renderer					= null;
	private			Transform			m_TextWorldSpace			= null;
	private			bool				m_IsActive					= true;



	////////////////////////////////////////////////////////////////////////////////////////
	// Awake
	private void	Awake()
	{
		// Get components and other refs
		m_Renderer				= GetComponent<Renderer>();
		m_TextWorldSpace		= transform.GetChild( 0 );
		m_TextWorldSpace.gameObject.SetActive( false );

		// Load colors collection
		if ( m_ColorsCollection == null )
		{
			m_ColorsCollection = Resources.Load<ColorsCollection>( "Scriptables/DashTargetColors" );
			if ( m_ColorsCollection == null )
			{
				print( "Cannot load \"Scriptables/DashTargetColors\"" );
				enabled = false;
				return;
			}

			if ( m_ColorsCollection.Colors == null || m_ColorsCollection.Colors.Length < 4 )
			{
				print( "Colors number is less than 4" );
				enabled = false;
				return;
			}
		}

		// If has not a custom time curve return
		if ( m_HasTimeScaleCurveOverride == false )
			return;

		// PARSE CURVE IN ORDER TO GET MIN VALUE AND SET A CORRECT COLOR TO RENDER MATERIAL
#region CURVE PARSING
		float minValue = 1f;
		for ( float i = 0f; i < 1f; i += 0.05f )
		{
			float currentValue = m_DashTimeScaleCurve.Evaluate ( i );
			if ( currentValue < minValue )
			{
				minValue = currentValue;
			}
		}
		minValue = Mathf.Clamp( minValue, 0.05f, 1f );

		if ( minValue >= 0.7f )
		{
			m_Renderer.material.color = m_ColorsCollection.Colors[ 0 ];
			return;
		}

		if ( minValue < 0.7f && minValue >= 0.5f )
		{
			m_Renderer.material.color = m_ColorsCollection.Colors[ 1 ];
			return;
		}

		if ( minValue < 0.5f && minValue >= 0.25f )
		{
			m_Renderer.material.color = m_ColorsCollection.Colors[ 2 ];
			return;
		}

		if ( minValue < 0.25f && minValue >= 0f )
		{
			m_Renderer.material.color = m_ColorsCollection.Colors[ 3 ];
			return;
		}
#endregion
}


	////////////////////////////////////////////////////////////////////////////////////////
	// OnTargetReached
	public	void	OnTargetReached()
	{
		gameObject.layer = m_LayerIgnoreRaycast;
	}


	////////////////////////////////////////////////////////////////////////////////////////
	// OnReset
	public	void	OnReset()
	{
		m_Renderer.enabled = true;
		gameObject.layer = m_Layer_Default;
		m_IsActive = true;
	}


	////////////////////////////////////////////////////////////////////////////////////////
	// ShowText
	public	void	ShowText()
	{
		m_TextWorldSpace.gameObject.SetActive( true );
	}


	////////////////////////////////////////////////////////////////////////////////////////
	// HideText
	public	void	HideText()
	{
		m_TextWorldSpace.gameObject.SetActive( false );
	}


	////////////////////////////////////////////////////////////////////////////////////////
	// Enable
	public	void	Enable()
	{
		m_Renderer.enabled = true;
		gameObject.layer = m_Layer_Default;
		m_IsActive = true;
	}


	////////////////////////////////////////////////////////////////////////////////////////
	// Disable
	public	void	Disable()
	{
		m_Renderer.enabled = false;
		gameObject.layer = m_LayerIgnoreRaycast;
		m_IsActive = false;
	}


	////////////////////////////////////////////////////////////////////////////////////////
	// Update
	private void	Update()
	{
		if ( m_IsActive == true )
		{
			Vector3 lookPoint = Camera.main.transform.position;
			lookPoint.y = m_TextWorldSpace.position.y;
			m_TextWorldSpace.LookAt( lookPoint );
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
				Disable();
			}
			else					// Far
			{
				Enable();
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
				OnReset();
			}
			else					// Far
			{
				HideText();
				Disable();
			}
		}
	}
	
}