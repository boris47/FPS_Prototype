
using System.Linq;
using System.Reflection;
using System.Collections;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;

public struct KeyValue
{
	public	string	Key, Value;
	public	bool	IsOK;
};


public enum ESearchContext
{
	/// <summary> Only on this object </summary>
	LOCAL,
	/// <summary> On this object and children </summary>
	LOCAL_AND_CHILDREN,
	/// <summary> On this object and parents </summary>
	LOCAL_AND_PARENTS,
	/// <summary> On all the object hierarchy </summary>
	FROM_ROOT = LOCAL_AND_CHILDREN | LOCAL | LOCAL_AND_PARENTS
}

[System.Serializable]
public class ToJsonWrapper<T>
{
	[SerializeField]
	public T content = default(T);

	public ToJsonWrapper(T content)
	{
		this.content = content;
	}
}


public static class CustomAssertions
{
	public static bool IsTrue(bool condition, string message = null, Object context = null)
	{
		if (!condition)
		{
#if UNITY_EDITOR
			System.Diagnostics.Debugger.Break();
#endif
			UnityEngine.Debug.LogError(message ?? "CustomAssertions.IsTrue: Assertion Failed");
		}
		return condition;
	}

	public static bool IsNotNull(System.Object value, string message = null, Object context = null)
	{
		bool condition = value.IsNotNull();
		if (!condition)
#if UNITY_EDITOR
		{
			System.Diagnostics.Debugger.Break();
#endif
			UnityEngine.Debug.LogError(message ?? "CustomAssertions.IsNotNull: Assertion Failed");
		}
		return condition;
	}
}


namespace Utils
{
	public static class FlagsHelper
	{
		////////////////////////////////////////////////
		public static bool IsSet<T>(T flags, T flag)
		{
			uint flagsValue = (uint)(object)flags;
			uint flagValue = (uint)(object)flag;

			return (flagsValue & flagValue) != 0;
		}

		////////////////////////////////////////////////
		public static void Set<T>(ref T flags, T flag)
		{
			uint flagsValue = (uint)(object)flags;
			uint flagValue = (uint)(object)flag;
		
			flags = (T)(object)(flagsValue | flagValue);
		}

		////////////////////////////////////////////////
		public static void Unset<T>(ref T flags, T flag)
		{
			uint flagsValue = (uint)(object)flags;
			uint flagValue = (uint)(object)flag;

			flags = (T)(object)(flagsValue & (~flagValue));
		}
	}

	public	static	class LayersHelper
	{

		////////////////////////////////////////////////
		public	static	int		Layers_AllButOne( string layerName )
		{
			int layer = LayerMask.NameToLayer( layerName );

			int layerMask = 1 << layer;

			return ~layerMask;
		}

		////////////////////////////////////////////////
		public	static	int		Layers_OneOnly( string layerName )
		{
			int layer = LayerMask.NameToLayer( layerName );

			int layerMask = 1 << layer;

			return layerMask;
		}

		////////////////////////////////////////////////
		public	static LayerMask	Layers_InclusiveMask( int[] layers )
		{
			int m = 0;
			for (int l=0; l<layers.Length; l++)
			{
				m |= (1<<layers[l]);
			}
			return m;
		}
 
		////////////////////////////////////////////////
		public	static LayerMask	 Layers_ExclusiveMask( int[] layers )
		{
			int m = 0;
			for (int l=0; l<layers.Length; l++)
			{
				m |= (1<<layers[l]);
			}
			return ~m;
		}
	}

	public static class Base
	{
		////////////////////////////////////////////////
		public	static	bool	GetTemplateSingle<T>( ref T Output, bool bSpawnIfNecessary = false ) where T : Component
		{
			T[] results = Object.FindObjectsOfType<T>();
			if ( results.Length == 0 )
			{
				if ( bSpawnIfNecessary == true )
				{
					Output = new GameObject( typeof(T).FullName ).AddComponent<T>();
					return true; 
				}
				else
				{
					string msg = typeof(T).FullName + " not found!";
#if UNITY_EDITOR
					UnityEditor.EditorUtility.DisplayDialog( "Warning!", msg, "OK" );
#endif
					Debug.Log( msg );
					return false;
				}
			}

			if ( results.Length > 1 )
			{
				string msg = "Multiple components found:\n";
				foreach ( T a in results )
				{
					msg += "Root: " + a.transform.root.name + "\n";
				}
				msg += "\nIn order to work only one component must live";

#if UNITY_EDITOR
				UnityEditor.EditorUtility.DisplayDialog( "Warning!", msg, "OK" );
#endif
				Debug.Log( msg );
				return false;
			}

			Output = results[0];
			return true;
		}


		////////////////////////////////////////////////
		public static bool SerializeObject(object o, out string result)
		{
			result = string.Empty;
			try
			{
				using (MemoryStream memoryStream = new MemoryStream())
				{
					new BinaryFormatter().InstallSurrogates().Serialize(memoryStream, o);
					result = System.Convert.ToBase64String(memoryStream.ToArray());
				}
			}
			catch (System.Exception e)
			{
				Debug.LogException(e);
				return false;
			}
			return true;
		}

		////////////////////////////////////////////////
		public static bool DeserializeObject<T>(string str, out T result)
		{
			result = default(T);
			try
			{
				byte[] bytes = System.Convert.FromBase64String(str);
				using (MemoryStream stream = new MemoryStream(bytes))
				{
					result = (T)new BinaryFormatter().InstallSurrogates().Deserialize(stream);
				}
			}
			catch (System.Exception e)
			{
				Debug.LogException(e);
				return false;
			}
			return true;
		}

		////////////////////////////////////////////////
		public static bool SerializeToStream(in object o, out MemoryStream stream)
		{
			bool bResult = true;
			stream = new MemoryStream();
			try
			{
				new BinaryFormatter().InstallSurrogates().Serialize(stream, o);
			}
			catch (System.Exception e)
			{
				stream.Dispose();
				stream = null;
				bResult = false;
				Debug.LogException(e);
			}
			return bResult;
		}

		////////////////////////////////////////////////
		public static bool DeserializeFromStream(in MemoryStream stream, out object result)
		{
			result = default;
			bool bResult = true;
			long prevPosition = stream.Position;
			stream.Seek(0, SeekOrigin.Begin);
			try
			{
				new BinaryFormatter().InstallSurrogates().Deserialize(stream);
			}
			catch (System.Exception e)
			{
				stream.Seek(prevPosition, SeekOrigin.Begin);
				bResult = false;
				Debug.LogException(e);
			}
			return bResult;
		}


		/*	private static readonly List<System.Type> ExcludedTypes = new List<System.Type>()
			{
				typeof(UnityEngine.AI.NavMeshPath),
				typeof(UnityEngine.GameObject),

				typeof(UnityEngine.Component),
				typeof(UnityEngine.Mesh),
				typeof(UnityEngine.Material),
				typeof(UnityEngine.PhysicMaterial),
			};
		*/    ////////////////////////////////////////////////
		private static string[] Excluded = new string[]
		{
			"sleepVelocity", "sleepAngularVelocity", "inertiaTensor", "inertiaTensorRotation"
		};
		public static	void	GetComponentFieldsAndPropertiesInfo(in Component component, in Dictionary<string, object> propertiesInfo, in Dictionary<string, object> fieldsInfo)
		{
			System.Type componentType = component.GetType();
			
			GlobalManager.LoggerInstance?.SetExceptionsAsWarnings(true);
			{
				// Properties
				foreach (PropertyInfo property in componentType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
				{
					System.Type propertyType = property.PropertyType;
					bool bIsExcluded = Excluded.Any(excludedName => property.Name.ToLower().Contains(excludedName.ToLower()));
					if (!propertyType.IsClass && !propertyType.IsInterface && !bIsExcluded)
					{
						if (ReflectionHelper.GetPropertyValue(component, property.Name, out object value))
						{
							propertiesInfo.Add(property.Name, value);
						}
					}
				}

				// Fields
				foreach (FieldInfo field in componentType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
				{
					System.Type fieldType = field.FieldType;
					if (!fieldType.IsClass && !fieldType.IsInterface)
					{
						if (ReflectionHelper.GetFieldValue(component, field.Name, out object value))
						{
							fieldsInfo.Add(field.Name, value);
						}
					}
				}
			}
			GlobalManager.LoggerInstance?.SetExceptionsAsWarnings(false);
		}


		////////////////////////////////////////////////
		public static	void	SetComponentFieldsAndPropertiesInfo(in Component component, in Dictionary<string, object> propertiesInfo, in Dictionary<string, object> fieldsInfo)
		{
			System.Type componentType = component.GetType();

			GlobalManager.LoggerInstance?.SetExceptionsAsWarnings(true);
			{
				// Properties
				foreach (KeyValuePair<string, object> foundProperty in propertiesInfo)
				{
					PropertyInfo property = componentType.GetProperty(foundProperty.Key, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

					ReflectionHelper.SetPropertyValue(component, property.Name, foundProperty.Value);
				}

				// Fields
				foreach (KeyValuePair<string, object> foundField in fieldsInfo)
				{
					FieldInfo field = componentType.GetField(foundField.Key, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

					ReflectionHelper.SetFieldValue(component, field.Name, foundField.Value);
				}
			}
			GlobalManager.LoggerInstance?.SetExceptionsAsWarnings(false);
		}
		

		////////////////////////////////////////////////
		private	static	void	CloneComponent( Component component, GameObject destinationObj, bool copyProperties = false )
		{
			global::System.Type componentType = component.GetType();
		
			if (!destinationObj.TryGetComponent(out Component tmpComponent))
			{
				tmpComponent = destinationObj.AddComponent( componentType );
			}
			
			if (copyProperties)
			{
				foreach(PropertyInfo foundProperty in componentType.GetProperties())
				{
					if (foundProperty.CanWrite)
					{
						foundProperty.SetValue( tmpComponent, foundProperty.GetValue( component, null ) , null );
					}
				}
			}

			foreach(FieldInfo foundField in componentType.GetFields())
			{
				foundField.SetValue( tmpComponent, foundField.GetValue( component ) );
			}
		}


		////////////////////////////////////////////////
		public static	void	Clone( GameObject sourceObj, GameObject destinationObj, bool copyProperties = false, global::System.Type[] copyComponents = null )
		{
			// ALL COMPONENTS AND PROPERTIES EXCEPT MESH FILTER MESH PROPERTY
			Component[] copyModelComponents = sourceObj.GetComponents<Component>();
			for ( int i = 0; i < copyModelComponents.Length; i++ )
			{
				Component copyModelComponent = copyModelComponents[i];
				if (copyComponents.IsNotNull() && global::System.Array.Exists( copyComponents, c => copyModelComponent.GetType() == c || copyModelComponent.GetType().IsSubclassOf(c)) )
				{
					CloneComponent( copyModelComponent, destinationObj, copyProperties );
				}
			}

			if ( sourceObj.TryGetComponent( out MeshFilter sourceMeshFilter ) && destinationObj.TryGetComponent( out MeshFilter destMeshFilter ) )
			{
				destMeshFilter.mesh = sourceMeshFilter.mesh;
			}

			if ( sourceObj.TryGetComponent( out Renderer sourceRenderer ) && destinationObj.TryGetComponent( out Renderer destRenderer ) )
			{
				destRenderer.material = sourceRenderer.material;
			}

			// SCALE
			destinationObj.transform.localScale = sourceObj.transform.localScale;
		}


		////////////////////////////////////////////////
		private	static	bool	TrySearchResults<T>( GameObject GameObject, ESearchContext Context, out T[] results)
		{
			results = null;
			switch (Context)
			{
				case ESearchContext.LOCAL				: { results = GameObject.GetComponents<T>(); break; }
				case ESearchContext.LOCAL_AND_CHILDREN	: { results = GameObject.GetComponentsInChildren<T>(includeInactive: true); break; }
				case ESearchContext.LOCAL_AND_PARENTS	: { results = GameObject.GetComponentsInParent<T>(includeInactive: true); break; }
				case ESearchContext.FROM_ROOT			: { results = GameObject.transform.root.GetComponentsInChildren<T>(includeInactive: true); break; }
			}
			return results != null && results.Length > 0;
		}


		////////////////////////////////////////////////
		public	static	bool	TrySearchComponent<T>( GameObject GameObject, ESearchContext Context, out T Component, global::System.Predicate<T> Filter = null )
		{
			Component = default(T);
			if (TrySearchResults( GameObject, Context, out T[] results))
			{
				// Filtered search
				if (Filter != null)
				{
					Component = System.Array.Find(results, Filter);
				}
				// Normal search
				else
				{
					Component = results[0];
				}
				return Component.IsNotNull();
			}
			return false;
		}


		////////////////////////////////////////////////
		public	static	bool	TrySearchComponents<T>( GameObject GameObject, ESearchContext Context, out T[] Components, global::System.Predicate<T> Filter = null ) where T : Component
		{
			Components = null;
			if (TrySearchResults(GameObject, Context, out T[] results))
			{
				// Filtered search
				if (Filter != null)
				{
					Components = global::System.Array.FindAll(results, Filter);
				}
				// Normal search
				else
				{
					Components = results;
				}
				return true;
			}
			return false;
		}


		////////////////////////////////////////////////
		public	static IEnumerator DestroyChildren( Transform t, int StartIndex = 0, int EndIndex = int.MaxValue )
		{
			int childCount = t.childCount;

			if ( StartIndex > childCount )
				yield break;

			if ( EndIndex > childCount )
				EndIndex = childCount;

			GameObject bin = new GameObject();
			bin.hideFlags = HideFlags.DontSave;
			Transform binTransform = bin.transform;

			// Move children into bin
			for ( int i = EndIndex - 1; i >= StartIndex; i-- )
			{
				Transform child = t.GetChild( i );
				child.SetParent( binTransform );
			}

			// Destroy one gameobject every frame
			for ( int i = 0; i < binTransform.childCount; i++ )
			{
				Transform child = binTransform.GetChild( i );

				Object.Destroy( child.gameObject );
				yield return null;
			}

			Object.Destroy( bin );
		}
	}


	////////////////////////////////////////////////
	public class DoubleBuffer<T>
	{
		private readonly	T[]		m_Buffer = null;
		private				uint	m_Latest = 0;

		//----------------------------------------------
		public DoubleBuffer( T t1, T t2 )	{ m_Buffer = new T[] { t1, t2 }; }
		//----------------------------------------------
		public T Current()		=> m_Buffer[m_Latest];
		//----------------------------------------------
		public T Previous()		=> m_Buffer[(m_Latest + 1) % 2];
		//----------------------------------------------
		public T SwapBuffers()	 => m_Buffer[m_Latest = (m_Latest + 1) % 2];
	}

}