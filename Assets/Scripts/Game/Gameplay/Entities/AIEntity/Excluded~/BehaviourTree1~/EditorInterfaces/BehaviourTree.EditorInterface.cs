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
		BTNode CreateNode(System.Type nodeType);
		void DeleteNode(BTNode nodeToDelete);
		void AddChildTo(BTNode parent, BTNode child);
		void RemoveChildFrom(BTNode parent, BTNode child);
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
				m_RootNode = ScriptableObject.CreateInstance<BTRootNode>();
				m_RootNode.name = "Root Node";
				m_RootNode.AsEditorInterface.BehaviourTree = this;

				if (!m_IsInstance)
				{
					EditorUtility.SetDirty(m_RootNode);
					EditorUtility.SetDirty(this);
					AssetDatabase.AddObjectToAsset(m_RootNode, this);
					AssetDatabase.SaveAssets();
				}
			}
			return m_RootNode;
		}


		//////////////////////////////////////////////////////////////////////////		
		BTNode IBTEditorInterface.CreateNode(System.Type nodeType)
		{
			BTNode node = ScriptableObject.CreateInstance(nodeType) as BTNode;
			node.name = nodeType.Name;
			node.AsEditorInterface.BehaviourTree = this;
			m_Nodes.Add(node);

			if (!m_IsInstance)
			{
				EditorUtility.SetDirty(node);
				EditorUtility.SetDirty(this);

				AssetDatabase.AddObjectToAsset(node, this);
				AssetDatabase.SaveAssets();
			}
			return node;
		}


		//////////////////////////////////////////////////////////////////////////
		void IBTEditorInterface.DeleteNode(BTNode nodeToDelete)
		{
			m_Nodes.Remove(nodeToDelete);
			if (!m_IsInstance)
			{
				EditorUtility.SetDirty(this);
				AssetDatabase.RemoveObjectFromAsset(nodeToDelete);
				AssetDatabase.SaveAssets();
			}
		}

		//////////////////////////////////////////////////////////////////////////
		void IBTEditorInterface.AddChildTo(BTNode parent, BTNode child)
		{
			if (!m_IsInstance)
			{
				EditorUtility.SetDirty(parent);
				EditorUtility.SetDirty(child);
			}
			if (parent is BTRootNode rootNode)
			{
				rootNode.SetChild(child);
			}

			if (parent is BTDecoratorNode decorator)
			{
				decorator.SetChild(child);
			}

			if (parent is BTCompositeNode composite)
			{
				composite.Children.Add(child);
			}
			child.AsEditorInterface.Parent = parent;
		}


		//////////////////////////////////////////////////////////////////////////
		void IBTEditorInterface.RemoveChildFrom(BTNode parent, BTNode child)
		{
			if (!m_IsInstance)
			{
				EditorUtility.SetDirty(parent);
				EditorUtility.SetDirty(child);
			}
			if (parent is BTDecoratorNode decorator)
			{
				decorator.SetChild(null);
			}

			if (parent is BTCompositeNode composite)
			{
				composite.Children.Remove(child);
			}
			child.AsEditorInterface.Parent = null;
		}
	}
}

#endif
