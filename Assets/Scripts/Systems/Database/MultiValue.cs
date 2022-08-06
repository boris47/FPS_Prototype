
namespace DatabaseCore
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
		public int Size => m_ValuesList.Count;

		//-------------------------------------------------------
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		//-------------------------------------------------------
		public List<Value>.Enumerator GetEnumerator() => m_ValuesList.GetEnumerator();


		/////////////////////////////////////////////////////////
		public MultiValue(in Value[] InValues = null, in int InCapacity = 1)
		{
			m_ValuesList = InValues == null ? new List<Value>(InCapacity) : new List<Value>(InValues);
		}

		/////////////////////////////////////////////////////////
		public T Get<T>(in int InIndex, in T InDefault = default(T))
		{
			if (m_ValuesList.IsValidIndex(InIndex))
			{
				return m_ValuesList[InIndex].As<T>();
			}
			return InDefault;
		}

		/////////////////////////////////////////////////////////
		public bool TryGet<T>(in int InIndex, out T OutResult)
		{
			if (m_ValuesList.IsValidIndex(InIndex))
			{
				OutResult = m_ValuesList[InIndex].As<T>();
				return true;
			}

			OutResult = default(T);
			return false;
		}

		/////////////////////////////////////////////////////////
		public void Add(in Value InValue) => m_ValuesList.Add(InValue);

		/////////////////////////////////////////////////////////
		public bool TryDeductType(out System.Type OutType)
		{
			OutType = null;
			System.Type elementType = m_ValuesList[0].GetType();
			if (m_ValuesList.TrueForAll(v => v.GetType() == elementType)) // All same type
			{
				if (elementType == typeof(int) || elementType == typeof(uint) || elementType == typeof(float) || elementType == typeof(double))
				{
					if (m_ValuesList.Count == 2)
					{
						OutType = typeof(UnityEngine.Vector2);
					}

					if (m_ValuesList.Count == 3)
					{
						OutType = typeof(UnityEngine.Vector3);
					}

					if (m_ValuesList.Count == 4)
					{
						OutType = typeof(UnityEngine.Vector4);
					}
				}
				else
				{
					OutType = elementType;
				}
			}
			else
			{
				UnityEngine.Debug.Log($"{nameof(MultiValue)} containing different types");
			}
			return OutType != null;
		}

		/////////////////////////////////////////////////////////
		public override string ToString() => string.Join(", ", m_ValuesList);
	}
}
