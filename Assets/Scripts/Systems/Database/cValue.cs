
namespace DatabaseCore
{
	public class Value
	{
		public	readonly	object			StoredValue		= null;
		private readonly 	System.Type		m_Type			= null;


		///////////////////////////////////////////////////////////////////////////////
		public Value(in object InValue)
		{
			StoredValue = InValue;
			m_Type = InValue?.GetType() ?? null;
		}

		///////////////////////////////////////////////////////////////////////////////
		public bool Is<T>() => Is(typeof(T));
		public bool Is(in System.Type InType)
		{
			return m_Type == InType;
		}

		///////////////////////////////////////////////////////////////////////////////
		public T As<T>()
		{
			T OutResult = default(T);
			try
			{
				OutResult = (T)System.Convert.ChangeType(StoredValue, typeof(T));
			}
			catch (System.Exception e)
			{
				UnityEngine.Debug.LogException(e);
			}
			return OutResult;
		}

		///////////////////////////////////////////////////////////////////////////////
		public new System.Type GetType() => m_Type;

		///////////////////////////////////////////////////////////////////////////////
		private string InternalToString()
		{
			string OutResult = null;
			if (m_Type == typeof(float))
			{
				OutResult = ((float)StoredValue).ToString("0.0000000");
			}
			else
			{
				OutResult = StoredValue.ToString();
			}

			return OutResult;
		}
		
		///////////////////////////////////////////////////////////////////////////////

		public bool				ToBool()			=> As<bool>();
		public int				ToInteger()			=> As<int>();
		public uint				ToUInteger()		=> As<uint>();
		public float			ToFloat()			=> As<float>();
		public double			ToDouble()			=> As<double>();
		public override string	ToString()			=> InternalToString();

		///////////////////////////////////////////////////////////////////////////////

		public static implicit operator bool	(in Value v) => v.ToBool();
		public static implicit operator int		(in Value v) => v.ToInteger();
		public static implicit operator uint	(in Value v) => v.ToUInteger();
		public static implicit operator float	(in Value v) => v.ToFloat();
		public static implicit operator double	(in Value v) => v.ToDouble();
		public static implicit operator string	(in Value v) => v.ToString();

		///////////////////////////////////////////////////////////////////////////////

		public static implicit operator Value	(in bool	b)	=> new Value(b);
		public static implicit operator Value	(in int		i)	=> new Value(i);
		public static implicit operator Value	(in uint	i)	=> new Value(i);
		public static implicit operator Value	(in float	f)	=> new Value(f);
		public static implicit operator Value	(in double	d)	=> new Value(d);
		public static implicit operator Value	(in string	s)	=> new Value(s);
		
		///////////////////////////////////////////////////////////////////////////////

		public static bool operator !(Value obj) => System.Collections.Generic.EqualityComparer<Value>.Default.Equals(obj, default);
	}


}
