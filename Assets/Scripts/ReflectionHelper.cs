using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

 
public static class ReflectionHelper
{
	private		static		readonly	Dictionary<Type, Type[]>	m_InterfaceToComponentMapping	= new Dictionary<Type, Type[]>();
	private		static		readonly	Dictionary<Type, Type[]>	m_ClassToComponentMapping		= new Dictionary<Type, Type[]>();
	private		static		readonly	Type[]						m_AllTypes						= Assembly.GetCallingAssembly().GetTypes();



	//////////////////////////////////////////////////////////////////////////
	// CONSTRUCTOR
	static ReflectionHelper()
	{
		foreach (Type currentType in m_AllTypes)
		{
			// Classes
			if (currentType.IsClass || currentType.IsAbstract)
			{
				var result = GetTypesInherited(currentType)
					.Where(t => !t.IsInterface)
					.ToHashSet();
				
				m_ClassToComponentMapping.Add(currentType, result.ToArray());
			}

			// interfaces
			if (currentType.IsInterface)
			{
				var result = GetTypesInherited(currentType)
					.Where(t => !t.IsInterface)
					.ToHashSet();

				m_InterfaceToComponentMapping.Add(currentType, result.ToArray());
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary>  </summary>
	/// <param name="obj"></param>
	/// <param name="propertyName"></param>
	/// <param name="value"></param>
	/// <returns></returns>
	public static bool GetPropertyValue<T>(in object obj, in string propertyName, out T value)
	{
		value = default;
		bool logger_canTalk = GlobalManager.LoggerInstance?.CanTalk() ?? false;
		bool bResult = true;
		GlobalManager.LoggerInstance?.Silence();
		{
			PropertyInfo propertyInfo = obj.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			try
			{
				MethodInfo getMethodInfo = propertyInfo.GetGetMethod();
				if (getMethodInfo.IsNotNull())
				{
					value = (T)getMethodInfo.Invoke(obj, null);
				}
				else
				{
					value = (T)propertyInfo.GetValue(obj);
				}
			}
			catch (System.Exception) { bResult = false; }
		}
		if (logger_canTalk) GlobalManager.LoggerInstance?.Talk();
		return bResult;
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary>  </summary>
	/// <param name="obj"></param>
	/// <param name="propertyName"></param>
	/// <param name="value"></param>
	/// <returns></returns>
	public static bool SetPropertyValue(in object obj, in string propertyName, in object value)
	{
		bool logger_canTalk = GlobalManager.LoggerInstance?.CanTalk() ?? false;
		bool bResult = true;
		GlobalManager.LoggerInstance?.Silence();
		{
			PropertyInfo propertyInfo = obj.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			try
			{
				MethodInfo setMethodInfo = propertyInfo.GetSetMethod();
				if (setMethodInfo.IsNotNull())
				{
					setMethodInfo.Invoke(obj, new object[] { value });
				}
				else
				{
					propertyInfo.SetValue(obj, value);
				}
			}
			catch (System.Exception) { bResult = false; }
		}
		if (logger_canTalk) GlobalManager.LoggerInstance?.Talk();
		return bResult;
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary>  </summary>
	/// <param name="obj"></param>
	/// <param name="fieldName"></param>
	/// <param name="value"></param>
	/// <returns></returns>
	public static bool GetFieldValue<T>(in object obj, in string fieldName, out T value)
	{
		value = default;
		bool logger_canTalk = GlobalManager.LoggerInstance?.CanTalk() ?? false;
		bool bResult = true;
		GlobalManager.LoggerInstance?.Silence();
		{
			FieldInfo fieldInfo = obj.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			try
			{
				value = (T)fieldInfo.GetValue(obj);
			}
			catch (System.Exception) { bResult = false; }
		}
		if (logger_canTalk) GlobalManager.LoggerInstance?.Talk();
		return bResult;
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary>  </summary>
	/// <param name="obj"></param>
	/// <param name="fieldName"></param>
	/// <param name="value"></param>
	/// <returns></returns>
	public static bool SetFieldValue(in object obj, in string fieldName, in object value)
	{
		bool logger_canTalk = GlobalManager.LoggerInstance?.CanTalk() ?? false;
		bool bResult = true;
		GlobalManager.LoggerInstance?.Silence();
		{
			FieldInfo fieldInfo = obj.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			try
			{
				fieldInfo.SetValue(obj, value);
			}
			catch (System.Exception) { bResult = false; }
		}
		if (logger_canTalk) GlobalManager.LoggerInstance?.Talk();
		return bResult;
	}


	//////////////////////////////////////////////////////////////////////////
	public static T[] FindObjectsImplementingInterface<T>() where T : class
	{
		List<T> resultList = new List<T>();
		if (m_InterfaceToComponentMapping.TryGetValue(typeof(T), out Type[] types))
		{
			foreach (Type curType in types)
			{
				var objects = UnityEngine.Object.FindObjectsOfType(curType);
				resultList.AddRange(objects.Select(t => t as T));
			}
		}

		return resultList.ToArray();
	}


	//////////////////////////////////////////////////////////////////////////
	public static T[] SelectImplements<T>(in UnityEngine.Object[] objects) where T : class
	{
		return objects.Where(obj => obj is T).ToList().ConvertAll(obj => obj as T).ToArray();
	}


	//////////////////////////////////////////////////////////////////////////
	public static Type[] FindInerithedFromInterface<T>(bool bInlcludeAbstracts)
	{
		return FindInerithedFromInterface(typeof(T), bInlcludeAbstracts);
	}


	//////////////////////////////////////////////////////////////////////////
	public static Type[] FindInerithedFromInterface(Type interfaceType, bool bInlcludeAbstracts)
	{
		Type[] types = new Type[0];
		if (interfaceType.IsInterface && m_InterfaceToComponentMapping.TryGetValue(interfaceType, out types))
		{
			// Remove abstract because usually is not needed
			if (!bInlcludeAbstracts)
			{
				types = types.Where(t => !t.IsAbstract).ToArray();
			}
		}
		return types;
	}


	//////////////////////////////////////////////////////////////////////////
	public static Type[] FindInerithedFromClass<T>(bool bInlcludeAbstracts) where T : class
	{
		return FindInerithedFromClass(typeof(T), bInlcludeAbstracts);
	}


	//////////////////////////////////////////////////////////////////////////
	public static Type[] FindInerithedFromClass(Type classType, bool bInlcludeAbstracts)
	{
		Type[] types = new Type[0];
		if (classType.IsClass && m_ClassToComponentMapping.TryGetValue(classType, out types))
		{
			// Remove abstract because usually is not needed
			if (!bInlcludeAbstracts)
			{
				types = types.Where(t => !t.IsAbstract).ToArray();
			}
		}
		return types;
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Given a types collection, search a method called as specified on it or its base class, then call it </summary>
	/// <param name="types"></param>
	/// <param name="methodName"></param>
	/// <param name="IsBaseMethod"> Specify if this method has to searched on base class or not</param>
	public	static void CallMethodOnTypes( Type[] types, string methodName, bool IsBaseMethod )
	{
		if (types.IsNotNull())
		{
			foreach (Type type in types)
			{
				Type typeToUse = IsBaseMethod ? type.BaseType : type;
				MethodInfo method = typeToUse.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
				if (method.IsNotNull())
				{
					if (method.IsStatic)
					{
						method.Invoke(null, null);
						continue;
					}

					object instance = typeToUse.GetField("m_Instance", BindingFlags.NonPublic | BindingFlags.Static)?.GetValue(null);
					if (instance.IsNotNull())
					{
						method.Invoke(instance, null);
					}
				}
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private static Type[] GetTypesInherited(Type type)
	{
		return m_AllTypes.Where(t => type.IsAssignableFrom(t) && t.IsSubclassOf(typeof(Component))).ToArray();
	}
}
	