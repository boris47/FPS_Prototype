using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Entities.AI.Components.Behaviours
{
	[BTNodeDetails("Composite")]
	public abstract partial class BTCompositeNode : BTNode, IParentNode
	{
		[SerializeField, /*HideInInspector*/]
		private					List<BTNode>				m_Children							= new List<BTNode>();

		[SerializeField, ReadOnly]
		protected				uint						m_CurrentIndex						= 0;

		[SerializeField, ToNodeInspector]
		protected				bool						m_MustRepeat						= false;

		//---------------------
		public					List<BTNode>				Children							=> m_Children;
		public					uint						CurrentIndex						=> m_CurrentIndex;
		protected virtual		int							MinimumChildrenCount				=> 0;


		//////////////////////////////////////////////////////////////////////////
		public void OverrideActiveChildIndex(in uint InChildIndex)
		{
			m_CurrentIndex = (uint)Mathf.Clamp(InChildIndex, 0, m_Children.Count);
		}

		//////////////////////////////////////////////////////////////////////////
		public void AddChild(in BTNode InNewChild, in uint? InPortIndex = null)
		{
			if (InPortIndex.HasValue && m_Children.IsValidIndex(InPortIndex.Value))
			{
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

		//////////////////////////////////////////////////////////////////////////
		protected override void CopyDataToInstance(in BTNode InNewInstance)
		{
			var node = InNewInstance as BTCompositeNode;
			node.m_Children = m_Children.ConvertAll(c => c.CloneInstance(node));
			node.m_CurrentIndex = 0u;
		}

		//////////////////////////////////////////////////////////////////////////
		protected override EBTNodeState OnActivation()
		{
			EBTNodeState OutResult = EBTNodeState.RUNNING;
			m_CurrentIndex = 0u;

		//	if (m_Children.Count < MinimumChildrenCount)
		//	{
		//		OutResult = EBTNodeState.SUCCEEDED; // By Design
		//	}
		//	else
		//	{
		//		m_Children.ForEach(c => c.ResetNode());
		//	}
			return OutResult;
		}

		//////////////////////////////////////////////////////////////////////////
		protected override EBTNodeState OnUpdateAborting()
		{
			return m_Children.At(m_CurrentIndex).Update();
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnTerminate()
		{
			base.OnTerminate();
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnAbortNodeRequested(in bool bAbortImmediately)
		{
			m_Children.At(m_CurrentIndex).RequestAbortNode(bAbortImmediately);
		}

		//////////////////////////////////////////////////////////////////////////
		protected sealed override void OnNodeReset()
		{
			m_CurrentIndex = 0u;
			m_Children.ForEach(c => c.ResetNode());
		}
	}
}
