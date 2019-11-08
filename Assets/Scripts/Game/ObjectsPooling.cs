
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public	class GameObjectsPoolConstructorData<T> {
	public GameObject				Model						= null;
	public uint						Size						= 1;
	public string					ContainerName				= "GameObjectsContainer_";
//	public Transform				Parent						= null;
	public System.Action<T>			ActionOnObject				= delegate { };
	public System.Action			ActionOnLoadEnd				= delegate { };
	public IEnumerator				CoroutineEnumerator			= null;
	public bool						IsAsyncBuild				= false;
}


/// <summary> Objects pool with a specified component added on every object </summary>
public	class GameObjectsPool<T> where T : UnityEngine.Component  {
	
	private	static	int			Counter				= 0;
	private	GameObject			m_ContainerGO		= null;
	private	List<T>				m_ObjectsPool		= null;
	private	int					m_InternalIndex		= 0;
	private	System.Action<T>	m_ActionOnObject	= delegate { };
	private	GameObject			m_ModelGO			= null;
	private	Coroutine			m_Coroutine			= null;
	private bool				m_bIsBuilding		= false;

	
	// Iterations
	public List<T>.Enumerator  GetEnumerator()
	{
		return m_ObjectsPool.GetEnumerator();
	}

	public	bool				IsValid
	{
		get { return m_ContainerGO.IsNotNull() && m_ObjectsPool.IsNotNull() && m_ModelGO.IsNotNull(); }
	}



	//////////////////////////////////////////////////////////////////////////
	// CONSTRUCTOR
	public	GameObjectsPool( GameObjectsPoolConstructorData<T> constructorData )
	{
		UnityEngine.Assertions.Assert.IsTrue
		(
			constructorData.Model != null,
			"GameObjectsPool trying to create a pool with null Model for component " + typeof(T).Name
		);

		UnityEngine.Assertions.Assert.IsTrue
		(
			constructorData.Size > 0,
			"GameObjectsPool trying to create a pool with null Model for component " + typeof(T).Name
		);

		UnityEngine.Assertions.Assert.IsTrue
		(
			constructorData.Model.transform.HasComponent<T>() == true,
			"GameObjectsPool trying to create a pool with Model with no component " + typeof(T).Name + ", Model is " + constructorData.Model.name
		);
		

		// Get data from GameObjectsPoolConstructorData
		m_ModelGO				= constructorData.Model;
		string containerName	= constructorData.ContainerName + Counter.ToString();
		uint poolSize			= constructorData.Size;


		// Assign action for every object
		m_ActionOnObject = constructorData.ActionOnObject ?? m_ActionOnObject;


		// Create game object container
		m_ContainerGO = new GameObject( containerName );
		m_ContainerGO.transform.SetPositionAndRotation( Vector3.up * 80000f, Quaternion.identity ); 
		Object.DontDestroyOnLoad(m_ContainerGO);
		Counter ++;
		

		// Create the internal pool
		if ( constructorData.IsAsyncBuild ) // Asyncronously
		{
			constructorData.CoroutineEnumerator = CreateItemsCO( constructorData );
			m_bIsBuilding = true;
			m_Coroutine = CoroutinesManager.Start( constructorData.CoroutineEnumerator, "GameObjectsPool::Constructor: Create items of " + m_ModelGO.name );
		}
		else
		// Instantly
		{
			m_ObjectsPool = new List<T>( (int)( poolSize ) );
			{
				for ( uint i = 0; i < poolSize; i++ )
				{
					T comp = Createitem( m_ModelGO );
					m_ActionOnObject( comp );
					m_ObjectsPool.Add( comp );
				}
			}
		}
	}



	//////////////////////////////////////////////////////////////////////////
	// CreateItemsCO
	private	IEnumerator CreateItemsCO( GameObjectsPoolConstructorData<T> constructorData )
	{
		yield return null;

		int size = (int) ( constructorData.Size );

		m_ObjectsPool = new List<T>( size );

		for ( uint i = 0; i < size; i++ )
		{
//				Debug.Log( "Creating " + model );
			T comp = Createitem( m_ModelGO );
			m_ActionOnObject( comp );
			m_ObjectsPool.Add( comp );
			yield return null;
		}
		
		constructorData.ActionOnLoadEnd();
		m_bIsBuilding = false;
		m_Coroutine = null;
	}



	//////////////////////////////////////////////////////////////////////////
	// Convert
	public		bool		Convert( GameObject model, System.Action<T> actionOnObject = null )
	{
		Debug.AssertFormat
		(
			model != null,
			"Trying to covert a GameObjectsPool using null model"
		);
		
		m_ModelGO = model;

		if ( m_bIsBuilding == true )
		{
			CoroutinesManager.Stop( m_Coroutine );
		}
			
		m_ActionOnObject = actionOnObject ?? m_ActionOnObject;

		Utils.Base.DestroyChildren( m_ContainerGO.transform );

		int size = m_ObjectsPool.Count;
		/*
		{
			m_ContainerGO.transform.DetachChildren();
			for ( int i = m_ObjectsPool.Count - 1; i >= 0; i-- )
			{
				Component comp = m_ObjectsPool[i];
				Object.Destroy( comp.gameObject );
				m_ObjectsPool.RemoveAt(i);
			}
		}
		*/
		m_ObjectsPool.Clear();
			
		// Create the internal pool
		for ( uint i = 0; i < size; i++ )
		{
			T comp = Createitem( model );
			m_ActionOnObject( comp );
			m_ObjectsPool.Add( comp );
		}
		
		return true;
	}



	//////////////////////////////////////////////////////////////////////////
	// Createitem
	private		T			Createitem( GameObject model )
	{	//											Model	Position,		Rotation			Parent
		GameObject objectCopy = Object.Instantiate( model, Vector3.zero, Quaternion.identity, m_ContainerGO.transform );
		T comp = objectCopy.GetComponent<T>();
		return comp;
	}



	//////////////////////////////////////////////////////////////////////////
	// ExecuteActionOnObject
	public		void		ExecuteActionOnObjects( System.Action<T> actionOnObject )
	{
		m_ObjectsPool.ForEach( actionOnObject );
	}



	//////////////////////////////////////////////////////////////////////////
	// Resize
	public		bool		Resize( uint newSize )
	{
		if ( IsValid == false )
			return false;

		// Skip for invalid new size
		if ( newSize == 0 )
		{
			return false;
		}

		// Return true if the requested size is the currently set
		if ( m_ObjectsPool.Count == newSize )
			return true;

		// Calculate the delta
		int delta = Mathf.Abs( m_ObjectsPool.Count - (int)newSize );

		int childCount = m_ObjectsPool.Count;

		if ( m_bIsBuilding == true )
		{
			CoroutinesManager.Stop( m_Coroutine );
		}

		// Enlarge
		if ( childCount < newSize )
		{
			for ( int i = 0; i < delta; i++ )
			{
				T comp = Createitem( m_ModelGO );
				m_ActionOnObject( comp );
				m_ObjectsPool.Add( comp );
			}
		}
		
		// Reduction
		if ( childCount > newSize )
		{
			for ( int i = delta - 1; i >= 0; i-- )
			{
				Component comp = m_ObjectsPool[i];
				m_ObjectsPool.RemoveAt(i);
				Object.Destroy( comp.gameObject );
			}
		}

		m_InternalIndex = 0;
		return true;
	}



	//////////////////////////////////////////////////////////////////////////
	// GetGameObject
	public		GameObject	GetGameObject()
	{
		if ( IsValid == false )
			return null;

		if ( m_InternalIndex >= m_ObjectsPool.Count )
		{
			m_InternalIndex = 0;
		}

		return m_ContainerGO.transform.GetChild( m_InternalIndex ).gameObject;
	}



	//////////////////////////////////////////////////////////////////////////
	// GetGameObject
	public		bool	GetGameObject( ref GameObject go )
	{
		go = GetGameObject();
		return go != null;
	}



	//////////////////////////////////////////////////////////////////////////
	// GetNextComponent
	public		T			GetNextComponent()
	{
		if ( IsValid == false )
			return null;

		m_InternalIndex ++;
		if ( m_InternalIndex >= m_ObjectsPool.Count )
		{
			m_InternalIndex = 0;
		}

		return m_ObjectsPool[ m_InternalIndex ];
	}



	//////////////////////////////////////////////////////////////////////////
	// GetAsModel
	public		T			PeekComponent()
	{
		if ( IsValid == false )
			return null;

		return m_ObjectsPool[ m_InternalIndex ];
	}



	//////////////////////////////////////////////////////////////////////////
	// GetAsModel
	public		T2			PeekComponent<T2>() where T2 : class
	{
		if ( IsValid == false )
			return null;

		return m_ObjectsPool[ m_InternalIndex ] as T2;
	}



	//////////////////////////////////////////////////////////////////////////
	// SetActive
	public		void		SetActive( bool state )
	{
		if ( IsValid == false )
			return;

		m_ContainerGO.SetActive( state );
	}



	//////////////////////////////////////////////////////////////////////////
	// Destroy
	internal	void		Destroy()
	{
		if ( IsValid == false )
			return;

		Counter --;

		if ( m_bIsBuilding == true )
		{
			CoroutinesManager.Stop( m_Coroutine );
		}
		
		m_ObjectsPool.Clear();

		Object.Destroy( m_ContainerGO );
	}

	

}


/// <summary> Components only pooler </summary>
public	class ObjectsPool<T> where T : UnityEngine.Component {

	private	List<T>		m_Storage			= null;
	private	int			m_InternalIndex		= 0;
	public	int			Count
	{
		get { return m_Storage.Count; }
	}


	//////////////////////////////////////////////////////////////////////////
	// Costructor
	public	ObjectsPool( uint size = 0 )
	{
		if ( size > 0 )
			m_Storage = new List<T>( (int)size );
	}


	//////////////////////////////////////////////////////////////////////////
	// Add
	public	void	Add( T newItem )
	{
		if ( newItem != null )
		{
			m_Storage.Add( newItem );
		}
	}

	//////////////////////////////////////////////////////////////////////////
	// Set
	public	void	Set( T[] array )
	{
		m_Storage = new List<T>( array );
		m_InternalIndex = 0;
	}


	//////////////////////////////////////////////////////////////////////////
	// Resize
	public void	Resize( uint newSize )
	{
		if ( m_Storage == null || ( m_Storage.Count == 0 && newSize > 0 ) )
		{
			m_Storage = new List<T>( (int)newSize );
		}

		if ( newSize == m_Storage.Count )
			return;

		m_InternalIndex = 0;
		
		m_Storage.Capacity = (int)newSize;
	}


	//////////////////////////////////////////////////////////////////////////
	// Get
	public	T	Get()
	{
		m_InternalIndex ++;
		if ( m_InternalIndex >= m_Storage.Count )
			m_InternalIndex = 0;

		return m_Storage[ m_InternalIndex ];
	}


	//////////////////////////////////////////////////////////////////////////
	// At
	public	T	At( int index )
	{
		if ( index >= m_Storage.Count )
			return null;

		return m_Storage[ index ];
	}


	//////////////////////////////////////////////////////////////////////////
	// Destroy
	internal	void	Destroy()
	{
		for ( int i = m_Storage.Count - 1; i >= 0; i-- )
		{
			Component comp = m_Storage[ i ];
			if ( comp.IsNotNull() )
			{
				Object.Destroy( comp );
			}
			m_Storage.RemoveAt(i);
		}
		m_Storage.Clear();
		m_Storage= null;
	}

}



public	class ClassPool<T> where T : class, new() {

	private	List<T>		m_Storage			= null;
	private	int			m_InternalIndex		= 0;
	public	int			Count
	{
		get { return m_Storage.Count; }
	}


	//////////////////////////////////////////////////////////////////////////
	// Costructor
	public	ClassPool( uint size = 0 )
	{
		m_Storage = new List<T>( (int)size );
		Resize( size );
	}


	//////////////////////////////////////////////////////////////////////////
	// Add
	public	void	Add( T newItem )
	{
		if ( newItem != null )
		{
			m_Storage.Add( newItem );
		}
	}

	//////////////////////////////////////////////////////////////////////////
	// Set
	public	void	Set( T[] array )
	{
		m_Storage = new List<T>( array );
		m_InternalIndex = 0;
	}


	//////////////////////////////////////////////////////////////////////////
	// Resize
	public bool	Resize( uint newSize )
	{
		// Skip for invalid new size
		if ( newSize == 0 )
		{
			return false;
		}

		// Return true if the requested size is the currently set
		if ( m_Storage.Count == newSize )
			return false;

		// Calculate the delta
		int delta = Mathf.Abs( m_Storage.Count - (int)newSize );


		// Enlarge
		if ( m_Storage.Count < newSize )
		{
			for ( int i = 0; i < delta; i++ )
			{
				m_Storage.Add( new T() );
			}
		}

		// Reduction
		if ( m_Storage.Count > newSize )
		{
			for ( int i = delta - 1; i >= 0; i-- )
			{
				m_Storage.RemoveAt(i);
			}
		}

		m_InternalIndex = 0;
		return true;
	}


	//////////////////////////////////////////////////////////////////////////
	// Get
	public	T	Get()
	{
		m_InternalIndex ++;
		if ( m_InternalIndex >= m_Storage.Count )
			m_InternalIndex = 0;

		return m_Storage[ m_InternalIndex ];
	}


	//////////////////////////////////////////////////////////////////////////
	// At
	public	T	At( int index )
	{
		if ( index >= m_Storage.Count )
			return default(T);

		return m_Storage[ index ];
	}


	//////////////////////////////////////////////////////////////////////////
	// Destroy
	internal	void	Destroy()
	{
		m_Storage.Clear();
		m_Storage= null;
	}

}



public	class ClassPoolStack<T> where T : class, new() {

	private	Stack<T>	m_Storage			= null;
	private	IEnumerator	m_Enumerator		= null;
	public	int			Count
	{
		get { return m_Storage.Count; }
	}


	//////////////////////////////////////////////////////////////////////////
	// Costructor
	public	ClassPoolStack( uint size = 0 )
	{
		m_Storage = new Stack<T>( (int)size );
		Resize( size );

		m_Enumerator = m_Storage.GetEnumerator();
	}


	//////////////////////////////////////////////////////////////////////////
	// Add
	public	void	Add( T newItem )
	{
		if ( newItem != null )
		{
			m_Storage.Push( newItem );
		}
	}

	//////////////////////////////////////////////////////////////////////////
	// Set
	public	void	Set( T[] array )
	{
		m_Storage = new Stack<T>( array );
		m_Enumerator = m_Storage.GetEnumerator();
	}

	//////////////////////////////////////////////////////////////////////////
	// Peek
	public	T	Peek()
	{
		return m_Enumerator.Current as T;
	}


	//////////////////////////////////////////////////////////////////////////
	// Resize
	public bool	Resize( uint newSize )
	{
		// Skip for invalid new size
		if ( newSize == 0 )
		{
			return false;
		}

		// Return true if the requested size is the currently set
		if ( m_Storage.Count == newSize )
			return false;

		// Calculate the delta
		int delta = Mathf.Abs( m_Storage.Count - (int)newSize );


		// Enlarge
		if ( m_Storage.Count < newSize )
		{
			for ( int i = 0; i < delta; i++ )
			{
				m_Storage.Push( new T() );
			}
		}

		// Reduction
		if ( m_Storage.Count > newSize )
		{
			for ( int i = delta - 1; i >= 0; i-- )
			{
				m_Storage.Pop();
			}
		}

		m_Enumerator.Reset();
		return true;
	}


	//////////////////////////////////////////////////////////////////////////
	// Get
	public	T	Get()
	{
		bool bHasNext = m_Enumerator.MoveNext();

		T result = m_Enumerator.Current as T;

		if ( bHasNext == false )
		{
			m_Enumerator.Reset();
		}

		return result;
	}


	//////////////////////////////////////////////////////////////////////////
	// Destroy
	internal	void	Destroy()
	{
		m_Storage.Clear();
		m_Storage= null;
	}

}