
using UnityEngine;

namespace Entities.AI
{
	[System.Serializable]
	public sealed class BlackboardKeySpecifier
	{
		public static readonly System.Type[] kSupportedValuesType = new System.Type[]
		{
			typeof(bool), typeof(int), typeof(float), typeof(string),
			typeof(Vector3), typeof(Quaternion), typeof(Entity)
		};

		[SerializeField, ReadOnly]
		private BlackboardEntryKey m_Key = null;

		[SerializeField, ReadOnly]
		private Utils.TypeIdentifier m_TypeIdentifier = null;

		private System.Type m_Type = null;

		public BlackboardEntryKey Key => m_Key;
		public System.Type Type => m_Type;


		//////////////////////////////////////////////////////////////////////////
		private BlackboardKeySpecifier(in BlackboardEntryKey InKey, in System.Type InType)
		{
			m_Key = InKey;
			m_Type = InType;
			m_TypeIdentifier = new Utils.TypeIdentifier(InType);
		}

		//////////////////////////////////////////////////////////////////////////
		public void Load()
		{
			if (m_Type == null)
			{
				m_TypeIdentifier.TryGetType(out m_Type);
			}
		}

#if UNITY_EDITOR
		public static class Editor
		{
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
			public static void SetType(in BlackboardKeySpecifier InBlackboardKeySpecifier, in System.Type InNewType)
			{
				InBlackboardKeySpecifier.m_TypeIdentifier = new Utils.TypeIdentifier(InNewType);
				InBlackboardKeySpecifier.m_Type = InNewType;
			}
		}
#endif
	}
}

