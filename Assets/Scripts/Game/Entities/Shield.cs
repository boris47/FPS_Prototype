using UnityEngine;
using System.Collections;

public interface IShield {

	float		Status				{ set; }
	Entity		Parent				{ set; }
	bool		IsUnbreakable		{ set; }

	void		OnHit				( IBullet bullet );
	void		OnReset				();
}

[RequireComponent( typeof ( Collider ) )]
public class Shield : MonoBehaviour, IShield {


	bool		IShield.IsUnbreakable		{	set { m_IsUnbreakable = value; } }
	Entity		IShield.Parent				{	set { m_Parent = value; }			}
	float		IShield.Status				{	set { m_Status = m_StartStatus = value; } }


	[SerializeField]
	private		bool		m_IsUnbreakable		= false;

	public		bool		IsUnbreakable				{	get { return m_IsUnbreakable;  }	}
	public		Collider	Collider					{	get { return m_Collider; }			}
	public		float		Status						{	get { return m_Status; }			}


	private		Entity		m_Parent			= null;
	private		Collider	m_Collider			= null;
	private		Renderer	m_Renderer			= null;
	private		float		m_Status			= 100f;
	private		float		m_StartStatus		= 0.0f;


	//////////////////////////////////////////////////////////////////////////
	// Awake
	private void Awake()
	{
		Utils.Base.SearchComponent( gameObject, ref m_Parent, SearchContext.PARENT );
		Utils.Base.SearchComponent( gameObject, ref m_Renderer, SearchContext.LOCAL );
		Utils.Base.SearchComponent( gameObject, ref m_Collider, SearchContext.LOCAL );
	}


	//////////////////////////////////////////////////////////////////////////
	// OnHit
	public	void	OnHit( IBullet bullet )
	{
		float damage = Random.Range( bullet.DamageMin, bullet.DamageMax );
		m_Status -= damage;
		if ( m_Status <= 0f )
		{
			m_Renderer.enabled = false;
			m_Collider.enabled = false;
		}

		// Notify hit
		m_Parent.OnHit( transform.position, bullet.WhoRef, 0.0f );
	}


	//////////////////////////////////////////////////////////////////////////
	// OnHit
	public	void	OnHit( float damage )
	{
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
		
		// Shield take hit
		OnHit( bullet );

		// Penetration effect
		if ( m_Parent != null && bullet.CanPenetrate == true && bullet.Weapon != null )
		{
			bullet.DamageMax *= 0.5f;
			bullet.DamageMin *= 0.5f;
			m_Parent.OnHit( bullet );
		}
		bullet.SetActive( false );
	}


	//////////////////////////////////////////////////////////////////////////
	// OnReset
	void	IShield.OnReset()
	{
		m_Renderer.enabled = true;
		m_Collider.enabled = true;
		m_Status = m_StartStatus;
	}


}
