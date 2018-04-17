using UnityEngine;
using System.Collections;

public interface IShield {

	float		Status				{ set; }
	Entity		Father				{ set; }
	bool		IsUnbreakable		{ set; }

	void		OnHit				( ref IBullet bullet );
	void		OnReset				();
}

[RequireComponent( typeof ( Collider ) )]
public class Shield : MonoBehaviour, IShield {

	[SerializeField]
	private		bool		m_IsUnbreakable		= false;
	public		bool		IsUnbreakable				{	get { return m_IsUnbreakable;  }	}
				bool		IShield.IsUnbreakable		{	set { m_IsUnbreakable = value; }	}

	private		float		m_Status			= 100f;
	public		float		Status						{	get { return m_Status; }			}
				float		IShield.Status				{	set { m_Status = value; }			}

	private		Entity		m_Father			= null;
				Entity		IShield.Father				{	set { m_Father = value; }			}

	private		Collider	m_Collider			= null;
	public		Collider	Collider					{	get { return m_Collider; }			}

	private		Renderer	m_Renderer			= null;


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
	public	void	OnHit( ref IBullet bullet )
	{
		float damage = Random.Range( bullet.DamageMin, bullet.DamageMax );
		m_Status -= damage;
		if ( m_Status <= 0f )
		{
			m_Renderer.enabled = false;
			m_Collider.enabled = false;
		}
	}

	
	//////////////////////////////////////////////////////////////////////////
	// OnCollisionEnter
	private void OnCollisionEnter( Collision collision )
	{
		// Skip if father is shiled owner
		IBullet bullet = collision.gameObject.GetComponent<IBullet>();
		if ( bullet == null )
			return;

//		EffectManager.Instance.PlayOnHit( transform.position, ( other.transform.position - transform.position ).normalized );

		if ( m_IsUnbreakable == true )
		{
			bullet.SetActive( false );
			return;
		}
		
		// Shiled take hit
		OnHit( ref bullet );

		// Penetration effect
		if ( m_Father != null && bullet.CanPenetrate == true && bullet.Weapon != null )
		{
			bullet.DamageMax *= 0.5f;
			bullet.DamageMin *= 0.5f;
			m_Father.OnHit( ref bullet );
		}
		bullet.SetActive( false );
	}


	//////////////////////////////////////////////////////////////////////////
	// OnReset
	void	IShield.OnReset()
	{
		m_Renderer.enabled = true;
		m_Collider.enabled = true;
	}


}
