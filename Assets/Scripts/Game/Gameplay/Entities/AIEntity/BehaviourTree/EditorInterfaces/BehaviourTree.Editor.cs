#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Entities.AI.Components.Behaviours
{
	public interface IBTEditorInterface
	{
		List<BTNode> Nodes { get; }
		BTRootNode RootNode { get; }
		
		Vector3 Position { get; set; }
		Vector3 Scale { get; set; }
		
		void EnsureBlackboard();
		BTRootNode EnsureRootNode();
		BTNode CreateNode(in System.Type InNodeType, in string InNodeName = null);
		void DeleteNode(in BTNode InNodeToDelete);
		void AddChildTo(in BTNode InParent, in BTNode InChild, in uint? InPortIndex = null);
		void RemoveChildFrom(in BTNode InParent, in BTNode InChild);
	}

	// Editor
	[CreateAssetMenu()]
	public partial class BehaviourTree : IBTEditorInterface
	{
		public			IBTEditorInterface	AsEditorInterface						=> this;
		
		[SerializeField, ReadOnly]
		private			Vector2				m_Position								= Vector2.zero;

		[SerializeField, ReadOnly]
		private			Vector3				m_Scale									= Vector3.one;
		
		Vector3								IBTEditorInterface.Position				{ get => m_Position;			set => m_Position = value; }
		Vector3								IBTEditorInterface.Scale				{ get => m_Scale;				set => m_Scale = value; }
		
		//---------------------
		List<BTNode>						IBTEditorInterface.Nodes				=> m_Nodes;
		BTRootNode							IBTEditorInterface.RootNode				=> m_RootNode;


		//////////////////////////////////////////////////////////////////////////
		void IBTEditorInterface.EnsureBlackboard()
		{
			if (m_BlackboardAsset == null)
			{
				using (new Utils.Editor.MarkAsDirty(this))
				{
					m_BlackboardAsset = CreateInstance<Blackboard>();
					m_BlackboardAsset.name = nameof(BlackboardAsset);
					AssetDatabase.AddObjectToAsset(m_BlackboardAsset, this);
				}
			}
		}


		//////////////////////////////////////////////////////////////////////////
		BTRootNode IBTEditorInterface.EnsureRootNode()
		{
			if (!m_RootNode)
			{
				m_RootNode = (BTRootNode)AsEditorInterface.CreateNode(typeof(BTRootNode), "Root Node");
			}
			return m_RootNode;
		}

		//////////////////////////////////////////////////////////////////////////
		BTNode IBTEditorInterface.CreateNode(in System.Type InNodeType, in string InNodeName)
		{
			BTNode node = ScriptableObject.CreateInstance(InNodeType) as BTNode;
			node.name = InNodeName ?? InNodeType.Name;
			node.AsEditorInterface.BehaviourTree = this;
			m_Nodes.Add(node);
			using (new Utils.Editor.MarkAsDirty(this))
			{
				AssetDatabase.AddObjectToAsset(node, this);
			}
			return node;
		}


		//////////////////////////////////////////////////////////////////////////
		void IBTEditorInterface.DeleteNode(in BTNode InNodeToDelete)
		{
			m_Nodes.Remove(InNodeToDelete);
			using (new Utils.Editor.MarkAsDirty(this))
			{
				AssetDatabase.RemoveObjectFromAsset(InNodeToDelete);
			}
			InNodeToDelete.Destroy();
		}

		//////////////////////////////////////////////////////////////////////////
		void IBTEditorInterface.AddChildTo(in BTNode InParent, in BTNode InChild, in uint? InPortIndex)
		{
			if (Utils.CustomAssertions.IsTrue(InParent is IParentNode))
			{
				using (new Utils.Editor.MarkAsDirty(InParent))
				{
					using (new Utils.Editor.MarkAsDirty(InChild))
					{
						switch (InParent)
						{
							case BTRootNode node: node.SetChild(InChild); break;
							case BTDecoratorNode node: node.SetChild(InChild); break;
							case BTCompositeNode node: node.AddChild(InChild, InPortIndex); break;
						}
						InChild.AsEditorInterface.Parent = InParent;
					}
				}
			}
		}


		//////////////////////////////////////////////////////////////////////////
		void IBTEditorInterface.RemoveChildFrom(in BTNode InParent, in BTNode InChild)
		{
			if (Utils.CustomAssertions.IsTrue(InParent is IParentNode))
			{
				using (new Utils.Editor.MarkAsDirty(InParent))
				{
					using (new Utils.Editor.MarkAsDirty(InChild))
					{
						switch (InParent)
						{
							case BTRootNode node: node.SetChild(null); break;
							case BTDecoratorNode node: node.SetChild(null); break;
							case BTCompositeNode node: node.RemoveChild(InChild); break;
						}
						InChild.AsEditorInterface.Parent = null;
					}
				}
			}
		}
	}
}

#endif
