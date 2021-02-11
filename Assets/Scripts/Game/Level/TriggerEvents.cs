
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
	private		Renderer		m_Renderer				= null;


	//////////////////////////////////////////////////////////////////////////
	// Awake
	private void Awake()
	{
		bool bHasCollider = transform.TrySearchComponent(ESearchContext.LOCAL, out m_Collider);

		if ( bHasCollider == false )
		{
			Destroy(this);
			return;
		}

		m_Collider.isTrigger = true; // ensure is used as trigger
		m_Collider.enabled = false;

		m_OnEnter.AddListener( ( GameObject go ) => { m_OnEnterEvent( go ); } );
		m_OnExit.AddListener ( ( GameObject go ) => { m_OnExitEvent( go ); } ); 

		GameManager.StreamEvents.OnSave += StreamEvents_OnSave;
		GameManager.StreamEvents.OnLoad += StreamEvents_OnLoad;
	}


	//////////////////////////////////////////////////////////////////////////
	private StreamUnit StreamEvents_OnSave( StreamData streamData )
	{
		// Skip if no required
		if (m_TriggerOnce == false )
			return null;

		StreamUnit streamUnit = streamData.NewUnit(gameObject );
		streamUnit.SetInternal( "HasTriggered", m_HasTriggered );

		return streamUnit;
	}


	//////////////////////////////////////////////////////////////////////////
	private StreamUnit StreamEvents_OnLoad( StreamData streamData )
	{
		// Skip if no required
		if (m_TriggerOnce == false )
			return null;

		// Get unit
		StreamUnit streamUnit = null;
		if ( streamData.TryGetUnit(gameObject, out streamUnit ) == false )
		{
			return null;
		}

		// TRIGGERED
		m_HasTriggered = streamUnit.GetAsBool( "HasTriggered" );
		
		return streamUnit;
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnEnable()
	{
		UnityEngine.Assertions.Assert.IsNotNull
		(
			m_Collider,
			"TriggerEvents::OnEnable: m_Collider is a null reference"
		);

		m_Collider.enabled = true;
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnDisable()
	{
		UnityEngine.Assertions.Assert.IsNotNull
		(
			m_Collider,
			"TriggerEvents::OnDisable: m_Collider is a null reference"
		);

		m_Collider.enabled = false;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnTriggerEnter
	private void OnTriggerEnter( Collider other )
	{
		if (m_TriggerOnce == true && m_HasTriggered == true )
			return;

		if (m_Target && other.transform.root.GetInstanceID() != m_Target.transform.root.GetInstanceID() )
			return;

		bool bIsEntity = Utils.Base.TrySearchComponent( other.gameObject, ESearchContext.CHILDREN, out Entity entity );
		if ( bIsEntity && m_BypassEntityCheck == false && entity.CanTrigger() == false )
		{
			return;
		}

		m_HasTriggered = true;

		m_OnEnter?.Invoke( other.gameObject );
	}


	//////////////////////////////////////////////////////////////////////////
	// OnTriggerExit
	private void OnTriggerExit( Collider other )
	{
		if (m_TriggerOnce == true && m_HasTriggered == true )
			return;

		if (m_Target && other.transform.root.GetInstanceID() != m_Target.transform.root.GetInstanceID())
			return;

		m_OnExit?.Invoke( other.gameObject );
	}


	private void OnDrawGizmos()
	{
		if (TryGetComponent(out m_Renderer) && m_Renderer.enabled)
		{
			return; // avoid Z-fighting in the editor
		}

		if (transform.TrySearchComponent(ESearchContext.LOCAL, out m_Collider ) )
		{
			Matrix4x4 mat = Gizmos.matrix;
			Gizmos.matrix = transform.localToWorldMatrix;

			if (m_Collider is BoxCollider )
			{
				BoxCollider thisCollider = m_Collider as BoxCollider;
				Gizmos.DrawCube( Vector3.zero, thisCollider.size );
			}
		
			if (m_Collider is SphereCollider )
			{
				SphereCollider thisCollider = m_Collider as SphereCollider;
				Gizmos.DrawSphere( Vector3.zero, thisCollider.radius );
			}

			Gizmos.matrix = mat;
		}
	}

}
