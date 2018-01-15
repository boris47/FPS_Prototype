using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract partial class LiveEntity {

	public enum eMotionType {
		None		= 1 << 0,
		Walking		= 1 << 1,
		Flying		= 1 << 2,
		Swimming	= 1 << 3,
		P1ToP2		= 1 << 4
	};

	// This variable control which physic to use on entity
	protected	eMotionType		m_MotionType		= eMotionType.Walking;
	public		eMotionType	MotionType {
		get { return m_MotionType; }
		set { SetMotionType( value ); }
	}


	protected	eMotionType	m_PrevMotionType		= eMotionType.Walking;
	public		eMotionType	PrevMotionType {
		get { return m_PrevMotionType; }
	}



	public		void	SetMotionType( eMotionType m )
	{
		m_PrevMotionType = m_MotionType;
		switch( m )
		{
			case eMotionType.Walking:	{ SetMotionTypeInternal( m, true,	new bool[3] { false, false, true } ); return; }
			case eMotionType.Flying:	{ SetMotionTypeInternal( m, false,	new bool[3] { true,  true,  true } ); return; }
			case eMotionType.Swimming:	{ SetMotionTypeInternal( m, false,	new bool[3] { true,  true,  true } ); return; }
			case eMotionType.P1ToP2:	{ SetMotionTypeInternal( m, false,	new bool[3] { false, false, true } ); return; }
		}

	}

	private	void	SetMotionTypeInternal( eMotionType type, bool useGravity, bool[] inputs )
	{
		if ( m_MotionType == type )
			return;

		m_PrevMotionType = m_MotionType;
		m_MotionType	 = type;

		m_States.Reset();

		m_RigidBody.useGravity = useGravity;

		Inputmanager.HoldCrouch = inputs[0];
		Inputmanager.HoldJump = inputs[1];
		Inputmanager.HoldRun = inputs[2];
	}

}
