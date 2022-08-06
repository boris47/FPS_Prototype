using System.Collections.Generic;
using UnityEngine;

namespace Entities.AI
{
	public class AIGroupSceneManager : GameMonoBehaviourSingleton<AIGroupSceneManager>
	{
		[SerializeField, ReadOnly]
		private List<AIGroup> m_Collection = new List<AIGroup>();

		//////////////////////////////////////////////////////////////////////////
		/// <summary> Register a new group if not already contained </summary>
		public void RegisterGroup(AIGroup group)
		{
			if (!m_Collection.Exists(g => g.Id == group.Id))
			{
				m_Collection.Add(group);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		/// <summary> Remove the group </summary>
		public void UnregisterGroup(AIGroup group)
		{
			if (m_Collection.Exists(g => g.Id == group.Id))
			{
				m_Collection.Remove(group);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		/// <summary> Removes all entities from the group and destroy it</summary>
		public void DisgregateGroup(AIGroup group)
		{
			foreach (Entity entity in group.GetEntites())
			{
///				entity.SetGroup(null);
			}

			Object.Destroy(group);
		}

		//////////////////////////////////////////////////////////////////////////
		/// <summary> Returns true if group with the given id is found </summary>
		public bool TryGetById(System.Guid id, out AIGroup outGroup)
		{
			outGroup = null;
			int index = m_Collection.FindIndex(i => i.Id == id);
			bool bResult = index >= 0;
			if (bResult)
			{
				outGroup = m_Collection[index];
			}
			return bResult;
		}
	}
}
