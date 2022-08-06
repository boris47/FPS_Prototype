
namespace DatabaseCore
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;

	[System.Serializable]
	public class Section : IEnumerable
	{
		// INTERNAL VARS
		[SerializeField]
		private		string				name			= null;

		[SerializeField]
		private		string				m_Context		= string.Empty;

		[SerializeField]
		private		string				m_FilePath		= string.Empty;

		[SerializeField]
		private		List<LineValue>		m_Linevalues	= new List<LineValue>();

		[SerializeField]
		private		List<string>		m_Mothers		= new List<string>();

		// Iteration
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		public List<LineValue>.Enumerator GetEnumerator() => m_Linevalues.GetEnumerator();

		//-------------------------------------------------------
		public string		Context							=> m_Context;
		public string		FilePath						=> m_FilePath;

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
		public LineValue	GetLineValue(string key)		=> m_Linevalues.Find(lv => lv.HasKey(key));
		//-------------------------------------------------------
		public string		GetSectionName()				=> name;


		/////////////////////////////////////////////////////////
		public Section(in string InSectionName, in string InFilePath, in string InContext = "None")
		{
			name = InSectionName;
			m_FilePath = InFilePath;
			m_Context = InContext;
		}

		public Section(in string InSectionName, in string InContext = "None")
		{
			name = InSectionName;
			m_FilePath = null;
			m_Context = InContext;
		}

		/////////////////////////////////////////////////////////
		public void Destroy() => m_Linevalues.ForEach(lv => lv.Destroy());

		/////////////////////////////////////////////////////////
		public bool Add(in LineValue InLineValue)
		{
			string keyToFind = InLineValue.Key;
			int index = m_Linevalues.FindIndex(s => s.Key == keyToFind);
			// Confirmed new linevalue
			if (index == -1)
			{
				m_Linevalues.Add(InLineValue);
			}
			else // overwrite of existing linevalue
			{
				m_Linevalues[index] = new LineValue(InLineValue);
			}
			return index > -1;
		}

		/////////////////////////////////////////////////////////
		public bool Remove(in string InLineValueID)
		{
			bool bOutResult = false;
			for (int i = m_Linevalues.Count - 1; i >= 0 && !bOutResult; i--)
			{
				LineValue lineValue = m_Linevalues[i];
				if (lineValue.Key == InLineValueID)
				{
					m_Linevalues[i].Destroy();
					m_Linevalues.RemoveAt(i);
					bOutResult = true;
				}
			}
			return bOutResult;
		}

		//////////////////////////////////////////////////////////////////////////
		public string GetRawValue(in string Key, in string Default = "") => TryGetLineValue(Key, out LineValue pLineValue) ? pLineValue.RawValue : Default;
		

		//////////////////////////////////////////////////////////////////////////
		//------- OPERATORS
		public static bool operator !(in Section InSection) => InSection == null;
		public static bool operator false(in Section InSection) => InSection == null;
		public static bool operator true(in Section InSection) => InSection.IsNotNull();
		public static Section operator +(in Section SectionA, in Section SectionB)
		{
			foreach (LineValue lineValue in SectionB)
			{
				if (!SectionA.HasKey(lineValue.Key))
				{
					SectionA.Add(lineValue);
				}
			}
			SectionA.m_Mothers.Add(SectionB.name);
			return SectionA;
		}

		#region TRY GET/AS

		/////////////////////////////////////////////////////////
		public bool TryGetLineValue(in string InKey, out LineValue OutLineValue)
		{
			for (int i = 0; i < m_Linevalues.Count; i++)
			{
				LineValue lineValue = m_Linevalues[i];
				if (lineValue.Key == InKey)
				{
					OutLineValue = lineValue;
					return true;
				}
			}

			OutLineValue = null;
			return false;
		}

		/////////////////////////////////////////////////////////
		public bool TryGetLineValueByIndex(in int InIndex, out LineValue OutLineValue)
		{
			if (m_Linevalues.IsValidIndex(InIndex))
			{
				OutLineValue = m_Linevalues[InIndex];
				return true;
			}

			OutLineValue = null;
			return false;
		}

		//////////////////////////////////////////////////////////////////////////
		public bool TryAs<T>(in string InKey, out T OutResult)
		{
			if (TryGetLineValue(InKey, out LineValue lineValue))
			{
				if (lineValue.Type == ELineValueType.SINGLE)
				{
					OutResult = lineValue.Value.As<T>();
					return true;
				}
			}
			OutResult = default(T);
			return false;
		}

		//////////////////////////////////////////////////////////////////////////
		public bool TryAs<T>(in string InKey, out T[] OutResult)
		{
			if (TryGetLineValue(InKey, out LineValue lineValue))
			{
				if (lineValue.Type == ELineValueType.MULTI)
				{
					Value[] values = lineValue.MultiValue.ValueList;
					System.Type requestedType = typeof(T);
					if (System.Array.TrueForAll(values, value => value.GetType().IsEquivalentTo(requestedType)))
					{
						OutResult = System.Array.ConvertAll(values, s => s.As<T>());
						return true;
					}
				}
			}
			OutResult = null;
			return false;
		}

		//////////////////////////////////////////////////////////////////////////
		public bool TryAsBool(in string InKey, out bool OutValue, in bool InDefault = default)
		{
			if (TryGetLineValue(InKey, out LineValue lineValue))
			{
				if (lineValue.Type == ELineValueType.SINGLE)
				{
					OutValue = lineValue.Value.As<bool>();
					return true;
				}
			}
			OutValue = InDefault;
			return false;
		}

		//////////////////////////////////////////////////////////////////////////
		public bool TryAsInt(in string InKey, out int OutValue, in int InDefault = default)
		{
			if (TryGetLineValue(InKey, out LineValue lineValue))
			{
				if (lineValue.Type == ELineValueType.SINGLE)
				{
					OutValue = lineValue.Value.As<int>();
					return true;
				}
			}
			OutValue = InDefault;
			return false;
		}

		//////////////////////////////////////////////////////////////////////////
		public bool TryAsUInt(in string InKey, out uint OutValue, in uint InDefault = default)
		{
			if (TryGetLineValue(InKey, out LineValue lineValue))
			{
				if (lineValue.Type == ELineValueType.SINGLE)
				{
					OutValue = lineValue.Value.As<uint>();
					return true;
				}
			}
			OutValue = InDefault;
			return false;
		}

		//////////////////////////////////////////////////////////////////////////
		public bool TryAsFloat(in string Key, out float Out, in float Default = default)
		{
			if (TryGetLineValue(Key, out LineValue lineValue))
			{
				if (lineValue.Type == ELineValueType.SINGLE)
				{
					Out = lineValue.Value.As<float>();
					return true;
				}
			}
			Out = Default;
			return false;
		}

		//////////////////////////////////////////////////////////////////////////
		public bool TryAsString(in string InKey, out string OutValue, in string InDefault = "")
		{
			if (TryGetLineValue(InKey, out LineValue lineValue))
			{
				if (lineValue.Type == ELineValueType.SINGLE)
				{
					OutValue = lineValue.Value.ToString();
					return true;
				}
			}
			OutValue = InDefault;
			return false;
		}

		//////////////////////////////////////////////////////////////////////////
		public bool TryAsMultiValue(in string InKey, in int InIndex, out Value OutValue)
		{
			if (TryGetLineValue(InKey, out LineValue lineValue))
			{
				if (lineValue.GetAsMulti(out MultiValue multiValue))
				{
					return multiValue.TryGet(InIndex - 1, out OutValue);
				}
			}
			OutValue = default;
			return false;
		}

		//////////////////////////////////////////////////////////////////////////
		public bool TryAsVec2(in string InKey, out Vector2 OutValue, in Vector2 InDefault = default)
		{
			if (TryGetLineValue(InKey, out LineValue lineValue))
			{
				if (lineValue.GetAsMulti(out MultiValue multiValue))
				{
					if (multiValue.TryGet(0, out float x) && multiValue.TryGet(1, out float y))
					{
						OutValue = new Vector2(x, y);
						return true;
					}
				}
			}
			OutValue = InDefault;
			return false;
		}

		//////////////////////////////////////////////////////////////////////////
		public bool TryAsVec3(in string InKey, out Vector3 OutValue, in Vector3 InDefault = default)
		{
			if (TryGetLineValue(InKey, out LineValue lineValue))
			{
				if (lineValue.GetAsMulti(out MultiValue multiValue))
				{
					if (multiValue.TryGet(0, out float x) && multiValue.TryGet(1, out float y) && multiValue.TryGet(2, out float z))
					{
						OutValue = new Vector3(x, y, z);
						return true;
					}
				}
			}
			OutValue = InDefault;
			return false;
		}

		//////////////////////////////////////////////////////////////////////////
		public bool TryAsVec4(in string InKey, out Vector4 OutValue, in Vector4 InDefault = default)
		{
			if (TryGetLineValue(InKey, out LineValue lineValue))
			{
				if (lineValue.GetAsMulti(out MultiValue multiValue))
				{
					if (multiValue.TryGet(0, out float x) && multiValue.TryGet(1, out float y) && multiValue.TryGet(2, out float z) && multiValue.TryGet(3, out float w))
					{
						OutValue = new Vector4(x, y, z, w);
						return true;
					}
				}
			}
			OutValue = InDefault;
			return false;
		}

		//////////////////////////////////////////////////////////////////////////
		public bool TryAsColor(in string InKey, out Color OutValue, in Color InDefault = default)
		{
			if (TryAsVec4(InKey, out Vector4 vec4, Vector4.zero))
			{
				float r = vec4[0], g = vec4[1], b = vec4[2], a = vec4[3];
				OutValue = new Color(r: r, g: g, b: b, a: a);
			}
			OutValue = InDefault;
			return false;
		}

		//////////////////////////////////////////////////////////////////////////
		public bool TryAsEnum<T>(in string InKey, out T OutValue, T InDefault = default) where T : struct
		{
			if (TryGetLineValue(InKey, out LineValue lineValue))
			{
				if (lineValue.Type == ELineValueType.SINGLE)
				{
					if (global::Utils.Converters.StringToEnum(lineValue.Value.As<string>(), out T value))
					{
						OutValue = value;
						return true;
					}
				}
			}
			OutValue = InDefault;
			return false;
		}

		#endregion TRY GET/AS

		#region AS

		//////////////////////////////////////////////////////////////////////////
		public T As<T>(in string InKey)
		{
			if (TryGetLineValue(InKey, out LineValue pLineValue))
			{
				if (global::Utils.CustomAssertions.IsTrue(pLineValue.Type == ELineValueType.SINGLE, $"Database::Section::{nameof(As)}: Line value for section {name} at key {InKey} is not of single type"))
				{
					return pLineValue.Value.As<T>();
				}
			}
			return default;
		}

		//////////////////////////////////////////////////////////////////////////
		public bool AsBool(in string InKey, in bool InDefault = default)
		{
			if (TryGetLineValue(InKey, out LineValue lineValue))
			{
				if (global::Utils.CustomAssertions.IsTrue(lineValue.Type == ELineValueType.SINGLE, $"Database::Section::{nameof(AsBool)}: Line value for section {name} at key {InKey} is not of single type"))
				{
					return lineValue.Value;
				}
			}
			return InDefault;
		}

		//////////////////////////////////////////////////////////////////////////
		public int AsInt(in string InKey, in int InDefault = default)
		{
			if (TryGetLineValue(InKey, out LineValue lineValue))
			{
				if (global::Utils.CustomAssertions.IsTrue(lineValue.Type == ELineValueType.SINGLE, $"Database::Section::{nameof(AsInt)}: Line value for section {name} at key {InKey} is not of single type"))
				{
					return lineValue.Value;
				}
			}
			return InDefault;
		}

		//////////////////////////////////////////////////////////////////////////
		public uint AsUInt(in string InKey, in uint InDefault = default)
		{
			if (TryGetLineValue(InKey, out LineValue lineValue))
			{
				if (global::Utils.CustomAssertions.IsTrue(lineValue.Type == ELineValueType.SINGLE, $"Database::Section::{nameof(AsUInt)}: Line value for section {name} at key {InKey} is not of single type"))
				{
					return lineValue.Value;
				}
			}
			return InDefault;
		}

		//////////////////////////////////////////////////////////////////////////
		public float AsFloat(in string InKey, in float InDefault = default)
		{
			if (TryGetLineValue(InKey, out LineValue lineValue))
			{
				if (global::Utils.CustomAssertions.IsTrue(lineValue.Type == ELineValueType.SINGLE, $"Database::Section::{nameof(AsFloat)}: Line value for section {name} at key {InKey} is not of single type"))
				{
					return lineValue.Value;
				}
			}
			return InDefault;
		}

		//////////////////////////////////////////////////////////////////////////
		public string AsString(in string InKey, in string InDefault = "")
		{
			if (TryGetLineValue(InKey, out LineValue lineValue))
			{
				if (global::Utils.CustomAssertions.IsTrue(lineValue.Type == ELineValueType.SINGLE, $"Database::Section::{nameof(AsString)}: Line value for section {name} at key {InKey} is not of single type"))
				{
					return lineValue.Value;
				}
			}
			return InDefault;
		}

		//////////////////////////////////////////////////////////////////////////
		public T OfMultiValue<T>(in string InKey, in int Index, T InDefault = default)
		{
			if (Index > 0 && TryGetLineValue(InKey, out LineValue lineValue) && lineValue.GetAsMulti(out MultiValue multiValue))
			{
				return multiValue.Get(Index - 1, InDefault);
			}
			return InDefault;
		}

		//////////////////////////////////////////////////////////////////////////
		public void AsMultiValue<T1, T2>(in string InKey, in int InIndex1, in int InIndex2, out T1 OutValue1, out T2 OutValue2)
		{
			OutValue1 = default(T1); OutValue2 = default(T2);
			if (TryGetLineValue(InKey, out LineValue lineValue) && lineValue.GetAsMulti(out MultiValue multiValue))
			{
				OutValue1 = multiValue.Get<T1>(InIndex1 - 1);
				OutValue2 = multiValue.Get<T2>(InIndex2 - 1);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		public void AsMultiValue<T1, T2, T3>(in string InKey, in int InIndex1, in int InIndex2, in int InIndex3, out T1 OutValue1, out T2 OutValue2, out T3 OutValue3)
		{
			OutValue1 = default(T1); OutValue2 = default(T2); OutValue3 = default(T3);
			if (TryGetLineValue(InKey, out LineValue lineValue) && lineValue.GetAsMulti(out MultiValue multiValue))
			{
				OutValue1 = multiValue.Get<T1>(InIndex1 - 1);
				OutValue2 = multiValue.Get<T2>(InIndex2 - 1);
				OutValue3 = multiValue.Get<T3>(InIndex3 - 1);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		public void AsMultiValue<T1, T2, T3, T4>(in string InKey, in int InIndex1, in int InIndex2, in int InIndex3, in int InIndex5, out T1 OutValue1, out T2 OutValue2, out T3 OutValue3, out T4 OutValue4)
		{
			OutValue1 = default(T1); OutValue2 = default(T2); OutValue3 = default(T3); OutValue4 = default(T4);
			if (TryGetLineValue(InKey, out LineValue lineValue) && lineValue.GetAsMulti(out MultiValue multiValue))
			{
				OutValue1 = multiValue.Get<T1>(InIndex1 - 1);
				OutValue2 = multiValue.Get<T2>(InIndex2 - 1);
				OutValue3 = multiValue.Get<T3>(InIndex3 - 1);
				OutValue4 = multiValue.Get<T4>(InIndex5 - 1);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		public int GetMultiSize(in string InKey)
		{
			if (TryGetLineValue(InKey, out LineValue lineValue) && lineValue.GetAsMulti(out MultiValue multiValue))
			{
				return multiValue.Size;
			}
			return default;
		}


		//////////////////////////////////////////////////////////////////////////
		public bool TryGetMultiAsArray<T>(in string InKey, out T[] OutArray)
		{
			if (TryGetLineValue(InKey, out LineValue lineValue))
			{
				if (lineValue.GetAsSingle(out Value value))
				{
					OutArray = new T[1] { value.As<T>() };
					return true;
				}

				if (lineValue.GetAsMulti(out MultiValue multiValue))
				{
					OutArray = System.Array.ConvertAll(multiValue.ValueList, (Value v) => v.As<T>());
					return true;
				}
			}

			OutArray = default;
			return false;
		}

		//////////////////////////////////////////////////////////////////////////
		public Vector2 AsVec2(in string InKey, in Vector2 InDefault = default)
		{
			if (TryGetLineValue(InKey, out LineValue lineValue) && lineValue.GetAsMulti(out MultiValue multiValue))
			{
				if (multiValue.TryGet(0, out float x) && multiValue.TryGet(1, out float y))
				{
					return new Vector2(x, y);
				}

			}
			return InDefault;
		}

		//////////////////////////////////////////////////////////////////////////
		public Vector3 AsVec3(in string InKey, in Vector3 InDefault = default)
		{
			if (TryGetLineValue(InKey, out LineValue lineValue) && lineValue.GetAsMulti(out MultiValue multiValue))
			{
				if (multiValue.TryGet(0, out float x) && multiValue.TryGet(1, out float y) && multiValue.TryGet(2, out float z))
				{
					return new Vector3(x, y, z);
				}
			}
			return InDefault;
		}

		//////////////////////////////////////////////////////////////////////////
		public Vector4 AsVec4(in string InKey, in Vector4 InDefault = default)
		{
			if (TryGetLineValue(InKey, out LineValue lineValue) && lineValue.GetAsMulti(out MultiValue multiValue))
			{
				if (multiValue.TryGet(0, out float x) && multiValue.TryGet(1, out float y) && multiValue.TryGet(2, out float z) && multiValue.TryGet(3, out float w))
				{
					return new Vector4(x, y, z, w);
				}

			}
			return InDefault;
		}

		//////////////////////////////////////////////////////////////////////////
		public Color AsColor(in string InKey, in Color? InDefault)
		{
			if (TryAsVec4(InKey, out Vector4 vec4, Vector4.zero))
			{
				return vec4;
			}
			return InDefault.GetValueOrDefault();
		}

		//////////////////////////////////////////////////////////////////////////
		public T AsEnum<T>(in string InKey, in T? InDefault) where T : struct
		{
			if (TryGetLineValue(InKey, out LineValue pLineValue))
			{
				if (global::Utils.CustomAssertions.IsTrue(pLineValue.Type == ELineValueType.SINGLE, $"Database::Section::AsEnum: Line value for section {name} at key {InKey} is not of single type"))
				{
					if (global::Utils.Converters.StringToEnum(pLineValue.Value.As<string>(), out T value))
					{
						return value;
					}
				}
			}
			return InDefault.GetValueOrDefault();
		}

		#endregion AS

		#region SET

		//////////////////////////////////////////////////////////////////////////
		public void SetValue(in string InKey, in Value InValue)
		{
			LineValue lineValue = GetLineValue(InKey) ?? new LineValue(InKey, ELineValueType.SINGLE);
			if (global::Utils.CustomAssertions.IsTrue(lineValue.Type == ELineValueType.SINGLE))
			{
				lineValue.Clear();
				lineValue.Set(InValue);
				m_Linevalues.Add(lineValue);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		public void SetMultiValue(in string InKey, in Value[] InValues)
		{
			LineValue lineValue = GetLineValue(InKey) ?? new LineValue(InKey, ELineValueType.MULTI);
			if (global::Utils.CustomAssertions.IsTrue(lineValue.Type == ELineValueType.MULTI))
			{
				lineValue.Clear();
				MultiValue multivalue = new MultiValue(InValues);
				lineValue.Set(multivalue);
				m_Linevalues.Add(lineValue);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		public void Set<T>(in string InKey, in T InValue) => SetValue(InKey, new Value(InValue));
		
		//////////////////////////////////////////////////////////////////////////
		public void SetVec2(in string InKey, in Vector2 InValue)
		{
			LineValue lineValue = GetLineValue(InKey) ?? new LineValue(InKey, ELineValueType.MULTI);
			if (global::Utils.CustomAssertions.IsTrue(lineValue.Type == ELineValueType.MULTI))
			{
				lineValue.Clear();
				Value[] vValues = new Value[2] { new Value(InValue.x), new Value(InValue.y) };
				MultiValue multivalue = new MultiValue(vValues);
				lineValue.Set(multivalue);
				m_Linevalues.Add(lineValue);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		public void SetVec3(in string InKey, in Vector3 InValue)
		{
			LineValue lineValue = GetLineValue(InKey) ?? new LineValue(InKey, ELineValueType.MULTI);
			if (global::Utils.CustomAssertions.IsTrue(lineValue.Type == ELineValueType.MULTI))
			{
				lineValue.Clear();
				Value[] vValues = new Value[] { new Value(InValue.x), new Value(InValue.y), new Value(InValue.z) };
				MultiValue multivalue = new MultiValue(vValues);
				lineValue.Set(multivalue);
				m_Linevalues.Add(lineValue);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		public void SetVec4(in string InKey, in Vector4 InValue)
		{
			LineValue lineValue = GetLineValue(InKey) ?? new LineValue(InKey, ELineValueType.MULTI);
			if (global::Utils.CustomAssertions.IsTrue(lineValue.Type == ELineValueType.MULTI))
			{
				lineValue.Clear();
				Value[] vValues = new Value[] { new Value(InValue.x), new Value(InValue.y), new Value(InValue.z), new Value(InValue.w) };
				MultiValue multivalue = new MultiValue(vValues);
				lineValue.Set(multivalue);
				m_Linevalues.Add(lineValue);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		public void SetColor(in string InKey, in Color InValue)
		{
			LineValue lineValue = GetLineValue(InKey) ?? new LineValue(InKey, ELineValueType.MULTI);
			if (global::Utils.CustomAssertions.IsTrue(lineValue.Type == ELineValueType.MULTI))
			{
				lineValue.Clear();
				Value[] vValues = new Value[] { new Value(InValue.r), new Value(InValue.g), new Value(InValue.b), new Value(InValue.a) };
				MultiValue multivalue = new MultiValue(vValues);
				lineValue.Set(multivalue);
				m_Linevalues.Add(lineValue);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		public void SetEnum<T>(in string InKey, in T InValue) where T : struct
		{
			if (global::Utils.CustomAssertions.IsTrue(typeof(T).IsEnum))
			{
				LineValue lineValue = GetLineValue(InKey) ?? new LineValue(InKey, ELineValueType.SINGLE);
				if (global::Utils.CustomAssertions.IsTrue(lineValue.Type == ELineValueType.SINGLE))
				{
					lineValue.Clear();
					lineValue.Set(InValue.ToString());
					m_Linevalues.Add(lineValue);
				}
			}
		}

		#endregion SET

		#region UTILS

		/////////////////////////////////////////////////////////
		public void PrintSection()
		{
			string[] buffer = Stringify().Split('\n');

			// Section name
			Debug.Log($"---|Section START {buffer[0]}");
			{
				for (int i = 1, Count = buffer.Length; i < Count; i++)
				{
					Debug.Log(buffer[i]);
				}
			}
			Debug.Log("---|Section END");

			/*
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
					if (LineValue.Value.StoredValue == null)
					{
						Debug.Log($"{result} {LineValue.RawValue}");
					}
					else
					{
						Debug.Log($"\t{result} {LineValue.Value.StoredValue}, {LineValue.Value.GetType().Name}");
					}
				}
			}
			Debug.Log("---|Section END");
			*/
		}

		/////////////////////////////////////////////////////////
		/// <summary> Return this section stringifyied </summary>
		public string Stringify()
		{
			List<string> lines = new List<string>();

			// SECTION DEFINITION
			System.Text.StringBuilder sectionDefinition = new System.Text.StringBuilder($"[{name}]");
			{
				// Concatenate mothers names
				if (m_Mothers.Count > 0)
				{
					sectionDefinition.Append($":{string.Join(",", m_Mothers)}");
				}
			}
			lines.Add(sectionDefinition.ToString());

			// Write key value pairs
			foreach (LineValue lineValue in m_Linevalues)
			{
				string value = lineValue.Type == ELineValueType.MULTI ? string.Join(", ", lineValue.MultiValue) : lineValue.Value.ToString();
				lines.Add($"{lineValue.Key}={value}");
			}

			return string.Join("\n", lines.ToArray());
		}

		#endregion UTILS
	};
}
