using System.Collections.Generic;
using UnityEngine;

namespace Entities.AI.Components.Behaviours
{
	[BTNodeDetails("Composite")]
	public abstract partial class BTCompositeNode : BTNode, IParentNode
	{
		protected class RuntimeData : RuntimeDataBase
		{
			public				uint						CurrentIndex						= 0u;
		}

		[SerializeField]
		private					BTNode[]					m_Children							= new BTNode[0];

		[SerializeField, ToNodeInspector]
		private					bool						m_MustRepeat						= false;

		protected				bool						MustRepeat							=> m_MustRepeat;

		//---------------------
		public					BTNode[]					Children							=> m_Children;


		//////////////////////////////////////////////////////////////////////////
		public void OverrideActiveChildIndex(in BTNodeInstanceData InThisNodeInstanceData, in uint InChildIndex)
		{
			RuntimeData nodeData = GetRuntimeData<RuntimeData>(InThisNodeInstanceData);
			{
				nodeData.CurrentIndex = (uint)Mathf.Clamp(InChildIndex, 0, m_Children.Length);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		public int IndexOf(in BTNode InNodeAsset) => System.Array.IndexOf(m_Children, InNodeAsset);


		//////////////////////////////////////////////////////////////////////////
		protected override RuntimeDataBase CreateRuntimeDataInstance(in BTNodeInstanceData InThisNodeInstanceData) => new RuntimeData();

		//////////////////////////////////////////////////////////////////////////
		protected override EBTNodeInitializationResult OnActivation(in BTNodeInstanceData InThisNodeInstanceData)
		{
			EBTNodeInitializationResult OutState = EBTNodeInitializationResult.RUNNING;

			RuntimeData nodeData = GetRuntimeData<RuntimeData>(InThisNodeInstanceData);
			{
				nodeData.CurrentIndex = 0u;
			}

			if (m_Children.Length == 0)
			{
				OutState = EBTNodeInitializationResult.SUCCEEDED;
			}

			return OutState;
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnNodeAbort(in BTNodeInstanceData InThisNodeInstanceData)
		{
			base.OnNodeAbort(InThisNodeInstanceData);

			RuntimeData nodeData = GetRuntimeData<RuntimeData>(InThisNodeInstanceData);
			{
				nodeData.CurrentIndex = 0u;
			}

			foreach (BTNode childAsset in m_Children)
			{
				childAsset.AbortAndResetNode(GetNodeInstanceData(InThisNodeInstanceData, childAsset));
			}
		}

		//////////////////////////////////////////////////////////////////////////
		protected sealed override void OnNodeReset(in BTNodeInstanceData InThisNodeInstanceData)
		{
			base.OnNodeReset(InThisNodeInstanceData);

			RuntimeData nodeData = GetRuntimeData<RuntimeData>(InThisNodeInstanceData);
			{
				nodeData.CurrentIndex = 0u;
			}

			foreach (BTNode childAsset in m_Children)
			{
				childAsset.ResetNode(GetNodeInstanceData(InThisNodeInstanceData, childAsset));
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
				List<BTNode> dynamicList = new List<BTNode>(InCompositeNode.m_Children);
				if (InPortIndex.HasValue)
				{
					if (!dynamicList.IsValidIndex(InPortIndex.Value))
					{
						dynamicList.Capacity = (int)InPortIndex.Value + 1;
					}
					dynamicList.Insert((int)InPortIndex.Value, InChildNode);
				}
				else
				{
					dynamicList.Add(InChildNode);
				}
				InCompositeNode.m_Children = dynamicList.ToArray();
			}

			//////////////////////////////////////////////////////////////////////////
			public static void SetChild(in BTCompositeNode InCompositeNode, in BTNode InChildNode, in uint InIndex)
			{
				List<BTNode> dynamicList = new List<BTNode>(InCompositeNode.m_Children);
				if (dynamicList.IsValidIndex(InIndex))
				{
					dynamicList[(int)InIndex] = InChildNode;
				}
				InCompositeNode.m_Children = dynamicList.ToArray();
			}

			//////////////////////////////////////////////////////////////////////////
			public static void RemoveChild(in BTCompositeNode InCompositeNode, in BTNode InChildNode)
			{
				List<BTNode> dynamicList = new List<BTNode>(InCompositeNode.m_Children);
				{
					dynamicList.Remove(InChildNode);
				}
				InCompositeNode.m_Children = dynamicList.ToArray();
			}

			//////////////////////////////////////////////////////////////////////////
			public static void RemoveInvalidChildAt(in BTCompositeNode InCompositeNode, in int InIndex)
			{
				List<BTNode> dynamicList = new List<BTNode>(InCompositeNode.m_Children);
				{
					dynamicList.RemoveAt(InIndex);
				}
				InCompositeNode.m_Children = dynamicList.ToArray();
			}

			//////////////////////////////////////////////////////////////////////////
			public static void SortChildren(in BTCompositeNode InCompositeNode)
			{
				static int SortByHorizontalPosition(BTNode left, BTNode right)
				{
					return BTNode.Editor.GetEditorGraphPosition(left).x < BTNode.Editor.GetEditorGraphPosition(right).x ? -1 : 1;
				}

					if (InCompositeNode.ShouldSortChildren())
					{
						using (new Utils.Editor.MarkAsDirty(InCompositeNode))
						{
						List<BTNode> dynamicList = new List<BTNode>(InCompositeNode.m_Children);
						{
							dynamicList.Sort(SortByHorizontalPosition);
						}
						InCompositeNode.m_Children = dynamicList.ToArray();
					}
				}
			}
		}

		//////////////////////////////////////////////////////////////////////////
		protected virtual bool ShouldSortChildren() => true;
	}
}
#endif