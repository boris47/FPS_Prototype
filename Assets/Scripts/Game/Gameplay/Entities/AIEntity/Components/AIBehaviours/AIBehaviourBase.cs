
using UnityEngine;

namespace Entities.AI.Components.Behaviours
{
	using Senses;

	public abstract class AIBehaviourBase : MonoBehaviour
	{
		[System.Flags]
		protected enum EPerceptions
		{
			None	= (1 << 0),
			Damage	= (1 << 1),
			Hear	= (1 << 2),
			Sight	= (1 << 3),
			Team	= (1 << 4),
			All		= Damage| Hear | Sight | Team
		}

		[SerializeField, ReadOnly]
		private				AIEntity										m_Owner											= null;

		[SerializeReference, ReadOnly]
		private				AIBehaviourBlackboard							m_Blackboard									= null;

		[SerializeField, ReadOnly]
		private				AIController									m_Controller									= null;

		[SerializeField, EnumBitField(typeof(EPerceptions))]
		private				EPerceptions									m_Perceptions									= EPerceptions.Sight;


		protected			AIEntity										Owner											=> m_Owner;
		protected			AIBehaviourBlackboard							Blackboard										=> m_Blackboard;
		protected			AIController									Controller										=> m_Controller;

		public abstract		string											Description										{ get; }


		//////////////////////////////////////////////////////////////////////////
		public void Setup(in AIEntity InOwner, in AIController InController, in AIBehaviourBlackboard InBlackboard)
		{
			m_Owner = InOwner;
			m_Controller = InController;
			m_Blackboard = InBlackboard;
		}


		//////////////////////////////////////////////////////////////////////////
		protected virtual void Awake()
		{
			
		}

		//////////////////////////////////////////////////////////////////////////
		protected virtual void OnValidate()
		{
			
		}

		//////////////////////////////////////////////////////////////////////////
		protected virtual void OnEnable()
		{
			if ((m_Perceptions & EPerceptions.Damage) == EPerceptions.Damage)
			{
				Controller.PerceptionComponent.OnNewSenseEvent += DamageListener;
			}

			if ((m_Perceptions & EPerceptions.Hear) == EPerceptions.Hear)
			{
				Controller.PerceptionComponent.OnNewSenseEvent += HearListener;
			}

			if ((m_Perceptions & EPerceptions.Sight) == EPerceptions.Sight)
			{
				Controller.PerceptionComponent.OnNewSenseEvent += SightListener;
			}

			if ((m_Perceptions & EPerceptions.Team) == EPerceptions.Team)
			{
				Controller.PerceptionComponent.OnNewSenseEvent += TeamListener;
			}
		}

		//////////////////////////////////////////////////////////////////////////
		protected virtual void OnDisable()
		{
			Controller.PerceptionComponent.OnNewSenseEvent -= DamageListener;
			Controller.PerceptionComponent.OnNewSenseEvent -= HearListener;
			Controller.PerceptionComponent.OnNewSenseEvent -= SightListener;
			Controller.PerceptionComponent.OnNewSenseEvent -= TeamListener;
		}

		//////////////////////////////////////////////////////////////////////////
		protected virtual void OnDamageEvent(in DamageEvent InEvent)
		{

		}

		//////////////////////////////////////////////////////////////////////////
		protected virtual void OnHearEvent(in HearingEvent InEvent)
		{

		}

		//////////////////////////////////////////////////////////////////////////
		protected virtual void OnSightEvent(in SightEvent InEvent)
		{

		}

		//////////////////////////////////////////////////////////////////////////
		protected virtual void OnTeamEvent(in TeamEvent InEvent)
		{

		}

		//////////////////////////////////////////////////////////////////////////
		private void DamageListener(in SenseEvent newSenseEvent)
		{
			if (newSenseEvent.SenseType == ESenses.DAMAGE)
			{
				OnDamageEvent(newSenseEvent as DamageEvent);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		private void HearListener(in SenseEvent newSenseEvent)
		{
			if (newSenseEvent.SenseType == ESenses.HEARING)
			{
				OnHearEvent(newSenseEvent as HearingEvent);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		private void SightListener(in SenseEvent newSenseEvent)
		{
			if (newSenseEvent.SenseType == ESenses.DAMAGE)
			{
				OnSightEvent(newSenseEvent as SightEvent);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		private void TeamListener(in SenseEvent newSenseEvent)
		{
			if (newSenseEvent.SenseType == ESenses.DAMAGE)
			{
				OnTeamEvent(newSenseEvent as TeamEvent);
			}
		}
	}
}

