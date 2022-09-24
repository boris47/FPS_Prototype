using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Entities.AI.Components
{
	/// <summary> Used to get or store a value inside a blackboard </summary>
	[System.Serializable]
	[CreateAssetMenu(fileName = "", menuName = "Memory/Identifier")]
	public sealed class MemoryIdentifier : ScriptableObject
	{
#if UNITY_EDITOR
		[SerializeField]
		private string m_DebugName = string.Empty;
		public string DebugName => m_DebugName;
#endif
		private uint UniqueId => Utils.Generic.GetUniqueId(name);

		//////////////////////////////////////////////////////////////////////////
		public bool IsEqualTo(in MemoryIdentifier InOtherKey) => UniqueId == InOtherKey.UniqueId;

		//////////////////////////////////////////////////////////////////////////
		public static bool IsValid(in MemoryIdentifier InMemoryIdentifier) => !string.IsNullOrEmpty(InMemoryIdentifier.name);
	}
}
