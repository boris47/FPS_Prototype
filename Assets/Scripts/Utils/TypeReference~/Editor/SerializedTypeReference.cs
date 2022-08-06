﻿namespace TypeReferences.Editor
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
			_typeNameProperty = typeReferenceProperty.FindPropertyRelative(TypeReference.NameOfTypeNameField);
			_guidProperty = typeReferenceProperty.FindPropertyRelative(TypeReference.NameOfGuidField);
			_guidAssignmentFailedProperty = typeReferenceProperty.FindPropertyRelative(nameof(TypeReference.GuidAssignmentFailed));

			SetGuidIfAssignmentFailed();
        }

        public string TypeNameAndAssembly
        {
            get => _typeNameProperty.stringValue;
            set
            {
				_typeNameProperty.stringValue = value;
				_guidProperty.stringValue = GetClassGuidFromTypeName(value);
            }
        }

        private bool GuidAssignmentFailed
        {
            get => _guidAssignmentFailedProperty.boolValue;
            set => _guidAssignmentFailedProperty.boolValue = value;
        }

        private string GUID
        {
            get => _guidProperty.stringValue;
            set => _guidProperty.stringValue = value;
        }

        public bool TypeNameHasMultipleDifferentValues => _typeNameProperty.hasMultipleDifferentValues;

        private void SetGuidIfAssignmentFailed()
        {
            if ( !GuidAssignmentFailed || string.IsNullOrEmpty(TypeNameAndAssembly))
                return;

			GuidAssignmentFailed = false;
			GUID = GetClassGuidFromTypeName(TypeNameAndAssembly);
        }

        /// <summary>
        /// Try finding the class type given the GUID of the file where it is located.
        /// If found, change the ClassTypeReference._typeNameAndAssembly value.
        /// </summary>
        public void TryUpdatingTypeUsingGUID()
        {
            if (GUID == string.Empty)
                return;

            string assetPath = AssetDatabase.GUIDToAssetPath(GUID);
			MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath);
            if (script == null)
                return;

            Type type = script.GetClass();
            string previousValue = TypeNameAndAssembly;
			TypeNameAndAssembly = TypeReference.GetTypeNameAndAssembly(type);
            Debug.LogFormat(
                "Type reference has been updated from '{0}' to '{1}'.",
                previousValue,
				TypeNameAndAssembly);
        }

        private string GetClassGuidFromTypeName(string typeName)
        {
			Type type = Type.GetType(typeName);
            return TypeReference.GetClassGUID(type);
        }
    }
}