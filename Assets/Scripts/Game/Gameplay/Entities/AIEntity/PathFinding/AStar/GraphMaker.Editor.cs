#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace AI.Pathfinding
{
	internal partial class GraphMaker : IVolumeIterator
	{
		[Header("Editor Only")]
		[SerializeField]
		[Range(0.1f, 10f)]
		private float m_ScanRadius = 1.1f;

		//////////////////////////////////////////////////////////////////////////
		void IVolumeIterator.OnIterationStart()
		{
			foreach (AINode item in FindObjectsOfType<AINode>())
			{
				item.gameObject.Destroy();
			}
		}

		//////////////////////////////////////////////////////////////////////////
		void IVolumeIterator.OnIteration(Vector3 InPosition)
		{
			// Create Node
			var t = new GameObject("AINode", typeof(AINode)).transform;
			t.SetParent(transform);
			t.position = InPosition;
		}

		//////////////////////////////////////////////////////////////////////////
		void IVolumeIterator.OnIterationCompleted()
		{
			foreach(AINode n in (m_Nodes = FindObjectsOfType<AINode>()))
			{
				n.FindNeighbours(m_ScanRadius, m_Nodes);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		private void OnDrawGizmosSelected()
		{
			if (Selection.activeTransform == transform)
			{
				foreach (AINode node in m_Nodes)
				{
					Gizmos.DrawSphere(node.transform.position, 0.15f);
				}
			}
		}

		//////////////////////////////////////////////////////////////////////////
		//////////////////////////////////////////////////////////////////////////

		[CustomEditor(typeof(GraphMaker))]
		private class GraphMakerEditor : Editor
		{
			private void OnEnable()
			{
				GraphMaker graphMaker = (GraphMaker)target;

				graphMaker.m_Nodes = FindObjectsOfType<AINode>();
			}

			public override void OnInspectorGUI()
			{
				DrawDefaultInspector();

				GraphMaker graphMaker = (GraphMaker)target;

				if (GUILayout.Button("Node Count"))
				{
					Debug.Log("Node Count: " + (graphMaker.m_Nodes ?? FindObjectsOfType<AINode>()).Length);
				}

				if (GUILayout.Button("Build"))
				{
					BuildNodes(graphMaker);

			//		foreach (AINode node in ())
					{
			//			node.FindNeighbours(graphMaker.m_ScanRadius, graphMaker.m_Nodes);
				//		EditorUtility.SetDirty(node);
					}
			//		Debug.Log("Built! Node count: " + graphMaker.m_Nodes.Length);
					//	EditorUtility.SetDirty(graphMaker);

				}
			}

			private static void BuildNodes(GraphMaker graphMaker)
			{
				EditorUtility.DisplayProgressBar("Building", "", 0f);
				{
					graphMaker.m_Nodes = FindObjectsOfType<AINode>();
					for (int i = 0, length = graphMaker.m_Nodes.Length; i < length; i++)
					{
						graphMaker.m_Nodes[i].FindNeighbours(graphMaker.m_ScanRadius, graphMaker.m_Nodes);
						EditorUtility.DisplayProgressBar("Building", "", i/length);
					}
				}
				EditorUtility.ClearProgressBar();

				Debug.Log("Built! Node count: " + graphMaker.m_Nodes.Length);
			}
		}
	}
}
#endif
