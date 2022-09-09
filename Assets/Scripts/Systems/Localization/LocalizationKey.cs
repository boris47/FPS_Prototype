
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Localization
{
	[System.Serializable]
	public sealed class LocalizationKey : ScriptableObject
	{
		[SerializeField, HideInInspector]
		private uint m_UniqueId = 0u;
		public uint UniqueId => m_UniqueId;


		//////////////////////////////////////////////////////////////////////////
		public bool IsEqualTo(in LocalizationKey InOtherKey) => UniqueId == InOtherKey.UniqueId;

		//////////////////////////////////////////////////////////////////////////
		public static implicit operator string(in LocalizationKey k) => k.name;
		public static implicit operator uint(in LocalizationKey k) => k.UniqueId;

#if UNITY_EDITOR
		//////////////////////////////////////////////////////////////////////////
		public static class Editor
		{
			private static readonly List<LocalizationKey> s_AllKeys = new List<LocalizationKey>();

			//////////////////////////////////////////////////////////////////////////
			public static void Reset()
			{
				s_AllKeys.Clear();
			}
			
			//////////////////////////////////////////////////////////////////////////
			public static bool TryGetById(uint InId, out LocalizationKey OutValue)
			{
				bool bResult = false;
				OutValue = null;
				if (s_AllKeys.TryFind(out LocalizationKey keyFound, out var _, k => k.m_UniqueId == InId))
				{
					OutValue = keyFound;
				}
				return bResult;
			}

			//////////////////////////////////////////////////////////////////////////
			public static LocalizationKey Get(uint InKeyId)
			{
				return s_AllKeys.FirstOrDefault(k => k.m_UniqueId == InKeyId);
			}
			
			//////////////////////////////////////////////////////////////////////////
			public static LocalizationKey Get(string InKeyName)
			{
				return s_AllKeys.FirstOrDefault(k => k.name == InKeyName);
			}
			
			//////////////////////////////////////////////////////////////////////////
			public static bool TryGet(uint InKeyId, out LocalizationKey OutLocalizationKey)
			{
				return s_AllKeys.TryFind(out OutLocalizationKey, out var _, k => k.m_UniqueId == InKeyId);
			}
			
			//////////////////////////////////////////////////////////////////////////
			public static bool TryGet(string InKeyName, out LocalizationKey OutLocalizationKey)
			{
				return s_AllKeys.TryFind(out OutLocalizationKey, out var _, k => k.name == InKeyName);
			}

			//////////////////////////////////////////////////////////////////////////
			public static bool Contains(string InName)
			{
				return s_AllKeys.Exists(k => k.name == InName);
			}

			//////////////////////////////////////////////////////////////////////////
			public static bool Contains(uint InKeyId)
			{
				return s_AllKeys.Exists(k => k.m_UniqueId == InKeyId);
			}

			//////////////////////////////////////////////////////////////////////////
			public static LocalizationKey Create(in string InKeyName)
			{
				LocalizationKey outValue = null;
				if (!string.IsNullOrEmpty(InKeyName))
				{
					outValue = CreateInstance<LocalizationKey>();
					outValue.name = InKeyName;
					outValue.m_UniqueId = Utils.Generic.GetUniqueId(InKeyName);
					s_AllKeys.Add(outValue);
				}
				return outValue;
			}

			//////////////////////////////////////////////////////////////////////////
			public static void Destroy(in LocalizationKey InLocalizationKey)
			{
				s_AllKeys.Remove(InLocalizationKey);
			}
		}

		[UnityEditor.CustomEditor(typeof(LocalizationKey))]
		public class LocalizationKeyEditor : UnityEditor.Editor
		{
			private LocalizationKey instance = null;

			private void OnEnable()
			{
				instance = target as LocalizationKey;
			}

			public override void OnInspectorGUI()
			{
				DrawDefaultInspector();
			}
		}

		[UnityEditor.CustomPropertyDrawer(typeof(LocalizationKey))]
		public sealed class ReadOnlyDrawer : UnityEditor.PropertyDrawer
		{
			public override float GetPropertyHeight(UnityEditor.SerializedProperty property, GUIContent label)
			{
				return UnityEditor.EditorGUI.GetPropertyHeight(property, label, true);
			}

			public override void OnGUI(Rect position, UnityEditor.SerializedProperty property, GUIContent label)
			{
				UnityEditor.EditorGUI.PropertyField(position, property, label, true);
			}
		}
#endif
	}
}

