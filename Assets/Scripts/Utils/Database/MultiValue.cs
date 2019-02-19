
namespace Database {

	using System.Collections.Generic;

	public class cMultiValue {

		private List<cValue>		m_ValuesArray			= new List<cValue>();
		public	List<cValue>		ValueArray
		{
			get { return m_ValuesArray; }
		}

		public	int				Size
		{
			get 
			{
				return m_ValuesArray.Count;
			}
		}


		public cMultiValue( cValue[] vValues )
		{
			m_ValuesArray = new List<cValue>( vValues );
		}


		// Indexer behaviour
		public cValue this[ int Index ]
		{
			get
			{
				if ( m_ValuesArray.Count > Index )
					return m_ValuesArray[Index];
				return null;
			}
		}

		/////////////////////////////////////////////////////////
		public void		Add( cValue pValue )
		{
			m_ValuesArray.Add( pValue );
		}
	
	}

}