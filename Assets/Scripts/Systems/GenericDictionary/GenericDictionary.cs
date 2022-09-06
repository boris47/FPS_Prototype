using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

[Serializable]
public class UDictionary
{
	[AttributeUsage(AttributeTargets.Field, Inherited = true)]
	public class ReadOnly : PropertyAttribute
	{

	}
	[AttributeUsage(AttributeTargets.Field, Inherited = true)]
	public class ModifiableOutside : PropertyAttribute
	{

	}

	[AttributeUsage(AttributeTargets.Field, Inherited = true)]
	public class Limiter : PropertyAttribute
	{
		public readonly float Value = 40f;

		public Limiter(float InLimiterValue)
		{
			Value = Mathf.Clamp(InLimiterValue, 0f, 100f);
		}
	}

#if UNITY_EDITOR
	[CustomPropertyDrawer(typeof(ReadOnly))]
	[CustomPropertyDrawer(typeof(UDictionary), true)]
	public class Drawer : PropertyDrawer
	{
		SerializedProperty property;

		public bool IsExpanded
		{
			get => property.isExpanded;
			set => property.isExpanded = value;
		}

		SerializedProperty keys;
		SerializedProperty values;

		public bool IsAligned => keys.arraySize == values.arraySize;

		ReorderableList list;

		GUIContent label;

		bool isReadOnly;
		bool bModifiableOutside;
		float limiterValue;

		public static float SingleLineHeight => EditorGUIUtility.singleLineHeight;

		public const float ElementHeightPadding = 6f;
		public const float ElementSpacing = 10f;
		public const float ElementFoldoutPadding = 20f;

		public const float TopPadding = 5f;
		public const float BottomPadding = 5f;

		void Init(SerializedProperty value)
		{
			if (SerializedProperty.EqualContents(value, property)) return;

			property = value;

			keys = property.FindPropertyRelative(nameof(keys));
			values = property.FindPropertyRelative(nameof(values));

			object[] attributes = fieldInfo.GetCustomAttributes(true);

			isReadOnly = attributes.Any(att => att is ReadOnly);
			bModifiableOutside = attributes.Any(att => att is ModifiableOutside);
			limiterValue = (attributes.FirstOrDefault(att => att is Limiter) as Limiter)?.Value ?? 40f;

			list = new ReorderableList(property.serializedObject, keys, bModifiableOutside, true, bModifiableOutside, bModifiableOutside)
			{
				drawHeaderCallback = DrawHeader,

				onAddCallback = Add,
				onRemoveCallback = Remove,

				elementHeightCallback = GetElementHeight,

				drawElementCallback = DrawElement,
			};

			list.onReorderCallbackWithDetails += Reorder;
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			Init(property);

			float height = TopPadding + BottomPadding;

			if (IsAligned)
				height += IsExpanded ? list.GetHeight() : list.headerHeight;
			else
				height += SingleLineHeight;

			return height;
		}

		public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
		{
			label.text = $" {label.text}";

			this.label = label;

			Init(property);

			rect = EditorGUI.IndentedRect(rect);

			rect.y += TopPadding;
			rect.height -= TopPadding + BottomPadding;

			if (IsAligned == false)
			{
				DrawAlignmentWarning(ref rect);
				return;
			}

			if (IsExpanded)
				DrawList(ref rect);
			else
				DrawCompleteHeader(ref rect);
		}

		void DrawList(ref Rect rect)
		{
			EditorGUIUtility.labelWidth = 80f;
			EditorGUIUtility.fieldWidth = 80f;

			list.DoList(rect);
		}

		void DrawAlignmentWarning(ref Rect rect)
		{
			float width = 80f;
			float spacing = 5f;

			rect.width -= width;

			EditorGUI.HelpBox(rect, "  Misalignment Detected", MessageType.Error);

			rect.x += rect.width + spacing;
			rect.width = width - spacing;

			if (GUI.Button(rect, "Fix"))
			{
				if (keys.arraySize > values.arraySize)
				{
					int difference = keys.arraySize - values.arraySize;

					for (int i = 0; i < difference; i++)
						keys.DeleteArrayElementAtIndex(keys.arraySize - 1);
				}
				else if (keys.arraySize < values.arraySize)
				{
					int difference = values.arraySize - keys.arraySize;

					for (int i = 0; i < difference; i++)
						values.DeleteArrayElementAtIndex(values.arraySize - 1);
				}
			}
		}

		#region Draw Header
		void DrawHeader(Rect rect)
		{
			rect.x += 10f;

			IsExpanded = EditorGUI.Foldout(rect, IsExpanded, label, true);
		}

		void DrawCompleteHeader(ref Rect rect)
		{
			ReorderableList.defaultBehaviours.DrawHeaderBackground(rect);

			rect.x += 6;
			rect.y += 0;

			DrawHeader(rect);
		}
		#endregion

		float GetElementHeight(int index)
		{
			float max = 0f;
			if (index < keys.arraySize)
			{
				SerializedProperty key = keys.GetArrayElementAtIndex(index);
				SerializedProperty value = values.GetArrayElementAtIndex(index);

				float kHeight = GetChildrenSingleHeight(key);
				float vHeight = GetChildrenSingleHeight(value);

				max = Math.Max(kHeight, vHeight);

				if (max < SingleLineHeight) max = SingleLineHeight;
			}

			return max + ElementHeightPadding;
		}

		#region Draw Element
		void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
		{
			rect.height -= ElementHeightPadding;
			rect.y += ElementHeightPadding * 0.5f;

			Rect[] areas = Split(rect, limiterValue, 100f - limiterValue);

			DrawKey(areas[0], index);
			DrawValue(areas[1], index);
		}

		void DrawKey(Rect rect, int index)
		{
			SerializedProperty property = keys.GetArrayElementAtIndex(index);

			rect.x += ElementSpacing * 0.5f;
			rect.width -= ElementSpacing;

			DrawField(rect, property);
		}

		void DrawValue(Rect rect, int index)
		{
			SerializedProperty property = values.GetArrayElementAtIndex(index);

			rect.x += ElementSpacing * 0.5f;
			rect.width -= ElementSpacing;

			DrawField(rect, property);
		}

		void DrawField(Rect rect, SerializedProperty property)
		{
			rect.height = SingleLineHeight;

			using (new EditorGUI.DisabledScope(disabled: isReadOnly))
			{
				if (IsInline(property))
				{
					EditorGUI.PropertyField(rect, property, GUIContent.none);
				}
				else
				{
					rect.x += ElementSpacing * 0.5f;
					rect.width -= ElementSpacing;

					foreach (SerializedProperty child in IterateChildern(property))
					{
						EditorGUI.PropertyField(rect, child, false);

						rect.y += SingleLineHeight + 2f;
					}
				}
			}
		}
		#endregion

		void Reorder(ReorderableList list, int oldIndex, int newIndex)
		{
			values.MoveArrayElement(oldIndex, newIndex);
		}

		void Add(ReorderableList list)
		{
			values.InsertArrayElementAtIndex(values.arraySize);

			ReorderableList.defaultBehaviours.DoAddButton(list);
		}

		void Remove(ReorderableList list)
		{
			values.DeleteArrayElementAtIndex(list.index);

			ReorderableList.defaultBehaviours.DoRemoveButton(list);
		}

		//Static Utility
		static Rect[] Split(Rect source, params float[] cuts)
		{
			Rect[] rects = new Rect[cuts.Length];

			float x = 0f;

			for (int i = 0; i < cuts.Length; i++)
			{
				rects[i] = new Rect(source);

				rects[i].x += x;
				rects[i].width *= cuts[i] / 100;

				x += rects[i].width;
			}

			return rects;
		}

		static bool IsInline(SerializedProperty property)
		{
			switch (property.propertyType)
			{
				case SerializedPropertyType.Generic:
					return property.hasVisibleChildren == false;
			}

			return true;
		}

		static IEnumerable<SerializedProperty> IterateChildern(SerializedProperty property)
		{
			string path = property.propertyPath;

			property.Next(true);

			while (true)
			{
				yield return property;

				if (property.NextVisible(false) == false) break;
				if (property.propertyPath.StartsWith(path) == false) break;
			}
		}

		float GetChildrenSingleHeight(SerializedProperty property)
		{
			if (IsInline(property)) return SingleLineHeight;

			float height = 0f;

			foreach (SerializedProperty child in IterateChildern(property))
				height += SingleLineHeight + 2f;

			return height;
		}
	}
#endif
}

[Serializable]
public class UDictionary<TKey, TValue> : UDictionary, IDictionary<TKey, TValue>
{
	[SerializeField]
	List<TKey> keys;
	public List<TKey> Keys => keys;
	ICollection<TKey> IDictionary<TKey, TValue>.Keys => keys;

	[SerializeField]
	List<TValue> values;
	public List<TValue> Values => values;
	ICollection<TValue> IDictionary<TKey, TValue>.Values => values;

	public int Count => keys.Count;

	public bool IsReadOnly => false;

	Dictionary<TKey, TValue> cache;

	public bool Cached => cache != null;

	public Dictionary<TKey, TValue> Dictionary
	{
		get
		{
			if (cache == null)
			{
				cache = new Dictionary<TKey, TValue>();

				for (int i = 0; i < keys.Count; i++)
				{
					if (keys[i] == null) continue;
					if (cache.ContainsKey(keys[i])) continue;

					cache.Add(keys[i], values[i]);
				}
			}

			return cache;
		}
	}

	public TValue this[TKey key]
	{
		get => Dictionary[key];
		set
		{
			int index = keys.IndexOf(key);

			if (index < 0)
			{
				Add(key, value);
			}
			else
			{
				values[index] = value;
				if (Cached) Dictionary[key] = value;
			}
		}
	}

	public bool TryGetValue(TKey key, out TValue value) => Dictionary.TryGetValue(key, out value);

	public bool ContainsKey(TKey key) => Dictionary.ContainsKey(key);
	public bool Contains(KeyValuePair<TKey, TValue> item) => ContainsKey(item.Key);

	public void Add(TKey key, TValue value)
	{
		if (keys.AddUnique(key))
		{
			values.Add(value);
		}

		if (Cached) Dictionary.Add(key, value);
	}
	public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);

	public bool Remove(TKey key)
	{
		int index = keys.IndexOf(key);

		if (index < 0) return false;

		keys.RemoveAt(index);
		values.RemoveAt(index);

		if (Cached) Dictionary.Remove(key);

		return true;
	}
	public bool Remove(KeyValuePair<TKey, TValue> item) => Remove(item.Key);

	public void Clear()
	{
		keys.Clear();
		values.Clear();

		if (Cached) Dictionary.Clear();
	}

	public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => (Dictionary as IDictionary).CopyTo(array, arrayIndex);

	public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => Dictionary.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => Dictionary.GetEnumerator();

	public UDictionary()
	{
		values = new List<TValue>();
		keys = new List<TKey>();
	}
}
