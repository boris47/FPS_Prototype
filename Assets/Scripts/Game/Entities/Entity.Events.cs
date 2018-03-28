
using UnityEngine;

public	delegate	void	VoidArgsDelegate();

interface IEntityInterface {

	void OnViewAreaEnter( Entity entity );
	void OnViewAreaExit ( Entity entity );
}

public abstract partial class Entity : IEntityInterface {

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

		if ( this is Player )
			return;

		m_ViewTrigger		= GetComponentInChildren<ViewAreaTrigger>();
		if ( m_ViewTrigger == null )
		{
			GameObject child = new GameObject( "VAT" );
			child.layer = 2;						// Ignore Raycast
			child.transform.SetParent( transform );
			m_ViewTrigger = child.AddComponent<ViewAreaTrigger>();
			SphereCollider trigger = child.GetComponent<SphereCollider>();
			trigger.isTrigger = true;
			trigger.radius = m_ViewRange;

		}
	}


	//////////////////////////////////////////////////////////////////////////
	// Update
	protected virtual	void				Update()
	{
		if ( this is Player )
			return;

		m_ThinkTimer += Time.deltaTime;
		if ( m_ThinkTimer > THINK_TIMER )
		{
			OnThink();
			m_ThinkTimer = 0f;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// View interfacer
	// VIEW TRIGGER EVENT
	void IEntityInterface.OnViewAreaEnter( Entity entity )
	{
		if ( m_Targets.Contains( entity ) )
			return;

		m_Targets.Add( entity );
	}


	// VIEW TRIGGER EVENT
	void IEntityInterface.OnViewAreaExit(Entity entity)
	{
		if ( m_Targets.Contains( entity ) == false )
			return;

		m_Targets.Remove( entity );
	}

}