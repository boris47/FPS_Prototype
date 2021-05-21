
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public static class Extensions
{
	/////////////////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////////////////
	#region C# OBJECT

	/// <summary> Check if Object is null internally </summary>
	public static	bool	IsNotNull(this System.Object obj)
	{
		bool bIsNotNull =  obj != null;
		return bIsNotNull;
	}

	#endregion C# OBJECT


	/////////////////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////////////////
	#region C# STRING

	/// <summary> Return true for empty or 'none' strings </summary>
	public static	bool			IsNone( this string str )
	{
		return string.IsNullOrEmpty(str) || str.ToLower().Trim() == "none";
	}
	
	/// <summary> This method also trim inside the string </summary>
	public static	string			TrimInside( this string str, params char[] trimChars )
	{
		List<char> charsToSearch = new List<char>(1);
		if ( trimChars != null && trimChars.Length > 0 )
		{
			charsToSearch.AddRange(trimChars);
		}
		else
		{
			charsToSearch.Add(' ');
		}

		for (int i = str.Length - 1; i >= 0; i--)
		{
			if (charsToSearch.IndexOf(str[i]) != -1)
			{
				str = str.Remove( i, 1 );
			}
		}
		return str;
	}


	/// <summary> Return true if parse succeeded, otherwise false </summary>
	/// <see cref="https://stackoverflow.com/questions/1082532/how-to-tryparse-for-enum-value"/>
	public	static	bool			TryConvertToEnum<TEnum>(this string str, ref TEnum result )
	{
		System.Type requestedType = typeof(TEnum);
		if ( System.Enum.IsDefined( requestedType, str) == false )
		{
			return false;
		}

		result = (TEnum)System.Enum.Parse( requestedType, str );
		return true;
	}

	#endregion // C# STRING


	/////////////////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////////////////
	#region C# ARRAY

	/// <summary> For a valid array return a random contained element </summary>
	public static	T				Random<T>( this System.Array a )
	{
		if ( a == null || a.Length == 0 )
			return default( T );

		return a.GetByIndex<T>( UnityEngine.Random.Range( 0, a.Length ) ); 
	}

	/// <summary> Tests if index is valid, i.e. greater than or equal to zero, and less than the number of elements in the array </summary>
	/// <param name="index">Index to test</param>
	/// <returns>returns True if index is valid. False otherwise</returns>
	public	static bool				IsValidIndex( this System.Array array, int index )
	{
		return index >= 0 && index < array.Length;
	}

	/// <summary> Allow to easly get a value from an array checking given index, default value is supported </summary>
	public	static	T				GetByIndex<T>( this System.Array array, int index, T Default = default(T) )
	{
		return IsValidIndex(array, index) ? (T)array.GetValue(index) : Default;
	}

	/// <summary> Allow to easly get a value from an array checking given index, default value is supported </summary>
	public	static	T				GetByIndexWrap<T>( this System.Array array, int index, T Default = default(T) )
	{
		if (index >= 0)
		{
			return IsValidIndex(array, index) ? (T)array.GetValue(index) : Default;
		}
		else // index < 0
		{
			int length = array.Length;
			while(index>=length) index -= length - 1;
			int selectedIndex = length - index;
			return IsValidIndex(array, selectedIndex) ? (T)array.GetValue(selectedIndex) : Default;
		}
	}




	/// <summary> Search along a monodimensional or a bidimensional array for an element that satisfies the predicate </summary>
	public static	bool			FindByPredicate<T>( this T[,] array, out T value, out Vector2 location, System.Predicate<T> predicate )
	{
		CustomAssertions.IsNotNull(array);
		CustomAssertions.IsNotNull(predicate);

		value = default;
		location = default;

		bool bIsFound = false;
		int dimensions = array.Rank;
		if (dimensions == 1)
		{
			int length = array.Length;
			for (int i = 0; i < length; i++)
			{
				T currentValue = (T)array.GetValue(i);
				if (predicate(currentValue))
				{
					value = currentValue;
					location = new Vector2(i, 0);
					bIsFound = true;
				}
			}
		}
		else if (dimensions == 2)
		{
			for (int dimension = 0; dimension < dimensions && !bIsFound; dimension++)
			{
				int upper = array.GetUpperBound(dimension);
				int lower = array.GetLowerBound(dimension);

				for (int index = lower; index <= upper && !bIsFound; index++)
				{
					T currentValue = array[dimension, index];
					if (predicate(currentValue))
					{
						value = currentValue;
						location = new Vector2(dimension, index);
						bIsFound = true;
					}
				}
			}
		}

		return bIsFound;
	}

	#endregion // C# ARRAY


	/// <summary> Convert the source into an hashset </summary>
	public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source, IEqualityComparer<T> comparer = null)
	{
		return new HashSet<T>(source, comparer);
	}

	public static Stack<T> ToStack<T>(this IEnumerable<T> source)
	{
		return new Stack<T>(source);
	}

	public static Queue<T> ToQueue<T>(this IEnumerable<T> source)
	{
		return new Queue<T>(source);
	}

	public static List<T> ToList<T>(this IEnumerable<T> source)
	{
		return new List<T>(source);
	}



	/// <summary> Select the element that better satisfy the selector with minimum value </summary>
	public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source, System.Func<TSource, TKey> selector)
	{
		if (source == null || source.Count() == 0 || selector == null) return default;

		using (IEnumerator<TSource> sourceIterator = source.GetEnumerator())
		{
			sourceIterator.MoveNext();
			Comparer<TKey> comparer = Comparer<TKey>.Default;
			TSource min = sourceIterator.Current;
			TKey minKey = selector(min);
			while (sourceIterator.MoveNext())
			{
				TSource candidate = sourceIterator.Current;
				TKey candidateProjected = selector(candidate);
				if (comparer.Compare(candidateProjected, minKey) < 0)
				{
					min = candidate;
					minKey = candidateProjected;
				}
			}
			return min;
		}
	}


	/// <summary> Select the element that better satisfy the selector with maximum value </summary>
	public static TSource MaxBy<TSource, TKey>(this IEnumerable<TSource> source, System.Func<TSource, TKey> selector)
	{
		if (source == null || source.Count() == 0 || selector == null) return default;

		using (IEnumerator<TSource> sourceIterator = source.GetEnumerator())
		{
			Comparer<TKey> comparer = Comparer<TKey>.Default;
			TSource max = sourceIterator.Current;
			TKey maxKey = selector(max);
			while (sourceIterator.MoveNext())
			{
				TSource candidate = sourceIterator.Current;
				TKey candidateProjected = selector(candidate);
				if (comparer.Compare(candidateProjected, maxKey) > 0)
				{
					max = candidate;
					maxKey = candidateProjected;
				}
			}
			return max;
		}
	}


	/// <summary> For a valid list return a random contained element </summary>
	public static T Random<T>(this IEnumerable<T> source)
	{
		if (source == null || source.Count() == 0)
			return default;

		return source.ElementAt(UnityEngine.Random.Range(0, source.Count() - 1));
	}


	/// <summary> Tests if index is valid, i.e. greater than or equal to zero, and less than the number of elements in the collection </summary>
	/// <param name="index">Index to test</param>
	/// <returns>returns True if index is valid. False otherwise</returns>
	public static bool IsValidIndex<T>(this IEnumerable<T> source, in int index)
	{
		return index >= 0 && index < source.Count();
	}


	/////////////////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////////////////
	#region C# LIST

	/// <summary> Resize the list with the new size </summary>
	public static	void			Resize<T>(this List<T> list, in uint newSize)
	{
		if (newSize > list.Capacity)
		{
			list.Capacity = (int)newSize;
		}
	}


	/// <summary> Ensure the the inserting element is only present one tine in the list </summary>
	public static	bool		AddUnique<T>( this List<T> list, T element, in System.Predicate<T> predicate = null )
	{
		System.Predicate<T> finalPredicate = predicate ?? delegate(T e) { return e.Equals( element ); };
		bool bAlreadyExists = list.FindIndex( finalPredicate ) > -1;
		if ( bAlreadyExists == false )
		{
			list.Add( element );
		}
		return bAlreadyExists;
	}

	#endregion // C# LIST


	/////////////////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////////////////
	#region C# DICTIONARY

	public static	void		AddRange<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, in Dictionary<TKey, TValue> other, in bool bOverwrite = false)
	{
		if (other != null)
		{
			foreach(KeyValuePair<TKey, TValue> pair in other)
			{
				if (dictionary.ContainsKey(pair.Key))
				{
					if (bOverwrite)
					{
						dictionary.Add(pair.Key, pair.Value);
					}
				}
				else
				{
					dictionary.Add(pair.Key, pair.Value);
				}
			}
		}
	}

	#endregion
}


