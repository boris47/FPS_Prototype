
using UnityEngine;


public class TriggerEvents : MonoBehaviour
{	
	public delegate void TargetTriggerDelegate(GameObject go);

	[SerializeField]
	private			GameEventArg1			m_OnEnter				= null;

	[SerializeField]
	private			GameEventArg1			m_OnExit				= null;

	[SerializeField]
	private			Entity					m_Target				= null;

	[SerializeField]
	private			bool					m_TriggerOnce			= false;

	[SerializeField]
	private			bool					m_BypassEntityCheck		= false;


	private			bool					m_HasTriggered			= false;
	private			Collider				m_Collider				= null;
	private	event TargetTriggerDelegate		m_OnEnterEvent		= delegate { };
	private	event TargetTriggerDelegate		m_OnExitEvent		= delegate { };


	public	event TargetTriggerDelegate		OnEnterEvent
	{
		add		{ if (value.IsNotNull()) m_OnEnterEvent += value; }
		remove	{ if (value.IsNotNull()) m_OnEnterEvent += value; }
	}

	public	event TargetTriggerDelegate		OnExitEvent
	{
		add		{ if (value.IsNotNull()) m_OnExitEvent += value; }
		remove	{ if (value.IsNotNull()) m_OnExitEvent += value; }
	}


	//////////////////////////////////////////////////////////////////////////
	private void Awake()
	{
		if (CustomAssertions.IsTrue(transform.TryGetComponent(out m_Collider)))
		{
			m_Collider.isTrigger = true; // ensure is used as trigger
			m_Collider.enabled = false;
		}

		m_OnEnter.AddListener(go => m_OnEnterEvent(go));
		m_OnExit.AddListener(go => m_OnExitEvent(go));

		if (CustomAssertions.IsNotNull(GameManager.StreamEvents))
		{
			GameManager.StreamEvents.OnSave += StreamEvents_OnSave;
			GameManager.StreamEvents.OnLoad += StreamEvents_OnLoad;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnDestroy()
	{
		if (GameManager.StreamEvents.IsNotNull())
		{
			GameManager.StreamEvents.OnSave -= StreamEvents_OnSave;
			GameManager.StreamEvents.OnLoad -= StreamEvents_OnLoad;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private bool StreamEvents_OnSave(StreamData streamData, ref StreamUnit streamUnit)
	{
		streamUnit = streamData.NewUnit(gameObject);
		streamUnit.SetInternal("HasTriggered", m_HasTriggered);
		streamUnit.SetInternal("TriggerOnce", m_TriggerOnce);
		return true;
	}


	//////////////////////////////////////////////////////////////////////////
	private bool StreamEvents_OnLoad(StreamData streamData, ref StreamUnit streamUnit)
	{
		// Get unit
		bool bResult = streamData.TryGetUnit(gameObject, out streamUnit);
		if (bResult)
		{
			// TRIGGERED
			m_HasTriggered = streamUnit.GetAsBool("HasTriggered");
		}
		return bResult;
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnEnable()
	{
		CustomAssertions.IsNotNull(m_Collider, "Collider is a null reference");

		m_Collider.enabled = true;
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnDisable()
	{
		CustomAssertions.IsNotNull( m_Collider, "Collider is a null reference" );

		m_Collider.enabled = false;
	}


	//////////////////////////////////////////////////////////////////////////
	private static void OnEnter(TriggerEvents triggerEvents, GameObject gameObject)
	{
		if (triggerEvents.m_TriggerOnce)
		{
			triggerEvents.m_HasTriggered = true;
			triggerEvents.m_Collider.enabled = false;
		}
		triggerEvents.m_OnEnter.Invoke(gameObject);
	}


	//////////////////////////////////////////////////////////////////////////
	private static void OnExit(TriggerEvents triggerEvents, GameObject gameObject)
	{
		triggerEvents.m_OnExit.Invoke(gameObject);
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnTriggerEnter(Collider other)
	{
		if (Utils.Base.TrySearchComponent(other.gameObject, ESearchContext.LOCAL_AND_PARENTS, out Entity entity))
		{
			if (m_Target)
			{
				if (m_Target.GetInstanceID() != entity.GetInstanceID())
				{
					return; // Different instance ID
				}

				if (!m_BypassEntityCheck && !entity.CanTrigger())
				{
					return; // Entity cannot trigger
				}
			}

			OnEnter(this, other.gameObject);
		}

		// Interactable (No target only)
		if (!m_Target && Utils.Base.TrySearchComponent(other.gameObject, ESearchContext.LOCAL, out Interactable bump))
		{
			OnEnter(this, other.gameObject);
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnTriggerExit(Collider other)
	{
		// Entity
		if (Utils.Base.TrySearchComponent(other.gameObject, ESearchContext.LOCAL_AND_PARENTS, out Entity entity))
		{
			if (m_Target && m_Target.GetInstanceID() != entity.GetInstanceID())
			{
				return; // Different instance ID
			}

			OnExit(this, other.gameObject);
		}
		// Interactable
		else if (Utils.Base.TrySearchComponent(other.gameObject, ESearchContext.LOCAL, out Interactable bump))
		{
			OnExit(this, other.gameObject);
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnDrawGizmos()
	{
		if (TryGetComponent(out Renderer renderer) && renderer.enabled)
		{
			return; // avoid Z-fighting in the editor
		}

		if (transform.TrySearchComponent(ESearchContext.LOCAL, out m_Collider))
		{
			Matrix4x4 mat = Gizmos.matrix;
			Gizmos.matrix = transform.localToWorldMatrix;

			if (m_Collider is BoxCollider boxCollider)
			{
				Gizmos.DrawCube( Vector3.zero, boxCollider.size );
			}
		
			if (m_Collider is SphereCollider sphereCollider)
			{
				Gizmos.DrawSphere( Vector3.zero, sphereCollider.radius );
			}

			Gizmos.matrix = mat;
		}
	}

}
