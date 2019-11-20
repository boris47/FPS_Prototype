
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public struct KeyValue {
	public	string	Key, Value;
	public	bool	IsOK;
};


public enum SearchContext {
	/// <summary> Only on this object </summary>
	LOCAL,
	/// <summary> On this object and children </summary>
	CHILDREN,
	/// <summary> On this object and parents </summary>
	PARENT,
	/// <summary> On all the object hierarchy </summary>
	FROM_ROOT = CHILDREN | LOCAL | PARENT
}

namespace Utils {

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

	public	static	class LayersHelper {

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

	public static class Base {


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
				foreach ( var a in results )
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
		private	static	void	CloneComponent( Component component, ref GameObject destinationObj, bool copyProperties = false )
		{
			global::System.Type componentType = component.GetType();
		
			Component tmpComponent = destinationObj.GetComponent( componentType );
			if ( tmpComponent == null )
				tmpComponent = destinationObj.AddComponent( componentType );

			if (copyProperties)
			{
				PropertyInfo[] foundProperties = componentType.GetProperties();
				for (int i = 0; i < foundProperties.Length; i++)
				{
					PropertyInfo foundProperty = foundProperties[i];
					if (foundProperty.CanWrite)
					{
						foundProperty.SetValue( tmpComponent, foundProperty.GetValue( component, null ) , null );
					}
				}
			}

			FieldInfo[] foundFields = componentType.GetFields();
			for (int i = 0; i < foundFields.Length; i++)
			{
				FieldInfo foundField = foundFields[i];
				foundField.SetValue( tmpComponent, foundField.GetValue( component ) );
			}
		}

		class dummy {}
		////////////////////////////////////////////////
		public static void Clone( ref GameObject sourceObj, ref GameObject destinationObj, bool copyProperties = false, global::System.Type[] copyComponents = null )
		{
			copyComponents = copyComponents ?? new global::System.Type[] { typeof(dummy) };

			// ALL COMPONENTS AND PROPERTIES EXCEPT MESH FILTER MESH PROPERTY
			Component[] copyModelComponents = sourceObj.GetComponents<Component>();
			for ( int i = 0; i < copyModelComponents.Length; i++ )
			{
				Component copyModelComponent = copyModelComponents[i];
				if ( global::System.Array.Exists( copyComponents, c => 
				copyModelComponent.GetType() == c || copyModelComponent.GetType().IsSubclassOf(c)) )
				{
					CloneComponent( copyModelComponent, ref destinationObj, copyProperties );
				}
			}

			// MESH FILTER MESH
			MeshFilter sourceMeshFilter = null, destMeshFilter = null;
			if ( sourceObj.TryGetComponent( out sourceMeshFilter ) && destinationObj.TryGetComponent( out destMeshFilter ) )
			{
				destMeshFilter.mesh = sourceMeshFilter.mesh;
			}

			//	RENDERER MATERIAL
			Renderer sourceRenderer = null, destRenderer = null;
			if ( sourceObj.TryGetComponent( out sourceRenderer ) && destinationObj.TryGetComponent( out destRenderer ) )
			{
				destRenderer.material = sourceRenderer.material;
			}

			// SCALE
			destinationObj.transform.localScale = sourceObj.transform.localScale;
		}


		////////////////////////////////////////////////
		private	static	T[]		SearchResults<T>( GameObject GameObject, SearchContext Context )
		{
			T[] results = null;
			
			switch ( Context )
			{
				case SearchContext.LOCAL:
				{
					results = GameObject.GetComponents<T>();
					break;
				}
					
				case SearchContext.CHILDREN:
				{
					results = GameObject.GetComponentsInChildren<T>();
					break;
				}
				
				case SearchContext.PARENT:
				{
					results = GameObject.GetComponentsInParent<T>();
					break;
				}

				case SearchContext.FROM_ROOT:
				{
					results = GameObject.transform.root.GetComponentsInChildren<T>();
					break;
				}
			}

			return results;
		}


		////////////////////////////////////////////////
		public	static	bool	SearchComponent<T1>( GameObject GameObject, ref T1 Component, SearchContext Context, global::System.Predicate<T1> Filter = null )
		{
			T1[] results = SearchResults<T1>( GameObject, Context );
			
			T1 result = default( T1 );

			if ( results != null && results.Length > 0 )
			{
				// Filtered search
				if (  Filter != null )
				{
					result = global::System.Array.Find( results, Filter );
				}
				// Normal search
				else
				{
					result = results[0];
				}
			}

			bool bHasValidResult = ( result != null );
			if ( bHasValidResult )
			{
				Component = result;
			}
			return bHasValidResult;
		}


		////////////////////////////////////////////////
		public	static	bool	SearchComponents<T1>( GameObject GameObject, ref T1[] Components, SearchContext Context, global::System.Predicate<T1> Filter = null )
		{
			T1[] results = SearchResults<T1>( GameObject, Context );

			// Filtered search
			if ( Filter != null && results.Length > 0 )
			{
				Components = global::System.Array.FindAll( results, ( T1 c ) => Filter( c ) );
			}
			// Normal search
			else
			{
				Components = results;
			}

			bool bHasValidResult = ( Components != null && Components.Length > 0 );
			return bHasValidResult;
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
	public class DoubleBuffer<T> {

		private	T[]		m_Buffer = null;
		private uint	m_Latest = 0;

		////////////////////////////////////////////////
		public DoubleBuffer( T t1, T t2 )	{		m_Buffer = new T[] { t1, t2 };						}
		////////////////////////////////////////////////
		public	T	Current()				{		return m_Buffer[ m_Latest ];						}
		////////////////////////////////////////////////
		public	T	Previous()				{		return m_Buffer[ ( m_Latest + 1 ) % 2 ];			}
		////////////////////////////////////////////////
		public	T	SwapBuffers()			{		return m_Buffer[ m_Latest = ( m_Latest + 1 ) % 2 ]; }

	}

}