using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CutScene {

	public class Cutscene_Waiter_Flag : Cutscene_Waiter_Base {

		public		void	CanContinue()
		{
			m_HasToWait = false;
		}

		public		override	void		Wait()
		{

		}

	}
}
