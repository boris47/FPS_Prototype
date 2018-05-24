
using UnityEngine;

public abstract class ObjectActivator : MonoBehaviour {

	protected	Collider		m_Collider		= null;
	public		Collider		Collider
	{
		get { return m_Collider; }
	}

	protected	Rigidbody		m_Rigidbody		= null;
	public		Rigidbody		Rigidbody
	{
		get { return m_Rigidbody; }
	}


	//////////////////////////////////////////////////////////////////////////
	// Awake
	protected	virtual	void	Awake()
	{
		m_Collider	= GetComponent<Collider>();
		m_Rigidbody	= GetComponent<Rigidbody>();
	}

}
