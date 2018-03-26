using UnityEngine;
using System.Collections;

public interface IShield {

	float		Status { set; }

}

[RequireComponent( typeof ( Collider ) )]
public class Shield : MonoBehaviour, IShield {

	private		float		m_Status		= 100f;
	public		float		Status
	{
		get { return m_Status; }
	}
				float		IShield.Status
	{
		set { m_Status = value; }
	}

	private		Entity		m_Father		= null;

	private		Renderer	m_Renderer		= null;
	private		Collider	m_Collider		= null;


	//////////////////////////////////////////////////////////////////////////
	// Awake
	private void Awake()
	{
		m_Father =  transform.parent.GetComponent<Entity>();

		m_Renderer = GetComponent<Renderer>();
		m_Collider = GetComponent<Collider>();
	}


	//////////////////////////////////////////////////////////////////////////
	// OnHit
	public	void	OnHit( Entity who, float damage )
	{
		m_Status -= damage;
		if ( m_Status <= 0f )
		{
			m_Renderer.enabled = false;
			m_Collider.enabled = false;
		}
	}

	
	//////////////////////////////////////////////////////////////////////////
	// OnTriggerEnter
	private void OnTriggerEnter( Collider other )
	{
		// Skip if father is shiled owner
		Bullet bullet = other.GetComponent<Bullet>();
		if ( bullet == null )
			return;

		if ( bullet.WhoRef == m_Father )
			return;
		
		// Reset bullet
		bullet.SetActive( false );
		
		// If shield is not "Broken"
		float damage	= Random.Range( bullet.DamageMin, bullet.DamageMax );
		Entity who		= bullet.WhoRef;
		OnHit( who,  damage );

		// Penetration effect
		if ( bullet.CanPenetrate == true && bullet.Weapon != null )
		{
			damage /=  2f;
			m_Father.OnHit( ref who, damage );
		}
	}
	

}
