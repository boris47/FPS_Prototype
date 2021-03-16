
namespace Database
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;

	// PUBLIC INTERFACE
	public interface ISection
	{
		void					Destroy							();
		int						Lines							();
		string					GetSectionName					();
		bool					Add								( LineValue LineValue );
		bool					Remove							( string LineValueID );
		bool					HasKey							( string Key );
		bool					IsChildOf						( in Section MotherSection );
		bool					IsChildOf						( string MotherName );

		// Section.Get
		string					GetRawValue						( in string Key, in string Default = "" );
		T						As				<T>				( in string Key );
		bool					AsBool							( in string Key, in bool Default = false );
		int						AsInt							( in string Key, in int Default = 0 );
		uint					AsUInt							( in string Key, in uint Default = 0u );
		float					AsFloat							( in string Key, in float Default = 0.0f );
		string					AsString						( in string Key, in string Default = "" );
		T						OfMultiValue	<T>				( in string Key, in int Index, in T Default );
		void					AsMultiValue	<T1,T2>			( in string Key, in int Idx1, in int Idx2, out T1 t1, out T2 t2 );
		void					AsMultiValue	<T1,T2,T3>		( in string Key, in int Idx1, in int Idx2, in int Idx3, out T1 t1, out T2 t2, out T3 t3 );
		void					AsMultiValue	<T1,T2,T3,T4>	( in string Key, in int Idx1, in int Idx2, in int Idx3, in int Idx4, out T1 t1, out T2 t2, out T3 t3, out T4 t4 );
		int						GetMultiSize					( in string Key );
		bool					TryGetMultiAsArray<T>			( in string Key, out T[] array );
		Vector2					AsVec2							( in string Key, in Vector2 Default = default );
		Vector3					AsVec3							( in string Key, in Vector3 Default = default );
		Vector4					AsVec4							( in string Key, in Vector4 Default = default );
		Color					AsColor							( in string Key, in Color? Default );

		// Section.GetAs
		bool					TryAs<T>						( in string Key, out T Out );
		bool					TryAs<T>						( in string Key, out T[] Out );
		bool					TryAsBool						( in string Key, out bool	Out, in bool	Default = false );
		bool					TryAsInt						( in string Key, out int	Out, in int		Default = 0	);
		bool					TryAsFloat						( in string Key, out float  Out, in float	Default = 0.0f );
		bool					TryAsString						( in string Key, out string Out, in string	Default = "" );
		bool					TryAsMultiValue					( in string Key, in int Index, out Value Out );
		bool					TryAsVec2						( in string Key, out Vector2 Out, in Vector2 Default = default );
		bool					TryAsVec3						( in string Key, out Vector3 Out, in Vector3 Default = default );
		bool					TryAsVec4						( in string Key, out Vector4 Out, in Vector4 Default = default );
		bool					TryAsColor						( in string Key, out Color Out, in Color Default );

		// Section.Set
		void					SetValue						( in string Key, in Value Value );
		void					SetMultiValue					( in string Key, in Value[] vValues );
		void					Set				<T>				( in string Key, in T Value );
		void					SetVec2							( in string Key, in Vector2 Vec );
		void					SetVec3							( in string Key, in Vector3 Vec );
		void					SetVec4							( in string Key, in Vector4 Vec );
		void					SetColor						( in string Key, in Color color );


		void					PrintSection					();
	}
	
	[System.Serializable]
	public partial class Section : ISection, IEnumerable
	{
		// INTERNAL VARS
		[SerializeField]
		private		string				name			= null;

		[SerializeField]
		private		string				m_Context		= string.Empty;

		[SerializeField]
		private		List<LineValue>		m_Linevalues	= new List<LineValue>();

		[SerializeField]
		private		List<string>		m_Mothers		= new List<string>();

		[SerializeField]
		public		bool				m_IsOK			{ get; private set; }

		// Iteration
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		public List<LineValue>.Enumerator GetEnumerator() => m_Linevalues.GetEnumerator();

		// Indexer
//		public LineValue this[int i] => (i > -1 && i < m_Linevalues.Count) ? m_Linevalues[i] : null;

		//-------------------------------------------------------
		public string		Context							=> m_Context;
		//-------------------------------------------------------
		public bool			IsChildOf(in Section mother)	=> IsChildOf(mother.GetSectionName());
		//-------------------------------------------------------
		public bool			IsChildOf(string MotherName)	=> m_Mothers.FindIndex(m => m == MotherName) > -1;
		//-------------------------------------------------------
		public bool			HasKey(string Key)				=> TryGetLineValue(Key, out LineValue bump);
		//-------------------------------------------------------
		public int			Lines()							=> m_Linevalues.Count;
		//-------------------------------------------------------
		public string[]		GetKeys()						=> m_Linevalues.ConvertAll(lv => lv.Key).ToArray();
		//-------------------------------------------------------
		public LineValue	GetLineValue(string key)		=> m_Linevalues.Find(lv => lv.IsKey(key));
		//-------------------------------------------------------
		public string		GetSectionName()				=> name;


		/////////////////////////////////////////////////////////
		public Section( string sectionName, string context )
		{
			name = sectionName;
			m_Context = context;
			m_IsOK = true;
		}

		/////////////////////////////////////////////////////////
		public	void					Destroy()
		{
			m_Linevalues.ForEach(lv => lv.Destroy());
		}

		/////////////////////////////////////////////////////////
		public	bool					Add( LineValue LineValue )
		{
			int index = m_Linevalues.FindIndex(s => s.Key == LineValue.Key);
			// Confirmed new linevalue
			if (index == -1)
			{
				m_Linevalues.Add(LineValue);
			}
			else // overwrite of existing linevalue
			{
				m_Linevalues[index] = new LineValue(LineValue);
			}
			return index > -1;
		}

		/////////////////////////////////////////////////////////
		public	bool					Remove( string lineValueID )
		{
			int index = m_Linevalues.FindIndex(s => s.Key == lineValueID);
			if (index > -1)
			{
				m_Linevalues[index].Destroy();
				m_Linevalues.RemoveAt(index);
				return true;
			}
			return false;
		}

		/////////////////////////////////////////////////////////
		public	bool					TryGetLineValue( string key, out LineValue lineValue )
		{
			lineValue = null;
			int index = m_Linevalues.FindIndex(lv => lv.IsKey(key));
			if (index > -1)
			{
				lineValue = m_Linevalues[index];
				return true;
			}
			return false;
		}

		/////////////////////////////////////////////////////////
		public	bool					TryGetLineValue( int index, out LineValue lineValue )
		{
			lineValue = null;
			if (m_Linevalues.IsValidIndex(index))
			{
				lineValue = m_Linevalues[index];
				return true;
			}
			return false;
		}

		/////////////////////////////////////////////////////////
		public	void					PrintSection()
		{
			Debug.Log($"---|Section START {name}");
			foreach (LineValue LineValue in m_Linevalues)
			{
				string result = LineValue.Key;
				if (LineValue.Type == ELineValueType.MULTI)
				{
					MultiValue multi = LineValue.MultiValue;
					for (int i = 0; i < multi.Size; i++)
					{
						result += $" {multi.Get<Value>(i)}";
					}
					Debug.Log($"\t{result}");
				}
				else
				{
					if (LineValue.Value.ToSystemObject() == null)
					{
						Debug.Log($"{result} {LineValue.RawValue}");
					}
					else
					{
						Debug.Log($"\t{result} {LineValue.Value.ToSystemObject()}, {LineValue.Value.ToSystemObject().GetType()}");
					}
				}
			}
			Debug.Log("---|Section END");
		}
		
		/////////////////////////////////////////////////////////
		public	void					SaveToBuffer( ref string buffer )
		{
			List<string> lines = new List<string>();

			// SECTION DEFINITION
			string sectionDefinition = $"[{name}]";
			{
				// Concatenate mothers names
				if (m_Mothers.Count > 0)
				{
					sectionDefinition += $":{m_Mothers[0]}";
					for (int i = 1; i < m_Mothers.Count; i++, sectionDefinition += ',')
					{
						string motherName = m_Mothers[i];
						sectionDefinition += motherName;
					}
				}
			}
			lines.Add(sectionDefinition);

			// Write key value pairs
			foreach (LineValue LineValue in m_Linevalues)
			{
				string key = LineValue.Key, valueStringified = "";
				if (LineValue.Type == ELineValueType.MULTI)
				{
					MultiValue multi = LineValue.MultiValue;
					valueStringified += multi.Get<string>(0);
					for (int i = 1; i < multi.Size; i++, valueStringified += ",")
					{
						string subValue = multi.Get<string>(i);
						valueStringified += subValue;
					}
				}
				else
				{
					Value value = LineValue.Value;
					valueStringified += value.ToString();
				}
				string keyValuePair = $"{key}={valueStringified}";
				lines.Add(keyValuePair);
			}

			string internalBuffer = string.Join("\n", lines.ToArray());
			buffer += internalBuffer;
		}

		//------- OPERATORS
		public static bool operator !(Section obj) => obj == null;
		public static bool operator false(Section obj) => obj == null;
		public static bool operator true(Section obj) => obj.IsNotNull();
		public static Section operator +(Section SecA, Section SecB)
		{
			if (SecB.m_IsOK)
			{
				foreach (LineValue lineValue in SecB)
				{
					if (!SecA.HasKey(lineValue.Key))
					{
						SecA.Add(lineValue);
					}
				}
				SecA.m_Mothers.Add(SecB.name);
			}
			return SecA;
		}

	};

	public partial class Section
	{
		//////////////////////////////////////////////////////////////////////////
		public	bool					TryAs<T>( in string Key, out T Out )
		{
			if (TryGetLineValue( Key, out LineValue pLineValue ) )
			{
				if ( pLineValue.Type == ELineValueType.SINGLE )
				{
					Out = pLineValue.Value.As<T>();
					return true;
				}
			}
			Out = default(T);
			return false;
		}

		//////////////////////////////////////////////////////////////////////////
		public	bool					TryAs<T>( in string Key, out T[] Out )
		{
			if (TryGetLineValue( Key, out LineValue pLineValue ) )
			{
				if ( pLineValue.Type == ELineValueType.MULTI )
				{
					Value[] values = pLineValue.MultiValue.ValueList;
					System.Type requestedType = typeof(T);
					if (System.Array.TrueForAll(values, (Value value) => value.GetType().IsEquivalentTo(requestedType)))
					{
						Out = System.Array.ConvertAll(values, (s) => s.As<T>());
						return true;
					}
				}
			}
			Out = null;
			return false;
		}


		//////////////////////////////////////////////////////////////////////////
		// bAsBool
		public	bool					TryAsBool( in string Key, out bool Out, in bool Default = default )
		{
			if (TryGetLineValue( Key, out LineValue pLineValue ) )
			{
				if ( pLineValue.Type == ELineValueType.SINGLE )
				{
					Out = pLineValue.Value.As<bool>();
					return true;
				}
			}
			Out = Default;
			return false;
		}


		//////////////////////////////////////////////////////////////////////////
		// bAsInt
		public	bool					TryAsInt( in string Key, out int Out, in int Default = default )
		{
			if (TryGetLineValue( Key, out LineValue pLineValue ) )
			{
				if ( pLineValue.Type == ELineValueType.SINGLE )
				{
					Out = pLineValue.Value.As<int>();
					return true;
				}
			}
			Out = Default;
			return false;
		}


		//////////////////////////////////////////////////////////////////////////
		// bAsInt
		public	bool					TryAsUInt( in string Key, out uint Out, in uint Default = default )
		{
			if (TryGetLineValue( Key, out LineValue pLineValue ) )
			{
				if ( pLineValue.Type == ELineValueType.SINGLE )
				{
					Out = pLineValue.Value.As<uint>();
					return true;
				}
			}
			Out = Default;
			return false;
		}


		//////////////////////////////////////////////////////////////////////////
		// bAsFloat
		public	bool					TryAsFloat( in string Key, out float Out, in float Default = default )
		{
			if (TryGetLineValue( Key, out LineValue pLineValue ) )
			{
				if ( pLineValue.Type == ELineValueType.SINGLE )
				{
					Out = pLineValue.Value.As<float>();
					return true;
				}
			}
			Out = Default;
			return false;
		}


		//////////////////////////////////////////////////////////////////////////
		// bAsString
		public	bool					TryAsString( in string Key, out string Out, in string Default = "" )
		{
			if (TryGetLineValue( Key, out LineValue pLineValue ) )
			{
				if ( pLineValue.Type == ELineValueType.SINGLE )
				{
					Out = pLineValue.Value.ToString();
					return true;
				}
			}
			Out = Default;
			return false;
		}


		//////////////////////////////////////////////////////////////////////////
		// bAsMultiValue
		public	bool					TryAsMultiValue( in string Key, in int Index, out Value Out )
		{
			if (TryGetLineValue( Key, out LineValue pLineValue ) )
			{
				if (pLineValue.GetAsMulti(out MultiValue pMultiValue) && pMultiValue.TryGet(Index - 1, out Out))
				{
					return true;
				}
			}
			Out = default;
			return false;
		}


		//////////////////////////////////////////////////////////////////////////
		// bAsVec2
		public	bool					TryAsVec2( in string Key, out Vector2 Out, in Vector2 Default = default )
		{
			if (TryGetLineValue( Key, out LineValue pLineValue ) )
			{
				if ( pLineValue.GetAsMulti(out MultiValue pMultiValue ) )
				{
					if (pMultiValue.TryGet(0, out float x) && pMultiValue.TryGet(1, out float y))
					{
						Out = new Vector2( x, y );
						return true;
					}
				}
			}
			Out = Default;
			return false;
		}


		//////////////////////////////////////////////////////////////////////////
		// bAsVec3
		public	bool					TryAsVec3( in string Key, out Vector3 Out, in Vector3 Default = default )
		{
			if (TryGetLineValue( Key, out LineValue pLineValue ) )
			{
				if ( pLineValue.GetAsMulti(out MultiValue pMultiValue ) )
				{
					if (pMultiValue.TryGet(0, out float x) && pMultiValue.TryGet(1, out float y) && pMultiValue.TryGet(2, out float z))
					{
						Out = new Vector3( x, y, z );
						return true;
					}
				}
			}
			Out = Default;
			return false;
		}


		//////////////////////////////////////////////////////////////////////////
		// bAsVec4
		public	bool					TryAsVec4( in string Key, out Vector4 Out, in Vector4 Default = default )
		{
			if (TryGetLineValue( Key, out LineValue pLineValue ) )
			{
				if ( pLineValue.GetAsMulti(out MultiValue pMultiValue ) )
				{
					if (pMultiValue.TryGet(0, out float x) && pMultiValue.TryGet(1, out float y) && pMultiValue.TryGet(2, out float z) && pMultiValue.TryGet(3, out float w))
					{
						Out = new Vector4( x, y, z, w );
						return true;
					}
				}
			}
			Out = Default;
			return false;
		}


		//////////////////////////////////////////////////////////////////////////
		// bAsColor
		public	bool					TryAsColor( in string Key, out Color Out, in Color Default = default )
		{
			if (TryAsVec4(Key, out Vector4 vec4, Vector4.zero))
			{
				float r = vec4[0], g = vec4[1], b = vec4[2], a = vec4[3];
				Out = new Color( r:r, g:g, b:b, a:a );
			}
			Out = Default;
			return false;
		}
	
	};

	public partial class Section
	{
		//////////////////////////////////////////////////////////////////////////
		// GetRawValue
		public	string					GetRawValue( in string Key, in string Default = "" )
		{
			return (TryGetLineValue( Key, out LineValue pLineValue ) ) ? pLineValue.RawValue : Default;
		}


		//////////////////////////////////////////////////////////////////////////
		// As<T>
		public	T						As<T>( in string Key )
		{
			if (TryGetLineValue(Key, out LineValue pLineValue))
			{
				CustomAssertions.IsTrue
				(
					pLineValue.Type == ELineValueType.SINGLE,
					$"Database::Section::As: Line value for section {name} at key {Key} is not of single type"
				);

				return pLineValue.Value.As<T>();
			}
			return default(T);
		}


		//////////////////////////////////////////////////////////////////////////
		// AsBool
		public	bool					AsBool( in string Key, in bool Default = default )
		{
			if (TryGetLineValue(Key, out LineValue pLineValue))
			{
				CustomAssertions.IsTrue
				(
					pLineValue.Type == ELineValueType.SINGLE,
					$"Database::Section::AsBool: Line value for section {name} at key {Key} is not of single type"
				);

				return pLineValue.Value.As<bool>();
			}
			return Default;
		}


		//////////////////////////////////////////////////////////////////////////
		// AsInt
		public	int						AsInt( in string Key, in int Default = default )
		{
			if (TryGetLineValue(Key, out LineValue pLineValue))
			{
				CustomAssertions.IsTrue
				(
					pLineValue.Type == ELineValueType.SINGLE,
					$"Database::Section::AsInt: Line value for section {name} at key {Key} is not of single type"
				);

				return pLineValue.Value.As<int>();
			}
			return Default;
		}

		//////////////////////////////////////////////////////////////////////////
		// AsInt ( UInt )
		public	uint					AsUInt( in string Key, in uint Default = default )
		{
			if (TryGetLineValue(Key, out LineValue pLineValue))
			{
				CustomAssertions.IsTrue
				(
					pLineValue.Type == ELineValueType.SINGLE,
					$"Database::Section::AsUInt: Line value for section {name} at key {Key} is not of single type"
				);

				return pLineValue.Value.As<uint>();
			}
			return Default;
		}


		//////////////////////////////////////////////////////////////////////////
		// AsFloat
		public	float					AsFloat( in string Key, in float Default = default )
		{
			if (TryGetLineValue(Key, out LineValue pLineValue))
			{
				CustomAssertions.IsTrue
				(
					pLineValue.Type == ELineValueType.SINGLE,
					$"Database::Section::AsFloat: Line value for section {name} at key {Key} is not of single type"
				);

				return pLineValue.Value.ToFloat();
			}
			return Default;
		}


		//////////////////////////////////////////////////////////////////////////
		// AsString
		public	string					AsString( in string Key, in string Default = "" )
		{
			if (TryGetLineValue(Key, out LineValue pLineValue))
			{
				CustomAssertions.IsTrue
				(
					pLineValue.Type == ELineValueType.SINGLE,
					$"Database::Section::AsString: Line value for section {name} at key {Key} is not of single type"
				);

				return pLineValue.Value.As<string>();
			}
			return Default;
		}


		//////////////////////////////////////////////////////////////////////////
		// AsMultiValue
		public	T						OfMultiValue<T>( in string Key, in int Index, in T Default = default )
		{
			if (Index > 0 && TryGetLineValue(Key, out LineValue pLineValue))
			{
				if (pLineValue.GetAsMulti(out MultiValue pMultiValue))
				{
					return pMultiValue.Get(Index - 1, Default);
				}
			}
			return Default;
		}


		//////////////////////////////////////////////////////////////////////////
		// AsMultiValue<T1,T2>
		public	void					AsMultiValue<T1,T2>( in string Key, in int Idx1, in int Idx2, out T1 t1, out T2 t2 )
		{
			t1 = default(T1); t2 = default(T2);
			if (TryGetLineValue(Key, out LineValue pLineValue))
			{
				if (pLineValue.GetAsMulti(out MultiValue pMultiValue))
				{
					t1 = pMultiValue.Get<T1>(Idx1 - 1);
					t2 = pMultiValue.Get<T2>(Idx2 - 1);
				}
			}
		}


		//////////////////////////////////////////////////////////////////////////
		// AsMultiValue<T1,T2,T3>
		public	void					AsMultiValue<T1,T2,T3>( in string Key, in int Idx1, in int Idx2, in int Idx3, out T1 t1, out T2 t2, out T3 t3 )
		{
			t1 = default(T1); t2 = default(T2); t3 = default(T3);
			if (TryGetLineValue(Key, out LineValue pLineValue))
			{
				if (pLineValue.GetAsMulti(out MultiValue pMultiValue))
				{
					t1 = pMultiValue.Get<T1>(Idx1 - 1);
					t2 = pMultiValue.Get<T2>(Idx2 - 1);
					t3 = pMultiValue.Get<T3>(Idx3 - 1);
				}
			}
		}


		//////////////////////////////////////////////////////////////////////////
		// AsMultiValue<T1,T2,T3,T4>
		public	void					AsMultiValue<T1,T2,T3,T4>( in string Key, in int Idx1, in int Idx2, in int Idx3, in int Idx4, out T1 t1, out T2 t2, out T3 t3, out T4 t4 )
		{
			t1 = default(T1); t2 = default(T2); t3 = default(T3); t4 = default(T4);
			if (TryGetLineValue(Key, out LineValue pLineValue))
			{
				if (pLineValue.GetAsMulti(out MultiValue pMultiValue))
				{
					t1 = pMultiValue.Get<T1>(Idx1 - 1);
					t2 = pMultiValue.Get<T2>(Idx2 - 1);
					t3 = pMultiValue.Get<T3>(Idx3 - 1);
					t4 = pMultiValue.Get<T4>(Idx4 - 1);
				}
			}
		}


		//////////////////////////////////////////////////////////////////////////
		// GetMultiSize
		public	int						GetMultiSize( in string Key )
		{
			if (TryGetLineValue(Key, out LineValue pLineValue))
			{
				if (pLineValue.GetAsMulti(out MultiValue pMultiValue))
				{
					return pMultiValue.Size;
				}
			}
			return 0;
		}


		//////////////////////////////////////////////////////////////////////////
		// bGetMultiAsArray
		public	bool					TryGetMultiAsArray<T>( in string Key, out T[] array )
		{
			if (TryGetLineValue(Key, out LineValue pLineValue))
			{
				if (pLineValue.GetAsSingle(out Value value))
				{
					array = new T[1] { value.As<T>() };
					return true;
				}

				if (pLineValue.GetAsMulti(out MultiValue multiValue))
				{
					array = System.Array.ConvertAll(multiValue.ValueList, (Value v) => v.As<T>());
					return true;
				}
			}
			array = null;
			return false;
		}


		//////////////////////////////////////////////////////////////////////////
		// bAsVec2
		public	Vector2		AsVec2( in string Key, in Vector2 Default = default )
		{
			if (TryGetLineValue(Key, out LineValue pLineValue))
			{
				if (pLineValue.GetAsMulti(out MultiValue pMultiValue))
				{
					if (pMultiValue.TryGet(0, out float x) && pMultiValue.TryGet(1, out float y))
					{
						return new Vector2(x, y);
					}
				}
			}
			return Default;
		}


		//////////////////////////////////////////////////////////////////////////
		// bAsVec3
		public	Vector3		AsVec3( in string Key, in Vector3 Default = default )
		{
			if (TryGetLineValue(Key, out LineValue pLineValue))
			{
				if (pLineValue.GetAsMulti(out MultiValue pMultiValue))
				{
					if (pMultiValue.TryGet(0, out float x) && pMultiValue.TryGet(1, out float y) && pMultiValue.TryGet(2, out float z))
					{
						return new Vector3(x, y, z);
					}
				}
			}
			return Default;
		}


		//////////////////////////////////////////////////////////////////////////
		// bAsVec4
		public	Vector4		AsVec4( in string Key, in Vector4 Default = default )
		{
			if (TryGetLineValue(Key, out LineValue pLineValue))
			{
				if (pLineValue.GetAsMulti(out MultiValue pMultiValue))
				{
					if (pMultiValue.TryGet(0, out float x) && pMultiValue.TryGet(1, out float y) && pMultiValue.TryGet(2, out float z) && pMultiValue.TryGet(3, out float w))
					{
						return new Vector4(x, y, z, w);
					}
				}
			}
			return Default;
		}


		//////////////////////////////////////////////////////////////////////////
		// bAsColor
		public	Color		AsColor( in string Key, in Color? Default )
		{
			if (TryAsVec4(Key, out Vector4 vec4, Vector4.zero))
			{
				return vec4;
			}
			return Default.GetValueOrDefault();
		}

	};

	// Section SET
	public partial class Section
	{
	
		//////////////////////////////////////////////////////////////////////////
		// SetValue
		public	void SetValue( in string Key, in Value Value )
		{
			LineValue pLineValue = GetLineValue(Key) ?? new LineValue(Key, ELineValueType.MULTI);

			pLineValue.Clear();
			pLineValue.Set(Value);
			m_Linevalues.Add(pLineValue);
		}


		//////////////////////////////////////////////////////////////////////////
		// SetMultiValue
		public	void SetMultiValue( in string Key, in Value[] vValues )
		{
			LineValue pLineValue = GetLineValue(Key) ?? new LineValue(Key, ELineValueType.MULTI);

			pLineValue.Clear();
			MultiValue multivalue = new MultiValue(vValues);
			pLineValue.Set(multivalue);
			m_Linevalues.Add(pLineValue);
		}


		//////////////////////////////////////////////////////////////////////////
		// Set<T>
		public	void Set<T>( in string Key, in T Value )
		{
			SetValue( Key, new Value( Value ) );
		}


		//////////////////////////////////////////////////////////////////////////
		// SetVec2
		public	void SetVec2( in string Key, in Vector2 Vec )
		{
			LineValue pLineValue = GetLineValue(Key) ?? new LineValue(Key, ELineValueType.MULTI);

			pLineValue.Clear();
			Value[] vValues = new Value[2] { new Value(Vec.x), new Value(Vec.y) };
			MultiValue multivalue = new MultiValue(vValues);
			pLineValue.Set(multivalue);
			m_Linevalues.Add(pLineValue);
		}


		//////////////////////////////////////////////////////////////////////////
		// SetVec3
		public	void SetVec3( in string Key, in Vector3 Vec )
		{
			LineValue pLineValue = GetLineValue(Key) ?? new LineValue(Key, ELineValueType.MULTI);

			pLineValue.Clear();
			Value[] vValues = new Value[] { new Value(Vec.x), new Value(Vec.y), new Value(Vec.z) };
			MultiValue multivalue = new MultiValue(vValues);
			pLineValue.Set(multivalue);
			m_Linevalues.Add(pLineValue);
		}


		//////////////////////////////////////////////////////////////////////////
		// SetVec4
		public	void SetVec4( in string Key, in Vector4 Vec )
		{
			LineValue pLineValue = GetLineValue(Key) ?? new LineValue(Key, ELineValueType.MULTI);

			pLineValue.Clear();
			Value[] vValues = new Value[] { new Value(Vec.x), new Value(Vec.y), new Value(Vec.z), new Value(Vec.w) };
			MultiValue multivalue = new MultiValue(vValues);
			pLineValue.Set(multivalue);
			m_Linevalues.Add(pLineValue);
		}


		//////////////////////////////////////////////////////////////////////////
		// SetVec4
		public	void SetColor( in string Key, in Color color )
		{
			LineValue pLineValue = GetLineValue(Key) ?? new LineValue(Key, ELineValueType.MULTI);

			pLineValue.Clear();
			Value[] vValues = new Value[] { new Value(color.r), new Value(color.g), new Value(color.b), new Value(color.a) };
			MultiValue multivalue = new MultiValue(vValues);
			pLineValue.Set(multivalue);
			m_Linevalues.Add(pLineValue);
		}
	};
}