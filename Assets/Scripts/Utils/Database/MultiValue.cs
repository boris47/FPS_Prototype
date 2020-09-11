
namespace Database
{

	using System.Collections;
	using System.Collections.Generic;

	[System.Serializable]
	public class MultiValue : IEnumerable
	{
		private readonly List<Value> m_ValuesList = new List<Value>();

		//-------------------------------------------------------
		public Value[] ValueList => this.m_ValuesList.ToArray();

		//-------------------------------------------------------
		public int Size
		{
			get
			{
				return this.m_ValuesList.Count;
			}
		}

		//-------------------------------------------------------
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		//-------------------------------------------------------
		public List<Value>.Enumerator GetEnumerator()
		{
			return this.m_ValuesList.GetEnumerator();
		}


		public MultiValue(Value[] vValues = null, int capacity = 1)
		{
			this.m_ValuesList = vValues == null ? new List<Value>(capacity) : new List<Value>(vValues);
		}


		// Indexer behaviour
		public Value this[int Index]
		{
			get
			{
				if (this.m_ValuesList.Count > Index)
					return this.m_ValuesList[Index];
				return null;
			}
		}

		/////////////////////////////////////////////////////////
		public void Add(Value pValue)
		{
			this.m_ValuesList.Add(pValue);
		}


		/////////////////////////////////////////////////////////
		public bool DeductType(ref System.Type typeFound)
		{
			bool result = true;
			{
				System.Type elementType = this.m_ValuesList[0].GetType();
				if (this.m_ValuesList.TrueForAll(v => v.GetType() == elementType))
				{
					if (elementType == typeof(int) || elementType == typeof(float))
					{
						if (this.m_ValuesList.Count == 2)
						{
							typeFound = typeof(UnityEngine.Vector2);
						}

						if (this.m_ValuesList.Count == 3)
						{
							typeFound = typeof(UnityEngine.Vector3);
						}

						if (this.m_ValuesList.Count == 4)
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