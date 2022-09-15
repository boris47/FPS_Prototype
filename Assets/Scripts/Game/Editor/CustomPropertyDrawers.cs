using UnityEngine;
using UnityEditor;
using System.Collections.Generic;


public struct IndentLevelScope : System.IDisposable
{
	bool m_Disposed;
	int m_IndentLevel;

	public IndentLevelScope(int indent = 1)
	{
		m_Disposed = false;
		m_IndentLevel = indent;

		EditorGUI.indentLevel += m_IndentLevel;
	}

	public void Dispose()
	{
		if (m_Disposed)
		{
			return;
		}
		m_Disposed = true;

		EditorGUI.indentLevel += m_IndentLevel;
	}
}


[CustomPropertyDrawer(typeof(ConditionalPropertyAttribute))]
public class ConditionalPropertyDrawer : PropertyDrawer
{
	private static SerializedProperty GetPropertyByPath(in SerializedObject InSerializedObject, in SerializedProperty InSerializedProperty, string InFieldName)
	{
		// If this property is defined inside a nested type 
		// (like a struct inside a MonoBehaviour), look for
		// our condition field inside the same nested instance.
		string thisPropertyPath = InSerializedProperty.propertyPath;
		int last = thisPropertyPath.LastIndexOf('.');
		if (last > 0)
		{
			string containerPath = thisPropertyPath.Substring(0, last + 1);
			InFieldName = containerPath + InFieldName;
		}
		return InSerializedObject.FindProperty(InFieldName);
	}

	private static bool Compare(in bool a, in bool b, in EOperators op)
	{
		switch (op)
		{
			case EOperators.EQUAL: return a == b;
			case EOperators.NOT_EQUAL: return a != b;
			default: return a == b;
		}
	}

	private static bool Compare(in int a, in int b, in EOperators op)
	{
		switch (op)
		{
			case EOperators.GREATER: return a > b;
			case EOperators.GREATER_OR_EQUAL: return a >= b;
			case EOperators.EQUAL: return a == b;
			case EOperators.NOT_EQUAL: return a != b;
			case EOperators.LESS_OR_EQUAL: return a <= b;
			case EOperators.LESS: return a < b;
			default: return false;
		}
	}

	private static bool Compare(in float a, in float b, in EOperators op)
	{
		switch (op)
		{
			case EOperators.GREATER: return a > b;
			case EOperators.GREATER_OR_EQUAL: return a >= b;
			case EOperators.EQUAL: return a == b;
			case EOperators.NOT_EQUAL: return a != b;
			case EOperators.LESS_OR_EQUAL: return a <= b;
			case EOperators.LESS: return a < b;
			default: return false;
		}
	}

	public static bool Compare(in string a, in string b, in EOperators op)
	{
		switch (op)
		{
			case EOperators.EQUAL: return a == b;
			case EOperators.NOT_EQUAL: return a != b;
			default: return a == b;
		}
	}

	private static bool CompareProperties(in SerializedProperty propA, in EOperators op, in SerializedProperty propB)
	{
		switch (propA.propertyType)
		{
			case SerializedPropertyType.Boolean:	return Compare(propA.boolValue, propB.boolValue, op);
			case SerializedPropertyType.Integer:	return Compare(propA.intValue, propB.intValue, op);
			case SerializedPropertyType.Float:		return Compare(propA.floatValue, propB.floatValue, op);
			case SerializedPropertyType.String:		return Compare(propA.stringValue, propB.stringValue, op);
			default: return true;
		}
	}

	private bool ShouldShow(in SerializedProperty property)
	{
		bool outValue = true;

		ConditionalPropertyAttribute conditionAttribute = (ConditionalPropertyAttribute)attribute;
		SerializedProperty conditionProperty = GetPropertyByPath(property.serializedObject, property, conditionAttribute.FieldName);
		if (conditionProperty.IsNotNull())
		{
			bool IsOtherFieldName = conditionAttribute.IsOtherFieldName;
			if (IsOtherFieldName)
			{
				SerializedProperty otherConditionProperty = GetPropertyByPath(property.serializedObject, property, conditionAttribute.StringValue);
				if (otherConditionProperty.IsNotNull())
				{
					if (otherConditionProperty.propertyType == conditionProperty.propertyType)
					{
						outValue = CompareProperties(conditionProperty, conditionAttribute.Operator, otherConditionProperty);
					}
				}
				else
				{
					// Unable to handle comparison
					// A warning would spam in console
					outValue = true;
				}
			}
			else
			{
				EOperators op = conditionAttribute.Operator;
				switch (conditionProperty.propertyType)
				{
					case SerializedPropertyType.Boolean: outValue = Compare(conditionProperty.boolValue, conditionAttribute.BoolValue, op); break;
					case SerializedPropertyType.Integer: outValue = Compare(conditionProperty.intValue, conditionAttribute.IntValue, op); break;
					case SerializedPropertyType.Float: outValue = Compare(conditionProperty.floatValue, conditionAttribute.FloatValue, op); break;
					case SerializedPropertyType.String: outValue = Compare(conditionProperty.stringValue, conditionAttribute.StringValue, op); break;
					default: outValue = true; break;
				}
			}
		}
		return outValue;
	}

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		if (ShouldShow(property))
		{
			EditorGUI.PropertyField(position, property, label, true);
		}
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		if (ShouldShow(property))
		{
			// Provision the normal vertical spacing for this control.
			return EditorGUI.GetPropertyHeight(property, label, true);
		}
		else
		{
			// Collapse the unseen derived property.
			return -EditorGUIUtility.standardVerticalSpacing;
		}
	}
}



// Custom property inspector of type 'SurfaceManager.RegisteredMaterial'
[CustomPropertyDrawer(typeof(SurfaceManager.RegisteredTexture))]
public sealed class SurfaceManager_MaterialDrawer : PropertyDrawer
{
	//
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		SerializedProperty texture = property.FindPropertyRelative("texture");
		SerializedProperty surfaceIndex = property.FindPropertyRelative("surfaceIndex");

		// Set the new rect
		position.x = 0f;
		position.width *= 0.5f;
		position.height = 16f;

		// Draw the texture field
		EditorGUI.PropertyField(position, texture, GUIContent.none);

		// Draw the type field
		position.x = position.xMax;

		/* TODO Restore Editor Part
		surfaceIndex.intValue = EditorGUI.Popup(position, surfaceIndex.intValue, SurfaceManager.Instance?.GetAllSurfaceNames() ?? new string[0]);
		*/
	}

	//
	public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => 32f;
}


[CustomPropertyDrawer(typeof(ReadOnlyAttribute), true)]
public sealed class ReadOnlyDrawer : PropertyDrawer
{
	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		return EditorGUI.GetPropertyHeight(property, label, true);
	}

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		using (new EditorGUI.DisabledScope(disabled: true))
		{
			EditorGUI.PropertyField(position, property, label, true);
		}
	}
}


[CustomPropertyDrawer(typeof(ObsoleteInspectorAttribute))]
public sealed class ObsoleteInspectorDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		System.ObsoleteAttribute obsoleteAttribute = (System.ObsoleteAttribute)System.Attribute.GetCustomAttribute(fieldInfo, typeof(System.ObsoleteAttribute));
		EditorStyles.label.richText = true;
		EditorGUI.PropertyField(position, property, new GUIContent($"<color=red><i>{label.text}</i></color>", $"**OBSOLETE**\n\n{obsoleteAttribute.Message}"), true);
		EditorStyles.label.richText = false;
	}
}


[CustomPropertyDrawer(typeof(EnumBitFieldAttribute))]
public class EnumBitFieldAttributeDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		EnumBitFieldAttribute enumFlagsAttribute = attribute as EnumBitFieldAttribute;
		string[] names = System.Enum.GetNames(enumFlagsAttribute.EnumType);
		property.intValue = EditorGUI.MaskField(position, label, property.intValue, names);
	}
}

[CustomPropertyDrawer(typeof(Matrix4x4))]
public sealed class Matrix4x4Drawer : PropertyDrawer
{
	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		return ((EditorGUIUtility.singleLineHeight + 2f) * 4f) - 2f;
	}

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		float labelWidth = EditorGUIUtility.labelWidth;
		float lineHeight = EditorGUIUtility.singleLineHeight;
		float indent = EditorGUI.indentLevel * 14;

		GUI.Label(new Rect(position.x + indent, position.y, labelWidth - indent, lineHeight), label);

		float left = position.x + labelWidth;
		float width = (position.width - labelWidth) / 4;

		for (int y = 0; y < 4; y++)
		{
			for (int x = 0; x < 4; x++)
			{
				property.Next(true);

				// The math here probably could be simplified. I wanted to make sure the gaps between the
				// columns are exactly 2 pixels at all times. Because aesthetics.
				float x0 = Mathf.Round(left + (width * x)) + 2f;
				float x1 = Mathf.Round(left + (width * (x + 1f)));
				var rect = new Rect(x0 - 15, position.y + (y * (lineHeight + 2f)), x1 - x0 + 15f, lineHeight);

				// Avoid displaying useless values values like 6.123234e-17
				float oldValue = (float)System.Math.Round(property.floatValue, 6);
				float newValue = EditorGUI.FloatField(rect, oldValue);
				if (newValue != oldValue)
				{
					property.floatValue = newValue;
				}
			}
		}
	}
}

// With help of: https://github.com/garettbass/UnityExtensions.ArrayDrawer/blob/master/Editor/ArrayDrawer.cs
[CustomPropertyDrawer(typeof(Queue<>))]
public sealed class QueueDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		position.height = EditorGUIUtility.singleLineHeight;
		DefaultPropertyField(position, property, label);

		if (property.isExpanded && property.hasVisibleChildren)
		{
			float spacing = EditorGUIUtility.standardVerticalSpacing;
			using (new IndentLevelScope(1))
			{
				foreach (SerializedProperty child in EnumerateChildProperties(property))
				{
					position.y += spacing;
					position.y += position.height;
					position.height = EditorGUI.GetPropertyHeight(child, includeChildren: true);
					EditorGUI.PropertyField(position, child, includeChildren: true);
				}
			}
		}
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{

		float height = EditorGUIUtility.singleLineHeight;
		{
			if (property.isExpanded && property.hasVisibleChildren)
			{
				float spacing = EditorGUIUtility.standardVerticalSpacing;
				foreach (SerializedProperty child in EnumerateChildProperties(property))
				{
					height += spacing;
					height += EditorGUI.GetPropertyHeight(child, includeChildren: true);
				}
			}
		}
		return height;
	}

	private delegate bool DefaultPropertyFieldDelegate(Rect position, SerializedProperty property, GUIContent label);
	private static readonly DefaultPropertyFieldDelegate s_DefaultPropertyField = (DefaultPropertyFieldDelegate) System.Delegate.CreateDelegate
	(
		typeof(DefaultPropertyFieldDelegate),
		null,
		typeof(EditorGUI).GetMethod("DefaultPropertyField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
	);
	private static IEnumerable<SerializedProperty> EnumerateChildProperties(SerializedProperty property)
	{
		var iterator = property.Copy();
		var end = iterator.GetEndProperty();
		if (iterator.NextVisible(enterChildren: true))
		{
			do
			{
				if (SerializedProperty.EqualContents(iterator, end))
					yield break;

				yield return iterator;
			}
			while (iterator.NextVisible(enterChildren: false));
		}
	}

	private static bool DefaultPropertyField(Rect position, SerializedProperty property,GUIContent label) => s_DefaultPropertyField(position, property, label);
	
}
