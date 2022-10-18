using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

namespace Entities.AI
{
	using Components;

	[CustomPropertyDrawer(typeof(BlackboardEntryKey))]
	public sealed class BlackboardEntryKeyDrawer : PropertyDrawer
	{
		private bool m_IsInitialized = false;
		private SerializedProperty m_Property = null;
		private Blackboard m_BlackboardAsset = null;

		private uint m_CurrentUniqueId = 0u;
		private int m_SelectedIndex = 0;
		private string[] m_CurrentOptions = null;
		List<BlackboardKeySpecifier> m_ItemsList = new List<BlackboardKeySpecifier>();


		//////////////////////////////////////////////////////////////////////////
		private void Initialize(in SerializedProperty InProperty)
		{
			if (!m_IsInitialized)
			{
				m_Property = InProperty;

				if (m_Property.serializedObject.targetObject is IBlackboardProjector blackboardProjector)
				{
					m_BlackboardAsset = blackboardProjector.GetBlackboardAsset();

					if (m_BlackboardAsset.IsNotNull())
					{
						UpdateBlackboardKeys();
					}
				}

				m_IsInitialized = true;
			}
		}

		//////////////////////////////////////////////////////////////////////////
		private void UpdateBlackboardKeys()
		{
			m_ItemsList.Clear();
			if (Utils.CustomAssertions.IsNotNull(m_BlackboardAsset))
			{
				for (int i = 0, length = Blackboard.Editor.GetKeysCount(m_BlackboardAsset); i < length; i++)
				{
					if (Utils.CustomAssertions.IsTrue(Blackboard.Editor.TryGetKeyAt(m_BlackboardAsset, i, out BlackboardKeySpecifier keySpecifier)))
					{
						m_ItemsList.Add(keySpecifier);
					}
				}
			}

			m_CurrentOptions = m_ItemsList.Select(i => i.Key.Name).ToArray();

			UpdateCurrentUniqueID();

			UpdateCurrentSelected();
		}

		//////////////////////////////////////////////////////////////////////////
		private void UpdateCurrentUniqueID()
		{
			m_CurrentUniqueId = 0u;
			if (m_Property.propertyType == SerializedPropertyType.ManagedReference)
			{
				if (m_Property.managedReferenceValue is BlackboardEntryKey entryKey && entryKey.IsNotNull())
				{
					m_CurrentUniqueId = entryKey.UniqueId;
				}
			}
			else
			{
				m_CurrentUniqueId = Utils.Generic.GetUniqueId(m_Property.FindPropertyRelative("m_Name").stringValue);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		private void UpdateCurrentSelected()
		{
			if (!m_ItemsList.TryFind(out BlackboardKeySpecifier _, out m_SelectedIndex, i => i.Key?.UniqueId == m_CurrentUniqueId))
			{
				m_SelectedIndex = 0;
			}
		}

		//////////////////////////////////////////////////////////////////////////
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			Initialize(property);

			return base.GetPropertyHeight(property, label);
		}

		//////////////////////////////////////////////////////////////////////////
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			Initialize(property);

			EditorGUI.LabelField(position, label);

			int tempIndex = EditorGUI.Popup(position, m_SelectedIndex, m_CurrentOptions);
			if (tempIndex != m_SelectedIndex)
			{
				if (Blackboard.Editor.TryGetKeyAt(m_BlackboardAsset, tempIndex, out BlackboardKeySpecifier newKeySpecifier))
				{
					m_Property.managedReferenceValue = newKeySpecifier.Key;
					m_SelectedIndex = tempIndex;
				}
				else
				{
					Debug.LogError($"Previous Index: {m_SelectedIndex}, new index {tempIndex}. Out of index or invalid {nameof(BlackboardKeySpecifier)} at index {tempIndex}");
				}
			}
		}
	}
}