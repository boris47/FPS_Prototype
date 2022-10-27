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
		public IReadOnlyList<BTNode>						Children							=> m_Children;

		protected virtual		int							MinimumChildrenCount				=> 0;

		//////////////////////////////////////////////////////////////////////////
		public void OverrideActiveChildIndex(in BTNodeInstanceData InThisNodeInstanceData, in uint InChildIndex)
		{
			RuntimeData nodeData = GetRuntimeData<RuntimeData>(InThisNodeInstanceData);

			nodeData.CurrentIndex = (uint)Mathf.Clamp(InChildIndex, 0, m_Children.Count);
		}

		//////////////////////////////////////////////////////////////////////////
		public int IndexOf(in BTNode InNode) => m_Children.IndexOf(InNode);

		//////////////////////////////////////////////////////////////////////////
		protected override RuntimeDataBase CreateRuntimeDataInstance(in BTNodeInstanceData InThisNodeInstanceData) => new RuntimeData();

		//////////////////////////////////////////////////////////////////////////
		protected override EBTNodeState OnActivation(in BTNodeInstanceData InThisNodeInstanceData)
		{
			EBTNodeState OutState = EBTNodeState.RUNNING;

			RuntimeData nodeData = GetRuntimeData<RuntimeData>(InThisNodeInstanceData);
			
			nodeData.CurrentIndex = 0u;

			if (m_Children.Count == 0)
			{
				OutState = EBTNodeState.SUCCEEDED;
			}

			return OutState;
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnNodeAbort(in BTNodeInstanceData InThisNodeInstanceData)
		{
			base.OnNodeAbort(InThisNodeInstanceData);

			RuntimeData nodeData = GetRuntimeData<RuntimeData>(InThisNodeInstanceData);

			if (m_Children.IsValidIndex(nodeData.CurrentIndex))
			{
				BTNode child = m_Children.At(nodeData.CurrentIndex);
				BTNodeInstanceData childInstanceData = GetChildInstanceData(InThisNodeInstanceData, child);
				child.AbortAndResetNode(childInstanceData);
			}

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

#if UNITY_EDITOR
namespace Entities.AI.Components.Behaviours
{
	public abstract partial class BTCompositeNode
	{
		public static new class Editor
		{
			//////////////////////////////////////////////////////////////////////////
			public static void AddChild(in BTCompositeNode InCompositeNode, in BTNode InChildNode, in uint? InPortIndex)
			{
				if (InPortIndex.HasValue)
				{
					if (!InCompositeNode.m_Children.IsValidIndex(InPortIndex.Value))
					{
						InCompositeNode.m_Children.Capacity = (int)InPortIndex.Value + 1;
					}
					InCompositeNode.m_Children.Insert((int)InPortIndex.Value, InChildNode);
				}
				else
				{
					InCompositeNode.m_Children.Add(InChildNode);
				}
			}

			//////////////////////////////////////////////////////////////////////////
			public static void SetChild(in BTCompositeNode InCompositeNode, in BTNode InChildNode, in uint InIndex)
			{
				if (InCompositeNode.m_Children.IsValidIndex(InIndex))
				{
					InCompositeNode.m_Children[(int)InIndex] = InChildNode;
				}
			}

			//////////////////////////////////////////////////////////////////////////
			public static void RemoveChild(in BTCompositeNode InCompositeNode, in BTNode InChildNode)
			{
				InCompositeNode.m_Children.Remove(InChildNode);
			}

			//////////////////////////////////////////////////////////////////////////
			public static void RemoveInvalidChildAt(in BTCompositeNode InCompositeNode, in int InIndex)
			{
				InCompositeNode.m_Children.RemoveAt(InIndex);
			}

			//////////////////////////////////////////////////////////////////////////
			public static void SortChildren(in BTCompositeNode InCompositeNode)
			{
				static int SortByHorizontalPosition(BTNode left, BTNode right)
				{
					return BTNode.Editor.GetEditorGraphPosition(left).x < BTNode.Editor.GetEditorGraphPosition(right).x ? -1 : 1;
				}

				if (InCompositeNode.m_Children.Count > 1)
				{
					if (InCompositeNode.ShouldSortChildren())
					{
						using (new Utils.Editor.MarkAsDirty(InCompositeNode))
						{
							InCompositeNode.m_Children.Sort(SortByHorizontalPosition);
						}
					}
				}
			}
		}

		//////////////////////////////////////////////////////////////////////////
		protected virtual bool ShouldSortChildren() => true;
	}
}
#endif