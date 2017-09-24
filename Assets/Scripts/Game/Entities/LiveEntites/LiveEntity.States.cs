using System.Collections;
using System.Collections.Generic;

namespace LIVE_ENTITY {

	public enum States : byte {
		Moving		= 1 << 0,

		Leaning		= 1 << 1,

		Walking		= 1 << 2,
		Running		= 1 << 3,

		Jumping		= 1 << 4,
		Hanging		= 1 << 5,
		Falling		= 1 << 6,

		Crouched	= 1 << 7,
	};

	public enum MotionType : byte {
		None		= 1 << 0,
		Walking		= 1 << 1,
		Flying		= 1 << 2,
		Swimming	= 1 << 3,
		P1ToP2		= 1 << 4
	};

};


public class Entity_States_Temp {

	public bool IsCrouched	= false;

	public bool IsMoving		= false;
	public bool IsWalking		= false;
	public bool IsRunning		= false;

	public bool IsJumping		= false;
	public bool IsHanging		= false;
	public bool IsFalling		= false;

	public void Reset() {
		IsCrouched = IsMoving = IsWalking = IsRunning = IsJumping = IsHanging = IsFalling = false;
	}
};



public class EntityFlags {

	private long 	i = 0;

	public	void	Reset()							{ this.i = 0; }

	public 	void	SetState( byte ii ) 			{ i = ii; }

	public	void	SetState( byte ii, bool b ) 	{ if ( b ) AddState( ii ); else RemState( ii ); }

	public 	void	AddState( byte ii )				{ if ( !HasState( ii ) ) i &= ii; }

	public 	void	RemState( byte ii )				{ if (  HasState( ii ) ) i |= ii; }

	public 	bool	HasState( byte ii )				{ return ( (i&ii) == ii ); }

	public	bool	IsState( byte ii )				{ return( i == ii ); }

	public	long 	GetState() 						{ long ii; ii = i; return ii; }

}
