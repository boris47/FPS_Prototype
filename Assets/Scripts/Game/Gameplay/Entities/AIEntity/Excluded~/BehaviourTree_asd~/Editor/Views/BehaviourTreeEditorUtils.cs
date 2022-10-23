using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using System.Reflection;

namespace Entities.AI.Components.Behaviours
{
	//////////////////////////////////////////////////////////////////////////
	internal static class Extensions
	{
		public static NodeViewBase GetAsBTNodeView(this Node nodeView) => nodeView as NodeViewBase;
		public static BTNode GetBTNode(this Node nodeView) => nodeView.GetAsBTNodeView().BehaviourTreeNode;

		public static NodeViewPort GetBTNodePort(this Port port) => port as NodeViewPort;
		public static NodeViewBase GetNodeViewBase(this Port port) => port.node.GetAsBTNodeView();
		public static BTNode GetBTNode(this Port port) => port.GetBTNodePort().Node;
	}


	//////////////////////////////////////////////////////////////////////////
	public static class BehaviourTreeEditorUtils
	{
		//////////////////////////////////////////////////////////////////////////
		public static bool TryGetNameAndDescription(in System.Type InBTNodeType, out string OutName, out string OutDescription)
		{
			OutName = InBTNodeType.Name;
			OutDescription = "No Description";
			if (Utils.CustomAssertions.IsTrue(ReflectionHelper.IsInerithedFrom(typeof(BTNode), InBTNodeType)))
			{
				System.Type currentType = InBTNodeType;
				BTNodeDetailsAttribute attribute = null;
				while (currentType.IsNotNull() && attribute == null)
				{
					attribute = currentType.GetCustomAttribute<BTNodeDetailsAttribute>();
					if (attribute.IsNotNull())
					{
						OutName = string.IsNullOrEmpty(attribute.Name) ? OutName : attribute.Name;
						OutDescription = string.IsNullOrEmpty(attribute.Description) ? "No Description" : attribute.Description;
					}
					else
					{
						currentType = currentType.BaseType;
					}
				}
			}
			return !string.IsNullOrEmpty(OutName) && !string.IsNullOrEmpty(OutDescription);
		}

		//////////////////////////////////////////////////////////////////////////
		public static bool TryGetStyleAssetPath(string closeFileName, out string OutValue)
		{
			OutValue = null;
			string[] res = System.IO.Directory.GetFiles(Application.dataPath, $"{closeFileName}.cs", System.IO.SearchOption.AllDirectories)
				// We are excluding excluded files
				.Where(s => !s.Contains('~')).ToArray();

			if (Utils.CustomAssertions.IsTrue(res.TryGetByIndex(0, out string path)))
			{
				path = System.IO.Path.GetDirectoryName(path.Substring(path.IndexOf("Assets"))).Replace("\\", "/");
				if (!string.IsNullOrEmpty(path))
				{
					OutValue = path;
				}
			}
			return OutValue.IsNotNull();
		}


		//////////////////////////////////////////////////////////////////////////
		public static void AssignChildrenIndexes(in BTRootNode InRootNode)
		{
			static void AssignIndexToNode(in BTNode InNode, ref uint currentIndex)
			{
				// Update this node index
				BTNode.Editor.SetNodeIndex(InNode, currentIndex++);
			//	InNode.UpdateView();

				if (InNode is IParentNode asParentNode)
				{
					// Sort child if composite
					if (InNode is BTCompositeNode compositeEditor)
					{
						BTCompositeNode.Editor.SortChildren(compositeEditor);
					}

					for (uint i = 0, count = (uint)asParentNode.Children.Count; i < count; i++)
					{
						BTNode child = asParentNode.Children.At(i);
						AssignIndexToNode(child, ref currentIndex);
					}
				}
			}

			// Check for orphan nodes
			{
				if (InRootNode.BehaviourTreeAsset.AsEditorInterface.Nodes.Any())
				{
					BehaviourTree tree = InRootNode.BehaviourTreeAsset;
					List<BTNode> nodes = tree.AsEditorInterface.Nodes;
					if (tree.TryGetSubObjectsOfType(out BTNode[] subAssets))
					{
						// Removing sub-assets not referenced in tree node list
						foreach (BTNode node in subAssets)
						{
							// sub asset is not contained in the tree node list
							if (!nodes.Contains(node))
							{
								if (EditorUtility.DisplayDialog($"Unknown Node", $"Found node {node.name} stored as sub-asset but not included in node list, removing it", "OK"))
								{
									// Remove node as sub asset from this tree
									AssetDatabase.RemoveObjectFromAsset(node);
								}
							}
						}

						// Adding to tree object those nodes that are referenced in tree node list but not sub-assets
						foreach (BTNode node in nodes)
						{
							if (!subAssets.Contains(node))
							{
								if (EditorUtility.DisplayDialog($"Unknown Node", $"Found node {node.name} stored in node list not added as sub-asset, adding it", "OK"))
								{
									AssetDatabase.AddObjectToAsset(node, tree);
								}
							}
						}
					}
				}
			}

			{
				uint currentIndex = 0u;
				AssignIndexToNode(InRootNode, ref currentIndex);

				static int SortByNodeIndex(BTNode left, BTNode right) => left.NodeIndex < right.NodeIndex ? -1 : 1;
				InRootNode.BehaviourTreeAsset.AsEditorInterface.Nodes.Sort(SortByNodeIndex);
				if (InRootNode.BehaviourTreeAsset.AsEditorInterface.Nodes.IsValidIndex(0))
				{
					BehaviourTree tree = InRootNode.BehaviourTreeAsset;
					List<BTNode> nodes = tree.AsEditorInterface.Nodes;
					EditorUtility.SetDirty(tree);
					{
						for (int i = 0; i < nodes.Count; i++)
						{
							BTNode node = nodes[i];
							node.name = $"{i.ToString().PadLeft(3, '0')}. {node.GetType().Name}";
						}
					}
					AssetDatabase.SaveAssetIfDirty(tree);
				}
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	internal static class BTAutoArrangeHelpers
	{
		private class NodeBoundsInfo
		{
			public Vector2 SubGraphBBox = Vector2.zero;
			public List<NodeBoundsInfo> Children = new List<NodeBoundsInfo>();
		};

		//////////////////////////////////////////////////////////////////////////
		public static void AutoArrange(in Node InRootNode, in float? InHorizontalLeafDistance = 20f, in float? InVerticalDistance = 30f)
		{
			NodeBoundsInfo BBoxTree = new NodeBoundsInfo();
			GetNodeSizeInfo(InRootNode, BBoxTree, InHorizontalLeafDistance.Value, InVerticalDistance.Value);
			AutoArrangeNodes(InRootNode, BBoxTree, 0f, InRootNode.GetPosition().height + InVerticalDistance.Value, InHorizontalLeafDistance.Value, InVerticalDistance.Value);

			Rect position = InRootNode.GetPosition();
			{
				position.xMin = (BBoxTree.SubGraphBBox.x * 0.5f) - (InRootNode.GetPosition().width * 0.5f);
				position.yMin = 0f;
			}
			InRootNode.SetPosition(position);
		}

		//////////////////////////////////////////////////////////////////////////
		private static IReadOnlyList<Node> GetChildrenNodes(in Node InParentNode)
		{
			static int SortByHorizontalPosition(Node left, Node right) => left.GetPosition().xMin < right.GetPosition().xMin ? -1 : 1;

			List<Node> children = new List<Node>();
			if (InParentNode.outputContainer.IsNotNull())
			{
				foreach(Port outputPort in InParentNode.outputContainer.Query<Port>().ToList())
				{
					foreach (Edge edge in outputPort.connections)
					{
						// edge.input because it's the input port of the child
						children.Add(edge.input.node);
					}
				}
			}
			children.Sort(SortByHorizontalPosition);
			return children;
		}

		//////////////////////////////////////////////////////////////////////////
		private static void GetNodeSizeInfo(in Node InParentNode, in NodeBoundsInfo InBBoxTree, in float InHorizontalLeafDistance, in float InVerticalDistance)
		{
			Rect parentPositionAndSize = InParentNode.GetPosition();
			InBBoxTree.SubGraphBBox = new Vector2(parentPositionAndSize.width, parentPositionAndSize.height);
			float LevelWidth = 0f, LevelHeight = 0f;

			IReadOnlyList<Node> children = GetChildrenNodes(InParentNode);
			for (int i = 0, Count = children.Count; i < Count; i++)
			{
				NodeBoundsInfo ChildBounds = InBBoxTree.Children.AddRef(new NodeBoundsInfo());
				GetNodeSizeInfo(children[i], ChildBounds, InHorizontalLeafDistance, InVerticalDistance);
				
				LevelWidth += ChildBounds.SubGraphBBox.x + (i == Count - 1 ? 0f : InHorizontalLeafDistance);
				if (ChildBounds.SubGraphBBox.y > LevelHeight)
				{
					LevelHeight = ChildBounds.SubGraphBBox.y;
				}
			}

			if (LevelWidth > InBBoxTree.SubGraphBBox.x)
			{
				InBBoxTree.SubGraphBBox.x = LevelWidth;
			}

			InBBoxTree.SubGraphBBox.y += LevelHeight;
		}

		//////////////////////////////////////////////////////////////////////////
		private static void AutoArrangeNodes(in Node InParentNode, in NodeBoundsInfo InBBoxTree, in float InPosX, in float InPosY, in float InHorizontalLeafDistance, in float InVerticalDistance)
		{
			if (InBBoxTree.Children.Count > 0)
			{
				int BBoxIndex = 0;
				float localPosX = InPosX;

				IReadOnlyList<Node> children = GetChildrenNodes(InParentNode);
				for (int i = 0, Count = children.Count; i < Count; i++)
				{
					Node childNode = children[i];
					AutoArrangeNodes(childNode, InBBoxTree.Children[BBoxIndex], localPosX, InPosY + (childNode.GetPosition().height + InVerticalDistance), InHorizontalLeafDistance, InVerticalDistance);

					Rect position = childNode.GetPosition();
					{
						position.xMin = (InBBoxTree.Children[BBoxIndex].SubGraphBBox.x * 0.5f) - (position.width * 0.5f) + localPosX;
						position.yMin = InPosY;
					}
					childNode.SetPosition(position);

					localPosX += InBBoxTree.Children[BBoxIndex].SubGraphBBox.x + (i == Count - 1 ? 0f : 20f);
					BBoxIndex++;
				}
			}
		}
	}
}
