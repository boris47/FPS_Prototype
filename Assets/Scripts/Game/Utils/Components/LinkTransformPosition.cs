
using UnityEngine;

public class LinkTransformPosition : MonoBehaviour
{
	public enum EUpdatePhase
	{
		Update, LateUpdate, FixedUpdate
	};

	[SerializeField]
	private EUpdatePhase m_UpdatePhase = EUpdatePhase.Update;

	[SerializeField]
	private Transform m_Source = null;

	[SerializeField]
	private Transform m_Target = null;

	[SerializeField]
	private Vector3 m_Offset = Vector3.zero;


	/////////////////////////////////////////////////////////
	private void Awake()
	{
		enabled = HaveValidEntities();
	}

	/////////////////////////////////////////////////////////
	public void SetSource(Transform InSource)
	{
		m_Source = InSource;
		enabled = HaveValidEntities();
	}

	/////////////////////////////////////////////////////////
	public void SetTarget(Transform InTarget)
	{ 
		m_Target = InTarget;
		enabled = HaveValidEntities();
	}

	/////////////////////////////////////////////////////////
	public void SetOffset(Vector3 InOffset) => m_Offset = InOffset;

	/////////////////////////////////////////////////////////
	private void FixedUpdate()
	{
		if ((enabled = HaveValidEntities()) && m_UpdatePhase == EUpdatePhase.FixedUpdate)
		{
			UpdatePosition();
		}
	}

	/////////////////////////////////////////////////////////
	private void Update()
	{
		if ((enabled = HaveValidEntities()) && m_UpdatePhase == EUpdatePhase.Update)
		{
			UpdatePosition();
		}
	}

	/////////////////////////////////////////////////////////
	private void LateUpdate()
	{
		if ((enabled = HaveValidEntities()) && m_UpdatePhase == EUpdatePhase.LateUpdate)
		{
			UpdatePosition();
		}
	}

	/////////////////////////////////////////////////////////
	private void UpdatePosition() => m_Target.position = m_Source.position + (m_Source.rotation * m_Offset);

	/////////////////////////////////////////////////////////
	private bool HaveValidEntities() => m_Source.IsNotNull() && m_Target.IsNotNull();
}
