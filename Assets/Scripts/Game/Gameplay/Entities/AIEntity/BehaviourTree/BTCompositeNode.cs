using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Entities.AI.Components.Behaviours
{
	[BTNodeDetails("Composite")]
	public abstract partial class BTCompositeNode : BTNode, IParentNode
	{
		protected class RuntimeData : RuntimeDataBase
		{
			[ReadOnly]
			public				uint						CurrentIndex						= 0u;
		}

		[SerializeField, /*HideInInspector*/]
		private					List<BTNode>				m_Children							= new List<BTNode>();

		[SerializeField, ToNodeInspector]
		protected				bool						m_MustRepeat						= false;

		//---------------------
		public					List<BTNode>				Children							=> m_Children;
		protected virtual		int							MinimumChildrenCount				=> 0;

		#region MOVE TO EDITOR INTERFACE

		//////////////////////////////////////////////////////////////////////////
		public void AddChild(in BTNode InNewChild, in uint? InPortIndex = null)
		{
			if (InPortIndex.HasValue)
			{
				if (!m_Children.IsValidIndex(InPortIndex.Value))
				{
					m_Children.Capacity = (int)InPortIndex.Value+1;
				}
				m_Children.Insert((int)InPortIndex.Value, InNewChild);
			}
			else
			{
				m_Children.Add(InNewChild);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		public void RemoveChild(in BTNode InChild)
		{
			m_Children.Remove(InChild);
		}

		#endregion

		//////////////////////////////////////////////////////////////////////////
		public void OverrideActiveChildIndex(in BTNodeInstanceData InThisNodeInstanceData, in uint InChildIndex)
		{
			RuntimeData nodeData = GetRuntimeData<RuntimeData>(InThisNodeInstanceData);

			nodeData.CurrentIndex = (uint)Mathf.Clamp(InChildIndex, 0, m_Children.Count);
		}

		//////////////////////////////////////////////////////////////////////////
		protected override RuntimeDataBase CreateRuntimeDataInstance(in BTNodeInstanceData InThisNodeInstanceData) => new RuntimeData();

		//////////////////////////////////////////////////////////////////////////
		protected override EBTNodeState OnActivation(in BTNodeInstanceData InThisNodeInstanceData)
		{
			EBTNodeState OutResult = EBTNodeState.RUNNING;

			RuntimeData nodeData = GetRuntimeData<RuntimeData>(InThisNodeInstanceData);
			
			nodeData.CurrentIndex = 0u;

			return OutResult;
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnTerminate(in BTNodeInstanceData InThisNodeInstanceData)
		{
			base.OnTerminate(InThisNodeInstanceData);
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnAbortNodeRequested(in BTNodeInstanceData InThisNodeInstanceData)
		{
			base.OnAbortNodeRequested(InThisNodeInstanceData);

			RuntimeData nodeData = GetRuntimeData<RuntimeData>(InThisNodeInstanceData);

			BTNode child = m_Children.At(nodeData.CurrentIndex);
			BTNodeInstanceData childInstanceData = GetChildInstanceData(InThisNodeInstanceData, child);
			m_Children.At(nodeData.CurrentIndex).RequestAbortNode(childInstanceData);
		}

		//////////////////////////////////////////////////////////////////////////
		protected sealed override void OnNodeReset(in BTNodeInstanceData InThisNodeInstanceData)
		{
			base.OnNodeReset(InThisNodeInstanceData);

			RuntimeData nodeData = GetRuntimeData<RuntimeData>(InThisNodeInstanceData);
			nodeData.CurrentIndex = 0u;

			for (uint i = 0, count = (uint)m_Children.Count; i < count; i++)
			{
				BTNode child = m_Children.At(i);
				BTNodeInstanceData childInstanceData = GetChildInstanceData(InThisNodeInstanceData, child);
				child.ResetNode(childInstanceData);
			}
		}
	}
}
