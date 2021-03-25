﻿using UnityEngine;
using UnityEditor;


// Custom property inspector of type 'SurfaceManager.RegisteredMaterial'
[CustomPropertyDrawer(typeof(SurfaceManager.RegisteredTexture))]
public class SurfaceManager_MaterialDrawer : PropertyDrawer
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
		/* TODO Restor Editor Part
		surfaceIndex.intValue = EditorGUI.Popup(position, surfaceIndex.intValue, SurfaceManager.Instance?.GetAllSurfaceNames() ?? new string[0]);
		*/
	}

	//
	public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => 32f;
}

[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		return EditorGUI.GetPropertyHeight(property, label, true);
	}

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		GUI.enabled = false;
		{
			EditorGUI.PropertyField(position, property, label, true);
		}
		GUI.enabled = true;
	}
}


[CustomPropertyDrawer(typeof(ObsoleteInspectorAttribute))]
class ObsoleteInspectorDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		System.ObsoleteAttribute obsoleteAttribute = ( System.ObsoleteAttribute ) System.Attribute.GetCustomAttribute(fieldInfo, typeof( System.ObsoleteAttribute ) );
		EditorStyles.label.richText = true;
		EditorGUI.PropertyField(position, property, new GUIContent("<color=red><i>" + label.text + "</i></color>", "**OBSOLETE**\n\n" + obsoleteAttribute.Message), true);
		EditorStyles.label.richText = false;
	}
}
