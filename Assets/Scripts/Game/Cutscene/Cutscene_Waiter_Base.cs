using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CutScene {

	public abstract class Cutscene_Waiter_Base : MonoBehaviour {

		protected		bool	m_HasToWait		= true;
		public			bool	HasToWait
		{
			get { return m_HasToWait; }
		}

		public	abstract	void	Wait();

		private void Awake()
		{
			enabled = false;
		}

	}

}