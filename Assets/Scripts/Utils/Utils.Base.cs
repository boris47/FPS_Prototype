
using System.Reflection;
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
	ALL = CHILDREN | LOCAL | PARENT
}

namespace Utils {

	public static class Base {

		public	static	bool		GetTemplateSingle<T>( ref T Output, bool bSpawnIfNecessary = false ) where T : Component
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

		public	static	int		LayersAllButOne( int all, int one )
		{
			// This would creates a layer only with the one given layer
			int layerMask = 1 << one;

			// But instead we want a layer mask with all except given layer. The ~ operator does this, it inverts a bitmask.
			layerMask = ~layerMask;

			return layerMask;
		}


		private	static	void	CloneComponent( ref Component component, ref GameObject destinationObj, bool copyProperties = false )
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


		public	static	void	Clone( ref GameObject sourceObj, ref GameObject destinationObj, bool copyProperties = false )
		{
			// ALL COMPONENTS AND PROPERTIES EXCEPT MESH FILTER MESH PROPERTY
			Component[] copyModelComponents = sourceObj.GetComponents<Component>();
			for (int i = 0; i < copyModelComponents.Length; i++)
			{
				Component copyModelComponent = copyModelComponents[ i ];
				CloneComponent( ref copyModelComponent, ref destinationObj, copyProperties );
			}

			// MESH FILTER MESH
			MeshFilter sourceMeshFilter = sourceObj.GetComponent<MeshFilter>();
			if ( sourceMeshFilter != null )
				destinationObj.GetComponent<MeshFilter>().mesh = sourceMeshFilter.mesh;

			//	RENDERER MATERIAL
			Material returnHighlightMaterial = sourceObj.GetComponent<Renderer>().material;
			destinationObj.GetComponent<Renderer>().material = returnHighlightMaterial;

			// SCALE
			destinationObj.transform.localScale = sourceObj.transform.localScale;
		}


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

				case SearchContext.ALL:
				{
					results = GameObject.transform.root.GetComponentsInChildren<T>();
					break;
				}
			}

			return results;
		}


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
	}

	public class DoubleBuffer<T> {

		private	T[]		m_Buffer = null;
		private uint	m_Latest = 0;

		public DoubleBuffer( T t1, T t2 )
		{
			m_Buffer = new T[] { t1, t2 };
		}

		public	T	Current()
		{
			return m_Buffer[ m_Latest ];
		}

		public	T Previous()
		{
			return m_Buffer[ ( m_Latest + 1 ) % 2 ];
		}

		public	T SwapBuffers()
		{
			m_Latest = ( m_Latest + 1 ) % 2;
			return m_Buffer[ m_Latest ];
		}
	}

}