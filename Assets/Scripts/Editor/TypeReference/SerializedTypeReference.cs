namespace TypeReferences.Editor
{
    using System;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// A class that gives access to serialized properties inside <see cref="TypeReference"/>.
    /// </summary>
    internal class SerializedTypeReference
    {
        private readonly SerializedProperty _typeNameProperty;
        private readonly SerializedProperty _guidProperty;
        private readonly SerializedProperty _guidAssignmentFailedProperty;
        private readonly TypeReference _typeRefInstance;

        public SerializedTypeReference(SerializedProperty typeReferenceProperty)
        {
			this._typeNameProperty = typeReferenceProperty.FindPropertyRelative(TypeReference.NameOfTypeNameField);
			this._guidProperty = typeReferenceProperty.FindPropertyRelative(TypeReference.NameOfGuidField);
			this._guidAssignmentFailedProperty = typeReferenceProperty.FindPropertyRelative(nameof(TypeReference.GuidAssignmentFailed));

			this.SetGuidIfAssignmentFailed();
        }

        public string TypeNameAndAssembly
        {
            get => this._typeNameProperty.stringValue;
            set
            {
				this._typeNameProperty.stringValue = value;
				this._guidProperty.stringValue = this.GetClassGuidFromTypeName(value);
            }
        }

        private bool GuidAssignmentFailed
        {
            get => this._guidAssignmentFailedProperty.boolValue;
            set => this._guidAssignmentFailedProperty.boolValue = value;
        }

        private string GUID
        {
            get => this._guidProperty.stringValue;
            set => this._guidProperty.stringValue = value;
        }

        public bool TypeNameHasMultipleDifferentValues => this._typeNameProperty.hasMultipleDifferentValues;

        private void SetGuidIfAssignmentFailed()
        {
            if ( !this.GuidAssignmentFailed || string.IsNullOrEmpty(this.TypeNameAndAssembly))
                return;

			this.GuidAssignmentFailed = false;
			this.GUID = this.GetClassGuidFromTypeName(this.TypeNameAndAssembly);
        }

        /// <summary>
        /// Try finding the class type given the GUID of the file where it is located.
        /// If found, change the ClassTypeReference._typeNameAndAssembly value.
        /// </summary>
        public void TryUpdatingTypeUsingGUID()
        {
            if (this.GUID == string.Empty)
                return;

            string assetPath = AssetDatabase.GUIDToAssetPath(this.GUID);
			MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath);
            if (script == null)
                return;

            Type type = script.GetClass();
            string previousValue = this.TypeNameAndAssembly;
			this.TypeNameAndAssembly = TypeReference.GetTypeNameAndAssembly(type);
            Debug.LogFormat(
                "Type reference has been updated from '{0}' to '{1}'.",
                previousValue,
				this.TypeNameAndAssembly);
        }

        private string GetClassGuidFromTypeName(string typeName)
        {
			Type type = Type.GetType(typeName);
            return TypeReference.GetClassGUID(type);
        }
    }
}