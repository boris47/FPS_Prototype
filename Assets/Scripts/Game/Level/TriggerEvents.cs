
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
		bool bHasCollider = this.transform.SearchComponent( ref this.m_Collider, ESearchContext.LOCAL );

		if ( bHasCollider == false )
		{
			Destroy(this);
			return;
		}

		this.m_Collider.isTrigger = true; // ensure is used as trigger
		this.m_Collider.enabled = false;

		this.m_OnEnter.AddListener( ( GameObject go ) => { m_OnEnterEvent( go ); } );
		this.m_OnExit.AddListener ( ( GameObject go ) => { m_OnExitEvent( go ); } ); 

		GameManager.StreamEvents.OnSave += this.StreamEvents_OnSave;
		GameManager.StreamEvents.OnLoad += this.StreamEvents_OnLoad;
	}


	//////////////////////////////////////////////////////////////////////////
	private StreamUnit StreamEvents_OnSave( StreamData streamData )
	{
		// Skip if no required
		if (this.m_TriggerOnce == false )
			return null;

		StreamUnit streamUnit = streamData.NewUnit(this.gameObject );
		streamUnit.SetInternal( "HasTriggered", this.m_HasTriggered );

		return streamUnit;
	}


	//////////////////////////////////////////////////////////////////////////
	private StreamUnit StreamEvents_OnLoad( StreamData streamData )
	{
		// Skip if no required
		if (this.m_TriggerOnce == false )
			return null;

		// Get unit
		StreamUnit streamUnit = null;
		if ( streamData.GetUnit(this.gameObject, ref streamUnit ) == false )
		{
			return null;
		}

		// TRIGGERED
		this.m_HasTriggered = streamUnit.GetAsBool( "HasTriggered" );
		
		return streamUnit;
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnEnable()
	{
		UnityEngine.Assertions.Assert.IsNotNull
		(
			this.m_Collider,
			"TriggerEvents::OnEnable: m_Collider is a null reference"
		);

		this.m_Collider.enabled = true;
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnDisable()
	{
		UnityEngine.Assertions.Assert.IsNotNull
		(
			this.m_Collider,
			"TriggerEvents::OnDisable: m_Collider is a null reference"
		);

		this.m_Collider.enabled = false;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnTriggerEnter
	private void OnTriggerEnter( Collider other )
	{
		if (this.m_TriggerOnce == true && this.m_HasTriggered == true )
			return;

		if (this.m_Target && other.transform.root.GetInstanceID() != this.m_Target.transform.root.GetInstanceID() )
			return;

		bool bIsEntity = Utils.Base.SearchComponent( other.gameObject, out Entity entity, ESearchContext.CHILDREN );
		if ( bIsEntity && this.m_BypassEntityCheck == false && entity.CanTrigger() == false )
		{
			return;
		}

		this.m_HasTriggered = true;

		this.m_OnEnter?.Invoke( other.gameObject );
	}


	//////////////////////////////////////////////////////////////////////////
	// OnTriggerExit
	private void OnTriggerExit( Collider other )
	{
		if (this.m_TriggerOnce == true && this.m_HasTriggered == true )
			return;

		if (this.m_Target && other.transform.root.GetInstanceID() != this.m_Target.transform.root.GetInstanceID())
			return;

		this.m_OnExit?.Invoke( other.gameObject );
	}

}
