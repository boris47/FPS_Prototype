using UnityEngine;
using System.Collections.Generic;

namespace Entities.AI.Components.Behaviours
{
	[BTNodeDetails("Root Node", "The root node")]
	public sealed partial class BTRootNode : BTNode, IParentNode
	{
		[SerializeField, HideInInspector]
		private			BTNode			m_Child					= null;

		[SerializeField, ToNodeInspector(bInShowDefaultLabel: true)]
		private			bool			m_MustRepeat			= false;

		//---------------------
		public			BTNode			Child					=> m_Child;

		public			List<BTNode> Children
		{
			get
			{
				List<BTNode> children = new List<BTNode>();
				if (m_Child.IsNotNull())
				{
					children.Add(m_Child);
				}
				return children;
			}
		}

		//////////////////////////////////////////////////////////////////////////
		protected sealed override EBTNodeState OnUpdate(in BTNodeInstanceData InThisNodeInstanceData, in float InDeltaTime)
		{
			EBTNodeState OutState = EBTNodeState.SUCCEEDED;
			if (m_Child.IsNotNull())
			{
				BTNodeInstanceData childInstanceData = GetChildInstanceData(InThisNodeInstanceData, m_Child);

				OutState = m_Child.UpdateNode(childInstanceData, InDeltaTime);
				if (BTNode.IsFinished(OutState) && m_MustRepeat)
				{
					m_Child.ResetNode(childInstanceData);
					OutState = EBTNodeState.RUNNING;
				}
			}
			return OutState;
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnNodeAbort(in BTNodeInstanceData InThisNodeInstanceData)
		{
			base.OnNodeAbort(InThisNodeInstanceData);

			if (m_Child.IsNotNull())
			{
				BTNodeInstanceData childInstanceData = GetChildInstanceData(InThisNodeInstanceData, m_Child);

				m_Child.AbortAndResetNode(childInstanceData);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		protected sealed override void OnNodeReset(in BTNodeInstanceData InThisNodeInstanceData)
		{
			base.OnNodeReset(InThisNodeInstanceData);

			if (m_Child.IsNotNull())
			{
				BTNodeInstanceData childInstanceData = GetChildInstanceData(InThisNodeInstanceData, m_Child);

				m_Child.ResetNode(childInstanceData);
			}
		}
	}
}

#if UNITY_EDITOR
namespace Entities.AI.Components.Behaviours
{
	using UnityEditor;

	public sealed partial class BTRootNode
	{
		//////////////////////////////////////////////////////////////////////////
		public static void SetChild(in BTRootNode InRootNode, in BTNode InChildNode)
		{
			InRootNode.m_Child = InChildNode;
		}
	}
}
#endif