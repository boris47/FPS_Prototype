
namespace Database
{

	using System.Collections;
	using System.Collections.Generic;

	[System.Serializable]
	public class MultiValue : IEnumerable
	{
		private readonly List<Value> m_ValuesList = new List<Value>();

		//-------------------------------------------------------
		public Value[] ValueList => m_ValuesList.ToArray();

		//-------------------------------------------------------
		public int Size
		{
			get
			{
				return m_ValuesList.Count;
			}
		}

		//-------------------------------------------------------
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		//-------------------------------------------------------
		public List<Value>.Enumerator GetEnumerator()
		{
			return m_ValuesList.GetEnumerator();
		}


		public MultiValue(Value[] vValues = null, int capacity = 1)
		{
			m_ValuesList = vValues == null ? new List<Value>(capacity) : new List<Value>(vValues);
		}

		// Indexer behaviour
/*		public Value this[int Index]
		{
			get
			{
				if (m_ValuesList.IsValidIndex(Index))
				{
					return m_ValuesList[Index];
				}
				return null;
			}
		}
*/

		/////////////////////////////////////////////////////////
		public T Get<T>(int Index, T Default = default(T))
		{
			if (m_ValuesList.IsValidIndex(Index))
			{
				return m_ValuesList[Index].As<T>();
			}
			return Default;
		}

		/////////////////////////////////////////////////////////
		public bool TryGet<T>(int Index, out T ouput, T Default = default(T))
		{
			if (m_ValuesList.IsValidIndex(Index))
			{
				ouput = m_ValuesList[Index].As<T>();
				return true;
			}
			ouput = Default;
			return false;
		}

		/////////////////////////////////////////////////////////
		public void Add(Value pValue)
		{
			m_ValuesList.Add(pValue);
		}


		/////////////////////////////////////////////////////////
		public bool DeductType(ref System.Type typeFound)
		{
			bool result = true;
			{
				System.Type elementType = m_ValuesList[0].GetType();
				if (m_ValuesList.TrueForAll(v => v.GetType() == elementType))
				{
					if (elementType == typeof(int) || elementType == typeof(float))
					{
						if (m_ValuesList.Count == 2)
						{
							typeFound = typeof(UnityEngine.Vector2);
						}

						if (m_ValuesList.Count == 3)
						{
							typeFound = typeof(UnityEngine.Vector3);
						}

						if (m_ValuesList.Count == 4)
						{
							typeFound = typeof(UnityEngine.Vector4);
						}
					}
					else
					{
						typeFound = elementType;
					}
				}
				else
				{
					UnityEngine.Debug.Log("Multivalue containing different types");
					result = false;
				}
			}
			return result;
		}

	}

}