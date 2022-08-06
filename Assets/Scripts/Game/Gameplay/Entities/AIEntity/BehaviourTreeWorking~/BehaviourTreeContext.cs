using System.Linq;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

namespace Entities.AI.Components.Behaviours
{
	[System.Serializable]
	public class BTNodeContext { }
	public class BehaviourTreeContext
	{
		private Dictionary<string, BTNodeContext> m_DataTable = new Dictionary<string, BTNodeContext>();

		//////////////////////////////////////////////////////////////////////////
		public T GetOrAdd<T>(in string InKey) where T : BTNodeContext, new()
		{
			T outResult = default;
			if (m_DataTable.TryGetValue(InKey, out BTNodeContext data))
			{
				outResult = (T)data;
			}
			else
			{
				BTNodeContext outData = (T)System.Activator.CreateInstance(typeof(T));
				m_DataTable.Add(InKey, outData);
				outResult = (T)outData;
			}
			return outResult;
		}

		//////////////////////////////////////////////////////////////////////////
		public bool TryGet<T>(in string InKey, out T OutResult) where T : BTNodeContext
		{
			if (m_DataTable.TryGetValue(InKey, out BTNodeContext data))
			{
				OutResult = (T)data;
				return true;
			}
			OutResult = default;
			return false;
		}

		//////////////////////////////////////////////////////////////////////////
		public bool Remove(in string InKey) => m_DataTable.Remove(InKey);
		
	}
}
