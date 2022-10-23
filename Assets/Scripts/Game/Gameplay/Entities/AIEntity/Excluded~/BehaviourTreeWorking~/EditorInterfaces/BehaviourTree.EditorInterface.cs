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
		bool IsInstance { get; }

		void UpdateSource();
		BTRootNode EnsureRootNode();
		BTNode CreateNode(in System.Type InNodeType, in string InNodeName = null);
		void DeleteNode(in BTNode InNodeToDelete);
		void AddChildTo(in BTNode InParent, in BTNode InChild);
		void RemoveChildFrom(in BTNode InParent, in BTNode InChild);
	}

	// Editor
	[CreateAssetMenu()]
	public partial class BehaviourTree : IBTEditorInterface
	{
		public			IBTEditorInterface	AsEditorInterface					=> this;

		[SerializeField, ReadOnly]
		private			BehaviourTree		m_Source							= null;
		
		[SerializeField, ReadOnly]
		private			Vector2				m_Position							= Vector2.zero;

		Vector3 IBTEditorInterface.Position
		{
			get => m_Position;
			set => m_Position = value;
		}

		//---------------------
		List<BTNode> IBTEditorInterface.Nodes => m_Nodes;
		BTRootNode IBTEditorInterface.RootNode => m_RootNode;

		//////////////////////////////////////////////////////////////////////////
		void IBTEditorInterface.UpdateSource()
		{
			/*
			if (CustomAssertions.IsTrue(m_IsInstance, "Trying to save a non instance tree"))
			{
				EditorUtility.SetDirty(m_Source);
				{
					m_Source.m_RootNode = m_RootNode.CloneObject() as BTRootNode;
				}
				AssetDatabase.SaveAssets();



				BehaviourTree newInstance = CreateInstance(this);

				EditorUtility.SetDirty(m_Source);
				{
					// Ensure initial tree state
					newInstance.m_TreeState = EBTNodeResult.RUNNING;
					newInstance.m_IsInstance = false;

					// Set the new root node
					{
						newInstance.m_RootNode.name = newInstance.m_RootNode.name.Replace("(Clone)", "");
						AssetDatabase.AddObjectToAsset(newInstance.m_RootNode, m_Source);
						m_Source.m_RootNode = newInstance.m_RootNode;
					}

					// Remove all previous nodes
					m_Source.m_Nodes.ForEach(node => AssetDatabase.RemoveObjectFromAsset(node));
					m_Source.m_Nodes.Clear();

					// Add all the new children to he source
					Traverse(newInstance.m_RootNode, n =>
					{
						n.name = n.name.Replace("(Clone)", "");
						AssetDatabase.AddObjectToAsset(n, m_Source);
						m_Source.m_Nodes.Add(n);
					});
				}
			}
			*/
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
			
			if (!m_IsInstance)
			{
				EditorUtility.SetDirty(node);
				EditorUtility.SetDirty(this);

				AssetDatabase.AddObjectToAsset(node, this);
				AssetDatabase.SaveAssetIfDirty(node);
				AssetDatabase.SaveAssetIfDirty(this);
			}
			return node;
		}


		//////////////////////////////////////////////////////////////////////////
		void IBTEditorInterface.DeleteNode(in BTNode InNodeToDelete)
		{
			m_Nodes.Remove(InNodeToDelete);
			if (!m_IsInstance)
			{
				EditorUtility.SetDirty(this);
				AssetDatabase.RemoveObjectFromAsset(InNodeToDelete);
				AssetDatabase.SaveAssetIfDirty(this);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		void IBTEditorInterface.AddChildTo(in BTNode InParent, in BTNode InChild)
		{
			if (CustomAssertions.IsTrue(InParent is IParentNode))
			{
				if (!m_IsInstance) // skipping for runtime instances
				{
					EditorUtility.SetDirty(InParent);
					EditorUtility.SetDirty(InChild);
				}
				switch (InParent)
				{
					case BTRootNode node: node.SetChild(InChild); break;
					case BTDecoratorNode node: node.SetChild(InChild); break;
					case BTCompositeNode node: node.Children.Add(InChild); break;
				}
				InChild.AsEditorInterface.Parent = InParent;
				if (!m_IsInstance) // skipping for runtime instances
				{
					AssetDatabase.SaveAssetIfDirty(InParent);
					AssetDatabase.SaveAssetIfDirty(InChild);
				}
			}
		}


		//////////////////////////////////////////////////////////////////////////
		void IBTEditorInterface.RemoveChildFrom(in BTNode InParent, in BTNode InChild)
		{
			if (CustomAssertions.IsTrue(InParent is IParentNode))
			{
				if (!m_IsInstance) // skipping for runtime instances
				{
					EditorUtility.SetDirty(InParent);
					EditorUtility.SetDirty(InChild);
				}

				switch (InParent)
				{
					case BTRootNode node: node.SetChild(null); break;
					case BTDecoratorNode node: node.SetChild(null); break;
					case BTCompositeNode node: node.Children.Remove(InChild); break;
				}
				InChild.AsEditorInterface.Parent = null;
			}
		}
	}
}

#endif
