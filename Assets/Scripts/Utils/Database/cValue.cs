
namespace Database
{
	public class Value
	{
		private	readonly	object			m_Value	= null;
		private readonly 	System.Type		m_Type	= null;


		///////////////////////////////////////////////////////////////////////////////
		public	Value( object value )
		{
			this.m_Value = value;
			this.m_Type = value.GetType();
		}


		///////////////////////////////////////////////////////////////////////////////
		public	bool	Is<T>()
		{	
			System.Type requiredType = typeof(T);
			return this.m_Type == requiredType;
		}


		///////////////////////////////////////////////////////////////////////////////
		public T As<T>()
		{
			try
			{
				T result = (T) System.Convert.ChangeType(this.m_Value, typeof(T) );
				return result;
			}
			catch( System.Exception e )
			{
				UnityEngine.Debug.LogException( e );
				return default(T);
			}
		}


		///////////////////////////////////////////////////////////////////////////////
		public	new 	System.Type	GetType()
		{
			return this.m_Type;
		}


		///////////////////////////////////////////////////////////////////////////////
		private string InternalToString()
		{
			string result = null;
			if (this.m_Type == typeof(float) )
			{
				result = ((float)this.m_Value).ToString("0.0000000");
			}
			else
			{
				result = this.m_Value.ToString();
			}

			return result;
		}

		///////////////////////////////////////////////////////////////////////////////

		public	bool			ToBool()			{	return this.As<bool>();			}
		public	int				ToInteger()			{	return this.As<int>();			}
		public	float			ToFloat()			{	return this.As<float>();			}
		public override string	ToString()			{	return this.InternalToString();	}
		public	object			ToSystemObject()	{	return this.m_Value;				}


		///////////////////////////////////////////////////////////////////////////////

		public static implicit operator bool	( Value v )		{	return v.ToBool();		}
		public static implicit operator int		( Value v )		{	return v.ToInteger();	}
		public static implicit operator float	( Value v )		{	return v.ToFloat();		}
		public static implicit operator string	( Value v )		{	return v.ToString();	}

		///////////////////////////////////////////////////////////////////////////////

		public static implicit operator Value	( bool b )			{	return new Value( b );	}
		public static implicit operator Value	( int i )			{	return new Value( i );	}
		public static implicit operator Value	( float f )			{	return new Value( f );	}
		public static implicit operator Value	( string s )		{	return new Value( s );	}

		///////////////////////////////////////////////////////////////////////////////

	}


}