
using UnityEngine;


public class TriggerEvents : MonoBehaviour {
	
	public	delegate	void		TargetTriggerDelegate( GameObject go );

	[SerializeField]
	private		GameEventArg1			m_OnEnter				= null;

	[SerializeField]
	private		GameEventArg1			m_OnExit				= null;

	[SerializeField]
	private		GameObject				m_Target				= null;

	[SerializeField]
	private		bool					m_TriggerOnce			= false;

	[SerializeField]
	private		bool					m_BypassEntityCheck		= false;

	private		event TargetTriggerDelegate	m_OnEnterEvent	= delegate( GameObject go ) { };
	public event	TargetTriggerDelegate	OnEnterEvent
	{
		add		{ if ( value.IsNotNull() ) m_OnEnterEvent += value; }
		remove	{ if ( value.IsNotNull() ) m_OnEnterEvent += value; }
	}


	private		event TargetTriggerDelegate	m_OnExitEvent	= delegate( GameObject go ) { };
	public event	TargetTriggerDelegate	OnExitEvent
	{
		add		{ if ( value.IsNotNull() ) m_OnExitEvent += value; }
		remove	{ if ( value.IsNotNull() ) m_OnExitEvent += value; }
	}

	private		bool			m_HasTriggered			= false;

	private		Collider		m_Collider				= null;


	//////////////////////////////////////////////////////////////////////////
	// Awake
	private void Awake()
	{
		bool bHasCollider = transform.SearchComponent( ref m_Collider, SearchContext.LOCAL );

		if ( bHasCollider == false )
		{
			Destroy(this);
			return;
		}

		m_Collider.isTrigger = true; // ensure is used as trigger
		m_Collider.enabled = false;

		m_OnEnter.AddListener( ( GameObject go ) => { m_OnEnterEvent( go ); } ); 
		m_OnExit.AddListener ( ( GameObject go ) => { m_OnExitEvent( go ); } ); 

		GameManager.StreamEvents.OnSave += OnSave;
		GameManager.StreamEvents.OnLoad += OnLoad;
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnEnable()
	{
		m_Collider.enabled = true;
	}

	private void OnDisable()
	{
		m_Collider.enabled = false;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnSave ( override )
	private StreamUnit OnSave( StreamData streamData )
	{
		// Skip if no required
		if ( m_TriggerOnce == false )
			return null;

		StreamUnit streamUnit		= streamData.NewUnit( gameObject );
		streamUnit.SetInternal( "HasTriggered", m_HasTriggered );

		return streamUnit;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnLoad ( override )
	private StreamUnit OnLoad( StreamData streamData )
	{
		// Skip if no required
		if ( m_TriggerOnce == false )
			return null;

		// Get unit
		StreamUnit streamUnit = null;
		if ( streamData.GetUnit( gameObject, ref streamUnit ) == false )
		{
			return null;
		}
		
		// TRIGGERED
		m_HasTriggered = streamUnit.GetAsBool( "HasTriggered" );
		
		return streamUnit;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnTriggerEnter
	private void OnTriggerEnter( Collider other )
	{
		if ( m_TriggerOnce == true && m_HasTriggered == true )
			return;

		if ( m_Target && other.gameObject.GetInstanceID() != m_Target.GetInstanceID() )
			return;

		Entity entity = null;
		bool bIsEntity = Utils.Base.SearchComponent( other.gameObject, ref entity, SearchContext.ALL );
		if ( bIsEntity && m_BypassEntityCheck == false && entity.CanTrigger() == false )
		{
			return;
		}

		m_HasTriggered = true;

		if ( m_OnEnter.IsNotNull() )
		{
			m_OnEnter.Invoke( other.gameObject );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// OnTriggerExit
	private void OnTriggerExit( Collider other )
	{
		if ( m_TriggerOnce == true && m_HasTriggered == true )
			return;

		if ( m_Target && other.gameObject.GetInstanceID() != m_Target.GetInstanceID() )
			return;

		if ( m_OnExit.IsNotNull() )
		{
			m_OnExit.Invoke( other.gameObject );
		}
	}

}
