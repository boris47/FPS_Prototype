
using UnityEngine;
using System.Collections.Generic;
using System.Linq;


#if UNITY_EDITOR

using UnityEditor;

[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
	public override float GetPropertyHeight( SerializedProperty property, GUIContent label )
	{
		return EditorGUI.GetPropertyHeight(property, label, true);
	}

	public override void OnGUI( Rect position, SerializedProperty property, GUIContent label )
	{
		GUI.enabled = false;
		{
			EditorGUI.PropertyField( position, property, label, true );
		}
		GUI.enabled = true;
	}

}

/////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////


/////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////
////////////////   READ ONLY ATTRIBUTE   ////////////////
/////////////////       [ReadOnly]      /////////////////
public class ReadOnlyAttribute : PropertyAttribute
{}


/////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////
/////////////////   OBSOLE ATTRIBUTE   //////////////////
//////////////     [ObsoleteInspector]     //////////////
public class ObsoleteInspectorAttribute : PropertyAttribute {
}









/*
/// <summary> Custom property drawer for <see cref="ClassTypeReference"/> properties. </summary>
[CustomPropertyDrawer(typeof(ClassTypeReference))]
[CustomPropertyDrawer(typeof(ClassTypeConstraintAttribute), true)]
public sealed class ClassTypeReferencePropertyDrawer : PropertyDrawer
{
	private static readonly string COMMAND_NAME = "TypeReferenceUpdated";
	private static readonly int s_ControlHint = typeof(ClassTypeReferencePropertyDrawer).GetHashCode();
	private static readonly GenericMenu.MenuFunction2 s_OnSelectedTypeName = (object userData) =>
	{
		System.Type selectedType = userData as System.Type;

		s_SelectedClassRef = GetClassRef(selectedType);

		Event typeReferenceUpdatedEvent = EditorGUIUtility.CommandEvent(COMMAND_NAME);
		EditorWindow.focusedWindow.SendEvent(typeReferenceUpdatedEvent);
	};

	private static GUIContent s_TempContent = new GUIContent();
	private static int s_SelectionControlID;
	private static string s_SelectedClassRef;


	private static string DrawTypeSelectionControl(Rect position, GUIContent label, string classRef, ClassTypeConstraintAttribute filter)
	{
		if (label != null && label != GUIContent.none)
		{
			position = EditorGUI.PrefixLabel(position, label);
		}

		bool triggerDropDown = false;
		int controlID = GUIUtility.GetControlID(s_ControlHint, FocusType.Keyboard, position);
		switch (Event.current.GetTypeForControl(controlID))
		{
			case EventType.ExecuteCommand:
			{
				if (Event.current.commandName == COMMAND_NAME)
				{
					if (s_SelectionControlID == controlID)
					{
						if (classRef != s_SelectedClassRef)
						{
							classRef = s_SelectedClassRef;
							GUI.changed = true;
						}

						s_SelectionControlID = 0;
						s_SelectedClassRef = null;
					}
				}
				break;
			}
			case EventType.MouseDown:
			{
				if (GUI.enabled && position.Contains(Event.current.mousePosition))
				{
					GUIUtility.keyboardControl = controlID;
					triggerDropDown = true;
					Event.current.Use();
				}
				break;
			}
			case EventType.KeyDown:
			{
				if (GUI.enabled && GUIUtility.keyboardControl == controlID)
				{
					if (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.Space)
					{
						triggerDropDown = true;
						Event.current.Use();
					}
				}
				break;
			}
			case EventType.Repaint:
			{
				// Remove assembly name from content of popup control.
				string[] classRefParts = classRef.Split(',');
				string trimmed = classRefParts[0].Trim();
				if (trimmed == "")
				{
					s_TempContent.text = "(None)";
				}
				else if (System.Type.GetType(classRef) == null)
				{
					s_TempContent.text += " {Missing}";
				}
				else
				{
					s_TempContent.text = trimmed;
				}

				EditorStyles.popup.Draw(position, s_TempContent, controlID);
				break;
			}
		}

		if (triggerDropDown)
		{
			s_SelectionControlID = controlID;
			s_SelectedClassRef = classRef;

			List<System.Type> filteredTypes = new List<System.Type>();
			if (filter is ClassExtendsAttribute classExtend)
			{
				filteredTypes = ReflectionHelper.FindInerithedFromClass(classExtend.BaseType, classExtend.AllowAbstract);
			}
			if (filter is ClassImplementsAttribute classImplement)
			{
				filteredTypes = ReflectionHelper.FindInerithedFromInterface(classImplement.InterfaceType, classImplement.AllowAbstract);
			}

			DisplayDropDown(position, filteredTypes, System.Type.GetType(classRef));
		}

		return classRef;
	}

	private static void DisplayDropDown(Rect position, List<System.Type> types, System.Type selectedType)
	{
		GenericMenu menu = new GenericMenu();

		menu.AddItem(new GUIContent("(None)"), selectedType == null, s_OnSelectedTypeName, null);
		menu.AddSeparator("");

		for (int i = 0; i < types.Count; ++i)
		{
			System.Type type = types[i];
			string menuLabel = type.FullName;
			if (!string.IsNullOrEmpty(menuLabel))
			{
				GUIContent content = new GUIContent(menuLabel);
				menu.AddItem(content, type == selectedType, s_OnSelectedTypeName, type);
			}
		}

		menu.DropDown(position);
	}

	private static string GetClassRef(System.Type type)
	{
		return type != null
			? type.FullName + ", " + type.Assembly.GetName().Name
			: "";
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		return EditorStyles.popup.CalcHeight(GUIContent.none, 0);
	}

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		SerializedProperty classRef = property.FindPropertyRelative("_classRef");
		classRef.stringValue = DrawTypeSelectionControl(position, label, classRef.stringValue, this.attribute as ClassTypeConstraintAttribute);
	}
}


/// <summary> Base class for class selection constraints that can be applied when selecting a <see cref="ClassTypeReference"/> with the Unity inspector. </summary>
public abstract class ClassTypeConstraintAttribute : PropertyAttribute
{
	/// <summary> Gets or sets whether abstract classes can be selected from drop-down. Defaults to a value of <c>false</c> unless explicitly specified. </summary>
	public bool AllowAbstract { get; set; } = false;
}

/// <summary> Constraint that allows selection of classes that extend a specific class when selecting a <see cref="ClassTypeReference"/> with the Unity inspector. </summary>
[System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false)]
public sealed class ClassExtendsAttribute : ClassTypeConstraintAttribute
{
	/// <summary> Initializes a new instance of the <see cref="ClassExtendsAttribute"/> class. </summary>
	/// <param name="baseType">Type of class that selectable classes must derive from.</param>
	public ClassExtendsAttribute(System.Type baseType)
	{
		this.BaseType = baseType;
	}

	/// <summary> Gets the type of class that selectable classes must derive from. </summary>
	public System.Type BaseType { get; private set; }
}

/// <summary> Constraint that allows selection of classes that implement a specific interface when selecting a <see cref="ClassTypeReference"/> with the Unity inspector. </summary>
[System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false)]
public sealed class ClassImplementsAttribute : ClassTypeConstraintAttribute
{
	/// <summary> Initializes a new instance of the <see cref="ClassImplementsAttribute"/> class. </summary>
	/// <param name="interfaceType">Type of interface that selectable classes must implement.</param>
	public ClassImplementsAttribute(System.Type interfaceType)
	{
		this.InterfaceType = interfaceType;
	}

	/// <summary> Gets the type of interface that selectable classes must implement. </summary>
	public System.Type InterfaceType { get; private set; }
}
*/



/*
[CustomPropertyDrawer(typeof(SubClassOf<Entity>))]
public sealed class ClassTypeReferenceTemplatedPropertyDrawer : PropertyDrawer
{
	private static readonly string FIELD_NAME = "m_TypeName";

//	private string[] m_TypeNames = null;

	// surfaceIndex.intValue = EditorGUI.Popup(position, surfaceIndex.intValue, this.surfManag.GetAllSurfaceNames());
	// this.m_DefinedSurfaces.Select(surface => surface.name).ToArray();

	private static string GetClassRef(ref Rect position, string typeName, GUIContent label)
	{
		if (label != null && label != GUIContent.none)
		{
			position = EditorGUI.PrefixLabel(position, label);
		}

		System.Type type = System.Type.GetType(typeName);
		if (string.IsNullOrEmpty(typeName) || type == null)
		{
			return null;
		}

		string[] typeNames = null;
		if (type.IsClass)
		{
			typeNames = ReflectionHelper.FindInerithedFromClass(type, true).Select(t => t.FullName).ToArray();
		}

		if (type.IsInterface)
		{
			typeNames = ReflectionHelper.FindInerithedFromInterface(type, false).Select(t => t.FullName).ToArray();
		}

		if (typeNames == null)
		{
			return null;
		}

		int index = System.Array.FindIndex(typeNames, name => name == typeName);
		index = EditorGUI.Popup(position, index, typeNames);

		string newName = typeNames[index];
		return typeName;
	}

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		SerializedProperty typeName = property.FindPropertyRelative(FIELD_NAME);
		typeName.stringValue = ClassTypeReferenceTemplatedPropertyDrawer.GetClassRef(ref position, typeName.stringValue, label);
	}
}*/


#endif