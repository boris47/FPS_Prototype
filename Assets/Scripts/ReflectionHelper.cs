using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Component = UnityEngine.Component;
using Object = UnityEngine.Object;
using System.Reflection;

public interface IOnSceneLoadEvents
{
	void OnBeforeSceneActivation();

	void OnAfterSceneActivation();

	void OnAfterLoadedData();
}


 
public static class ReflectionHelper
{
//	private static Dictionary<Type, List<Type>> _classToComponentMapping = new Dictionary<Type, List<Type>>();
	private static Dictionary<Type, List<Type>> _interfaceToComponentMapping = new Dictionary<Type, List<Type>>();
	private static Dictionary<Type, List<Type>> _classToComponentMapping = new Dictionary<Type, List<Type>>();
	private static Type[] _allTypes;


	//////////////////////////////////////////////////////////////////////////
	// CONSTRUCTOR
	static ReflectionHelper()
	{
		_allTypes = GetAllTypes();

#region TODO
		/*
		// Classes

		// Assembly-CSharp.dll
		// Assembly-CSharp-firstpass.dll
		// Assembly-CSharp-Editor.dll
		// Assembly-CSharp-Editor-firstpass.dll
		
		var result = _allTypes
			.Where( t => {
				string typeName = t.ToString().ToLower();
				string typeModule = t.ToString().ToLower();
				return t.IsClass 
//				&& typeModule.StartsWith( "system" )
				&&	!( typeName.Contains( "unity" ) || typeName.Contains( "system." )
					 || typeName.Contains( "mono." ) || typeName.Contains( "icsharpcode." )
					 || typeName.Contains( "nsubstitute" ) || typeName.Contains( "nunit." ) || typeName.Contains( "microsoft." )
					 || typeName.Contains( "boo." ) || typeName.Contains( "serializ" ) || typeName.Contains( "json" )
					 || typeName.Contains( "log." ) || typeName.Contains( "logging" ) || typeName.Contains( "test" )
					 || typeName.Contains( "editor" ) || typeName.Contains( "debug" ) )

					&& !typeName.Contains( "object" ) 
					&& !typeName.Contains( "anonymous" )
					&& !typeName.StartsWith( "interop" )
					&& !typeName.StartsWith( "assembly" )
					&& !typeName.Contains( "assemblyref" )
					&& !typeName.Contains( "consts" )
					&& !typeName.Contains( "locale" )
					&& !typeName.Contains( "sr" )
					&& !typeName.StartsWith( "internal" )
					&& !typeName.StartsWith( "xammac" )
					&& !typeName.StartsWith( "unityeditorinternal" )
					&& !typeName.StartsWith( "jetbrains" )
					&& !typeName.StartsWith( "ms" )
					&& !typeName.StartsWith( "syntaxtree" )
					&& !typeName.Contains( "excss" )
					&& !typeName.StartsWith( "<" )
					&& !typeName.StartsWith( "ProjectNull" )
					;
			})
			.Select( t => t.Module.ToString() );
		System.IO.File.WriteAllText( "AllClasses.txt", string.Join( Environment.NewLine, result ) );
		*/
		/*
		_allTypes
			.Where( t => t.IsClass )
			.Where( curClass => {
				string typeName = curClass.ToString().ToLower();
				return 
					!( typeName.Contains( "unity" ) || typeName.Contains( "system." )
					 || typeName.Contains( "mono." ) || typeName.Contains( "icsharpcode." )
					 || typeName.Contains( "nsubstitute" ) || typeName.Contains( "nunit." ) || typeName.Contains( "microsoft." )
					 || typeName.Contains( "boo." ) || typeName.Contains( "serializ" ) || typeName.Contains( "json" )
					 || typeName.Contains( "log." ) || typeName.Contains( "logging" ) || typeName.Contains( "test" )
					 || typeName.Contains( "editor" ) || typeName.Contains( "debug" ) )

					&& !typeName.Contains( "object" ) 
					&& !typeName.Contains( "anonymous" )
					&& !typeName.StartsWith( "interop" )
					&& !typeName.Contains( "assemblyref" )
					&& !typeName.Contains( "consts" )
					&& !typeName.Contains( "locale" )
					&& !typeName.Contains( "sr" )
					&& !typeName.StartsWith( "internal" )
					&& !typeName.StartsWith( "xammac" )
					&& !typeName.StartsWith( "unityeditorinternal" )
					&& !typeName.StartsWith( "jetbrains" )
					&& !typeName.StartsWith( "ms" )
					&& !typeName.StartsWith( "syntaxtree" )
					;
			})
			.ToList()
			.ForEach(curClass => {

				IList<Type> typesInherited = GetTypesInherited( curClass );
				if ( typesInherited.Count > 0 )
				{
					List<Type> componentsList = new List<Type>();

					foreach (Type curType in typesInherited)
					{
						//Ignore non-component classes
						if ( !( typeof(Component) == curType || curType.IsSubclassOf( typeof(Component) ) ) )
							continue;

						if ( !componentsList.Contains( curType ) )
							componentsList.Add( curType );
					}

					_classToComponentMapping.Add( curClass, componentsList );
				}
			}
		);
		*/
#endregion

		// Interfaces
		foreach ( Type currentType in _allTypes)
		{
			string typeName = currentType.ToString().ToLower();

			//Skip system interfaces
			if ( typeName.Contains( "unity" ) || typeName.Contains( "system." )
				 || typeName.Contains( "mono." ) || typeName.Contains( "icsharpcode." )
				 || typeName.Contains( "nsubstitute" ) || typeName.Contains( "nunit." ) || typeName.Contains( "microsoft." )
				 || typeName.Contains( "boo." ) || typeName.Contains( "serializ" ) || typeName.Contains( "json" )
				 || typeName.Contains( "log." ) || typeName.Contains( "logging" ) || typeName.Contains( "test" )
				 || typeName.Contains( "editor" ) || typeName.Contains( "debug" ) )
				continue;


			if (currentType.IsClass || currentType.IsAbstract)
			{
				List<Type> componentsList = new List<Type>();
				List<Type> typesImplementingTheInterface = GetTypesInherited(currentType);
				foreach (Type curType in typesImplementingTheInterface)
				{
					//Skip interfaces
					if (curType.IsInterface)
						continue;

					//Ignore non-component classes
	//				if (!(typeof(Component) == curType || curType.IsSubclassOf(typeof(Component))))
	//					continue;

					if (!componentsList.Contains(curType))
						componentsList.Add(curType);
				}
				_classToComponentMapping.Add(currentType, componentsList);
			}


			if (currentType.IsInterface)
			{
				List<Type> typesImplementingTheInterface = GetTypesInherited(currentType);
		//		if (typesImplementingTheInterface.Count <= 0)
		//			continue;

				List<Type> componentsList = new List<Type>();
				foreach (Type curType in typesImplementingTheInterface)
				{
					//Skip interfaces
					if (curType.IsInterface)
						continue;

					//Ignore non-component classes
	//				if (!(typeof(Component) == curType || curType.IsSubclassOf(typeof(Component))))
	//					continue;

					if (!componentsList.Contains(curType))
						componentsList.Add(curType);
				}
				_interfaceToComponentMapping.Add(currentType, componentsList);
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public static List<T> FindObjectsImplementingInterface<T>() where T : class
	{
		List<T> resultList = new List<T>();

		List<Type> types = _interfaceToComponentMapping[ typeof(T) ];
		if ( null == types || types.Count <= 0 )
		{
			return resultList;
		}

		foreach ( Type curType in types )
		{
			Object[] objects = Object.FindObjectsOfType( curType );
			if ( null == objects || objects.Length <= 0 )
				continue;

			List<T> tList = new List<T>();
			foreach ( Object curObj in objects )
			{
				if (!(curObj is T curObjAsT))
				{
					Debug.LogError("Unable to cast '" + curObj.GetType() + "' to '" + typeof(T) + "'");
					continue;
				}
				tList.Add( curObjAsT );
			}
			resultList.AddRange( tList );
		}

		return resultList;
	}


	public static List<T> SelectImplementInterface<T>(Object[] objects) where T : class
	{
		return objects.Where( obj => obj is T ).ToList().ConvertAll( obj => obj as T );
	}



	//////////////////////////////////////////////////////////////////////////
	public static T FindObject<T>() where T : class
	{
		IList<T> list = FindObjectsImplementingInterface<T>();

		return list[0];
	}



	//////////////////////////////////////////////////////////////////////////
	public static List<Type> FindInerithedFromInterface<T>(bool bInlcludeAbstracts)
	{
		return FindInerithedFromInterface(typeof(T), bInlcludeAbstracts);
	}

	public static List<Type> FindInerithedFromInterface(Type type, bool bInlcludeAbstracts)
	{
		List<Type> types = new List<Type>();
		if (_interfaceToComponentMapping.TryGetValue(type, out types))
		{
			// Remove abstract because usually is not needed
			if (!bInlcludeAbstracts)
			{
				types.RemoveAll(t => t.IsAbstract);
			}
		}
		return types;
	}

	//////////////////////////////////////////////////////////////////////////
	public static List<Type> FindInerithedFromClass<T>(bool bInlcludeAbstracts) where T : class
	{
		return FindInerithedFromClass(typeof(T), bInlcludeAbstracts);
	}

	//////////////////////////////////////////////////////////////////////////
	public static List<Type> FindInerithedFromClass(Type type, bool bInlcludeAbstracts)
	{
		List<Type> types = new List<Type>();
		if (_classToComponentMapping.TryGetValue(type, out types))
		{
			// Remove abstract because usually is not needed
			if (!bInlcludeAbstracts)
			{
				types.RemoveAll(t => t.IsAbstract);
			}
		}
		return types;
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Given a types collection, search a method called as specified on it or its base class, then call it
	/// </summary>
	/// <param name="types"></param>
	/// <param name="methodName"></param>
	/// <param name="IsBaseMethod"> Specify if this method has to searched on base class or not</param>
	public	static void CallMethodOnTypes( List<Type> types, string methodName, bool IsBaseMethod )
	{
		if ( types != null && types.Count > 0 )
		{
			foreach( Type type in types )
			{
				Type typeToUse = IsBaseMethod ? type.BaseType : type;
				MethodInfo method = typeToUse.GetMethod( methodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance );
				if ( method != null )
				{
					if ( method.IsStatic )
					{
						method.Invoke( null, null );
						continue;
					}

					object instance = typeToUse.GetField( "m_Instance", BindingFlags.NonPublic | BindingFlags.Static )?.GetValue( null );
					if ( instance != null )
					{
						method.Invoke( instance, null );
					}
				}
			}
		}
	}



	//////////////////////////////////////////////////////////////////////////
	private static Type[] GetAllTypes()
	{
		List<Type> res = new List<Type>();
		foreach ( Assembly assembly in AppDomain.CurrentDomain.GetAssemblies() )
		{
			res.AddRange( assembly.GetTypes() );
		}

		return res.ToArray();
	}



	//////////////////////////////////////////////////////////////////////////
	private static List<Type> GetTypesInherited( Type type )
	{
		//Caching
		if ( null == _allTypes )
		{
			_allTypes = GetAllTypes();
		}

		List<Type> res = new List<Type>();

		foreach (Type curType in _allTypes)
		{
			if ( !( type.IsAssignableFrom( curType ) && curType.IsSubclassOf( typeof( Component ) ) ) )
				continue;

			res.Add( curType );
		}

		return res;
	}

}
	