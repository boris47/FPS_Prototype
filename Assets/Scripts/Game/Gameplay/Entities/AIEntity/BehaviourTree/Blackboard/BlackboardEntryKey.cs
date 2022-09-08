
using UnityEngine;

namespace Entities.AI
{
	/// <summary> Used to get or store a value inside a blackboard </summary>
	[System.Serializable]
	[CreateAssetMenu(fileName = "", menuName = "Blackboard/Entry Key")]
	public sealed class BlackboardEntryKey: ScriptableObject
	{
#if UNITY_EDITOR
		[SerializeField]
		private string m_DebugName = string.Empty;
		public string DebugName => m_DebugName;
#endif
		[SerializeField, ReadOnly]
		private uint m_UniqueId = 0u;
		private uint UniqueId
		{
			get
			{
				if (m_UniqueId == 0u)
				{
					m_UniqueId = Utils.Generic.GetUniqueId(name);
				}
				return m_UniqueId;
			}
		}

		//////////////////////////////////////////////////////////////////////////
		public bool IsEqualTo(in BlackboardEntryKey InOtherKey) => UniqueId == InOtherKey.UniqueId;
	}
}

