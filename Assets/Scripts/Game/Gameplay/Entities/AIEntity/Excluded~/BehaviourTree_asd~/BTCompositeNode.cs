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
				m_Children.At(i).ResetNode(childInstanceData);
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
			public static void RemoveChild(in BTCompositeNode InCompositeNode, in BTNode InChildNode)
			{
				InCompositeNode.m_Children.Remove(InChildNode);
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