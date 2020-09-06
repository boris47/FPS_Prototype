
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
	private bool				m_IsBuilding		= false;

	
	// Iterations
	public List<T>.Enumerator  GetEnumerator()
	{
		return this.m_ObjectsPool.GetEnumerator();
	}

	public	bool				IsValid
	{
		get { return this.m_ContainerGO.IsNotNull() && this.m_ObjectsPool.IsNotNull() && this.m_ModelGO.IsNotNull(); }
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
		this.m_ModelGO				= constructorData.Model;
		string containerName	= constructorData.ContainerName + Counter.ToString();
		uint poolSize			= constructorData.Size;


		// Assign action for every object
		this.m_ActionOnObject = constructorData.ActionOnObject ?? this.m_ActionOnObject;


		// Create game object container
		this.m_ContainerGO = new GameObject( containerName );
		this.m_ContainerGO.transform.SetPositionAndRotation( Vector3.up * 80000f, Quaternion.identity ); 
		Object.DontDestroyOnLoad(this.m_ContainerGO);
		Counter ++;
		

		// Create the internal pool
		if ( constructorData.IsAsyncBuild ) // Asyncronously
		{
			constructorData.CoroutineEnumerator = this.CreateItemsCO( constructorData );
			this.m_IsBuilding = true;
			this.m_Coroutine = CoroutinesManager.Start( constructorData.CoroutineEnumerator, "GameObjectsPool::Constructor: Create items of " + this.m_ModelGO.name );
		}
		else
		// Instantly
		{
			this.m_ObjectsPool = new List<T>( (int)( poolSize ) );
			{
				for ( uint i = 0; i < poolSize; i++ )
				{
					T comp = this.Createitem(this.m_ModelGO );
					this.m_ActionOnObject( comp );
					this.m_ObjectsPool.Add( comp );
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

		this.m_ObjectsPool = new List<T>( size );

		for ( uint i = 0; i < size; i++ )
		{
//				Debug.Log( "Creating " + model );
			T comp = this.Createitem(this.m_ModelGO );
			this.m_ActionOnObject( comp );
			this.m_ObjectsPool.Add( comp );
			yield return null;
		}
		
		constructorData.ActionOnLoadEnd();
		this.m_IsBuilding = false;
		this.m_Coroutine = null;
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

		this.m_ModelGO = model;

		if (this.m_IsBuilding == true )
		{
			CoroutinesManager.Stop(this.m_Coroutine );
		}

		this.m_ActionOnObject = actionOnObject ?? this.m_ActionOnObject;

		Utils.Base.DestroyChildren(this.m_ContainerGO.transform );

		int size = this.m_ObjectsPool.Count;
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
		this.m_ObjectsPool.Clear();
			
		// Create the internal pool
		for ( uint i = 0; i < size; i++ )
		{
			T comp = this.Createitem( model );
			this.m_ActionOnObject( comp );
			this.m_ObjectsPool.Add( comp );
		}
		
		return true;
	}



	//////////////////////////////////////////////////////////////////////////
	// Createitem
	private		T			Createitem( GameObject model )
	{	//											Model	Position,		Rotation			Parent
		GameObject objectCopy = Object.Instantiate( model, Vector3.zero, Quaternion.identity, this.m_ContainerGO.transform );
		T comp = objectCopy.GetComponent<T>();
		return comp;
	}



	//////////////////////////////////////////////////////////////////////////
	// ExecuteActionOnObject
	public		void		ExecuteActionOnObjects( System.Action<T> actionOnObject )
	{
		this.m_ObjectsPool.ForEach( actionOnObject );
	}



	//////////////////////////////////////////////////////////////////////////
	// Resize
	public		bool		Resize( uint newSize )
	{
		if (this.IsValid == false )
			return false;

		// Skip for invalid new size
		if ( newSize == 0 )
		{
			return false;
		}

		// Return true if the requested size is the currently set
		if (this.m_ObjectsPool.Count == newSize )
			return true;

		// Calculate the delta
		int delta = Mathf.Abs(this.m_ObjectsPool.Count - (int)newSize );

		int childCount = this.m_ObjectsPool.Count;

		if (this.m_IsBuilding == true )
		{
			CoroutinesManager.Stop(this.m_Coroutine );
		}

		// Enlarge
		if ( childCount < newSize )
		{
			for ( int i = 0; i < delta; i++ )
			{
				T comp = this.Createitem(this.m_ModelGO );
				this.m_ActionOnObject( comp );
				this.m_ObjectsPool.Add( comp );
			}
		}
		
		// Reduction
		if ( childCount > newSize )
		{
			for ( int i = delta - 1; i >= 0; i-- )
			{
				Component comp = this.m_ObjectsPool[i];
				this.m_ObjectsPool.RemoveAt(i);
				Object.Destroy( comp.gameObject );
			}
		}

		this.m_InternalIndex = 0;
		return true;
	}



	//////////////////////////////////////////////////////////////////////////
	// GetGameObject
	public		GameObject	GetGameObject()
	{
		if (this.IsValid == false )
			return null;

		if (this.m_InternalIndex >= this.m_ObjectsPool.Count )
		{
			this.m_InternalIndex = 0;
		}

		return this.m_ContainerGO.transform.GetChild(this.m_InternalIndex ).gameObject;
	}



	//////////////////////////////////////////////////////////////////////////
	// GetGameObject
	public		bool	GetGameObject( ref GameObject go )
	{
		go = this.GetGameObject();
		return go != null;
	}



	//////////////////////////////////////////////////////////////////////////
	// GetNextComponent
	public		T			GetNextComponent()
	{
		if (this.IsValid == false )
			return null;

		this.m_InternalIndex ++;
		if (this.m_InternalIndex >= this.m_ObjectsPool.Count )
		{
			this.m_InternalIndex = 0;
		}

		return this.m_ObjectsPool[this.m_InternalIndex ];
	}



	//////////////////////////////////////////////////////////////////////////
	// GetAsModel
	public		T			PeekComponent()
	{
		if (this.IsValid == false )
			return null;

		return this.m_ObjectsPool[this.m_InternalIndex ];
	}



	//////////////////////////////////////////////////////////////////////////
	// GetAsModel
	public		T2			PeekComponent<T2>() where T2 : class
	{
		if (this.IsValid == false )
			return null;

		return this.m_ObjectsPool[this.m_InternalIndex ] as T2;
	}



	//////////////////////////////////////////////////////////////////////////
	// SetActive
	public		void		SetActive( bool state )
	{
		if (this.IsValid == false )
			return;

		this.m_ContainerGO.SetActive( state );
	}



	//////////////////////////////////////////////////////////////////////////
	// Destroy
	internal	void		Destroy()
	{
		if (this.IsValid == false )
			return;

		Counter --;

		if (this.m_IsBuilding == true )
		{
			CoroutinesManager.Stop(this.m_Coroutine );
		}

		this.m_ObjectsPool.Clear();

		Object.Destroy(this.m_ContainerGO );
	}

	

}


/// <summary> Components only pooler </summary>
public	class ObjectsPool<T> where T : UnityEngine.Component {

	private	List<T>		m_Storage			= null;
	private	int			m_InternalIndex		= 0;
	public	int			Count
	{
		get { return this.m_Storage.Count; }
	}


	//////////////////////////////////////////////////////////////////////////
	// Costructor
	public	ObjectsPool( uint size = 0 )
	{
		if ( size > 0 )
			this.m_Storage = new List<T>( (int)size );
	}


	//////////////////////////////////////////////////////////////////////////
	// Add
	public	void	Add( T newItem )
	{
		if ( newItem != null )
		{
			this.m_Storage.Add( newItem );
		}
	}

	//////////////////////////////////////////////////////////////////////////
	// Set
	public	void	Set( T[] array )
	{
		this.m_Storage = new List<T>( array );
		this.m_InternalIndex = 0;
	}


	//////////////////////////////////////////////////////////////////////////
	// Resize
	public void	Resize( uint newSize )
	{
		if (this.m_Storage == null || (this.m_Storage.Count == 0 && newSize > 0 ) )
		{
			this.m_Storage = new List<T>( (int)newSize );
		}

		if ( newSize == this.m_Storage.Count )
			return;

		this.m_InternalIndex = 0;

		this.m_Storage.Capacity = (int)newSize;
	}


	//////////////////////////////////////////////////////////////////////////
	// Get
	public	T	Get()
	{
		this.m_InternalIndex ++;
		if (this.m_InternalIndex >= this.m_Storage.Count )
			this.m_InternalIndex = 0;

		return this.m_Storage[this.m_InternalIndex ];
	}


	//////////////////////////////////////////////////////////////////////////
	// At
	public	T	At( int index )
	{
		if ( index >= this.m_Storage.Count )
			return null;

		return this.m_Storage[ index ];
	}


	//////////////////////////////////////////////////////////////////////////
	// Destroy
	internal	void	Destroy()
	{
		for ( int i = this.m_Storage.Count - 1; i >= 0; i-- )
		{
			Component comp = this.m_Storage[ i ];
			if ( comp.IsNotNull() )
			{
				Object.Destroy( comp );
			}
			this.m_Storage.RemoveAt(i);
		}
		this.m_Storage.Clear();
		this.m_Storage = null;
	}

}



public	class ClassPool<T> where T : class, new() {

	private	List<T>		m_Storage			= null;
	private	int			m_InternalIndex		= 0;
	public	int			Count
	{
		get { return this.m_Storage.Count; }
	}


	//////////////////////////////////////////////////////////////////////////
	// Costructor
	public	ClassPool( uint size = 0 )
	{
		this.m_Storage = new List<T>( (int)size );
		this.Resize( size );
	}


	//////////////////////////////////////////////////////////////////////////
	// Add
	public	void	Add( T newItem )
	{
		if ( newItem != null )
		{
			this.m_Storage.Add( newItem );
		}
	}

	//////////////////////////////////////////////////////////////////////////
	// Set
	public	void	Set( T[] array )
	{
		this.m_Storage = new List<T>( array );
		this.m_InternalIndex = 0;
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
		if (this.m_Storage.Count == newSize )
			return false;

		// Calculate the delta
		int delta = Mathf.Abs(this.m_Storage.Count - (int)newSize );


		// Enlarge
		if (this.m_Storage.Count < newSize )
		{
			for ( int i = 0; i < delta; i++ )
			{
				this.m_Storage.Add( new T() );
			}
		}

		// Reduction
		if (this.m_Storage.Count > newSize )
		{
			for ( int i = delta - 1; i >= 0; i-- )
			{
				this.m_Storage.RemoveAt(i);
			}
		}

		this.m_InternalIndex = 0;
		return true;
	}


	//////////////////////////////////////////////////////////////////////////
	// Get
	public	T	Get()
	{
		this.m_InternalIndex ++;
		if (this.m_InternalIndex >= this.m_Storage.Count )
			this.m_InternalIndex = 0;

		return this.m_Storage[this.m_InternalIndex ];
	}


	//////////////////////////////////////////////////////////////////////////
	// At
	public	T	At( int index )
	{
		if ( index >= this.m_Storage.Count )
			return default(T);

		return this.m_Storage[ index ];
	}


	//////////////////////////////////////////////////////////////////////////
	// Destroy
	internal	void	Destroy()
	{
		this.m_Storage.Clear();
		this.m_Storage = null;
	}

}



public	class ClassPoolStack<T> where T : class, new() {

	private	Stack<T>	m_Storage			= null;
	private	IEnumerator	m_Enumerator		= null;
	public	int			Count
	{
		get { return this.m_Storage.Count; }
	}


	//////////////////////////////////////////////////////////////////////////
	// Costructor
	public	ClassPoolStack( uint size = 0 )
	{
		this.m_Storage = new Stack<T>( (int)size );
		this.Resize( size );

		this.m_Enumerator = this.m_Storage.GetEnumerator();
	}


	//////////////////////////////////////////////////////////////////////////
	// Add
	public	void	Add( T newItem )
	{
		if ( newItem != null )
		{
			this.m_Storage.Push( newItem );
		}
	}

	//////////////////////////////////////////////////////////////////////////
	// Set
	public	void	Set( T[] array )
	{
		this.m_Storage = new Stack<T>( array );
		this.m_Enumerator = this.m_Storage.GetEnumerator();
	}

	//////////////////////////////////////////////////////////////////////////
	// Peek
	public	T	Peek()
	{
		return this.m_Enumerator.Current as T;
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
		if (this.m_Storage.Count == newSize )
			return false;

		// Calculate the delta
		int delta = Mathf.Abs(this.m_Storage.Count - (int)newSize );


		// Enlarge
		if (this.m_Storage.Count < newSize )
		{
			for ( int i = 0; i < delta; i++ )
			{
				this.m_Storage.Push( new T() );
			}
		}

		// Reduction
		if (this.m_Storage.Count > newSize )
		{
			for ( int i = delta - 1; i >= 0; i-- )
			{
				this.m_Storage.Pop();
			}
		}

		this.m_Enumerator.Reset();
		return true;
	}


	//////////////////////////////////////////////////////////////////////////
	// Get
	public	T	Get()
	{
		bool bHasNext = this.m_Enumerator.MoveNext();

		T result = this.m_Enumerator.Current as T;

		if ( bHasNext == false )
		{
			this.m_Enumerator.Reset();
		}

		return result;
	}


	//////////////////////////////////////////////////////////////////////////
	// Destroy
	internal	void	Destroy()
	{
		this.m_Storage.Clear();
		this.m_Storage = null;
	}

}