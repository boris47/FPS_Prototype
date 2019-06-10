
namespace Database {

	using System.Collections;
	using System.Collections.Generic;

	public class cMultiValue : IEnumerable {

		private List<cValue>		m_ValuesList			= new List<cValue>();
		public	List<cValue>		ValueList
		{
			get { return m_ValuesList; }
		}

		public	int				Size
		{
			get 
			{
				return m_ValuesList.Count;
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return (IEnumerator) GetEnumerator();
		}

		public List<cValue>.Enumerator  GetEnumerator()
		{
			return m_ValuesList.GetEnumerator();
		}


		public cMultiValue( cValue[] vValues )
		{
			m_ValuesList = new List<cValue>( vValues );
		}


		// Indexer behaviour
		public cValue this[ int Index ]
		{
			get
			{
				if ( m_ValuesList.Count > Index )
					return m_ValuesList[Index];
				return null;
			}
		}

		/////////////////////////////////////////////////////////
		public void		Add( cValue pValue )
		{
			m_ValuesList.Add( pValue );
		}


		public	bool	DeductType( ref System.Type typeFound )
		{
			bool result = true;
			{
				System.Type elementType = m_ValuesList[0].GetType();
				bool bIsSameType = m_ValuesList.TrueForAll( v => v.GetType() == elementType );
				if ( bIsSameType )
				{
					if ( elementType == typeof( int ) || elementType == typeof( float ) )
					{
						if ( m_ValuesList.Count == 2 )
						{
							typeFound = typeof( UnityEngine.Vector2 );
						}

						if ( m_ValuesList.Count == 3 )
						{
							typeFound = typeof( UnityEngine.Vector3 );
						}

						if ( m_ValuesList.Count == 4 )
						{
							typeFound = typeof( UnityEngine.Vector4 );
						}
					}
					else
					{
						typeFound = elementType;
					}
				}
				else
				{
					UnityEngine.Debug.Log( "Multivalue of not of same type requeste in DeductType" );
					result = false;
				}
			}
			return result;
		}
	
	}

}