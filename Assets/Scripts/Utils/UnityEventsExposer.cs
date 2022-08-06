using UnityEngine;
using UnityEngine.Events;

public class UnityEventsExposer : MonoBehaviour
{
	public	UnityEvent			m_OnAwake				= null;
	public	UnityEvent			m_OnEnable				= null;
	public	UnityEvent			m_OnStart				= null;
	public	UnityEvent			m_OnDisable				= null;
	public	UnityEvent			m_OnDestroy				= null;

	public	UnityEvent			m_OnCollisionEnter		= null;
	public	UnityEvent			m_OnCollisionStay		= null;
	public	UnityEvent			m_OnCollisionExit		= null;

	public	UnityEvent			m_OnTriggerEnter		= null;
	public	UnityEvent			m_OnTriggerStay			= null;
	public	UnityEvent			m_OnTriggerExit			= null;


	// Awake is called when the script instance is being loaded
	private void Awake() => m_OnAwake?.Invoke();
	
	// This function is called when the object becomes enabled and active
	private void OnEnable() => m_OnEnable?.Invoke();
	
	// Start is called just before any of the Update methods is called the first time
	private void Start() => m_OnStart?.Invoke();
	
	// This function is called when the behaviour becomes disabled or inactive
	private void OnDisable() => m_OnDestroy?.Invoke();

	// This function is called when the MonoBehaviour will be destroyed
	private void OnDestroy() => m_OnDestroy?.Invoke();

	// OnCollisionEnter is called when this collider/rigidbody has begun touching another rigidbody/collider
	private void OnCollisionEnter(Collision collision) => m_OnCollisionEnter?.Invoke();

	// OnCollisionStay is called once per frame for every collider/rigidbody that is touching rigidbody/collider
	private void OnCollisionStay(Collision collision) => m_OnCollisionStay?.Invoke();

	// OnCollisionExit is called when this collider/rigidbody has stopped touching another rigidbody/collider
	private void OnCollisionExit(Collision collision) => m_OnCollisionExit?.Invoke();

	// OnTriggerEnter is called when the Collider other enters the trigger
	private void OnTriggerEnter(Collider other) => m_OnTriggerEnter?.Invoke();

	// OnTriggerStay is called once per frame for every Collider other that is touching the trigger
	private void OnTriggerStay(Collider other) => m_OnTriggerStay?.Invoke();

	// OnTriggerExit is called when the Collider other has stopped touching the trigger
	private void OnTriggerExit(Collider other) => m_OnTriggerExit?.Invoke();
}
