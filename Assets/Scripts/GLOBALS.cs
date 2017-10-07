using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static class Defaults {

	public const int	INT_ZERO	= 0;
	public const int	INT_ONE		= 1;

	public const float	FLOAT_ZERO	= 0.0f;
	public const float	FLOAT_ONE	= 1.0f;
	
};


public static class GLOBALS {

	public	static Reader			Settings	= null;

	public	static Reader			Configs		= null;

	public	static Inputmanager		InputMgr	= new Inputmanager();


	public	static	Player			Player1		= null;
	public	static	Player			Player2		= null;
	public	static	Player			Player3		= null;
	public	static	Player			Player4		= null;

}
