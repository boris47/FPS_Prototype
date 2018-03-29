
using CFG_Reader;
using UnityEngine;
using System.Collections.Generic;


public interface IEntity {

	bool			IsLiveEntity();
	LiveEntity		GetAsLiveEntity();

	bool			IsHuman();
	Human			GetAsHuman();

	void			SetInWater( bool b );
	bool			IsInWater();

	void			SetUnderWater( bool b );
	bool			IsUnderWater();
}

public enum BrainState {
	QUIET,
	WARNING,
	ATTACKING
}



[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof( CapsuleCollider ))]
public abstract partial class Entity : MonoBehaviour, IEntity {

	public enum ENTITY_TYPE {
		NONE,
		ACTOR,
		HUMAN,
		ROBOT,
		ANIMAL,
		OBJECT
	};

	private	static uint CurrentID = 0;
	public	static uint NewID() {
		return CurrentID++;
	}

	public		bool			IsActive						{ get; set; }

	protected 	uint			m_ID							= 0;
	public		uint			ID
	{
		get { return m_ID; }
	}
	[SerializeField]
	protected	float			m_Health						= 1f;
	public		float			Health
	{
		get { return m_Health; }
		set { m_Health = value; }
	}
	[SerializeField]
	protected	float			m_ViewRange						= 10f;
	public		float			ViewRange
	{
		get { return m_ViewRange; }
		set { m_ViewRange = value; }
	}

	public		string			Section
	{
		get { return m_SectionName; }
	}

	public		bool			IsLiveEntity()
	{
		return this is LiveEntity;
	}

	public		LiveEntity		GetAsLiveEntity()
	{
		return this as LiveEntity;
	}

	public		bool			IsHuman()
	{
		return this is LiveEntity;
	}

	public		Human			GetAsHuman()
	{
		return this as Human;
	}

	public		void			SetInWater( bool b )			{ m_IsInWater = b; }
	public		bool			IsInWater()						{ return m_IsInWater; }

	public		void			SetUnderWater( bool b )			{ m_IsUnderWater = b; }
	public		bool			IsUnderWater()					{ return m_IsUnderWater; }

	protected	Section			m_SectionRef					= null;

	protected 	string			m_SectionName					= "None";
	[System.NonSerialized]
	protected 	ENTITY_TYPE		m_EntityType					= ENTITY_TYPE.NONE;

	[System.NonSerialized]
	protected	BrainState		m_BrainState					= BrainState.QUIET;
	
	protected 	bool			m_IsInWater						= false;
	protected 	bool			m_IsUnderWater					= false;

	protected	Rigidbody		m_RigidBody						= null;
	public		Rigidbody		RigidBody
	{
		get { return m_RigidBody; }
	}

	protected	CapsuleCollider	m_PhysicCollider				= null;
	public		CapsuleCollider	PhysicCollider
	{
		get { return m_PhysicCollider; }
	}

	protected	ViewAreaTrigger	m_ViewTrigger					= null;
	public		ViewAreaTrigger	m_ViewArea
	{
		get { return m_ViewTrigger; }
	}

	protected	List<Entity>	m_Targets						= new List<Entity>();
	protected	Entity			m_CurrentTarget					= null;

	protected	bool 			m_IsOK							= false;
}
