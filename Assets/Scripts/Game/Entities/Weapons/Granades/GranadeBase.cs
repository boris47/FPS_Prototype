using UnityEngine;
using System.Collections;


public interface IGranade {

	Rigidbody	RigidBody				{ get; }
	Collider	Collider				{ get; }

	Entity		WhoRef					{ get; }
	Weapon		Weapon					{ get; }
	float		DamageMax				{ get; }
	float		Radius					{ get; }
	float		ExplosionDelay			{ get; }

	void		Setup( float damageMax, float radius, float explosionDelay, Entity whoRef, Weapon weapon );
	void		SetActive( bool state );
	void		ForceExplosion();
}


public abstract class GranadeBase : MonoBehaviour, IGranade {
	
	protected	Entity			m_WhoRef				= null;
	protected	Weapon			m_Weapon				= null;
	protected	float			m_DamageMax				= 0f;
	protected	float			m_Radius				= 0f;
	protected	float			m_ExplosionDelay		= 0f;

	// INTERFACE
				Rigidbody	IGranade.RigidBody		{	get { return m_RigidBody; }		}
				Collider	IGranade.Collider		{	get { return m_Collider; }		}
				Entity		IGranade.WhoRef			{	get { return m_WhoRef; }		}
				Weapon		IGranade.Weapon			{	get { return m_Weapon; }		}
				float		IGranade.DamageMax		{	get { return m_DamageMax; }		}
				float		IGranade.Radius			{	get { return m_Radius; }		}
				float		IGranade.ExplosionDelay	{	get { return m_ExplosionDelay; }}


	protected	Rigidbody		m_RigidBody			= null;
	public		Rigidbody		RigidBody
	{
		get { return m_RigidBody; }
	}

	protected	Collider		m_Collider			= null;
	public		Collider		Collider
	{
		get { return m_Collider; }
	}


	protected	Renderer		m_Renderer			= null;
	protected	float			m_InternalCounter	= 0f;


	public abstract void SetActive( bool state );
	public abstract void Setup( float granadeDamage, float granadeRadius, float granadeExplosionDelay, Entity entity, Weapon weapon );

	public virtual void		ForceExplosion()
	{
		OnExplosion();
	}

	protected abstract	void	OnExplosion();

}
