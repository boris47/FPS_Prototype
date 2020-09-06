namespace TypeReferences.Editor
{
    using TypeReferences;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Custom property drawer for <see cref="TypeReference"/> properties.
    /// </summary>
    [CustomPropertyDrawer(typeof(TypeReference))]
    [CustomPropertyDrawer(typeof(TypeOptionsAttribute), true)]
    public sealed class TypeReferencePropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorStyles.popup.CalcHeight(GUIContent.none, 0);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position = ExcludeLabelFromPositionIfNecessary(position, label);
			this.DrawTypeReferenceField(position, property);
        }

        private static Rect ExcludeLabelFromPositionIfNecessary(Rect position, GUIContent label)
        {
            if (label == null || label == GUIContent.none)
                return position;

			Rect positionExcludingLabel = EditorGUI.PrefixLabel(position, label);
            return positionExcludingLabel;
        }

        private void DrawTypeReferenceField(Rect position, SerializedProperty property)
        {
			TypeOptionsAttribute constraints = this.attribute as TypeOptionsAttribute;
			SerializedTypeReference serializedTypeRef = new SerializedTypeReference(property);

			TypeDropDownDrawer dropDown = new TypeDropDownDrawer(
                serializedTypeRef.TypeNameAndAssembly,
                constraints,
				this.fieldInfo?.DeclaringType);

			TypeFieldDrawer fieldDrawer = new TypeFieldDrawer(serializedTypeRef, position, dropDown);

            fieldDrawer.Draw();
        }
    }
}
