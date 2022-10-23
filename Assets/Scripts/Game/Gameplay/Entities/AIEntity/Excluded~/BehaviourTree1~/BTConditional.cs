using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Entities.AI.Components.Behaviours
{
	[System.Serializable]
	public abstract partial class BTConditional : ScriptableObject
	{
	//	[SerializeField, ReadOnly]
		protected		BehaviourTree			m_BehaviourTree			= null;

	//	[SerializeField, ReadOnly]
		protected		AIController			m_Owner					= null;

	//	[SerializeField, ReadOnly]
		protected		BTCompositeNode			m_NodeConditionalized	= null;

		//---------------------
		public			BTCompositeNode			NodeConditionalized		=> m_NodeConditionalized;


		//////////////////////////////////////////////////////////////////////////
		public BTConditional CloneInstance()
		{
			var newInstance = Instantiate(this);
			CopyDataToInstance(newInstance);
			return newInstance;
		}

		//////////////////////////////////////////////////////////////////////////
		public void OnAwake(in BTCompositeNode InNodeConditionalized, in BehaviourTree InBehaviourTree)
		{
			if (CustomAssertions.IsNotNull(InNodeConditionalized))
			{
				m_NodeConditionalized = InNodeConditionalized;
			}

			if (CustomAssertions.IsNotNull(InBehaviourTree))
			{
				m_BehaviourTree = InBehaviourTree;
				m_Owner = InBehaviourTree.Owner;
			}

			OnAwakeInternal();
		}

		//////////////////////////////////////////////////////////////////////////
		public void OnEnableObserver()
		{
			if (CustomAssertions.IsNotNull(GameManager.CyclesEvents))
			{
				GameManager.CyclesEvents.OnFrame += OnFrame;
			}
			OnEnableObserverInternal();
		}

		//////////////////////////////////////////////////////////////////////////
		public void OnRemoveObserver()
		{
			if (GameManager.CyclesEvents.IsNotNull())
			{
				GameManager.CyclesEvents.OnFrame -= OnFrame;
			}
			OnRemoveObserverInternal();
		}

		//////////////////////////////////////////////////////////////////////////
		protected abstract void CopyDataToInstance(in BTConditional InNewInstance);

		//////////////////////////////////////////////////////////////////////////
		protected virtual void OnAwakeInternal() { }

		//////////////////////////////////////////////////////////////////////////
		public abstract bool GetEvaluation();

		//////////////////////////////////////////////////////////////////////////
		protected abstract void OnEnableObserverInternal();

		//////////////////////////////////////////////////////////////////////////
		protected virtual void OnFrame(float InDeltaTime) { }

		//////////////////////////////////////////////////////////////////////////
		protected abstract void OnRemoveObserverInternal();
	}
}
