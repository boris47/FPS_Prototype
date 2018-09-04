using UnityEngine;
using System.Collections;
using System;

namespace AI.Pathfinding {

	public interface IHeapItem<T> : IComparable<T>, IEquatable<T> {
		int HeapIndex {
			get;
			set;
		}
	}

	public class Heap<T> where T : IHeapItem<T> {
	
		private	T[]		m_Tree					= null;
		private	int		m_CurrentItemCount		= 0;
		private	int		m_Size					= 0;
	
		public	int Capacity	{		get {		return m_Tree.Length;		}	}
		public int	Count		{		get {		return m_CurrentItemCount;	}	}


		//////////////////////////////////////////////////////////////////////////
		// Heap ( Constructor )
		public Heap( int maxHeapSize )
		{
			m_Tree = new T[ m_Size = maxHeapSize ];
		}


		//////////////////////////////////////////////////////////////////////////
		// Reset
		public void Reset()
		{
			Array.Clear( m_Tree, 0, m_Size );
			m_CurrentItemCount = 0;
		}
		

		//////////////////////////////////////////////////////////////////////////
		// Add
		public void Add( T item )
		{
			item.HeapIndex = m_CurrentItemCount;
			m_Tree[m_CurrentItemCount] = item;

			SortUp( item );

			m_CurrentItemCount ++;
		}


		//////////////////////////////////////////////////////////////////////////
		// RemoveFirst
		public T RemoveFirst()
		{
			T firstItem = m_Tree[0];

			m_CurrentItemCount--;

			m_Tree[0] = m_Tree[ m_CurrentItemCount ];
			m_Tree[0].HeapIndex = 0;

			SortDown( m_Tree[0] );

			return firstItem;
		}


		//////////////////////////////////////////////////////////////////////////
		// Contains
		public bool Contains( T item )
		{
			T item2 = m_Tree[ item.HeapIndex ] ;
			return  item2 != null && item.Equals( item2 );
		}


		//////////////////////////////////////////////////////////////////////////
		// SortDown
		private void SortDown( T item )
		{
			while ( true )
			{
				int childIndexLeft = item.HeapIndex * 2 + 1;
				int childIndexRight = item.HeapIndex * 2 + 2;
				int swapIndex = 0;

				if ( childIndexLeft >= m_CurrentItemCount )
					return;

				swapIndex = childIndexLeft;

				if ( childIndexRight < m_CurrentItemCount )
				{
					if ( m_Tree[ childIndexLeft ].CompareTo( m_Tree[ childIndexRight] ) < 0 )
					{
						swapIndex = childIndexRight;
					}
				}

				T item2 = m_Tree[ swapIndex ];
				if ( item.CompareTo( item2 ) >= 0 )
					return;

				Swap ( item, item2 );
			}
		}
	

		//////////////////////////////////////////////////////////////////////////
		// SortUp
		private void SortUp( T item )
		{
			int parentIndex = ( item.HeapIndex - 1 ) / 2;
		
			while (true)
			{
				T parentItem = m_Tree[ parentIndex ];
				if ( item.CompareTo( parentItem ) <= 0 )
					break;

				Swap ( item, parentItem );

				parentIndex = ( item.HeapIndex - 1 ) / 2;
			}
		}
		

		//////////////////////////////////////////////////////////////////////////
		// Swap
		private void Swap( T itemA, T itemB )
		{
			m_Tree[ itemA.HeapIndex ] = itemB;
			m_Tree[ itemB.HeapIndex ] = itemA;

			int itemAIndex = itemA.HeapIndex;

			itemA.HeapIndex = itemB.HeapIndex;
			itemB.HeapIndex = itemAIndex;
		}
	}

}