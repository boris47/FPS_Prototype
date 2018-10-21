using UnityEngine;
using System.Collections;

public interface IShield {

	float		Status				{ set; }
	Entity		Parent				{ set; }
	bool		IsUnbreakable		{ set; }

	void		OnHit				( Vector3 startPosition, Entity whoRef, Weapon weaponRef, float damage, bool canPenetrate = false );
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
	public		void		OnHit( Vector3 startPosition, Entity whoRef, Weapon weaponRef, float damage, bool canPenetrate = false )
	{
		if ( canPenetrate == true && weaponRef != null )
		{
			damage *= 0.5f;
			m_Parent.OnHit( startPosition, whoRef, damage );
		}

		// notify hit
		m_Parent.OnHit( startPosition, whoRef, 0.0f );

		// Shield damage
		TakeDamage( damage );
	}


	//////////////////////////////////////////////////////////////////////////
	// TakeDamage
	public	void	TakeDamage( float damage )
	{
		if ( m_IsUnbreakable == true )
		{
			return;
		}

		m_Status -= damage;
		if ( m_Status <= 0.0f )
		{
			m_Renderer.enabled = false;
			m_Collider.enabled = false;
		}
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
