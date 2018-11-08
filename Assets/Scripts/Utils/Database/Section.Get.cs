
namespace Database {

	public partial class Section {


		//////////////////////////////////////////////////////////////////////////
		// ValueType
		public	global::System.Type		ValueType( string Key )
		{
			cLineValue pLineValue = null;
			if ( ( pLineValue = this[ Key ] ) != null )
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
		public	string					GetRawValue( string Key, string Default = "" )
		{
			cLineValue pLineValue = null;
			return ( ( pLineValue = this[ Key ] ) != null ) ? pLineValue.RawValue : Default;
		}


		//////////////////////////////////////////////////////////////////////////
		// As<T>
		public	T						As<T>( string Key )
		{
			cLineValue pLineValue = this[ Key ];
			if ( pLineValue != null )
			{
				if ( pLineValue.Type == LineValueType.SINGLE )
				{
					if ( pLineValue.Value.ToSystemObject().GetType() == typeof( T ) )
						return pLineValue.Value.As<T>();
				}
			}
			return default( T );
		}


		//////////////////////////////////////////////////////////////////////////
		// AsBool
		public	bool					AsBool( string Key, bool Default = false )
		{
			cLineValue pLineValue = this[ Key ];
			if ( pLineValue != null )
			{
				if ( pLineValue.Type == LineValueType.SINGLE )
				{
					if ( pLineValue.Value.ToSystemObject().GetType() == typeof( bool ) )
						return pLineValue.Value.ToBool();
				}
			}
			return Default;
		}


		//////////////////////////////////////////////////////////////////////////
		// AsInt
		public	int						AsInt( string Key, int Default = 0 )
		{
			cLineValue pLineValue = this[ Key ];
			if ( pLineValue != null )
			{
				if ( pLineValue.Type == LineValueType.SINGLE )
				{
					if ( pLineValue.Value.ToSystemObject().GetType() == typeof( int ) )
						return pLineValue.Value.ToInteger();
				}
			}
			return Default;
		}


		//////////////////////////////////////////////////////////////////////////
		// AsFloat
		public	float					AsFloat( string Key, float Default = 0.0f )
		{
			cLineValue pLineValue = this[ Key ];
			if ( pLineValue != null )
			{
				if ( pLineValue.Type == LineValueType.SINGLE )
				{
					if ( pLineValue.Value.ToSystemObject().GetType() == typeof( float ) )
						return pLineValue.Value.ToFloat();
				}
			}
			return Default;
		}


		//////////////////////////////////////////////////////////////////////////
		// AsString
		public	string					AsString( string Key, string Default = "" )
		{
			cLineValue pLineValue = this[ Key ];
			if ( pLineValue != null )
			{
				if ( pLineValue.Type == LineValueType.SINGLE )
				{
					if ( pLineValue.Value.ToSystemObject().GetType() == typeof( string ) )
						return pLineValue.Value.ToString();
				}
			}
			return Default;
		}


		//////////////////////////////////////////////////////////////////////////
		// AsMultiValue
		public	cValue					AsMultiValue( string Key, int Index )
		{
			cLineValue pLineValue		= this[ Key ];
			if ( pLineValue != null )
			{
				cMultiValue pMultiValue = pLineValue.MultiValue;
				if ( pMultiValue != null )
				{
					return pMultiValue [Index - 1 ];
				}
			}
			return null;
		}


		//////////////////////////////////////////////////////////////////////////
		// AsMultiValue<T1,T2>
		public	void					AsMultiValue<T1,T2>( string Key, int Idx1, int Idx2, ref T1 t1, ref T2 t2 )
		{
			cLineValue pLineValue		= this[ Key ];
			if ( pLineValue != null )
			{
				cMultiValue pMultiValue = pLineValue.MultiValue;
				if ( pMultiValue != null )
				{
					t1 = pMultiValue [Idx1 - 1].As<T1>();
					t2 = pMultiValue [Idx2 - 1].As<T2>();
				}
			}
		}


		//////////////////////////////////////////////////////////////////////////
		// AsMultiValue<T1,T2,T3>
		public	void					AsMultiValue<T1,T2,T3>( string Key, int Idx1, int Idx2, int Idx3, ref T1 t1, ref T2 t2, ref T3 t3 )
		{
			cLineValue pLineValue		= this[ Key ];
			if ( pLineValue != null )
			{
				cMultiValue pMultiValue = pLineValue.MultiValue;
				if ( pMultiValue != null )
				{
					t1 = pMultiValue [Idx1 - 1].As<T1>();
					t2 = pMultiValue [Idx2 - 1].As<T2>();
					t3 = pMultiValue [Idx3 - 1].As<T3>();
				}
			}
		}


		//////////////////////////////////////////////////////////////////////////
		// AsMultiValue<T1,T2,T3,T4>
		public	void					AsMultiValue<T1,T2,T3,T4>( string Key, int Idx1, int Idx2, int Idx3, int Idx4, ref T1 t1, ref T2 t2, ref T3 t3, ref T4 t4 )
		{
			cLineValue pLineValue		= this[ Key ];
			if ( pLineValue != null )
			{
				cMultiValue pMultiValue = pLineValue.MultiValue;
				if ( pMultiValue != null )
				{
					t1 = pMultiValue [Idx1 - 1].As<T1>();
					t2 = pMultiValue [Idx2 - 1].As<T2>();
					t3 = pMultiValue [Idx3 - 1].As<T3>();
					t4 = pMultiValue [Idx4 - 1].As<T4>();
				}
			}
		}


		//////////////////////////////////////////////////////////////////////////
		// GetMultiSize
		public	int						GetMultiSize( string Key )
		{
			cLineValue pLineValue = null; cMultiValue pMultiValue = null;
			return ( ( pLineValue = this[ Key ] ) != null ) && ( ( pMultiValue = pLineValue.MultiValue ) != null ) ? pMultiValue.Size : 0;
		}

	};

}