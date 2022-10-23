using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Entities.AI.Components.Behaviours
{
	public abstract partial class BTCompositeNode : BTNode, IParentNode
	{
		[SerializeField, /*ReadOnly,*/ HideInInspector]
		private			List<BTNode>			m_Children					= new List<BTNode>();

		[SerializeField, ReadOnly]
		protected		uint					m_CurrentIndex				= 0;

		[SerializeField, ToNodeInspector]
		protected		bool					m_MustRepeat				= false;

		//---------------------
		public			List<BTNode>			Children					=> m_Children;
		public			uint					CurrentIndex				=> m_CurrentIndex;

		//////////////////////////////////////////////////////////////////////////
		public void OverrideActiveChildIndex(in uint InChildIndex) => m_CurrentIndex = (uint)Mathf.Clamp(InChildIndex, 0, m_Children.Count);

		//////////////////////////////////////////////////////////////////////////
		protected override void CopyDataToInstance(in BTNode InNewInstance)
		{
			var node = InNewInstance as BTCompositeNode;
			node.m_Children = m_Children.ConvertAll(c => c.CloneInstance(node));
			node.m_CurrentIndex = 0u;
		}

		//////////////////////////////////////////////////////////////////////////
		protected sealed override void OnActivation()
		{
			m_CurrentIndex = 0u;

			if (m_Children.Count == 0)
			{
				SetNodeState(EBTNodeState.SUCCEEDED);
			}
			else
			{
				m_Children.ForEach(c => c.ResetNode());
			}
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnTerminate(in bool bIsAbort)
		{
			if (bIsAbort)
			{
				m_Children.At(m_CurrentIndex).AbortNode();
			}
		}

		//////////////////////////////////////////////////////////////////////////
		protected sealed override void OnNodeReset()
		{
			m_CurrentIndex = 0u;
			m_Children.ForEach(c => c.ResetNode());
		}
	}
}
