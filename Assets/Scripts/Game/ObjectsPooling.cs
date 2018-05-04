

using UnityEngine;


/// <summary> Generic object pooler </summary>
public	class GameObjectsPool {

	private	static	int		Counter				= 0;

	private	GameObject		m_Container			= null;
	private	int				m_InternalIndex		= 0;


	//////////////////////////////////////////////////////////////////////////
	// Constructor
	public	GameObjectsPool( GameObject model, int size, System.Action<GameObject> actionOnObject = null )
	{
		if ( m_Container == null )
		{
			m_Container = new GameObject( "GameObjectsContainer_" + Counter.ToString() );
			Counter ++;
		}

		for ( int i = 0; i < size; i++ )
		{
			GameObject objectCopy = Object.Instantiate( model );
			objectCopy.transform.SetParent( m_Container.transform );
			if ( actionOnObject != null )
				actionOnObject( objectCopy );
		}
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
	internal	void	Destroy()
	{
		if ( m_Container == null )
			return;

		for ( int i = 0; i < m_Container.transform.childCount; i++ )
		{
			Object.Destroy( m_Container.transform.GetChild( i ).gameObject );
		}
		Object.Destroy( m_Container );

		Counter --;
	}
}


/// <summary> Object pooler with a specified component added on every object </summary>
public	class GameObjectsPool<T> where T : UnityEngine.Component  {
	
	private	static	int			Counter				= 0;

	private	GameObject			m_Container			= null;
	private	ObjectsPool<T>		m_ObjectsPool		= null;
	private	int					m_InternalIndex		= 0;
	private	System.Action<T>	m_Action			= null;

	//////////////////////////////////////////////////////////////////////////
	// Constructor
	public	GameObjectsPool( GameObject model, uint size, string containerName = "GameObjectsContainer_", bool permanent = false, System.Action<T> actionOnObject = null )
	{
		if ( m_Container == null )
		{
			m_Container = new GameObject( containerName + Counter.ToString() );
			Counter ++;
		}

		m_Action = actionOnObject;

		if ( permanent == true )
			Object.DontDestroyOnLoad( m_Container );

		T[] array = new T[ size ];
		m_ObjectsPool = new ObjectsPool<T>( size );

		for ( int i = 0; i < size; i++ )
		{
			GameObject objectCopy = Object.Instantiate( model );
			objectCopy.transform.SetParent( m_Container.transform );
			T comp = objectCopy.GetComponent<T>();
			if ( actionOnObject != null )
				actionOnObject( comp );

			array[ i ] = comp;
		}

		m_ObjectsPool.Set( array );
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


	//////////////////////////////////////////////////////////////////////////
	// SetActive
	public	void		SetActive( bool state )
	{
		m_Container.SetActive( state );
	}

	//////////////////////////////////////////////////////////////////////////
	// Destroy
	internal	void	Destroy()
	{
		if ( m_Container == null )
			return;

		for ( int i = 0; i < m_Container.transform.childCount; i++ )
		{
			Object.Destroy( m_Container.transform.GetChild( i ).gameObject );
		}

		Object.Destroy( m_Container );

		Counter --;
	}

}


/// <summary> Components pooler </summary>
public	class ObjectsPool<T> where T : UnityEngine.Component {

	private	T[]		m_Storage			= null;
	private	int		m_InternalIndex		= 0;


	//////////////////////////////////////////////////////////////////////////
	// Costructor
	public	ObjectsPool( uint size = 0 )
	{
		if ( size > 0 )
			m_Storage = new T[ size ];
	}


	//////////////////////////////////////////////////////////////////////////
	// Set
	public	void	Set( T[] array )
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


	//////////////////////////////////////////////////////////////////////////
	// Destroy
	internal	void	Destroy()
	{
		for ( int i = 0; i < m_Storage.Length; i++ )
		{
			Component comp = m_Storage[ i ];
			if ( comp != null )
			{
				Object.Destroy( comp );
			}
		}
	}
}