
using UnityEngine;

public class LinkTransformRotation : MonoBehaviour
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
	private void FixedUpdate()
	{
		if ((enabled = HaveValidEntities()) && m_UpdatePhase == EUpdatePhase.FixedUpdate)
		{
			m_Target.rotation = m_Source.rotation;
		}
	}

	/////////////////////////////////////////////////////////
	private void Update()
	{
		if ((enabled = HaveValidEntities()) && m_UpdatePhase == EUpdatePhase.Update)
		{
			m_Target.rotation = m_Source.rotation;
		}
	}

	/////////////////////////////////////////////////////////
	private void LateUpdate()
	{
		if ((enabled = HaveValidEntities()) && m_UpdatePhase == EUpdatePhase.LateUpdate)
		{
			m_Target.rotation = m_Source.rotation;
		}
	}

	/////////////////////////////////////////////////////////
	private bool HaveValidEntities() => m_Source.IsNotNull() && m_Target.IsNotNull();
}
