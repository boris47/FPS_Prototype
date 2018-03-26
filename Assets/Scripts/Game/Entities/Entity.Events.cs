
using UnityEngine;

public	delegate	void	VoidArgsDelegate();

public abstract partial class Entity {

	private	const		float				THINK_TIMER		= 0.2f;

	private				float				m_ThinkTimer	= 0f;

	public	event		VoidArgsDelegate	OnKilled		= null;



	//////////////////////////////////////////////////////////////////////////
	// OnHit
	public	abstract	void				OnHit( ref Entity who, float damage );


	//////////////////////////////////////////////////////////////////////////
	// OnHurt
	public	abstract	void				OnHurt( ref Entity who, float damage );


	//////////////////////////////////////////////////////////////////////////
	// OnKill
	public	virtual		void				OnKill()
	{
		if ( OnKilled != null )
			OnKilled();
	}


	//////////////////////////////////////////////////////////////////////////
	// OnThink
	public	abstract	void				OnThink();


	//////////////////////////////////////////////////////////////////////////
	// Awake
	protected virtual	void				Awake()
	{
		m_ThinkTimer		= 0f;
		m_PhysicCollider	= GetComponent<CapsuleCollider>();
		m_ViewTrigger		= GetComponent<SphereCollider>();

		if ( m_ViewTrigger == null && ( this is Player ) == false )
		{
			m_ViewTrigger = gameObject.AddComponent<SphereCollider>();
			m_ViewTrigger.isTrigger = true;
		}

		m_ViewTrigger.radius = m_ViewRange;

	}


	//////////////////////////////////////////////////////////////////////////
	// Update
	protected virtual	void				Update()
	{
		m_ThinkTimer += Time.deltaTime;
		if ( m_ThinkTimer > THINK_TIMER )
		{
			OnThink();
			m_ThinkTimer = 0f;
		}
	}

}