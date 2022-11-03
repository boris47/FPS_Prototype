
using System.Linq;
using System.Collections.Generic;

public static class Extensions_CSharp
{
	/////////////////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////////////////
	#region C# OBJECT

	/////////////////////////////////////////////////////////////////////////////
	/// <summary> Check if Object is null internally </summary>
	public static bool IsNotNull(this object ThisObject) => !System.Object.ReferenceEquals(ThisObject, null);

	public static bool IsNull(this object ThisObject) => System.Object.ReferenceEquals(ThisObject, null);

	#endregion C# OBJECT


	/////////////////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////////////////
	#region C# STRING

	/////////////////////////////////////////////////////////////////////////////
	/// <summary> Return true for null/empty or |none| strings </summary>
	public static bool IsNone(this string ThisString)
	{
		return string.IsNullOrEmpty(ThisString) || ThisString.ToLower().Trim().Equals("none");
	}


	/////////////////////////////////////////////////////////////////////////////
	/// <summary> This method also trim inside the string </summary>
	public static string TrimInside(this string ThisString, params char[] InTrimChars)
	{
		System.Text.StringBuilder outString = new System.Text.StringBuilder();

		InTrimChars = InTrimChars ?? new char[0];
		if (InTrimChars.Length == 0)
		{
			InTrimChars.Append(' ');
		}

		foreach (char charr in ThisString)
		{
			if (!InTrimChars.Contains(charr))
			{
				outString.Append(charr);
			}
		}
		return outString.ToString();
	}


	/////////////////////////////////////////////////////////////////////////////
	/// <summary>  </summary>
	public static bool TryGetSubstring(this string ThisString, out string OutString, in char InStartChar, in char InEndChar, bool bTrimResult = false)
	{
		OutString = null;
		int startIndex = ThisString.IndexOf(InStartChar);
		if (startIndex >= 0)
		{
			int endIndex = ThisString.IndexOf(InEndChar, startIndex + 1);
			if (endIndex > 0)
			{
				OutString = ThisString.Substring(startIndex, endIndex);
				if (bTrimResult)
				{
					OutString = OutString.Trim();
				}
			}
		}
		return OutString.IsNotNull();
	}


	/////////////////////////////////////////////////////////////////////////////
	/// <summary> Return true if parse succeeded, otherwise false <br/>
	/// <see href="https://stackoverflow.com/questions/1082532/how-to-tryparse-for-enum-value"/>
	/// </summary>
	public static bool TryConvertToEnum<TEnum>(this string ThisString, out TEnum OutResult)
	{
		System.Type requestedType = typeof(TEnum);
		if (System.Enum.IsDefined(requestedType, ThisString))
		{
			OutResult = (TEnum)System.Enum.Parse(requestedType, ThisString);
			return true;
		}

		OutResult = default;
		return false;
	}

	#endregion // C# STRING


	/////////////////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////////////////
	#region FLOAT

	/////////////////////////////////////////////////////////////////////////////
	/// <summary> Is floatA equal to zero? Takes floating point inaccuracy into account, by using Epsilon. </summary>
	/// <param name="ThisFloat"></param>
	/// <returns></returns>
	public static bool IsEqualToZero(this float ThisFloat)
	{
		return System.Math.Abs(ThisFloat) < float.Epsilon;
	}

	/////////////////////////////////////////////////////////////////////////////
	/// <summary> Is floatA not equal to zero? Takes floating point inaccuracy into account, by using Epsilon. </summary>
	/// <param name="ThisFloat"></param>
	/// <returns></returns>
	public static bool NotEqualToZero(this float ThisFloat)
	{
		return System.Math.Abs(ThisFloat) > float.Epsilon;
	}

	/////////////////////////////////////////////////////////////////////////////
	/// <summary> Wraps a float between -180 and 180. </summary>
	/// <param name="ThisFloat">The float to wrap.</param>
	/// <returns>A value between -180 and 180.</returns>
	public static float Wrap180(this float ThisFloat)
	{
		ThisFloat %= 360.0f;
		if (ThisFloat < -180.0f)
		{
			ThisFloat += 360.0f;
		}
		else if (ThisFloat > 180.0f)
		{
			ThisFloat -= 360.0f;
		}
		return ThisFloat;
	}

	/////////////////////////////////////////////////////////////////////////////
	/// <summary> Wraps a float between 0 and 1. </summary>
	/// <param name="ThisFloat">The float to wrap.</param>
	/// <returns>A value between 0 and 1.</returns>
	public static float Wrap1(this float ThisFloat)
	{
		ThisFloat %= 1.0f;
		if (ThisFloat < 0.0f)
		{
			ThisFloat += 1.0f;
		}
		return ThisFloat;
	}

	/////////////////////////////////////////////////////////////////////////////
	/// <summary> Gets the fraction portion of a float. </summary>
	/// <param name="ThisFloat">The float.</param>
	/// <returns>The fraction portion of a float.</returns>
	public static float GetFraction(this float ThisFloat)
	{
		return ThisFloat - (float)System.Math.Floor(ThisFloat);
	}

	/////////////////////////////////////////////////////////////////////////////
	public static void Swap(this ref float ThisFloat, ref float InOtherFloat)
	{
		float tmp = ThisFloat;
		InOtherFloat = ThisFloat;
		ThisFloat = tmp;
	}

	#endregion


	/////////////////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////////////////
	#region C# ARRAY

	/////////////////////////////////////////////////////////////////////////////
	/// <summary> For a valid array return a random contained element </summary>
	public static T Random<T>(this System.Array ThisArray)
	{
		if (ThisArray == null || ThisArray.Length == 0)
			return default(T);

		return ThisArray.GetByIndex<T>(UnityEngine.Random.Range(0, ThisArray.Length));
	}

	/////////////////////////////////////////////////////////////////////////////
	public static T ByEnum<E, T>(this System.Array ThisArray, E InEnumValue) where E : System.Enum
	{
		int index = System.Array.IndexOf(System.Enum.GetValues(typeof(E)), InEnumValue);
		return ThisArray.GetByIndex<T>(index);
	}

	/////////////////////////////////////////////////////////////////////////////
	/// <summary> Tests if index is valid, i.e. greater than or equal to zero, and less than the number of elements in the array </summary>
	/// <param name="InIndices">Indexes to test</param>
	/// <returns>returns True if index is valid. False otherwise</returns>
	public static bool IsValidIndex(this System.Array ThisArray, params int[] InIndices)
	{
		int indicesLen = InIndices.Length;
		if (indicesLen == 0) return false;

		for (int currentDimension = 0; currentDimension < indicesLen; currentDimension++)
		{
			//          out of bound                         out of bound         the current index is greater or equal then than the array length
			if (InIndices[currentDimension] < 0 || currentDimension > ThisArray.Rank || InIndices[currentDimension] >= ThisArray.GetLength(currentDimension))
			{
				return false;
			}
		}
		return true;
	}


	/////////////////////////////////////////////////////////////////////////////
	/// <summary> Allow to easy get a value from an array checking given index, default value is supported </summary>
	public static T GetByIndex<T>(this System.Array ThisArray, int InIndex, T Default = default(T))
	{
		return IsValidIndex(ThisArray, InIndex) ? (T)ThisArray.GetValue(InIndex) : Default;
	}

	/////////////////////////////////////////////////////////////////////////////
	/// <summary> Allow to easy get a value from an array checking given index, default value is supported </summary>
	public static bool TryGetByIndex<T>(this System.Array ThisArray, uint InIndex, out T OutValue)
	{
		return TryGetByIndex(ThisArray, (int)InIndex, out OutValue);
	}
	/////////////////////////////////////////////////////////////////////////////
	/// <summary> Allow to easy get a value from an array checking given index, default value is supported </summary>
	public static bool TryGetByIndex<T>(this System.Array ThisArray, int InIndex, out T OutValue)
	{
		OutValue = default;
		if (IsValidIndex(ThisArray, InIndex))
		{
			OutValue = (T)ThisArray.GetValue(InIndex);
		}
		return !System.Collections.Generic.EqualityComparer<T>.Default.Equals(OutValue, default);
	}


	/////////////////////////////////////////////////////////////////////////////
	/// <summary> Allow to easy get a value from an array checking given index, default value is supported </summary>
	public static T GetByIndexWrap<T>(this System.Array ThisArray, int InIndex, T InDefault = default(T))
	{
		if (InIndex >= 0)
		{
			return IsValidIndex(ThisArray, InIndex) ? (T)ThisArray.GetValue(InIndex) : InDefault;
		}
		else // index < 0
		{
			int length = ThisArray.Length;
			while (InIndex >= length) InIndex -= length - 1;
			int selectedIndex = length - InIndex;
			return IsValidIndex(ThisArray, selectedIndex) ? (T)ThisArray.GetValue(selectedIndex) : InDefault;
		}
	}

	#endregion // C# ARRAY


	/////////////////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////////////////
	#region C# IEnumerable

	/////////////////////////////////////////////////////////////////////////////
	/// <summary> Select the element that better satisfy the selector with minimum value </summary>
	public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> ThisEnumerable, System.Func<TSource, TKey> InSelector, IComparer<TKey> InComparer = null)
	{
		if (ThisEnumerable == null || ThisEnumerable.Count() == 0 || InSelector == null) return default;

		using (IEnumerator<TSource> sourceIterator = ThisEnumerable.GetEnumerator())
		{
			sourceIterator.MoveNext();
			TSource min = sourceIterator.Current;
		//	if (sourceIterator.MoveNext())
			{
				InComparer ??= Comparer<TKey>.Default;
				TKey minKey = InSelector(min);
				while (sourceIterator.MoveNext())
				{
					TSource candidate = sourceIterator.Current;
					TKey candidateProjected = InSelector(candidate);
					if (InComparer.Compare(candidateProjected, minKey) < 0)
					{
						min = candidate;
						minKey = candidateProjected;
					}
				}
			}
			return min;
		}
	}


	/////////////////////////////////////////////////////////////////////////////
	/// <summary> Select the element that better satisfy the selector with maximum value </summary>
	public static TSource MaxBy<TSource, TKey>(this IEnumerable<TSource> ThisEnumerable, System.Func<TSource, TKey> InSelector, IComparer<TKey> InComparer = null)
	{
		if (ThisEnumerable == null || ThisEnumerable.Count() == 0 || InSelector == null) return default;

		using (IEnumerator<TSource> sourceIterator = ThisEnumerable.GetEnumerator())
		{
			sourceIterator.MoveNext();
			TSource min = sourceIterator.Current;
		//	if (sourceIterator.MoveNext())
			{
				InComparer ??= Comparer<TKey>.Default;
				TKey minKey = InSelector(min);
				while (sourceIterator.MoveNext())
				{
					TSource candidate = sourceIterator.Current;
					TKey candidateProjected = InSelector(candidate);
					if (InComparer.Compare(candidateProjected, minKey) > 0)
					{
						min = candidate;
						minKey = candidateProjected;
					}
				}
			}
			return min;
		}
	}


	/////////////////////////////////////////////////////////////////////////////
	/// <summary> For a valid list return a random contained element </summary>
	public static T Random<T>(this IEnumerable<T> ThisEnumerable)
	{
		T OutResult = default;
		if (ThisEnumerable.IsNotNull() && ThisEnumerable.Any())
		{
			OutResult = ThisEnumerable.ElementAt(UnityEngine.Random.Range(0, ThisEnumerable.Count() - 1));
		}
		return OutResult;
	}


	/////////////////////////////////////////////////////////////////////////////
	/// <summary> Tests if index is valid, i.e. greater than or equal to zero, and less than the number of elements in the collection </summary>
	/// <param name="InIndex">Index to test</param>
	/// <returns>Returns True if index is valid. False otherwise</returns>
	public static bool IsValidIndex<T>(this IEnumerable<T> ThisEnumerable, in uint InIndex) => IsValidIndex(ThisEnumerable, (int)InIndex);


	/////////////////////////////////////////////////////////////////////////////
	/// <summary> Tests if index is valid, i.e. greater than or equal to zero, and less than the number of elements in the collection </summary>
	/// <param name="index">Index to test</param>
	/// <returns>Returns True if index is valid. False otherwise</returns>
	public static bool IsValidIndex<T>(this IEnumerable<T> ThisEnumerable, in int InIndex) => InIndex >= 0 && InIndex < ThisEnumerable.Count();

	#endregion


	/////////////////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////////////////
	#region C# LIST

	/////////////////////////////////////////////////////////////////////////////
	/// <summary> Resize the list with the new size </summary>
	public static void Resize<T>(this List<T> ThisList, in uint InNewSize)
	{
		Resize(ThisList, (int)InNewSize);
	}
	
	/////////////////////////////////////////////////////////////////////////////
	/// <summary> Resize the list with the new size </summary>
	public static void Resize<T>(this List<T> ThisList, in int InNewSize)
	{
		int cur = ThisList.Count;
		if (InNewSize < cur)
		{
			ThisList.RemoveRange(InNewSize, cur - InNewSize);
		}
		else if (InNewSize > cur)
		{
			if (InNewSize > ThisList.Capacity)//this bit is purely an optimisation, to avoid multiple automatic capacity changes.
			{
				ThisList.Capacity = InNewSize;
			}

			ThisList.AddRange(Enumerable.Repeat(default(T), InNewSize - cur));
		}
	}

	/////////////////////////////////////////////////////////////////////////////
	public static bool TryGetByIndex<T, K>(this List<T> ThisList, in uint InIndex, out K OutValue) where K : T
	{
		return TryGetByIndex(ThisList, (int)InIndex, out OutValue);
	}

	/////////////////////////////////////////////////////////////////////////////
	/// <summary> Syntactic sugar for FindIndex and index check </summary>
	public static bool TryGetByIndex<T, K>(this List<T> ThisList, in int InIndex, out K OutValue) where K : T
	{
		if (ThisList.IsValidIndex(InIndex))
		{
			OutValue = (K)ThisList[InIndex];
			return true;
		}
		OutValue = default;
		return false;
	}

	/////////////////////////////////////////////////////////////////////////////
	/// <summary> Syntactic sugar for FindIndex and index check </summary>
	public static bool TryFind<T, K>(this List<T> ThisList, out K OutValue, out int OutIndex, in System.Predicate<T> InPredicate, in int InStartIndex = 0) where K : T
	{
		OutIndex = ThisList.FindIndex(InStartIndex, InPredicate);
		if (OutIndex > -1)
		{
			OutValue = (K)ThisList[OutIndex];
			return true;
		}
		OutValue = default;
		return false;
	}

	#endregion // C# LIST


	/////////////////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////////////////
	#region C# ILIST

	/////////////////////////////////////////////////////////////////////////////
	/// <summary> Ensure the the inserting element is only present one tine in the list </summary>
	/// <returns> Return true if item has been added otherwise false </returns>	
	public static bool AddUnique<T>(this IList<T> ThisList, T InElement, in System.Func<T, bool> InPredicate = null)
	{
		System.Func<T, bool> finalPredicate = InPredicate ?? delegate (T e) { return e.Equals(InElement); };
		bool bCanBeAdded = !ThisList.Any(finalPredicate);
		if (bCanBeAdded)
		{
			ThisList.Add(InElement);
		}
		return bCanBeAdded;
	}


	/////////////////////////////////////////////////////////////////////////////
	/// <summary> Add the value returning the same value </summary>
	public static K AddRef<T, K>(this IList<T> ThisIList, in K InValue) where K : T
	{
		ThisIList.Add(InValue);
		return InValue;
	}

	/////////////////////////////////////////////////////////////////////////////
	/// <summary> Add the value returning the same value </summary>
	public static K AddUniqueRef<T, K>(this IList<T> ThisIList, in K InValue) where K : T
	{
		ThisIList.AddUnique(InValue);
		return InValue;
	}

	/////////////////////////////////////////////////////////////////////////////
	/// <summary> Alias for collection[index] </summary>
	public static T At<T>(this IList<T> ThisIList, int InIndex) => ThisIList[InIndex];

	/////////////////////////////////////////////////////////////////////////////
	/// <summary> Alias for collection[(int)index] </summary>
	public static T At<T>(this IList<T> ThisIList, uint InIndex) => ThisIList[(int)InIndex];

	#endregion // C# ILIST


	/////////////////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////////////////
	#region C# DICTIONARY

	/////////////////////////////////////////////////////////////////////////////
	public static void AddRange<TKey, TValue>(this Dictionary<TKey, TValue> ThisDictionary, in Dictionary<TKey, TValue> InOtherDictionary, in bool bOverwrite = false)
	{
		if (InOtherDictionary != null)
		{
			foreach (KeyValuePair<TKey, TValue> pair in InOtherDictionary)
			{
				if (ThisDictionary.ContainsKey(pair.Key))
				{
					if (bOverwrite)
					{
						ThisDictionary.Add(pair.Key, pair.Value);
					}
				}
				else
				{
					ThisDictionary.Add(pair.Key, pair.Value);
				}
			}
		}
	}


	/////////////////////////////////////////////////////////////////////////////
	public static TValue FindOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> ThisDictionary, in TKey InKey, in System.Func<TValue> InDefaultCtor = null)
	{
		if (ThisDictionary.TryGetValue(InKey, out TValue value))
		{
			return value;
		}
		else
		{
			return ThisDictionary[InKey] = InDefaultCtor.IsNotNull() ? InDefaultCtor.Invoke() : default(TValue);
		}
	}


	/////////////////////////////////////////////////////////////////////////////
	public static void Deconstruct<T1, T2>(this KeyValuePair<T1, T2> ThisTuple, out T1 OutKey, out T2 OutValue)
	{
		OutKey = ThisTuple.Key;
		OutValue = ThisTuple.Value;
	}

	#endregion
}