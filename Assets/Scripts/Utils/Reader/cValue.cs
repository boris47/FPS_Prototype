

namespace CFG_Reader {

	public class cValue {

		public	object	Value
		{
			get;
			private set;
		}

		public	cValue( object value )
		{
			Value = value;
		}
	
		public	cValue( System.Type type )
		{
			if ( type == typeof ( bool ) )		Value = false;
			if ( type == typeof ( int ) )		Value = 0;
			if ( type == typeof ( float ) )		Value = 0.0f;
			if ( type == typeof ( string ) )	Value = "";
		}
	

		public T1 As<T1>()
		{
			return (T1) Value;
		}

		///////////////////////////////////////////////////////////////////////////////

		public bool ToBool()
		{
			bool bValue = false;
			try { bValue = ( bool ) Value; } catch {}
			return bValue;
		}

		public int ToInteger()
		{
			int iValue = 0;
			try { iValue = ( int ) Value; } catch {}
			return iValue;
		}

		public float ToFloat()
		{
			float fValue = 0.0f;
			try { fValue = ( float ) Value; } catch {}
			return fValue;
		}

		public override string ToString()
		{
			return Value.ToString();
		}
	
		public object ToSystemObject()
		{
			return Value;
		}


		///////////////////////////////////////////////////////////////////////////////

		public static implicit operator bool( cValue v )
		{
			return v.ToBool();
		}

		public static implicit operator int( cValue v )
		{
			return v.ToInteger();
		}
	
		public static implicit operator float( cValue v )
		{
			return v.ToFloat();
		}
	

		public static implicit operator string( cValue v )
		{
			return v.ToString();
		}

		///////////////////////////////////////////////////////////////////////////////

		public static implicit operator cValue( bool b )
		{
			return new cValue( b );
		}

		public static implicit operator cValue( int i )
		{
			return new cValue( i );
		}

		public static implicit operator cValue( float f )
		{
			return new cValue( f );
		}

		public static implicit operator cValue( string s )
		{
			return new cValue( s );
		}

		///////////////////////////////////////////////////////////////////////////////

	}


}