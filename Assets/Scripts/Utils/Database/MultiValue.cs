
namespace Database {

	using System.Collections;
	using System.Collections.Generic;

	public class cMultiValue : IEnumerable {

		private List<cValue>		m_ValuesList			= new List<cValue>();
		public	List<cValue>		ValueList
		{
			get { return this.m_ValuesList; }
		}

		public	int				Size
		{
			get 
			{
				return this.m_ValuesList.Count;
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return (IEnumerator)this.GetEnumerator();
		}

		public List<cValue>.Enumerator  GetEnumerator()
		{
			return this.m_ValuesList.GetEnumerator();
		}

		public cMultiValue( cValue[] vValues = null, int capacity = 1 )
		{
			this.m_ValuesList = vValues == null ? new List<cValue>(capacity) : new List<cValue>( vValues );
		}


		// Indexer behaviour
		public cValue this[ int Index ]
		{
			get
			{
				if (this.m_ValuesList.Count > Index )
					return this.m_ValuesList[Index];
				return null;
			}
		}

		/////////////////////////////////////////////////////////
		public void		Add( cValue pValue )
		{
			this.m_ValuesList.Add( pValue );
		}


		public	bool	DeductType( ref System.Type typeFound )
		{
			bool result = true;
			{
				System.Type elementType = this.m_ValuesList[0].GetType();
				bool bIsSameType = this.m_ValuesList.TrueForAll( v => v.GetType() == elementType );
				if ( bIsSameType )
				{
					if ( elementType == typeof( int ) || elementType == typeof( float ) )
					{
						if (this.m_ValuesList.Count == 2 )
						{
							typeFound = typeof( UnityEngine.Vector2 );
						}

						if (this.m_ValuesList.Count == 3 )
						{
							typeFound = typeof( UnityEngine.Vector3 );
						}

						if (this.m_ValuesList.Count == 4 )
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