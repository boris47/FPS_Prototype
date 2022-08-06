
using UnityEngine;

namespace Entities.AI.Components
{
	public abstract class AIMovementCollider : EntityComponent
	{
		public		delegate	void	OnTriggerEventDel(in Collider other);

		private		event				OnTriggerEventDel		 m_OnTriggerEnterEv			= delegate { };
		private		event				OnTriggerEventDel		 m_OnTriggerExitEv			= delegate { };

		public event OnTriggerEventDel OnTriggerEnterEv
		{
			add		{ if (value.IsNotNull()) m_OnTriggerEnterEv += value; }
			remove	{ if (value.IsNotNull()) m_OnTriggerEnterEv -= value; }
		}

		public event OnTriggerEventDel OnTriggerExitEv
		{
			add		{	if (value.IsNotNull())	m_OnTriggerExitEv += value; }
			remove	{	if (value.IsNotNull())	m_OnTriggerExitEv -= value; }
		}


		//////////////////////////////////////////////////////////////////
		private void OnTriggerEnter(Collider other) => m_OnTriggerEnterEv(other);

		//////////////////////////////////////////////////////////////////
		private void OnTriggerExit(Collider other) => m_OnTriggerExitEv(other);
	}
}
