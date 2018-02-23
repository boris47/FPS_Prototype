
namespace CFG_Reader {

	public class cMultiValue {

		private cValue[]		m_ValuesArray			= null;
		public	cValue[]		ValueArray
		{
			get; private set;
		}

		public	int				Size
		{
			get 
			{
				if ( m_ValuesArray != null )
					return m_ValuesArray.Length;
				return 0;
			}
		}


		public cMultiValue( cValue[] vValues )
		{
			m_ValuesArray = vValues;
		}


		// Indexer behaviour
		public cValue this[ int Index ]
		{
			get
			{
				if ( ( m_ValuesArray != null ) && Index < m_ValuesArray.Length )
					return m_ValuesArray[ Index ];
				return null;
			}
		}

		/////////////////////////////////////////////////////////
		public void		Add( cValue pValue )
		{
			System.Array.Resize<cValue>( ref m_ValuesArray, m_ValuesArray.Length + 1 );
			m_ValuesArray[ m_ValuesArray.Length - 1 ] = pValue;
		}
	
	}

}