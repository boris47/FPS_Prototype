

namespace Database {

	public class cValue {

		private	readonly	object			m_Value	= null;
		private readonly 	System.Type		m_Type	= null;


		///////////////////////////////////////////////////////////////////////////////

		public	cValue( object value )
		{
			m_Value = value;
			m_Type = value.GetType();
		}

		///////////////////////////////////////////////////////////////////////////////

		public	cValue( System.Type type )
		{
			m_Value = null;

			// In case of a value type use Activator.CreateInstance and it should work fine
			if ( type.IsValueType )
			{
				m_Value = System.Activator.CreateInstance( type );
			}
			m_Type = type;
		}


		///////////////////////////////////////////////////////////////////////////////

		public	bool	Is<T>()
		{
			return m_Type == default(T).GetType();
		}


		///////////////////////////////////////////////////////////////////////////////

		public T As<T>()
		{
			T result;
			try
			{
				result = (T) m_Value;
			}
			catch ( System.Exception )
			{
				result = default( T );
			}
			return result;
		}

		///////////////////////////////////////////////////////////////////////////////

		public	bool			ToBool()			{	return As<bool>();			}
		public	int				ToInteger()			{	return As<int>();			}
		public	float			ToFloat()			{	return As<float>();			}
		public override string	ToString()			{	return m_Value.ToString();	}
		public	object			ToSystemObject()	{	return m_Value;				}


		///////////////////////////////////////////////////////////////////////////////

		public static implicit operator bool	( cValue v )		{	return v.ToBool();		}
		public static implicit operator int		( cValue v )		{	return v.ToInteger();	}
		public static implicit operator float	( cValue v )		{	return v.ToFloat();		}
		public static implicit operator string	( cValue v )		{	return v.ToString();	}

		///////////////////////////////////////////////////////////////////////////////

		public static implicit operator cValue	( bool b )			{	return new cValue( b );	}
		public static implicit operator cValue	( int i )			{	return new cValue( i );	}
		public static implicit operator cValue	( float f )			{	return new cValue( f );	}
		public static implicit operator cValue	( string s )		{	return new cValue( s );	}

		///////////////////////////////////////////////////////////////////////////////

	}


}