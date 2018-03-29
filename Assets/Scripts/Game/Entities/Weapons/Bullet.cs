using UnityEngine;
using System.Collections;

[RequireComponent( typeof( Rigidbody ), typeof( Collider ) )]
public class Bullet : MonoBehaviour {
	
	public	Entity		WhoRef			= null;
	public	Weapon		Weapon			= null;
	public	float		DamageMin		= 0f;
	public	float		DamageMax		= 0f;
	public	bool		IsCloseRange	= false;
	public	bool		CanPenetrate	= false;
	public	Vector3		FirePosition	= Vector3.zero;
	public	float		Speed			= 15f;

	public	float		MaxLifeTime		= 3f;
	public	float		CurrentLifeTime	= 0f;


	private	Rigidbody	m_RigidBody		= null;
	public	Rigidbody	RigidBody
	{
		get { return m_RigidBody; }
	}

	private	Collider	m_Collider		= null;
	public	Collider	Collider
	{
		get { return m_Collider; }
	}
	private	Renderer	m_Renderer		= null;



	private void Awake()
	{
		m_RigidBody	= GetComponent<Rigidbody>();
		m_Collider	= GetComponent<Collider>();
		m_Renderer	= GetComponent<Renderer>();
	}


	private void OnEnable()
	{
		CurrentLifeTime = 0f;
	}

	private void Update()
	{
		CurrentLifeTime += Time.deltaTime;

		if ( CurrentLifeTime > MaxLifeTime )
		{
			SetActive( false );
		}

	}

	public	void	SetActive( bool state )
	{
		// Reset
		if ( state == false )
		{
			transform.position		= Vector3.zero;
			m_RigidBody.velocity	= Vector3.zero;
			FirePosition			= Vector3.zero;
		}
		m_RigidBody.detectCollisions = state;
		m_Collider.enabled = state;
		m_Renderer.enabled = state;
		this.enabled = state;
	}

	public	void	SetVelocity( Vector3 vector )
	{
		m_RigidBody.velocity = vector;
	}



	private void OnCollisionEnter( Collision collision )
	{
		// When hit another bullet reset both bullets
//		Bullet bullet = collision.gameObject.GetComponent<Bullet>();
//		if ( bullet != null )
//		{
//			bullet.SetActive( false );
			
//		}

		Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();
		if ( rb != null )
		{
			rb.AddForce( m_RigidBody.velocity );
		}

		this.SetActive( false );
	}

}
