
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
	private void UpdatePosition() => m_Target.position = m_Source.position;

	/////////////////////////////////////////////////////////
	private bool HaveValidEntities() => m_Source.IsNotNull() && m_Target.IsNotNull();
}
