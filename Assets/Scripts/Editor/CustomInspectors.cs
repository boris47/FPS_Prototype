using UnityEngine;
using UnityEditor;


// Custom property inspector of type 'RegisteredMaterial'
[CustomPropertyDrawer(typeof(RegisteredMaterial))]
public class MaterialDrawer : PropertyDrawer {

	SurfaceManager surfManag = null;

	//
	public override void OnGUI (Rect position, SerializedProperty property, GUIContent label)
	{
		if ( surfManag == null )
		{
			surfManag = GameObject.FindObjectOfType<SurfaceManager>();
			return;
		}

		SerializedProperty texture = property.FindPropertyRelative("texture");
		SerializedProperty surfaceIndex = property.FindPropertyRelative("surfaceIndex");

		// Showing labels for the fields
		GUI.Label(position, "Texture");
		position.x = position.width / 2f;
		GUI.Label(position, "GroundType");

		// Set the new rect 
		position.height = 16f;
		position.y = position.yMax;
		position.x = 0f;
		position.width /= 2.2f;

		// Draw the texture field
		EditorGUI.PropertyField(position, texture, GUIContent.none);

		// Draw the type field
		position.x = position.xMax;
		surfaceIndex.intValue = EditorGUI.Popup(position, surfaceIndex.intValue, surfManag.GetAllSurfaceNames());
	}

	//
	public override float GetPropertyHeight( SerializedProperty property, GUIContent label )
	{
		return 32f;
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
