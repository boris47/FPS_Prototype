using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Entities.AI.Components.Behaviours
{
	[BTNodeDetails("Decorator")]
	public abstract class BTDecoratorNode : BTNode, IParentNode
	{
		[SerializeField, /*ReadOnly*/HideInInspector]
		private				BTNode			m_Child					= null;

		//---------------------
		public				BTNode			Child					=> m_Child;

		public				List<BTNode>	Children
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
		public void SetChild(BTNode child) => m_Child = child;
		

		//////////////////////////////////////////////////////////////////////////
		protected override void OnAbortNodeRequested(in BTNodeInstanceData InThisNodeInstanceData)
		{
			base.OnAbortNodeRequested(InThisNodeInstanceData);

			if (m_Child.IsNotNull())
			{
				BTNodeInstanceData childInstanceData = GetChildInstanceData(InThisNodeInstanceData, m_Child);
				if (Utils.CustomAssertions.IsNotNull(childInstanceData))
				{
					m_Child.RequestAbortNode(childInstanceData);
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
