
using System.Reflection;
using UnityEngine;


//* TYPES */
public enum LineValueType		{ SINGLE, MULTI };

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
	ALL = CHILDREN | PARENT
}

namespace Utils {

	public static class Base {

		public	static	int		LayersAllButOne( int all, int one )
		{
			return ~( all << one );
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


		public	static	bool	SearchComponent<T1>( GameObject GameObject, ref T1 Component, SearchContext Context )
		{
			T1 result = default( T1 ); // null 

			switch ( Context )
			{
				case SearchContext.LOCAL:
				{
					result = GameObject.GetComponent<T1>();
					break;
				}
					
				case SearchContext.CHILDREN:
				{
					result = GameObject.GetComponentInChildren<T1>();
					break;
				}

				case SearchContext.PARENT:
				{
					result = GameObject.GetComponentInParent<T1>();
					break;
				}

				case SearchContext.ALL:
				{
					result = GameObject.GetComponentInChildren<T1>();
					if ( result == null )
					{
						result = GameObject.GetComponentInParent<T1>();
					}
					break;
				}
			}

			Component = result;
			return ( result != null );
		}
	}

}