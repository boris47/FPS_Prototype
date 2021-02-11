
namespace Database
{
	public class Value
	{
		private	readonly	object			m_Value	= null;
		private readonly 	System.Type		m_Type	= null;


		///////////////////////////////////////////////////////////////////////////////
		public	Value( object value )
		{
			m_Value = value;
			m_Type = value.GetType();
		}


		///////////////////////////////////////////////////////////////////////////////
		public	bool	Is<T>()
		{	
			System.Type requiredType = typeof(T);
			return m_Type == requiredType;
		}


		///////////////////////////////////////////////////////////////////////////////
		public T As<T>()
		{
			try
			{
				T result = (T) System.Convert.ChangeType(m_Value, typeof(T) );
				return result;
			}
			catch( System.Exception e )
			{
				UnityEngine.Debug.LogException( e );
				return default(T);
			}
		}


		///////////////////////////////////////////////////////////////////////////////
		public new System.Type GetType() => m_Type;


		///////////////////////////////////////////////////////////////////////////////
		private string InternalToString()
		{
			string result = null;
			if (m_Type == typeof(float) )
			{
				result = ((float)m_Value).ToString("0.0000000");
			}
			else
			{
				result = m_Value.ToString();
			}

			return result;
		}

		///////////////////////////////////////////////////////////////////////////////

		public bool				ToBool()			=> As<bool>();
		public int				ToInteger()			=> As<int>();
		public float			ToFloat()			=> As<float>();
		public override string	ToString()			=> InternalToString();
		public object			ToSystemObject()	=> m_Value;


		///////////////////////////////////////////////////////////////////////////////

		public static implicit operator bool	(Value v) => v.ToBool();
		public static implicit operator int		(Value v) => v.ToInteger();
		public static implicit operator float	(Value v) => v.ToFloat();
		public static implicit operator string	(Value v) => v.ToString();
		
		///////////////////////////////////////////////////////////////////////////////

		public static implicit operator Value	(bool   b)	=> new Value(b);
		public static implicit operator Value	(int    i)	=> new Value(i);
		public static implicit operator Value	(float  f)	=> new Value(f);
		public static implicit operator Value	(string s)	=> new Value(s);

		///////////////////////////////////////////////////////////////////////////////

		public static bool operator !(Value obj) => obj == null;
	}


}