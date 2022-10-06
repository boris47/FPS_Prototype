
using UnityEngine;

namespace Entities.AI
{
	/// <summary> Used to get or store a value inside a blackboard </summary>
	[System.Serializable]
	public sealed class BlackboardEntryKey
	{
		[SerializeField]
		private string m_Name = string.Empty;

		public uint UniqueId => Utils.Generic.GetUniqueId(m_Name);
		public string Name => m_Name;

		//////////////////////////////////////////////////////////////////////////
		public bool IsEqualTo(in BlackboardEntryKey InOtherKey) => UniqueId == InOtherKey.UniqueId;

		//////////////////////////////////////////////////////////////////////////
		public static implicit operator BlackboardEntryKey(in string InString) => new BlackboardEntryKey(InString);
		public static implicit operator string(in BlackboardEntryKey InKey) => InKey.IsNotNull() ? InKey.m_Name : null;

		//////////////////////////////////////////////////////////////////////////
		private BlackboardEntryKey(in string InName)
		{
			m_Name = InName;
		}
	}
}

