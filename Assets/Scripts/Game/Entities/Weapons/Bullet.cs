using UnityEngine;
using System.Collections;

public interface IBullet {

	Rigidbody	RigidBody				{ get; }
	Collider	Collider				{ get; }

	Entity		WhoRef					{ get; }
	Weapon		Weapon					{ get; }
	float		DamageMin				{ get; }
	float		DamageMax				{ get; }
	bool		CanPenetrate			{ get; }

	void		Setup( float damageMin, float damageMax, Entity whoRef, Weapon weapon, bool canPenetrate );
	void		SetActive( bool state );
	void		SetVelocity( Vector3 vector );
}

[RequireComponent( typeof( Rigidbody ), typeof( Collider ) )]
public class Bullet : MonoBehaviour, IBullet {
	
	[SerializeField]
	private		float		Speed					= 15f;

	[SerializeField]
	private		float		m_Range					= 30f;

	public		Collider	Collider				{		get { return m_Collider; }		}

	private		Rigidbody	m_RigidBody				= null;
	private		Collider	m_Collider				= null;
	private		Entity		m_WhoRef				= null;
	private		Weapon		m_Weapon				= null;
	private		float		m_DamageMin				= 0f;
	private		float		m_DamageMax				= 0f;
	private		bool		m_CanPenetrate			= false;

	// INTERFACE
				Rigidbody	IBullet.RigidBody		{	get { return m_RigidBody; }		}
				Collider	IBullet.Collider		{	get { return m_Collider; }		}
				Entity		IBullet.WhoRef			{	get { return m_WhoRef; }		}
				Weapon		IBullet.Weapon			{	get { return m_Weapon; }		}
				float		IBullet.DamageMin		{	get { return m_DamageMin; }		}
				float		IBullet.DamageMax		{	get { return m_DamageMax; }		}
				bool		IBullet.CanPenetrate	{	get { return m_CanPenetrate; }	}



	private		Renderer	m_Renderer				= null;

	private		Vector3		m_StartPosition			= Vector3.zero;


	private void Awake()
	{
		m_RigidBody	= GetComponent<Rigidbody>();
		m_Collider	= GetComponent<Collider>();
		m_Renderer	= GetComponent<Renderer>();

		m_RigidBody.useGravity					= false;
		m_RigidBody.interpolation				= RigidbodyInterpolation.Interpolate;
		m_RigidBody.collisionDetectionMode		= CollisionDetectionMode.ContinuousDynamic;
		m_RigidBody.maxAngularVelocity			= 0f;
	}


	public	void	Setup( float damageMin, float damageMax, Entity whoRef, Weapon weapon, bool canPenetrate )
	{
		m_DamageMin		= damageMin;
		m_DamageMax		= damageMax;
		m_WhoRef		= whoRef;
		m_Weapon		= weapon;
		m_CanPenetrate	= canPenetrate;
	}


	private	void	OnEnable()
	{
		m_RigidBody.angularVelocity = Vector3.zero;
	}

	private	void	Update()
	{
		float traveledDistance = ( m_StartPosition - transform.position ).sqrMagnitude;
		if ( traveledDistance > m_Range * m_Range )
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
		}
		m_StartPosition = transform.position;
		m_RigidBody.detectCollisions = state;
		m_Collider.enabled = state;
		m_Renderer.enabled = state;
		this.enabled = state;
	}

	public	void	SetVelocity( Vector3 vector )
	{
		m_RigidBody.velocity = vector.normalized * Speed;
	}



	private	void	OnCollisionEnter( Collision collision )
	{
		// When hit another bullet reset both bullets
//		Bullet bullet = collision.gameObject.GetComponent<Bullet>();
//		if ( bullet != null )
//		{
//			bullet.SetActive( false );
			
//		}

		Entity entity = collision.gameObject.GetComponent<Entity>();
		Shield shield = collision.gameObject.GetComponent<Shield>();
		if ( entity != null || shield != null )
		{
			EffectManager.Instance.PlayOnHit( collision.contacts[0].point, collision.contacts[0].normal );
		}


		Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();
		if ( rb != null )
		{
			rb.AddForce( m_RigidBody.velocity );
		}

		this.SetActive( false );
	}

}
