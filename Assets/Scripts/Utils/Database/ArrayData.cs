
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Database {

	// PUBLIC INTERFACE
	public interface IArrayData {

	}


	[System.Serializable]
	public partial class ArrayData : IArrayData, IEnumerable {

		// INTERNAL VARS
		[SerializeField]
		private		string				name			= null;

		[SerializeField]
		private		string				m_Context		= "";

		[SerializeField]
		private		List<cLineValue>	vList		= new List<cLineValue>();

		[SerializeField]
		private		List<string>		m_Mothers		= new List<string>();

		[SerializeField]
		public		bool				m_bIsOK
		{
			get; private set;
		}

		public	string	Context
		{
			get { return ( m_Context.IsNotNull() ) ? (string)m_Context.Clone() :""; }
		}



		IEnumerator IEnumerable.GetEnumerator()
		{
			return (IEnumerator) GetEnumerator();
		}
	
		public List<cLineValue>.Enumerator  GetEnumerator()
		{
			return vList.GetEnumerator();
		}


		public ArrayData( string arrayDataName, string context )
		{
			name = arrayDataName;
			m_Context = context;
			m_bIsOK = true;
		}

		public static bool operator !( ArrayData obj )
		{
			return obj == null;
		}

		public static bool operator false( ArrayData obj )
		{
			return obj == null;
		}

		public static bool operator true( ArrayData obj )
		{
			return obj != null;
		}


		public static ArrayData operator +( ArrayData listA, ArrayData listB )
		{
			if ( listB.m_bIsOK == true )
			{
				foreach( cLineValue lineValue in listB )
				{
					listA.Add( lineValue );
				}
				listA.m_Mothers.Add( listB.name );
			}
			return listA;
		}


		public	bool					IsChildOf						( ArrayData mother )
		{
			string motherName = mother.GetName();
			return ( m_Mothers.FindIndex( m => m == motherName ) > -1 );
		}

		public	bool					IsChildOf						( string MotherName )
		{
			return ( m_Mothers.FindIndex( m => m == MotherName ) > -1 );
		}

		public void Destroy()
		{
			vList.ForEach( ( cLineValue lv ) => lv.Destroy() );
		}

		public	int						Lines()
		{
			return vList.Count;
		}

		public	string					GetName()				{ return ( string ) name.Clone(); }


		//////////////////////////////////////////////////////////////////////////
		// Add
		public	bool				Add( cLineValue LineValue )
		{
			int index = vList.FindIndex( ( s ) => s.Key == LineValue.Key );
			// Confirmed new linevalue
			if ( index == -1 )
			{
				vList.Add( LineValue );
			}
			// overwrite of existing linevalue
			else
			{
				vList[ index ] = new cLineValue( LineValue );
			}
			return index > -1;
		}


		//////////////////////////////////////////////////////////////////////////
		// Remove
		public	bool					Remove( string lineValueID )
		{
			int index = vList.FindIndex( ( s ) => s.Key == lineValueID );
			if ( index > -1 )
			{
				vList[index].Destroy();
				vList.RemoveAt( index );
			}
			return index > -1;
		}


		//////////////////////////////////////////////////////////////////////////
		// bGetLineValue
		public	bool					bGetLineValue( int index, ref cLineValue lineValue )
		{
			if ( index < 0 || index >= vList.Count )
				return false;

			lineValue = vList[ index ];
			return true;
		}


		//////////////////////////////////////////////////////////////////////////
		// IsValidIndex
		public	bool					IsValidIndex( int index )
		{	
			return index > -1 && index < vList.Count;

		}



		//////////////////////////////////////////////////////////////////////////
		// ValueType
		public	global::System.Type		ValueType( int index )
		{
			cLineValue pLineValue = null;
			if ( bGetLineValue( index, ref pLineValue ) )
			{
				if ( pLineValue.Type == LineValueType.SINGLE )
				{
					return pLineValue.Value.GetType();
				}
			}
			return null;
		}


		//////////////////////////////////////////////////////////////////////////
		// GetRawValue
		public	string					GetRawValue( int index, string Default = "" )
		{
			cLineValue pLineValue = null;
			return ( bGetLineValue( index, ref pLineValue ) ) ? pLineValue.RawValue : Default;
		}


		//////////////////////////////////////////////////////////////////////////
		// As<T>
		public	T						As<T>( int index )
		{
			cLineValue pLineValue = null;
			if ( bGetLineValue( index, ref pLineValue ) && pLineValue.Type == LineValueType.SINGLE )
			{
				return pLineValue.Value.As<T>();
			}
			return default( T );
		}

		//////////////////////////////////////////////////////////////////////////
		// AsBool
		public	bool					AsBool( int index, bool Default = false )
		{
			cLineValue pLineValue = null;
			if ( bGetLineValue( index, ref pLineValue ) && pLineValue.Type == LineValueType.SINGLE )
			{
				return pLineValue.Value.As<bool>();
			}
			return Default;
		}


		//////////////////////////////////////////////////////////////////////////
		// AsInt
		public	int						AsInt( int index, int Default = 0 )
		{
			cLineValue pLineValue = null;
			if ( bGetLineValue( index, ref pLineValue ) && pLineValue.Type == LineValueType.SINGLE )
			{
				return pLineValue.Value.As<int>();
			}
			return Default;
		}

		//////////////////////////////////////////////////////////////////////////
		// AsInt ( UInt )
		public	uint					AsInt( int index, uint Default = 0u )
		{
			return (uint)AsInt( index, (int)Default );
		}


		//////////////////////////////////////////////////////////////////////////
		// AsFloat
		public	float					AsFloat( int index, float Default = 0.0f )
		{
			
			cLineValue pLineValue = null;
			if ( bGetLineValue( index, ref pLineValue ) && pLineValue.Type == LineValueType.SINGLE )
			{
				float value = pLineValue.Value;
				return value;
			}
			return Default;
		}


		//////////////////////////////////////////////////////////////////////////
		// AsString
		public	string					AsString( int index, string Default = "" )
		{
			cLineValue pLineValue = null;
			if ( bGetLineValue( index, ref pLineValue ) )
			{
				return pLineValue.Value.As<string>();
			}
			return Default;
		}


		//////////////////////////////////////////////////////////////////////////
		// bGetMultiAsArray
		public	bool						bGetMultiAsArray<T>( ref T[] array )
		{
			bool bResult = true;

			System.Type requiredType = typeof( T );

			System.Converter<cLineValue, T> converter = delegate( cLineValue v )
			{
				bResult &= v.Value.GetType() == requiredType;
				return v.Value.As<T>();
			};

			// Get a list of converted cvalues to requested type
			List<T> converted = vList.ConvertAll( converter );

			if ( bResult )
			{
				array = converted.ToArray();
			}
			else
			{
				converted.Clear();
				converted = null;
			}
			
			return bResult;
		}






























		//////////////////////////////////////////////////////////////////////////
		// bAs<T>
		public	bool					bAs<T>( int index, ref T Out )
		{
			cLineValue pLineValue = null;
			if ( bGetLineValue( index, ref pLineValue ) )
			{
				if ( pLineValue.Type == LineValueType.SINGLE )
				{
					Out = pLineValue.Value.As<T>();
					return true;
				}
			}
			Out = default(T);
			return false;
		}
		

		//////////////////////////////////////////////////////////////////////////
		// bAsBool
		public	bool					bAsBool( int index, ref bool Out, bool Default = false )
		{
			cLineValue pLineValue = null;
			if ( bGetLineValue( index, ref pLineValue ) )
			{
				if ( pLineValue.Type == LineValueType.SINGLE )
				{
					Out = pLineValue.Value.ToBool();
					return true;
				}
			}
			Out = Default;
			return false;
		}


		//////////////////////////////////////////////////////////////////////////
		// bAsInt
		public	bool					bAsInt( int index, ref int Out, int Default = 0 )
		{
			cLineValue pLineValue = null;
			if ( bGetLineValue( index, ref pLineValue ) )
			{
				if ( pLineValue.Type == LineValueType.SINGLE )
				{
					Out = pLineValue.Value.ToInteger();
					return true;
				}
			}
			Out = Default;
			return false;
		}


		//////////////////////////////////////////////////////////////////////////
		// bAsFloat
		public	bool					bAsFloat( int index, ref float Out, float Default = 0.0f )
		{
			cLineValue pLineValue = null;
			if ( bGetLineValue( index, ref pLineValue ) )
			{
				if ( pLineValue.Type == LineValueType.SINGLE )
				{
					Out = pLineValue.Value.ToFloat();
					return true;
				}
			}
			Out = Default;
			return false;
		}


		//////////////////////////////////////////////////////////////////////////
		// bAsString
		public	bool					bAsString( int index, ref string Out, string Default = "" )
		{
			cLineValue pLineValue = null;
			if ( bGetLineValue( index, ref pLineValue ) )
			{
				if ( pLineValue.Type == LineValueType.SINGLE )
				{
					Out = pLineValue.Value.ToString();
					return true;
				}
			}
			Out = Default;
			return false;
		}


	}

}