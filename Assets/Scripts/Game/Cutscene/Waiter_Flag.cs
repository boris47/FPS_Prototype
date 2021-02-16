using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Waiter_Flag : Waiter_Base {

	public		void	CanContinue()
	{
		HasToWait = false;
	}

	public		override	void		Wait()
	{

	}

}

