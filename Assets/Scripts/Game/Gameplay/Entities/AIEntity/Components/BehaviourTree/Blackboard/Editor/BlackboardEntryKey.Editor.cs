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
		private				bool							m_IsInitialized						= false;
		private				Blackboard						m_BlackboardAsset					= null;

		private				uint							m_CurrentUniqueId					= 0u;
		private				int								m_SelectedIndex						= 0;
		private				string[]						m_CurrentOptions					= new string[0];
		private				List<BlackboardEntryKey>		m_EntryKeyList						= new List<BlackboardEntryKey>();
		private				string							m_ErrorMsg							= string.Empty;
		private				System.Type[]					m_SupportedTypes					= new System.Type[0];


		//////////////////////////////////////////////////////////////////////////
		private void Initialize(in SerializedProperty InProperty)
		{
			if (!m_IsInitialized)
			{
				if (InProperty.serializedObject.targetObject is Components.Behaviours.BTNode node)
				{
					m_BlackboardAsset = node.BehaviourTreeAsset.BlackboardAsset;
					if (m_BlackboardAsset.IsNotNull())
					{
						if (InProperty.TryGetAttribute(out BlackboardKeyTypeAttribute att, false))
						{
							m_SupportedTypes = att.Types.Select(t =>
							{
								System.Type outValue = null;
								if (Utils.CustomAssertions.IsTrue(ReflectionHelper.IsInerithedFrom(typeof(BlackboardEntryBase), t)))
								{
									Utils.CustomAssertions.IsTrue(ReflectionHelper.TryGetGenericArg(t, out outValue));
								}
								return outValue;
							}).ToArray();

							UpdateBlackboardKeys();
							Blackboard.Editor.OnKeysModified -= OnKeysModified;
							Blackboard.Editor.OnKeysModified += OnKeysModified;

							m_IsInitialized = true;
						}
						else
						{
							m_ErrorMsg = $"Use {nameof(BlackboardKeyTypeAttribute)}";
						}
					}
					else
					{
						m_ErrorMsg = $"No blackboard asset!!!";
					}
				}
				else
				{
					m_ErrorMsg = $"Usable only on BT Nodes";
				}
			}
		}

		//////////////////////////////////////////////////////////////////////////
		private void OnKeysModified()
		{
			if (this.IsNotNull())
			{
				UpdateBlackboardKeys();
			}
			else
			{
				Blackboard.Editor.OnKeysModified -= OnKeysModified;
			}
		}

		private bool m_IsUpdating = false;

		//////////////////////////////////////////////////////////////////////////
		private void UpdateBlackboardKeys()
		{
			m_IsUpdating = true;

			// Re-fill entry list
			{
				m_EntryKeyList.Clear();
				m_EntryKeyList.Add(BlackboardEntryKey.None);
				if (Utils.CustomAssertions.IsNotNull(m_BlackboardAsset))
				{
					for (int i = 0, length = Blackboard.Editor.GetKeysCount(m_BlackboardAsset); i < length; i++)
					{
						if (Utils.CustomAssertions.IsTrue(Blackboard.Editor.TryGetKeyAt(m_BlackboardAsset, i, out BlackboardKeySpecifier keySpecifier)))
						{
							if (m_SupportedTypes.Contains(keySpecifier.Type))
							{
								m_EntryKeyList.Add(keySpecifier.Key);
							}
						}
					}
				}
			}

			// Update dropdown options
			m_CurrentOptions = m_EntryKeyList.Select(i => i.Name).ToArray();

			m_IsUpdating = false;
		}

		//////////////////////////////////////////////////////////////////////////
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			Initialize(property);

			if (m_IsInitialized && !m_IsUpdating && m_BlackboardAsset.IsNotNull())
			{
				// Ensure correct selection
				{
					m_CurrentUniqueId = 0u;
					m_SelectedIndex = 0;

					// Is value is already stored retrieve uniqueid and selection index
					if (fieldInfo.GetValue(property.serializedObject.targetObject) is BlackboardEntryKey entryKey && entryKey.IsNotNull())
					{
						if (Blackboard.Editor.HasKey(m_BlackboardAsset, entryKey))
						{
							m_CurrentUniqueId = entryKey.UniqueId;
						}

						m_EntryKeyList.TryFind(out BlackboardEntryKey _, out m_SelectedIndex, i => i.UniqueId == m_CurrentUniqueId);
					}
				}

				EditorGUI.LabelField(position, label);

				using (new EditorGUI.PropertyScope(position, label, property))
				{
					EditorGUI.BeginChangeCheck();
					{
						int tempIndex = EditorGUI.Popup(position, m_SelectedIndex, m_CurrentOptions);

						// Usage of fieldInfo.SetValue here
						// Ref: https://forum.unity.com/threads/how-to-set-serializedproperty-managedreferencevalue-to-null.746645/#post-5112854

						// Reset
						if (tempIndex == 0 && m_SelectedIndex > 0)
						{
							fieldInfo.SetValue(property.serializedObject.targetObject, null);
							m_CurrentUniqueId = 0u;
							m_SelectedIndex = 0;
						}
						//						Assignment								Change
						else if ((m_SelectedIndex == 0 && tempIndex > 0) || (tempIndex != m_SelectedIndex))
						{
							if (Blackboard.Editor.TryGetKey(m_BlackboardAsset, m_CurrentOptions[tempIndex], out BlackboardKeySpecifier newKeySpecifier))
							{
								fieldInfo.SetValue(property.serializedObject.targetObject, newKeySpecifier.Key);
								m_CurrentUniqueId = newKeySpecifier.Key.UniqueId;
								m_SelectedIndex = tempIndex;
							}
							else
							{
								Debug.LogError($"Assignment/Change: Previous Index: {m_SelectedIndex}, new index {tempIndex}. Out of index or invalid {nameof(BlackboardKeySpecifier)} at index {tempIndex}");
							}
						}
					}

					if (EditorGUI.EndChangeCheck())
					{
						property.serializedObject.Update();
					}

					property.serializedObject.ApplyModifiedProperties();
				}
			}
			else
			{
				EditorGUI.LabelField(position, m_ErrorMsg);
			}
		}
	}
}