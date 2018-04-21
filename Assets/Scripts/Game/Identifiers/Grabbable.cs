
using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class Grabbable : MonoBehaviour, IInteractable {

	private	Rigidbody	m_RigidBody		= null;
	public	Rigidbody	RigidBody		{ get { return m_RigidBody; } }

	[SerializeField]
	private	bool		m_CanInteract	= true;
	public	bool		CanInteract		{ get { return m_CanInteract; } set { m_CanInteract = value; } }


	void IInteractable.OnInteraction()
	{ }


	private void Awake()
	{
		m_RigidBody = GetComponent<Rigidbody>();
	}
}
