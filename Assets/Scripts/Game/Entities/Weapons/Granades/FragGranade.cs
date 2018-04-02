using UnityEngine;
using System.Collections;


public class FragGranade : GranadeBase {
	

	private void Awake()
	{
		m_RigidBody	= GetComponent<Rigidbody>();
		m_Collider	= GetComponent<Collider>();
		m_Renderer	= GetComponent<Renderer>();

		m_RigidBody.mass						= float.Epsilon;
		m_RigidBody.interpolation				= RigidbodyInterpolation.Interpolate;
		m_RigidBody.collisionDetectionMode		= CollisionDetectionMode.ContinuousDynamic;

		SetActive( false );
	}


	public override	void	Setup( float damageMax, float radius, float explosionDelay, Entity whoRef, Weapon weapon )
	{
		m_WhoRef			= whoRef;
		m_Weapon			= weapon;
		m_DamageMax			= damageMax;
		m_Radius			= radius;
		m_ExplosionDelay	= explosionDelay;
	}


	public override	void	SetActive( bool state )
	{
		// Reset
		if ( state == false )
		{
			transform.position		= Vector3.zero;
			m_RigidBody.velocity	= Vector3.zero;
		}
		m_RigidBody.useGravity			= state;
		m_RigidBody.detectCollisions	= state;
		m_Collider.enabled				= state;
		m_Renderer.enabled				= state;
		m_Renderer.material.SetColor( "_EmissionColor", Color.red );
		m_InternalCounter				= m_ExplosionDelay;
		this.enabled					= state;
	}


	public	float	GetRemainingTime()
	{
		return Mathf.Clamp( m_InternalCounter, 0f, 10f );
	}

	public	float	GetRemainingTimeNormalized()
	{
		return 1f - (  m_InternalCounter / m_ExplosionDelay );
	}




	private	float	m_Theta = 0f;
	private void Update()	// called only if active
	{

		m_InternalCounter -= Time.deltaTime;
		if ( m_InternalCounter < 0 )
		{
			OnExplosion();
			return;
		}

		m_Theta += Time.deltaTime * 8f;

		float emissionValue = ( 1f + Mathf.Sin( m_Theta ) ) * 5f;
		m_Renderer.material.SetColor( "_EmissionColor", Color.red * emissionValue );

	}

	public override void	ForceExplosion()
	{
		OnExplosion();
	}


	protected override	void	OnExplosion()
	{
		Collider[] colliders = Physics.OverlapSphere( transform.position, m_Radius );
		foreach ( Collider hit in colliders )
		{
			Entity entity = hit.GetComponent<Entity>();
			if ( entity != null )
			{
//				float dmgMult = Vector3.Distance( transform.position, entity.transform.position ) / m_Radius;
//				print( entity.name + ": " + dmgMult * DamageMax );
				entity.OnHit( ref m_WhoRef, m_DamageMax );
				EffectManager.Instance.PlayOnHit( entity.transform.position, Vector3.up );
			}

			Rigidbody rb = hit.GetComponent<Rigidbody>();
            if ( entity == null && rb != null )
			{
                rb.AddExplosionForce( 1000, transform.position, m_Radius, 3.0F );
			}			
		}

		SetActive( false );
		m_InternalCounter = 0f;
	}

	private void OnCollisionEnter( Collision collision )
	{
		if ( collision.gameObject.GetComponent<Entity>() != null )
		{
			OnExplosion();
		}
	}

}
