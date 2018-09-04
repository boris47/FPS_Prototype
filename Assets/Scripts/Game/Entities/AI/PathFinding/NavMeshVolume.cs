using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Collections.ObjectModel;

namespace AI.Pathfinding {

	[ExecuteInEditMode] [RequireComponent(typeof(BoxCollider))]
	public class NavMeshVolume : MonoBehaviour {

		[SerializeField]
		private		float			m_StepSize	= 1f;
		public		float			StepSize
		{
			get { return m_StepSize; }
			set { m_StepSize = Mathf.Max( 0.01f, value ); }
		}

		private		BoxCollider		m_Collider	= null;

		[SerializeField]
		private		List<AINode>	m_Nodes		= new List<AINode>();


		private void CreateColliderIfNeed()
		{
			if ( ( m_Collider = GetComponent<BoxCollider>() ) == null )
				m_Collider = gameObject.AddComponent<BoxCollider>();

			m_Collider.enabled = false;
		}

		private void OnEnable()
		{
			CreateColliderIfNeed();
		}

		private void OnValidate()
		{
			m_StepSize = Mathf.Max( m_StepSize, 0.1f );
		}


		public ReadOnlyCollection<AINode> GetNodes()
		{
			return new ReadOnlyCollection<AINode>( m_Nodes );
		}

		public void ReleaseNode( AINode node )
		{
			if ( node != null )
			{
				m_Nodes.Remove( node );

				DestroyImmediate( node.gameObject );
			}

		}


		public void	UpdateWithNodes( ref List<AINode> NodeList )
		{
			CreateColliderIfNeed();

			Quaternion prevRotation = transform.rotation;

			transform.rotation = Quaternion.identity;

			AINode nodePrefab = Resources.Load<AINode>( "Prefabs/AI/AINode" );

			// Find already inside nodes
			for ( int i = NodeList.Count - 1; i >= 0; i-- )
			{
				AINode node = NodeList[i];

				if ( IsNodeInside( node ) )
				{
					NodeList.Remove( node );

					//	Destroy this node
					NavMeshVolume volume = node.transform.parent.GetComponent<NavMeshVolume>();
					volume.ReleaseNode( node );
				}
			}


			float extentsX = transform.localScale.x / 2.0f;
			float extentsZ = transform.localScale.z / 2.0f;

			float currentStepX = transform.localScale.x;
			float currentStepZ = transform.localScale.z;

			while ( true )
			{
				float currentX = transform.position.x - extentsX + currentStepX;
				float currentZ = transform.position.z - extentsZ + currentStepZ;
				float currentY = transform.position.y;

				// Create AINode
				AINode node  = Instantiate<AINode>( nodePrefab, new Vector3( currentX, currentY, currentZ ), Quaternion.identity, transform );

				Vector3 localScale = node.transform.localScale;

				Vector3 lossyScale =  node.transform.lossyScale;

				Vector3 newScale = new Vector3( 
					localScale.x / lossyScale.x,
					localScale.y / lossyScale.y,
					localScale.z / lossyScale.z
				);

				node.transform.localScale = newScale;

				m_Nodes.Add( node );
				NodeList.Add( node );

				currentStepX -= m_StepSize;
				if ( currentStepX <= 0.0f )
				{
					currentStepZ -= m_StepSize;
					if ( currentStepZ <= 0.0f )
					{
						break;
					}
					currentStepX = transform.localScale.x;
				}
			}

			transform.rotation = prevRotation;
		}

		public	void	Clear()
		{
			for ( int i = m_Nodes.Count - 1; i >= 0; i-- )
			{
				AINode node = m_Nodes[i];
				ReleaseNode( node );
			}

//			GraphMaker.CollectNodes();
//			foreach( AINode node in GraphMaker.Nodes )
//			{
//				GraphMaker.UpdateNeighbours( node, m_StepSize, isUpdate: true );
//			}

			m_Nodes.Clear();
		}

		public	bool IsNodeInside( AINode node )
		{
			return GetComponent<MeshRenderer>().bounds.Contains( node.transform.position );
		}



		private void OnDrawGizmosSelected()
		{
			if ( m_Nodes != null && m_Nodes.Count > 0 )
			{
				foreach( AINode node in m_Nodes )
				{
					Gizmos.DrawSphere( node.transform.position, 0.5f );
				}
			}
		}
	}

}