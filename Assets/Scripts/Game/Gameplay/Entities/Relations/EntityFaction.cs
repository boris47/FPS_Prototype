using UnityEngine;

namespace Entities.Relations
{
	[System.Serializable]
	public class EntityFaction : ScriptableObject
	{
		private static EntityFaction s_Default = null;
		public static EntityFaction Default => s_Default.IsNotNull() ? s_Default : CreateDefault();


		[SerializeField, ReadOnly]
		private uint m_UniqueId = 0;

		public string FactionName => name;
		public uint UniqueId => m_UniqueId;


		//////////////////////////////////////////////////////////////////////////
		public bool IsEqual(EntityFaction other) => m_UniqueId == other.m_UniqueId;

		//////////////////////////////////////////////////////////////////////////
		public static uint GetUniqueIdFor(string InName) => Utils.Generic.GetUniqueId(InName);

		//////////////////////////////////////////////////////////////////////////
		public static implicit operator uint(EntityFaction f) => f.IsNotNull() ? f.m_UniqueId : 0;

		//////////////////////////////////////////////////////////////////////////
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
		private static EntityFaction CreateDefault()
		{
			if (s_Default == null)
			{
				s_Default = ScriptableObject.CreateInstance<EntityFaction>();
				{
					s_Default.name = RelationsData.DefaultFactionName;
					s_Default.m_UniqueId = GetUniqueIdFor(RelationsData.DefaultFactionName);
				}
			}
			return s_Default;
		}


#if UNITY_EDITOR
		//////////////////////////////////////////////////////////////////////////
		public static class Editor
		{
			//////////////////////////////////////////////////////////////////////////
			public static EntityFaction Create(in string InFactionName)
			{
				EntityFaction outValue = ScriptableObject.CreateInstance<EntityFaction>();
				{
					outValue.name = InFactionName;
					outValue.m_UniqueId = GetUniqueIdFor(InFactionName);
				}
				return outValue;
			}

			//////////////////////////////////////////////////////////////////////////
			public static void Rename(in EntityFaction InEntityFaction, in string InNewName)
			{
				InEntityFaction.name = InNewName;
				InEntityFaction.m_UniqueId = GetUniqueIdFor(InNewName);
			}

			//////////////////////////////////////////////////////////////////////////
			public static bool IsDefault(in EntityFaction InEntityFaction) => InEntityFaction.IsEqual(Default);
		}
#endif
	}
}
