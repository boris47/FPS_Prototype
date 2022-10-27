﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Entities.AI.Components.Behaviours
{
	[BTNodeDetails("Decorator")]
	public abstract partial class BTDecoratorNode : BTNode, IParentNode
	{
		[SerializeField, /*ReadOnly*/HideInInspector]
		private				BTNode			m_Child					= null;

		//---------------------
		public				BTNode			Child					=> m_Child;

		IReadOnlyList<BTNode>						IParentNode.Children
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
		protected override EBTNodeState OnActivation(in BTNodeInstanceData InThisNodeInstanceData)
		{
			EBTNodeState OutState = EBTNodeState.RUNNING;

			if (m_Child.IsNull())
			{
				OutState = EBTNodeState.SUCCEEDED;
			}

			return OutState;
		}

		//////////////////////////////////////////////////////////////////////////
		protected override EBTNodeState OnUpdate(in BTNodeInstanceData InThisNodeInstanceData, in float InDeltaTime)
		{
			EBTNodeState OutState = EBTNodeState.FAILED;
			if (Child.IsNotNull())
			{
				BTNodeInstanceData childInstanceData = GetChildInstanceData(InThisNodeInstanceData, Child);
				OutState = Child.UpdateNode(childInstanceData, InDeltaTime);
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
				if (Utils.CustomAssertions.IsNotNull(childInstanceData))
				{
					m_Child.AbortAndResetNode(childInstanceData);
				}
			}
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnNodeReset(in BTNodeInstanceData InThisNodeInstanceData)
		{
			base.OnNodeReset(InThisNodeInstanceData);

			if (m_Child.IsNotNull())
			{
				BTNodeInstanceData childInstanceData = GetChildInstanceData(InThisNodeInstanceData, m_Child);
				if (Utils.CustomAssertions.IsNotNull(childInstanceData))
				{
					m_Child.ResetNode(childInstanceData);
				}
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
				InDecoratorNode.m_Child = InChildNode;
			}
		}
	}
}
#endif