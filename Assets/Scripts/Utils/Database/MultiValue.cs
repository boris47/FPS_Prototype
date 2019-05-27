
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
	
	}

}