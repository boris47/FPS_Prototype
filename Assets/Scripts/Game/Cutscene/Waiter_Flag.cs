using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Waiter_Flag : Waiter_Base {

	public		void	CanContinue()
	{
		this.m_HasToWait = false;
	}

	public		override	void		Wait()
	{

	}

}

