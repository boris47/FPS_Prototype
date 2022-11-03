using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Entities.AI.Components.Behaviours
{
	[BTNodeDetails("Decorator")]
	public abstract partial class BTDecoratorNode : BTNode, IParentNode
	{
		[SerializeField, HideInInspector]
		private				BTNode			m_ChildAsset			= null;

		//---------------------
		public				BTNode			ChildAsset				=> m_ChildAsset;

		BTNode[]							IParentNode.Children
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
		/// <summary> OnNodeUpdate gets called once a child terminates its execution </summary>
		protected override EBTNodeState OnNodeUpdate(in BTNodeInstanceData InThisNodeInstanceData, in float InDeltaTime)
		{
			EBTNodeState OutState = base.OnNodeUpdate(InThisNodeInstanceData, InDeltaTime);
			BTNodeInstanceData childInstanceData = GetNodeInstanceData(InThisNodeInstanceData, m_ChildAsset);
			if (Utils.CustomAssertions.IsTrue(childInstanceData.NodeState != EBTNodeState.RUNNING))
			{
				if (childInstanceData.NodeState == EBTNodeState.INACTIVE)
				{
					ConditionalLog($"Child node {m_ChildAsset.name} set as running node", InThisNodeInstanceData);

					childInstanceData.SetAsRunningNode();

					OutState = EBTNodeState.RUNNING;
				}
				else
				{
					ConditionalLog($"Child node {m_ChildAsset.name} has finished with {childInstanceData.NodeState}", InThisNodeInstanceData);
			
					OutState = childInstanceData.NodeState;
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
		protected override void OnNodeReset(in BTNodeInstanceData InThisNodeInstanceData)
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

	public partial class BTDecoratorNode
	{
		public static new class Editor
		{
			//////////////////////////////////////////////////////////////////////////
			public static void SetChild(in BTDecoratorNode InDecoratorNode, in BTNode InChildNode)
			{
				InDecoratorNode.m_ChildAsset = InChildNode;
			}
		}
	}
}
#endif