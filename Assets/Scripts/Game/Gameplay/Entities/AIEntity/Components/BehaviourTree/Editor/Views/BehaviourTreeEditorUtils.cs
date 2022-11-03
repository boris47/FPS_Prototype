using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.Experimental.GraphView;

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
		private const string kNoDescText = "No Description";

		//////////////////////////////////////////////////////////////////////////
		public static bool TryGetNameAndDescription(in System.Type InBTNodeType, out string OutName, out string OutDescription)
		{
			OutName = InBTNodeType.Name;
			OutDescription = kNoDescText;
			if (Utils.CustomAssertions.IsTrue(ReflectionHelper.IsInerithedFrom(typeof(BTNode), InBTNodeType)))
			{
				System.Type currentType = InBTNodeType;
				BTNodeDetailsAttribute attribute = null;
				while (currentType.IsNotNull() && attribute == null)
				{
					if (ReflectionHelper.TryGetAttributeValue(currentType, (BTNodeDetailsAttribute att) => att, out attribute))
					{
						OutName = string.IsNullOrEmpty(attribute.Name) ? OutName : attribute.Name;
						OutDescription = string.IsNullOrEmpty(attribute.Description) ? kNoDescText : attribute.Description;
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
		internal static void UpdateNodeIndexes(in BehaviourTreeView InBehaviourTreeAssetView)
		{
			if (!EditorApplication.isPlayingOrWillChangePlaymode)
			{
				BehaviourTree behaviourTreeAsset = InBehaviourTreeAssetView.BehaviourTreeAsset;

				// Check for orphan nodes assets
				{
					BTNode[] nodes = BehaviourTree.Editor.GetAllNodes(behaviourTreeAsset);
					if (nodes.Any())
					{
						if (behaviourTreeAsset.TryGetSubObjectsOfType(out BTNode[] subAssets))
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
							//	foreach (BTNode node in nodes)
							//	{
							//		if (!subAssets.Contains(node))
							//		{
							//			if (EditorUtility.DisplayDialog($"Unknown Node", $"Found node {node.name} stored in node list not added as sub-asset, adding it", "OK"))
							//			{
							//				AssetDatabase.AddObjectToAsset(node, behaviourTreeAsset);
							//			}
							//		}
							//	}
						}
					}
				}

				// Actually update nodes indexes and rename assets
				{
					// Update indexes on used nodes
					BTNode[] allUsedNodes = BehaviourTree.Editor.GetNodesUsed(behaviourTreeAsset);
					{
						for (uint nodeIndex = 0, length = (uint)allUsedNodes.Length; nodeIndex < length; nodeIndex++)
						{
							BTNode.Editor.SetNodeIndex(allUsedNodes[nodeIndex], nodeIndex);
						}
					}

					// Ensure proper asset name and in graph node index
					using (new Utils.Editor.MarkAsDirty(behaviourTreeAsset))
					{
						foreach (BTNode node in allUsedNodes)
						{
							if (node.ParentAsset.IsNotNull() || node is BTRootNode)
							{
								node.name = $"{node.NodeIndex.ToString().PadLeft(3, '0')}.{node.GetType().Name}";
							}
							else
							{
								BTNode.Editor.SetNodeIndex(node, 1000u);  // Expose a const somewhere to limit node numbers
								node.name = $"---.{node.GetType().Name}";
							}

							// update in graph view
							if (InBehaviourTreeAssetView.TryFindNodeView(node, out NodeViewBase nodeVew))
							{
								nodeVew.UpdateView();
							}
						}

						// Sort all nodes by nodeIndex
						{
							static int SortByNodeIndex(BTNode left, BTNode right) => left.NodeIndex < right.NodeIndex ? -1 : 1;
							BehaviourTree.Editor.SortNodes(behaviourTreeAsset, SortByNodeIndex);
						}
					}
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
