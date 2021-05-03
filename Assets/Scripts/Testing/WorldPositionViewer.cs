using UnityEngine;

public class WorldPositionViewer : MonoBehaviour {

	[SerializeField, ReadOnly]
	private	Vector3			m_WorldPosition = Vector3.zero;

	private void OnValidate()
	{
		m_WorldPosition = transform.position;
	}

	private void OnEnable()
	{
		m_WorldPosition = transform.position;
	}

	private void OnDisable()
	{
		m_WorldPosition = transform.position;
	}

	private void Awake()
	{
		m_WorldPosition = transform.position;
	}

	
	void Start ()
	{
		m_WorldPosition = transform.position;
	}
	
}
