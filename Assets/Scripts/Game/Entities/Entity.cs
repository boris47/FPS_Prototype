using System.Collections;
using System.Collections.Generic;
using CFG_Reader;
using UnityEngine;

enum ENTITY_TYPE {
	NONE,
	ACTOR,
	HUMAN,
	ANIMAL,
	OBJECT
};


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



[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public abstract class Entity : MonoBehaviour, IEntity {

	private	static uint CurrentID = 0;
	public	static uint NewID() {
		return CurrentID++;
	}


	protected 	uint			m_ID							= 0;
	public		uint			ID {
		get { return m_ID; }
	}

	protected	Section			m_SectionRef					= null;

	protected 	string			m_SectionName					= "None";

	protected 	byte			m_EntityType					= ( byte ) ENTITY_TYPE.NONE;

	protected 	bool			m_IsInWater						= false;
	protected 	bool			m_IsUnderWater					= false;


	protected	Rigidbody		m_RigidBody						= null;


	protected	bool 			m_IsOK							= false;


	public		string			Section {
		get { return m_SectionName; }
	}

	public		bool			IsLiveEntity() {
		return this is LiveEntity;
	}

	public		LiveEntity		GetAsLiveEntity() {
		return this as LiveEntity;
	}

	public		bool			IsHuman() {
		return this is LiveEntity;
	}

	public		Human			GetAsHuman() {
		return this as Human;
	}

	public		void			SetInWater( bool b )			{ m_IsInWater = b; }
	public		bool			IsInWater()						{ return m_IsInWater; }

	public		void			SetUnderWater( bool b )			{ m_IsUnderWater = b; }
	public		bool			IsUnderWater()					{ return m_IsUnderWater; }



		

}
