

using UnityEngine;
using System.Reflection;

public static class ObjectCloner {

	private	static	void	CloneComponent( ref Component component, ref GameObject destinationObj, bool copyProperties = false )
	{
		System.Type componentType = component.GetType();
		
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

}


/// <summary> Generic object pooler </summary>
public class GameObjectsPool {

	private	static	int		Counter				= 0;

	private	GameObject		m_Container			= null;
	private	int				m_InternalIndex		= 0;


	//////////////////////////////////////////////////////////////////////////
	// Copstructor
	public	GameObjectsPool( ref GameObject model, int size, bool destroyModel, System.Action<GameObject> actionOnObject = null )
	{
		if ( m_Container == null )
		{
			m_Container = new GameObject( "GameObjectsContainer_" + Counter.ToString() );
			Counter ++;
		}

		for ( int i = 0; i < size; i++ )
		{
			GameObject objectCopy = new GameObject( model.name + "_" + i );
			objectCopy.hideFlags = HideFlags.NotEditable;
			ObjectCloner.Clone( ref model, ref objectCopy, true );
			objectCopy.transform.SetParent( m_Container.transform );
			if ( actionOnObject != null )
				actionOnObject( objectCopy );
		}

		// if model is not a prefab and user wants destroy it
		if ( model.scene.name != null && destroyModel == true )
			Object.Destroy( model.gameObject );
	}


	//////////////////////////////////////////////////////////////////////////
	// Get
	public	GameObject	Get()
	{
		m_InternalIndex ++;
		if ( m_InternalIndex >= m_Container.transform.childCount )
			m_InternalIndex = 0;

		return m_Container.transform.GetChild( m_InternalIndex ).gameObject;
	}


	//////////////////////////////////////////////////////////////////////////
	// Get<T>
	public	T	Get<T>()
	{
		GameObject go = Get();
		T component = go.GetComponent<T>();
		return component;
	}


	//////////////////////////////////////////////////////////////////////////
	// Destroy
	public	void	Destroy()
	{
		if ( m_Container == null )
			return;

		for ( int i = 0; i < m_Container.transform.childCount; i++ )
		{
			Object.Destroy( m_Container.transform.GetChild( i ).gameObject );
		}
	}
}



/// <summary> Object pooler with a specified component added on every object </summary>
public class GameObjectsPool<T> where T : UnityEngine.Component  {

	private	GameObject		m_Container			= null;
	private	ObjectsPool<T>	m_ObjectsPool		= null;
	private	int				m_InternalIndex		= 0;


	//////////////////////////////////////////////////////////////////////////
	// Costrunctor
	public	GameObjectsPool( string Name, int size = 0 )
	{
		if ( m_Container == null )
		{
			m_Container = new GameObject( Name +  "_GameObjectsContainer" );
		}

		m_ObjectsPool = new ObjectsPool<T>();
		T[] storage = new T[ size ];

		for ( int i = 0; i < size; i++ )
		{
			GameObject go = new GameObject();
			go.transform.SetParent( m_Container.transform );
			T comp = go.AddComponent<T>();
			storage[ i ] = comp;
		}

		m_ObjectsPool.Set( ref storage );

	}


	//////////////////////////////////////////////////////////////////////////
	// GetGameObject
	public	GameObject GetGameObject()
	{
		m_InternalIndex ++;
		if ( m_InternalIndex >= m_Container.transform.childCount )
			m_InternalIndex = 0;

		return m_Container.transform.GetChild( m_InternalIndex ).gameObject;
	}


	//////////////////////////////////////////////////////////////////////////
	// GetComponent
	public T	GetComponent()
	{
		m_InternalIndex ++;
		if ( m_InternalIndex >= m_Container.transform.childCount )
			m_InternalIndex = 0;

		return m_ObjectsPool.At( m_InternalIndex );
	}

}


/// <summary> Components pooler </summary>
public	class ObjectsPool<T> where T : UnityEngine.Component {

	private	T[]		m_Storage			= null;
	private	int		m_InternalIndex		= 0;


	//////////////////////////////////////////////////////////////////////////
	// Costructor
	public	ObjectsPool( int size = 0 )
	{
		if ( size > 0 )
			m_Storage = new T[ size ];
	}


	//////////////////////////////////////////////////////////////////////////
	// Set
	public	void	Set( ref T[] array )
	{
		m_Storage = array;
	}


	//////////////////////////////////////////////////////////////////////////
	// Resize
	public void	Resize( int newSize )
	{
		if ( m_Storage == null || m_Storage.Length == 0 )
		{
			m_Storage = new T[ newSize ];
			return;
		}

		if ( newSize > m_InternalIndex )
			m_InternalIndex = 0;

		T[] tmp = new T[ newSize ];

		System.Array.Copy( m_Storage, tmp, m_Storage.Length );

		m_Storage = tmp;
	}


	//////////////////////////////////////////////////////////////////////////
	// Get
	public	T	Get()
	{
		m_InternalIndex ++;
		if ( m_InternalIndex >= m_Storage.Length )
			m_InternalIndex = 0;

		return m_Storage[ m_InternalIndex ];
	}


	//////////////////////////////////////////////////////////////////////////
	// At
	public	T	At( int index )
	{
		if ( index >= m_Storage.Length )
			return null;

		return m_Storage[ index ];
	}

}