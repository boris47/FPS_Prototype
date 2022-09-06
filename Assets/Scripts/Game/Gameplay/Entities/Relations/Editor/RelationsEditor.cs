using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Entities.AI
{
	internal sealed class RelationsEditor : GuardedEditorWindow<RelationsEditor, RelationsData>
	{
		private static		string[]									m_FactionNames							= System.Enum.GetNames(typeof(EFactions));

		private				List<Rect>									m_RowElements							= new List<Rect>();
		private				List<Rect>									m_ListElements							= new List<Rect>();
		private				List<List<Rect>>							m_SlidersElements						= new List<List<Rect>>();


		//////////////////////////////////////////////////////////////////////////
		[MenuItem("Window/Relations Editor")]
		internal static void OnMenuItem()
		{
			OpenWindow("Relations Editor", RelationsData.ResourcePath, new Vector2(400f, 200f), new Vector2(1200, 900f));
		}
		
		//////////////////////////////////////////////////////////////////////////
		public static void OpenWindow(RelationsData InRelationsData)
		{
			OpenWindow("Relations Editor", RelationsData.ResourcePath, InRelationsData, new Vector2(400f, 200f), new Vector2(1200, 900f));
		}


		//////////////////////////////////////////////////////////////////////////
		protected override void OnEnable()
		{
			base.OnEnable();

			CreateInterface();
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnDisable()
		{
			base.OnDisable();

			m_RowElements.Clear();
			m_ListElements.Clear();
			m_SlidersElements.Clear();
		}

		//////////////////////////////////////////////////////////////////////////
		private void CreateInterface()
		{
			m_FactionNames = System.Enum.GetNames(typeof(EFactions));
			Vector2 biggestLabelSize = m_FactionNames.Select(s => EditorStyles.label.CalcSize(new GUIContent(s))).MaxBy(v => v.sqrMagnitude);

			const float sliderWidth = 100f;
			const float leftPanelStart = 10f;
			const float upRowPanelStart = 10f;
			const float columnSeparator = 15f;
			float rowLineHeight = EditorGUIUtility.singleLineHeight * 1.3f;
			float labelVsSliderWidth = Mathf.Max(biggestLabelSize.x, sliderWidth);
			float rightPanelStart = leftPanelStart + labelVsSliderWidth + 10f;
			float upTableStart = upRowPanelStart + rowLineHeight;

			Vector2 position = Vector2.zero;

			// Add top row labels
			position.Set(rightPanelStart, upRowPanelStart);
			{
				for (int i = 0, length = m_FactionNames.Length; i < length; i++)
				{
					string factionName = m_FactionNames[i];
					Vector2 factionNameSize = EditorStyles.label.CalcSize(new GUIContent(factionName));
					m_RowElements.Add(new Rect(position.x, position.y, factionNameSize.x, factionNameSize.y));
					position.x += labelVsSliderWidth + columnSeparator;
				}
			}

			// Left names list item
			position.Set(leftPanelStart, upTableStart);
			{
				for (int i = 0, length = m_FactionNames.Length; i < length; i++)
				{
					string factionName = m_FactionNames[i];
					Vector2 factionNameSize = EditorStyles.label.CalcSize(new GUIContent(factionName));
					m_ListElements.Add(new Rect(position.x, position.y, factionNameSize.x, factionNameSize.y));
					position.y += rowLineHeight;
				}
			}

			// Sliders
			position.Set(rightPanelStart, upTableStart);
			{
				for (int i = 0, length = m_FactionNames.Length; i < length; i++)
				{
					List<Rect> slidersForThisRow = m_SlidersElements.AddRef(new List<Rect>());
					for (int k = 0; k < length; k++)
					{
						slidersForThisRow.Add(new Rect(position.x - (labelVsSliderWidth * 0.25f) + (biggestLabelSize.x * 0.5f), position.y, labelVsSliderWidth * 0.5f, rowLineHeight));
						position.x += labelVsSliderWidth + columnSeparator;
					}
					position.x = rightPanelStart;
					position.y += rowLineHeight;
				}
			}
		}

		//////////////////////////////////////////////////////////////////////////
		private void OnGUI()
		{
			for (int i = 0, length = m_FactionNames.Length; i < length; i++)
			{
				string name = m_FactionNames[i];

				// Top Row Element
				if (m_RowElements.IsValidIndex(i))
				{
					Rect rect = m_RowElements[i];
					GUI.Label(rect, name, EditorStyles.label);
				}

				// Left List Element
				if (m_ListElements.IsValidIndex(i))
				{
					Rect rect = m_ListElements[i];
					GUI.Label(rect, name, EditorStyles.label);
				}

				// Sliders
				if (m_SlidersElements.IsValidIndex(i))
				{
					List<Rect> slidersForThisRow = m_SlidersElements[i];
					for (int k = 0; k < length; k++)
					{
						if (k != i)
						{
							Rect rect = slidersForThisRow[k];
							short currentValue = Data.EDITOR_ONLY_Data[i, k];
							short newValue = (short)GUI.HorizontalSlider(rect, currentValue, short.MinValue, short.MaxValue);
							if (currentValue != newValue)
							{
								EditorUtility.SetDirty(Data);
								Data.EDITOR_ONLY_Data[i, k] = newValue;
							}
						}
					}
				}
			}
		}


		//////////////////////////////////////////////////////////////////////////
		//////////////////////////////////////////////////////////////////////////

		[CustomEditor(typeof(RelationsData))]
		internal class RelationsDataCustomEditor : Editor
		{
			public override void OnInspectorGUI()
			{
				base.OnInspectorGUI();

				if (GUILayout.Button("Edit Relations"))
				{
					RelationsEditor.OpenWindow(serializedObject.targetObject as RelationsData);
				}
			}
		}
	}

}
