
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary> Object pooler with a specified component added on every object </summary>
public	class GameObjectsPool<T> where T : UnityEngine.Component  {
	
	private	static	int			Counter				= 0;
	private	GameObject			m_Container			= null;
	private	List<T>				m_ObjectsPool		= null;
	private	int					m_InternalIndex		= 0;
	private	System.Action<T>	m_ActionOnObject	= delegate( T component ) { };
	private	GameObject			m_ModelGO			= null;

	public	bool				IsValid
	{
		get { return m_Container != null && m_ObjectsPool != null && m_ModelGO != null; }
	}


	//////////////////////////////////////////////////////////////////////////
	// CONSTRUCTOR
	public	GameObjectsPool( GameObject model, uint size, string containerName = "GameObjectsContainer_", Transform parent = null, System.Action<T> actionOnObject = null )
	{
		if ( model != null && size > 0 )
		{
			m_ModelGO = model;

			m_Container = new GameObject( containerName + Counter.ToString() );
			Counter ++;

			if ( parent != null )
			{
				m_Container.transform.SetParent( parent );
			}

			// Assign action for every object
			if ( actionOnObject != null )
			{
				m_ActionOnObject = actionOnObject;
			}

			// Create the internal pool
			m_ObjectsPool = new List<T>( (int)size );
			{
				for ( uint i = 0; i < size; i++ )
				{
					T comp = Createitem( model );
					m_ActionOnObject( comp );
					m_ObjectsPool.Add( comp );
				}
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// Convert
	public		bool		Convert( GameObject model, System.Action<T> actionOnObject = null )
	{
		bool bIsValid = model != null;
		if ( bIsValid )
		{
			m_ModelGO = model;

			if ( actionOnObject != null )
			{
				m_ActionOnObject = actionOnObject;
			}

			int size = m_ObjectsPool.Count;
			m_ObjectsPool.Clear();

			// Clear previous childs
			for ( int i = m_Container.transform.childCount - 1; i >= 0; i-- )
			{
				Object.Destroy( m_Container.transform.GetChild(i).gameObject );
			}

			// Create the internal pool
			m_ObjectsPool = new List<T>( size );
			{
				for ( uint i = 0; i < size; i++ )
				{
					T comp = Createitem( model );
					m_ActionOnObject( comp );
					m_ObjectsPool.Add( comp );
				}
			}
		}
		return bIsValid;
	}


	//////////////////////////////////////////////////////////////////////////
	// Createitem
	private		T			Createitem( GameObject model )
	{	//											Model	Position,		Rotation			Parent
		GameObject objectCopy = Object.Instantiate( model, Vector3.zero, Quaternion.identity, m_Container.transform );
		T comp = objectCopy.GetComponent<T>();
		return comp;
	}


	//////////////////////////////////////////////////////////////////////////
	// ExecuteActionOnObject
	public		void		ExecuteActionOnObjectr( System.Action<T> actionOnObject )
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
		if ( m_Container.transform.childCount == newSize )
			return true;

		// Calculate the delta
		int delta = Mathf.Abs( m_Container.transform.childCount - (int)newSize );

		// Enlarge
		if ( m_Container.transform.childCount < newSize )
		{
			for ( int i = 0; i < delta; i++ )
			{
				T comp = Createitem( m_ModelGO );
				m_ActionOnObject( comp );
				m_ObjectsPool.Add( comp );
			}
		}

		// Reduction
		if ( m_Container.transform.childCount > newSize )
		{
			for ( int i = delta - 1; i >= 0; i-- )
			{
				Object.Destroy( m_Container.transform.GetChild(i).gameObject );
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

		return m_Container.transform.GetChild( m_InternalIndex ).gameObject;
	}


	//////////////////////////////////////////////////////////////////////////
	// GetGameObject
	public		bool	GetGameObject( ref GameObject go )
	{
		go = GetGameObject();
		return go != null;
	}


	//////////////////////////////////////////////////////////////////////////
	// GetComponent
	public		T			GetComponent()
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
	// GetComponent
	public		bool		GetComponent( ref T comp )
	{
		comp = GetComponent();
		return comp != null;
	}


	//////////////////////////////////////////////////////////////////////////
	// GetAsModel
	public		T			GetAsModel()
	{
		if ( IsValid == false )
			return null;

		return m_ObjectsPool[ m_InternalIndex ];
	}


	//////////////////////////////////////////////////////////////////////////
	// SetActive
	public		void		SetActive( bool state )
	{
		if ( IsValid == false )
			return;

		m_Container.SetActive( state );
	}


	//////////////////////////////////////////////////////////////////////////
	// Destroy
	internal	void		Destroy()
	{
		if ( IsValid == false )
			return;

		Counter --;
		for ( int i = m_Container.transform.childCount - 1; i >= 0; i-- )
		{
			Object.Destroy( m_Container.transform.GetChild( i ).gameObject );
		}

		Object.Destroy( m_Container );
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