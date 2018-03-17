

using UnityEngine;


public class GameObjectsPool {

	private	static	int		Counter				= 0;

	private	GameObject		m_RefModel			= null;
	private	GameObject		m_Container			= null;
	private	int				m_InternalIndex		= 0;

	private	int				m_OriginalSize		= 0;


	//////////////////////////////////////////////////////////////////////////
	// Copstructor
	public	GameObjectsPool( ref GameObject model, int size, bool destroyModel )
	{
		m_RefModel		= Object.Instantiate( model );
		m_OriginalSize	= size;

		if ( m_Container == null )
		{
			m_Container = new GameObject( "GameObjectsContainer_" + Counter.ToString() );
			Counter ++;
		}

		for ( int i = 0; i < size; i++ )
		{
			Object.Instantiate( m_RefModel ).transform.SetParent( m_Container.transform );
		}

		// if model is not a prefab and user wants destroy it
		if ( model.scene.name != null && destroyModel == true )
			Object.Destroy( model );
	}


	//////////////////////////////////////////////////////////////////////////
	// Get
	public	GameObject	Get()
	{
		// Restore child count if gameobjects have been destroyied outside
		if ( m_Container.transform.childCount < m_OriginalSize )
		{
			while( m_Container.transform.childCount < m_OriginalSize )
			{
				Object.Instantiate( m_RefModel ).transform.SetParent( m_Container.transform );
			}
		}

		m_InternalIndex ++;
		if ( m_InternalIndex >= m_Container.transform.childCount )
			m_InternalIndex = 0;

		return m_Container.transform.GetChild( m_InternalIndex ).gameObject;
	}

}




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