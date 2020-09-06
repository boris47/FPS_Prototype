namespace TypeReferences.Editor
{
    using TypeReferences;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Draws a <see cref="TypeReference"/> field and handles control over the drop-down list.
    /// </summary>
    internal class TypeFieldDrawer
    {
        private const string MissingSuffix = " {Missing}";

        private readonly SerializedTypeReference _serializedTypeRef;
        private readonly TypeDropDownDrawer _dropDownDrawer;
        private Rect _position;
        private bool _triggerDropDown;

        public TypeFieldDrawer(
            SerializedTypeReference serializedTypeRef,
            Rect position,
            TypeDropDownDrawer dropDownDrawer)
        {
			this._serializedTypeRef = serializedTypeRef;
			this._position = position;
			this._dropDownDrawer = dropDownDrawer;
        }

        public void Draw()
        {
            bool valueToRestore = EditorGUI.showMixedValue;
            EditorGUI.showMixedValue = this._serializedTypeRef.TypeNameHasMultipleDifferentValues;
			this.DrawTypeSelectionControl();
            EditorGUI.showMixedValue = valueToRestore;
        }

        private void DrawTypeSelectionControl()
        {
            int controlID = GUIUtility.GetControlID(
                CachedTypeReference.ControlHint,
                FocusType.Keyboard,
				this._position);

			this._triggerDropDown = false;

			this.ReactToCurrentEvent(controlID);

            if ( !this._triggerDropDown)
                return;

            CachedTypeReference.SelectionControlID = controlID;
            CachedTypeReference.SelectedTypeNameAndAssembly = this._serializedTypeRef.TypeNameAndAssembly;

			this._dropDownDrawer.Draw(this._position);
        }

        private void ReactToCurrentEvent(int controlID)
        {
            switch (Event.current.GetTypeForControl(controlID))
            {
                case EventType.ExecuteCommand:
                    if (Event.current.commandName == CachedTypeReference.ReferenceUpdatedCommandName)
						this.OnTypeReferenceUpdated(controlID);

                    break;

                case EventType.MouseDown:
					this.OnMouseDown(controlID);
                    break;

                case EventType.KeyDown:
					this.OnKeyDown(controlID);
                    break;

                case EventType.Repaint:
					this.DrawFieldContent(controlID);
                    break;
            }
        }

        private void OnMouseDown(int controlID)
        {
            if (!GUI.enabled || !this._position.Contains(Event.current.mousePosition))
                return;

            GUIUtility.keyboardControl = controlID;
			this._triggerDropDown = true;
            Event.current.Use();
        }

        private void OnKeyDown(int controlID)
        {
            bool keyboardFocusIsOnElement = GUI.enabled && GUIUtility.keyboardControl == controlID;

            bool necessaryKeyIsDown =
                Event.current.keyCode == KeyCode.Return
                || Event.current.keyCode == KeyCode.Space;

            if (keyboardFocusIsOnElement && necessaryKeyIsDown)
            {
				this._triggerDropDown = true;
                Event.current.Use();
            }
        }

        private void DrawFieldContent(int controlID)
        {
            CachedTypeReference.FieldContent.text = this.GetTypeNameForField();
            EditorStyles.popup.Draw(this._position, CachedTypeReference.FieldContent, controlID);
        }

        private string GetTypeNameForField()
        {
			string[] typeParts = this._serializedTypeRef.TypeNameAndAssembly.Split(',');
            string typeName = typeParts[0].Trim();

            if (typeName == string.Empty)
            {
                typeName = TypeReference.NoneElement;
            }
            else if (CachedTypeReference.GetType(this._serializedTypeRef.TypeNameAndAssembly) == null)
            {
				this._serializedTypeRef.TryUpdatingTypeUsingGUID();

                if (CachedTypeReference.GetType(this._serializedTypeRef.TypeNameAndAssembly) == null)
                    typeName += MissingSuffix;
            }

            return typeName;
        }

        private void OnTypeReferenceUpdated(int controlID)
        {
            if (CachedTypeReference.SelectionControlID != controlID)
                return;

            if (this._serializedTypeRef.TypeNameAndAssembly != CachedTypeReference.SelectedTypeNameAndAssembly)
            {
				this._serializedTypeRef.TypeNameAndAssembly = CachedTypeReference.SelectedTypeNameAndAssembly;
                GUI.changed = true;
            }

            CachedTypeReference.SelectionControlID = 0;
            CachedTypeReference.SelectedTypeNameAndAssembly = null;
        }
    }
}