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
	public		eMotionType		MotionType				{ get; set; }

	[System.NonSerialized]
	protected	eMotionType		m_PrevMotionType		= eMotionType.Walking;
	public		eMotionType		PrevMotionType
	{
		get { return m_PrevMotionType; }
	}



	public		void	SetMotionType( eMotionType m )
	{
		m_PrevMotionType = MotionType;
		switch( m )
		{	//													HoldCrouch, HoldJump, HoldRun
			case eMotionType.Walking:	{ SetMotionTypeInternal( m, false,	false,	true ); return; }
			case eMotionType.Flying:	{ SetMotionTypeInternal( m, true,	true,	true ); return; }
			case eMotionType.Swimming:	{ SetMotionTypeInternal( m, true,	true,	true ); return; }
			case eMotionType.P1ToP2:	{ SetMotionTypeInternal( m, false,	false,	true ); return; }
		}

	}

	private	void	SetMotionTypeInternal( eMotionType type, bool HoldCrouch, bool HoldJump, bool HoldRun )
	{
		if ( MotionType == type )
			return;

		m_PrevMotionType = MotionType;
		MotionType		= type;

		m_States.Reset();

		InputManager.HoldCrouch	= HoldCrouch;
		InputManager.HoldJump	= HoldJump;
		InputManager.HoldRun	= HoldRun;
	}

}
