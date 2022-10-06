
using UnityEngine;

namespace Entities.AI
{
	using TypeReferences;

	[System.Serializable]
	public sealed class BlackboardKeySpecifier
	{
		[SerializeField]
		private BlackboardEntryKey m_Key = null;

		[SerializeField, TypeOptions, ReadOnly]
		private TypeReference m_Type = null;

		public BlackboardEntryKey Key => m_Key;
		public System.Type Type => m_Type;

		//////////////////////////////////////////////////////////////////////////
		public static BlackboardKeySpecifier Create(in BlackboardEntryKey InKey, in System.Type InType)
		{
			if (!string.IsNullOrEmpty(InKey) && InType.IsNotNull())
			{
				return new BlackboardKeySpecifier(InKey, InType);
			}
			return null;
		}

		//////////////////////////////////////////////////////////////////////////
		private BlackboardKeySpecifier(in BlackboardEntryKey InKey, in System.Type InType)
		{
			m_Key = InKey;
			m_Type = InType;
		}
	}
}

