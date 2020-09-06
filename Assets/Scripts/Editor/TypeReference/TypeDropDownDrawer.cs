namespace TypeReferences.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using TypeReferences;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Draws expanded drop-down list of types.
    /// </summary>
    internal class TypeDropDownDrawer
    {
        private readonly Type _selectedType;
        private readonly TypeOptionsAttribute _constraints;
        private readonly Type _declaringType;
        private LimitedGenericMenu _menu;

        public TypeDropDownDrawer(string typeName, TypeOptionsAttribute constraints, Type declaringType)
        {
			this._selectedType = CachedTypeReference.GetType(typeName);
			this._constraints = constraints;
			this._declaringType = declaringType;
        }

        public void Draw(Rect position)
        {
			this._menu = new LimitedGenericMenu();

			this.AddNoneElementIfNotExcluded();

			Grouping grouping = this._constraints?.Grouping ?? TypeOptionsAttribute.DefaultGrouping;
			this.AddTypesToMenu(grouping);

			this._menu.DropDown(position);
        }

        private void AddNoneElementIfNotExcluded()
        {
            bool excludeNone = this._constraints?.ExcludeNone ?? false;
            if (excludeNone)
                return;

			this._menu.AddItem(
                new GUIContent(TypeReference.NoneElement),
				this._selectedType == null,
                CachedTypeReference.SelectedTypeName,
                null);

			this._menu.AddLineSeparator();
        }

        private void AddTypesToMenu(Grouping typeGrouping)
        {
			SortedList<string, Type> types = this.GetFilteredTypes();

			this.AddIncludedTypes(types);

            foreach (KeyValuePair<string, Type> nameTypePair in types)
            {
                string menuLabel = TypeNameFormatter.Format(nameTypePair.Value, typeGrouping);
				this.AddLabelIfNotEmpty(menuLabel, nameTypePair.Value);
            }
        }

        private SortedList<string, Type> GetFilteredTypes()
        {
			List<Assembly> typeRelatedAssemblies = TypeCollector.GetAssembliesTypeHasAccessTo(this._declaringType);

            if (this._constraints?.IncludeAdditionalAssemblies != null)
				this.IncludeAdditionalAssemblies(typeRelatedAssemblies);

			List<Type> filteredTypes = TypeCollector.GetFilteredTypesFromAssemblies(
                typeRelatedAssemblies,
				this._constraints);

			SortedList<string, Type> sortedTypes = new SortedList<string, Type>(filteredTypes.ToDictionary(type => type.FullName));

            return sortedTypes;
        }

        private void AddIncludedTypes(IDictionary<string, Type> types)
        {
			Type[] typesToInclude = this._constraints?.IncludeTypes;
            if (typesToInclude == null)
                return;

            foreach (Type typeToInclude in this._constraints?.IncludeTypes)
            {
                if (typeToInclude != null)
                    types.Add(typeToInclude.FullName ?? string.Empty, typeToInclude);
            }
        }

        private void AddLabelIfNotEmpty(string menuLabel, Type type)
        {
            if (string.IsNullOrEmpty(menuLabel))
                return;

			GUIContent content = new GUIContent(menuLabel);
			this._menu.AddItem(content, this._selectedType == type, CachedTypeReference.SelectedTypeName, type);
        }

        private void IncludeAdditionalAssemblies(ICollection<Assembly> typeRelatedAssemblies)
        {
            foreach (string assemblyName in this._constraints.IncludeAdditionalAssemblies)
            {
				Assembly additionalAssembly = TypeCollector.TryLoadAssembly(assemblyName);
                if (additionalAssembly == null)
                    continue;

                if ( ! typeRelatedAssemblies.Contains(additionalAssembly))
                    typeRelatedAssemblies.Add(additionalAssembly);
            }
        }
    }

    internal class LimitedGenericMenu
    {
        private readonly GenericMenu _menu;
        private int _itemCount;
        private const int ItemLimit = 1000;
        private bool _itemLimitAlreadyReached;

        public LimitedGenericMenu()
        {
			this._menu = new GenericMenu();
        }

        public void DropDown(Rect position)
        {
			this._menu.DropDown(position);
        }

        public void AddItem(GUIContent content, bool on, GenericMenu.MenuFunction2 func, object userData)
        {
            if (this._itemLimitAlreadyReached)
                return;

            if (this._itemCount == ItemLimit)
            {
                Debug.LogWarning("Item limit has been reached. Only the first 1000 items are shown in the list.");
				this._itemLimitAlreadyReached = true;
                return;
            }

			this._menu.AddItem(content, on, func, userData);
			this._itemCount++;
        }

        public void AddLineSeparator()
        {
			this._menu.AddSeparator(string.Empty);
        }
    }
}