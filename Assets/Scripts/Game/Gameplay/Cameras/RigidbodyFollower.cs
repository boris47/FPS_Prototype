
using UnityEngine;

[DefaultExecutionOrder(0)]
public class RigidbodyFollower : MonoBehaviour
{
	[SerializeField]
	private Rigidbody m_Rigidbody = null;

	[SerializeField]
	private Transform m_Follower = null;


	//////////////////////////////////////////////////////////////////////////
	private void Awake()
	{
		enabled = Utils.CustomAssertions.IsNotNull(m_Rigidbody) && Utils.CustomAssertions.IsNotNull(m_Follower);
	}

	//////////////////////////////////////////////////////////////////////////
	private void Update()
	{
		m_Follower.position = m_Rigidbody.position;
		m_Follower.rotation = m_Rigidbody.rotation;
	}
}