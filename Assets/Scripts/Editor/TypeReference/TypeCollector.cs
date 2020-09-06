﻿namespace TypeReferences.Editor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using TypeReferences;
    using UnityEngine;

    /// <summary>
    /// A class responsible for collecting types according to given filters and conditions.
    /// </summary>
    public static class TypeCollector
    {
        public static List<Assembly> GetAssembliesTypeHasAccessTo(Type type)
        {
			Assembly typeAssembly = type == null ? Assembly.Load("Assembly-CSharp") : type.Assembly;
			List<Assembly> assemblies = new List<Assembly> { typeAssembly };

            assemblies.AddRange(
                typeAssembly.GetReferencedAssemblies()
                    .Select(Assembly.Load));

            return assemblies;
        }

        public static List<Type> GetFilteredTypesFromAssemblies(
            IEnumerable<Assembly> assemblies,
            TypeOptionsAttribute filter)
        {
			List<Type> types = new List<Type>();

            foreach (Assembly assembly in assemblies)
                types.AddRange(GetFilteredTypesFromAssembly(assembly, filter));

            return types;
        }

        public static List<Type> GetVisibleTypesFromAssemblies(IEnumerable<Assembly> assemblies)
        {
			List<Type> types = new List<Type>();

            foreach (Assembly assembly in assemblies)
            {
                types.AddRange(GetVisibleTypesFromAssembly(assembly));
            }

            return types;
        }

        public static Assembly TryLoadAssembly(string assemblyName)
        {
            if (string.IsNullOrEmpty(assemblyName))
                return null;

            Assembly assembly = null;

            try
            {
                assembly = Assembly.Load(assemblyName);
            }
            catch (FileNotFoundException)
            {
                Debug.LogError($"{assemblyName} was not found. It will not be added to dropdown.");
            }
            catch (FileLoadException)
            {
                Debug.LogError($"Failed to load {assemblyName}. It will not be added to dropdown.");
            }
            catch (BadImageFormatException)
            {
                Debug.LogError($"{assemblyName} is not a valid assembly. It will not be added to dropdown.");
            }

            return assembly;
        }

        private static IEnumerable<Type> GetFilteredTypesFromAssembly(
            Assembly assembly,
            TypeOptionsAttribute filter)
        {
            return GetVisibleTypesFromAssembly(assembly)
                .Where(type => type.IsVisible && FilterConstraintIsSatisfied(filter, type));
        }

        private static IEnumerable<Type> GetVisibleTypesFromAssembly(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes().Where(type => type.IsVisible);
            }
            catch (ReflectionTypeLoadException e)
            {
                Debug.LogError($"Types could not be extracted from assembly {assembly}: {e.Message}");
                return new Type[0];
            }
        }

        private static bool FilterConstraintIsSatisfied(TypeOptionsAttribute filter, Type type)
        {
            if (filter == null)
                return true;

            return filter.MatchesRequirements(type);
        }
    }
}