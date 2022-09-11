using System.Collections.Generic;
using System.Reflection;


public static class ReflectionHelper
{

	//////////////////////////////////////////////////////////////////////////
	public static bool TryGetAttributeValue<V, T>(System.Type type, System.Func<V, T> OnAttribute, out T OutValue) where V : System.Attribute
	{
		OutValue = default;
		bool bResult = false;
		if (OnAttribute.IsNotNull())
		{
			V attribute = (V)System.Attribute.GetCustomAttribute(type, typeof(V));
			if (attribute.IsNotNull())
			{
				OutValue = OnAttribute(attribute);
				bResult = true;
			}
		}
		return bResult;
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary>  </summary>
	/// <param name="obj">System.Object or System.Type</param>
	/// <param name="propertyName"></param>
	/// <param name="OutValue"></param>
	/// <returns></returns>
	public static bool GetPropertyValue<T>(in object obj, in string propertyName, out T OutValue)
	{
		OutValue = default;
		using (new UnityLoggerStateScope(false))
		{
			PropertyInfo propertyInfo = ((obj as System.Type) ?? obj.GetType()).GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			try
			{
				MethodInfo getMethodInfo = propertyInfo.GetGetMethod();
				if (getMethodInfo.IsNotNull())
				{
					OutValue = (T)getMethodInfo.Invoke(obj, null);
				}
				else
				{
					OutValue = (T)propertyInfo.GetValue(obj);
				}
			}
			catch (System.Exception) {}
		}
		return !System.Collections.Generic.EqualityComparer<T>.Default.Equals(OutValue, default);
	}

	//////////////////////////////////////////////////////////////////////////
	/// <summary>  </summary>
	/// <param name="obj">System.Object or System.Type</param>
	/// <param name="propertyName"></param>
	/// <param name="value"></param>
	/// <returns></returns>
	public static bool SetPropertyValue(in object obj, in string propertyName, in object value)
	{
		bool bResult = true;
		using (new UnityLoggerStateScope(false))
		{
			PropertyInfo propertyInfo = ((obj as System.Type) ?? obj.GetType()).GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
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
		return bResult;
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary>  </summary>
	/// <param name="obj"></param>
	/// <param name="fieldName"></param>
	/// <param name="OutValue"></param>
	/// <returns></returns>
	public static bool GetFieldValue<T>(in object obj, in string fieldName, out T OutValue)
	{
		OutValue = default;
		using (new UnityLoggerStateScope(false))
		{
			FieldInfo fieldInfo = ((obj as System.Type) ?? obj.GetType()).GetField(fieldName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			try
			{
				OutValue = (T)fieldInfo.GetValue(obj);
			}
			catch (System.Exception) {}
		}
		return !System.Collections.Generic.EqualityComparer<T>.Default.Equals(OutValue, default);
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary>  </summary>
	/// <param name="obj"></param>
	/// <param name="fieldName"></param>
	/// <param name="value"></param>
	/// <returns></returns>
	public static bool SetFieldValue(in object obj, in string fieldName, in object value)
	{
		bool bResult = true;
		using (new UnityLoggerStateScope(false))
		{
			FieldInfo fieldInfo = ((obj as System.Type) ?? obj.GetType()).GetField(fieldName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			try
			{
				fieldInfo.SetValue(obj, value);
			}
			catch (System.Exception) { bResult = false; }
		}
		return bResult;
	}

	//////////////////////////////////////////////////////////////////////////
	public static bool IsInerithedFrom(in System.Type InBaseType, in System.Type InDerivedType)
	{
		// Determines whether an instance of a specified derived type can be assigned to a variable of the current baseType
		// Ref: https://docs.microsoft.com/en-us/dotnet/api/system.type.isassignablefrom
		bool bIsAssignableFrom = InBaseType.IsAssignableFrom(InDerivedType);

		// Determines whether the current type derives from the specified base type.
		// True if the current derivedType derives from baseType; otherwise, false.
		// This method also returns false if base type and the current type are equal.
		// Ref: https://docs.microsoft.com/en-us/dotnet/api/system.type.issubclassof
		bool bIsSubclassOf = InDerivedType.IsSubclassOf(InBaseType);

		return bIsAssignableFrom && bIsSubclassOf;
	}

	//////////////////////////////////////////////////////////////////////////
	public static bool TryGetGenericArg(in System.Type InCurrentType, out System.Type OutType, in int InIndex = 0)
	{
		OutType = default;

		if (InCurrentType.IsNotNull())
		{
			System.Type currentType = InCurrentType;
			System.Type[] genericArgs = currentType.GetGenericArguments();
			while (genericArgs.Length == 0 && currentType.IsNotNull())
			{
				genericArgs = currentType.GetGenericArguments();
				currentType = currentType.BaseType;
			}

			if (genericArgs.IsValidIndex(InIndex))
			{
				OutType = genericArgs[InIndex];
			}
		}
		return OutType.IsNotNull();
	}

	//////////////////////////////////////////////////////////////////////////
	public static List<System.Type> GetBaseTypesOf<BaseType, CurrentType>(in CurrentType currentType, in bool bOrderByTopType) where CurrentType : BaseType
	{
		return GetBaseTypesOf(typeof(BaseType), currentType.GetType(), bOrderByTopType);
	}

	//////////////////////////////////////////////////////////////////////////
	public static List<System.Type> GetBaseTypesOf(in System.Type InBaseType, in System.Type InCurrentType, in bool bOrderByTopType)
	{
		Utils.CustomAssertions.IsTrue(IsInerithedFrom(InBaseType, InCurrentType));
		List<System.Type> OutResult = new List<System.Type>();

		System.Type currentType = InCurrentType;
		while(currentType.IsNotNull() && currentType.BaseType.IsNotNull() && currentType.BaseType != InBaseType)
		{
			OutResult.Add(currentType.BaseType);
			currentType = currentType.BaseType;
		}

		if (bOrderByTopType)
		{
			OutResult.Reverse();
		}
		return OutResult;
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Given a types collection, search a method called as specified on it or its base class, then call it </summary>
	/// <param name="types"></param>
	/// <param name="methodName"></param>
	/// <param name="IsBaseMethod"> Specify if this method has to searched on base class or not</param>
	public static void CallStaticMethodOnTypes(IEnumerable<System.Type> types, string methodName, bool IsBaseMethod)
	{
		if (types.IsNotNull())
		{
			foreach (System.Type type in types)
			{
				if (type.IsNotNull())
				{
					System.Type typeToUse = IsBaseMethod ? type.BaseType ?? type : type;
					MethodInfo method = typeToUse.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
					if (method.IsNotNull() && method.IsStatic)
					{
						method.Invoke(null, null);
					}
				}
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private class UnityLoggerStateScope : System.IDisposable
	{
		private bool m_Disposed = default;
		private bool m_PreviousState = default;


		//////////////////////////////////////////////////////////////////////////
		public UnityLoggerStateScope(in bool enabled)
		{
			m_Disposed = false;

			m_PreviousState = UnityEngine.Debug.unityLogger.logEnabled;
			UnityEngine.Debug.unityLogger.logEnabled = enabled;
		}

		//////////////////////////////////////////////////////////////////////////
		void System.IDisposable.Dispose()
		{
			if (!m_Disposed)
			{
				m_Disposed = true;
				UnityEngine.Debug.unityLogger.logEnabled = m_PreviousState;
			}
		}
	}
}
