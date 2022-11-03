using UnityEngine;
using System.Collections.Generic;

namespace Entities.AI.Components.Behaviours
{
	[BTNodeDetails("Root Node", "The root node")]
	public sealed partial class BTRootNode : BTNode, IParentNode
	{
		[SerializeField, HideInInspector]
		private			BTNode			m_ChildAsset					= null;

		[SerializeField, ToNodeInspector(bInShowDefaultLabel: true)]
		private			bool			m_MustRepeat			= false;

		//---------------------
	//	public			BTNode			Child					=> m_Child;

		BTNode[]						IParentNode.Children
		{
			get
			{
				List<BTNode> children = new List<BTNode>();
				if (m_ChildAsset.IsNotNull())
				{
					children.Add(m_ChildAsset);
				}
				return children.ToArray();
			}
		}

		//////////////////////////////////////////////////////////////////////////
		protected override EBTNodeInitializationResult OnActivation(in BTNodeInstanceData InThisNodeInstanceData)
		{
			EBTNodeInitializationResult OutState = EBTNodeInitializationResult.SUCCEEDED;
			{
				if (m_ChildAsset.IsNotNull())
				{
					OutState = EBTNodeInitializationResult.RUNNING;
				}
			}
			return OutState;
		}

		//////////////////////////////////////////////////////////////////////////
		protected sealed override EBTNodeState OnNodeUpdate(in BTNodeInstanceData InThisNodeInstanceData, in float InDeltaTime)
		{
			EBTNodeState OutState = EBTNodeState.SUCCEEDED;
			if (m_ChildAsset.IsNotNull())
			{
				OutState = EBTNodeState.RUNNING;

				BTNodeInstanceData childInstanceData = GetNodeInstanceData(InThisNodeInstanceData, m_ChildAsset);

				if (Utils.CustomAssertions.IsTrue(childInstanceData.NodeState != EBTNodeState.RUNNING))
				{
					if (childInstanceData.NodeState == EBTNodeState.INACTIVE)
					{
						ConditionalLog($"Child node {m_ChildAsset.name} set as running node", InThisNodeInstanceData);
						childInstanceData.SetAsRunningNode();
					}
					else
					{
						ConditionalLog($"Child node {m_ChildAsset.name} has finished with {childInstanceData.NodeState}", InThisNodeInstanceData);
						if (m_MustRepeat)
						{
							m_ChildAsset.ResetNode(childInstanceData);
						}
						else
						{
							OutState = EBTNodeState.SUCCEEDED;
						}
					}
				}
			}
			return OutState;
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnNodeAbort(in BTNodeInstanceData InThisNodeInstanceData)
		{
			base.OnNodeAbort(InThisNodeInstanceData);

			if (m_ChildAsset.IsNotNull())
			{
				m_ChildAsset.AbortAndResetNode(GetNodeInstanceData(InThisNodeInstanceData, m_ChildAsset));
			}
		}

		//////////////////////////////////////////////////////////////////////////
		protected sealed override void OnNodeReset(in BTNodeInstanceData InThisNodeInstanceData)
		{
			base.OnNodeReset(InThisNodeInstanceData);

			if (m_ChildAsset.IsNotNull())
			{
				m_ChildAsset.ResetNode(GetNodeInstanceData(InThisNodeInstanceData, m_ChildAsset));
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
			InRootNode.m_ChildAsset = InChildNode;
		}
	}
}
#endif