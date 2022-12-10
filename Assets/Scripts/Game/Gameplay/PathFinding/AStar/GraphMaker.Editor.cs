#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace AI.Pathfinding
{
	public partial class GraphMaker
	{
		[Header("Editor Only")]
		[SerializeField]
		[Range(0.1f, 10f)]
		private float m_ScanRadius = 1.1f;

		[SerializeField]
		private bool m_ShowNodesOnParentSelection = false;

		public static bool ShowNodesOnParentSelection = false;

		private void OnValidate()
		{
			ShowNodesOnParentSelection = m_ShowNodesOnParentSelection;
		}


		//////////////////////////////////////////////////////////////////////////
		//////////////////////////////////////////////////////////////////////////

		[CustomEditor(typeof(GraphMaker))]
		private class GraphMakerEditor : Editor
		{
			private GraphMaker m_GraphMaker = null;

			//////////////////////////////////////////////////////////////////////////
			private void OnEnable()
			{
				m_GraphMaker = (GraphMaker)target;
			}

			//////////////////////////////////////////////////////////////////////////
			public override void OnInspectorGUI()
			{
				DrawDefaultInspector();

				if (GUILayout.Button("Node Count"))
				{
					Debug.Log($"Node Count: {m_GraphMaker.m_Nodes.Length}");
				}

				if (GUILayout.Button("Build"))
				{
					BuildNodes(m_GraphMaker);
				}

				if (GUILayout.Button("Clear"))
				{
					foreach (AINode node in m_GraphMaker.GetComponentsInChildren<AINode>())
					{
						node.gameObject.Destroy();
					}
				}
			}

			//////////////////////////////////////////////////////////////////////////
			private static void BuildNodes(GraphMaker graphMaker)
			{
				foreach (AINode node in graphMaker.GetComponentsInChildren<AINode>())
				{
					node.gameObject.Destroy();
				}

				if (graphMaker.m_NodesProvider.IsNotNull())
				{
					Vector3[] nodesPosition = graphMaker.m_NodesProvider.GetNodesPosition();
					List<AINode> nodes = new List<AINode>();

					EditorUtility.DisplayProgressBar("Building", "", 0f);
					{
						for (int i = 0, length = nodesPosition.Length; i < length; ++i)
						{
							Vector3 worldPosition = nodesPosition[i];
							GameObject go = new GameObject("AINode");
							Transform t = go.transform;
							t.SetParent(graphMaker.transform);
							t.position = worldPosition;
							t.localRotation = Quaternion.identity;
							nodes.AddRef(go.AddComponent<AINode>());
						}
						graphMaker.m_Nodes = nodes.ToArray();

						for (int i = 0, length = nodesPosition.Length; i < length; ++i)
						{
							graphMaker.m_Nodes[i].FindNeighbours(graphMaker.m_ScanRadius, graphMaker.m_Nodes);
							EditorUtility.DisplayProgressBar("Building", "", i/length);
						}
					}
					EditorUtility.ClearProgressBar();
				}
				Debug.Log("Built! Node count: " + graphMaker.m_Nodes.Length);
			}
		}
	}
}
#endif
