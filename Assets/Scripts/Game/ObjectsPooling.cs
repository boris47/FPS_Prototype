
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GameObjectsPoolConstructorData<T>
{
	public		GameObject				Model { private set; get; }		= null;
	public		uint					Size  { private set; get; }		= 1;
	public		string					ContainerName					= "GameObjectsContainer_";
	public		System.Action<T>		ActionOnObject					= delegate { };
	public		System.Action			ActionOnLoadEnd					= delegate { };
	public		IEnumerator				CoroutineEnumerator				= null;
	public		bool					IsAsyncBuild					= false;

	public GameObjectsPoolConstructorData(GameObject model, uint size)
	{
		Model = model;
		Size = size;
	}
}


/// <summary> Objects pool with a specified component added on every object </summary>
public class GameObjectsPool<T> where T : UnityEngine.Component
{
	private		static int				Counter					= 0;
	private		GameObject				m_ContainerGO			= null;
	private		List<T>					m_ObjectsPool			= new List<T>();
	private		int						m_InternalIndex			= 0;
	private		System.Action<T>		m_ActionOnObject		= delegate { };
	private		GameObject				m_ModelGO				= null;
	private		Coroutine				m_Coroutine				= null;
	private		bool					m_IsBuilding			= false;
	private		readonly string			m_PoolId				= string.Empty;

	public List<T>.Enumerator GetEnumerator() => m_ObjectsPool.GetEnumerator();
	public bool IsValid => m_ContainerGO.IsNotNull() && m_ObjectsPool.IsNotNull() && m_ModelGO.IsNotNull();
	public bool IsReady => !m_IsBuilding && m_Coroutine == null;


	//////////////////////////////////////////////////////////////////////////
	// CONSTRUCTOR
	public GameObjectsPool(GameObjectsPoolConstructorData<T> constructorData)
	{
		CustomAssertions.IsNotNull
		(
			constructorData.Model,
			$"GameObjectsPool trying to create a pool with null Model for component {typeof(T).Name}"
		);

		CustomAssertions.IsTrue
		(
			constructorData.Size > 0,
			$"GameObjectsPool trying to create a pool with null Model for component {typeof(T).Name}"
		);

		CustomAssertions.IsTrue
		(
			constructorData.Model.transform.HasComponent<T>(),
			$"GameObjectsPool trying to create a pool with Model with no component {typeof(T).Name}, Model is {constructorData.Model.name}"
		);

		// Get data from GameObjectsPoolConstructorData
		m_ModelGO = constructorData.Model;
		m_PoolId = $"{constructorData.ContainerName}_{Counter.ToString()}";

		// Assign action for every object
		m_ActionOnObject = constructorData.ActionOnObject ?? m_ActionOnObject;

		// Create game object container
		m_ContainerGO = new GameObject(m_PoolId);
		m_ContainerGO.transform.SetPositionAndRotation(Vector3.down * 80000f, Quaternion.identity);
		Object.DontDestroyOnLoad(m_ContainerGO);
		Counter++;

//		m_ObjectsPool = new List<T>((int)constructorData.Size);

		m_ObjectsPool.Resize(constructorData.Size);

		// Create the internal pool
		if (constructorData.IsAsyncBuild) // Asyncronously
		{
			constructorData.CoroutineEnumerator = CreateItemsCO(constructorData);
			m_IsBuilding = true;
			m_Coroutine = CoroutinesManager.Start(constructorData.CoroutineEnumerator, $"GameObjectsPool::Constructor: Create items of {m_ModelGO.name}");
		}
		else // Synchronously
		{
			for (uint i = 0; i < constructorData.Size; i++)
			{
				T comp = CreateItem(m_ModelGO);
				m_ActionOnObject(comp);
				m_ObjectsPool.Add(comp);
			}
		}
	}



	//////////////////////////////////////////////////////////////////////////
	// CreateItemsCO
	private IEnumerator CreateItemsCO(GameObjectsPoolConstructorData<T> constructorData)
	{
		yield return null;

		for (uint i = 0; i < constructorData.Size; i++)
		{
			T comp = CreateItem(m_ModelGO);
			m_ActionOnObject(comp);
			m_ObjectsPool.Add(comp);
			yield return null;
		}

		constructorData.ActionOnLoadEnd();
		m_IsBuilding = false;
		m_Coroutine = null;
	}



	//////////////////////////////////////////////////////////////////////////
	// Convert
	public bool Convert(GameObject model, System.Action<T> actionOnObject = null)
	{
		Debug.AssertFormat
		(
			model != null,
			"Trying to covert a GameObjectsPool using null model"
		);

		m_ModelGO = model;

		if (m_IsBuilding == true)
		{
			CoroutinesManager.Stop(m_Coroutine);
		}

		m_ActionOnObject = actionOnObject ?? m_ActionOnObject;

		CoroutinesManager.Start(Utils.Base.DestroyChildren(m_ContainerGO.transform));

		int size = m_ObjectsPool.Count;
		m_ObjectsPool.Clear();

		// Create the internal pool
		for (uint i = 0; i < size; i++)
		{
			T comp = CreateItem(model);
			m_ActionOnObject(comp);
			m_ObjectsPool.Add(comp);
		}

		return true;
	}



	//////////////////////////////////////////////////////////////////////////
	/// <summary> Internally create the model object </summary>
	private T CreateItem(GameObject model)
	{
		GameObject objectCopy = Object.Instantiate(model, Vector3.zero, Quaternion.identity, m_ContainerGO.transform);
		objectCopy.TryGetComponent(out T comp);
		return comp;
	}



	//////////////////////////////////////////////////////////////////////////
	/// <summary> Execute action on each element of the pool </summary>
	public void ExecuteActionOnObjects(System.Action<T> actionOnObject)
	{
		m_ObjectsPool.ForEach(actionOnObject);
	}



	//////////////////////////////////////////////////////////////////////////
	/// <summary> Resize the pool </summary>
	public bool Resize(uint newSize)
	{
		// Skip for invalid new size
		if (IsValid == false || newSize == 0)
		{
			return false;
		}
		
		// Return true if the requested size is the currently set
		if (m_ObjectsPool.Count == newSize)
		{
			return true;
		}

		if (m_IsBuilding == true)
		{
			CoroutinesManager.Stop(m_Coroutine);
		}

		// Calculate the delta
		int delta = Mathf.Abs(m_ObjectsPool.Count - (int)newSize);
		int childCount = m_ObjectsPool.Count;
		if (childCount < newSize) // Enlarge
		{
			for (int i = 0; i < delta; i++)
			{
				T comp = CreateItem(m_ModelGO);
				m_ActionOnObject(comp);
				m_ObjectsPool.Add(comp);
			}
		}
		else // Reduction
		{
			for (int i = delta - 1; i >= 0; i--)
			{
				Component comp = m_ObjectsPool[i];
				m_ObjectsPool.RemoveAt(i);
				Object.Destroy(comp.gameObject);
			}
		}

		// Reset internal index
		m_InternalIndex = 0;
		return true;
	}



	//////////////////////////////////////////////////////////////////////////
	/// <summary> Get the GameObject at the current index </summary>
	public GameObject GetGameObject()
	{
		if (m_ContainerGO.IsNotNull())
		{
			if (m_InternalIndex >= m_ObjectsPool.Count)
			{
				m_InternalIndex = 0;
			}
			return m_ContainerGO.transform.GetChild(m_InternalIndex).gameObject;
		}
		return null;
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Get the Component at the current index </summary>
	public T PeekComponent()
	{
		return m_ObjectsPool[m_InternalIndex];
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Get the Component at the current index cast to gived type </summary>
	public T2 TryPeekComponentAs<T2>() where T2 : class
	{
		return m_ObjectsPool[m_InternalIndex] as T2;
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Get the Component at next index </summary>
	public T GetNextComponent()
	{
		if (++m_InternalIndex >= m_ObjectsPool.Count)
		{
			m_InternalIndex = 0;
		}
		return m_ObjectsPool[m_InternalIndex];
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Set active state of the pool container </summary>
	public void SetActive(bool state)
	{
		if (m_ContainerGO.IsNotNull())
		{
			m_ContainerGO.SetActive(state);
		}
	}


	//////////////////////////////////////////////////////////////////////////
	internal void Destroy()
	{
		Counter--;

		if (m_IsBuilding == true)
		{
			CoroutinesManager.Stop(m_Coroutine);
		}
		m_ObjectsPool.Clear();
		if (m_ContainerGO)
		{
			Object.Destroy(m_ContainerGO);
		}
	}
}


/// <summary> Components only pooler </summary>
public class ObjectsPool<T> where T : UnityEngine.Component
{

	private List<T> m_Storage = null;
	private int m_InternalIndex = 0;
	public int Count
	{
		get { return m_Storage.Count; }
	}


	//////////////////////////////////////////////////////////////////////////
	// Costructor
	public ObjectsPool(uint size = 0)
	{
		if (size > 0)
		{
			m_Storage = new List<T>();
			m_Storage.Resize(size);
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// Add
	public void Add(T newItem)
	{
		if (newItem != null)
		{
			m_Storage.Add(newItem);
		}
	}

	//////////////////////////////////////////////////////////////////////////
	// Set
	public void Set(T[] array)
	{
		m_Storage = new List<T>(array);
		m_InternalIndex = 0;
	}


	//////////////////////////////////////////////////////////////////////////
	// Resize
	public void Resize(uint newSize)
	{
		if (m_Storage == null || (m_Storage.Count == 0 && newSize > 0))
		{
			m_Storage = new List<T>();
		}

		if (newSize == m_Storage.Count)
			return;

		m_InternalIndex = 0;

		m_Storage.Resize(newSize);
	}


	//////////////////////////////////////////////////////////////////////////
	// Get
	public T Get()
	{
		m_InternalIndex++;
		if (m_InternalIndex >= m_Storage.Count)
		{
			m_InternalIndex = 0;
		}

		return m_Storage[m_InternalIndex];
	}


	//////////////////////////////////////////////////////////////////////////
	// At
	public T At(int index)
	{
		return index >= m_Storage.Count ? null : m_Storage[index];
	}


	//////////////////////////////////////////////////////////////////////////
	// Destroy
	internal void Destroy()
	{
		for (int i = m_Storage.Count - 1; i >= 0; i--)
		{
			Component comp = m_Storage[i];
			if (comp.IsNotNull())
			{
				Object.Destroy(comp);
			}
		}
		m_Storage.Clear();
		m_Storage = null;
	}

}



public class ClassPool<T> where T : class, new()
{

	private List<T> m_Storage = null;
	private int m_InternalIndex = 0;
	public int Count
	{
		get { return m_Storage.Count; }
	}


	//////////////////////////////////////////////////////////////////////////
	// Costructor
	public ClassPool(uint size = 0)
	{
		m_Storage = new List<T>((int)size);
		Resize(size);
	}


	//////////////////////////////////////////////////////////////////////////
	// Add
	public void Add(T newItem)
	{
		if (newItem != null)
		{
			m_Storage.Add(newItem);
		}
	}

	//////////////////////////////////////////////////////////////////////////
	// Set
	public void Set(T[] array)
	{
		m_Storage = new List<T>(array);
		m_InternalIndex = 0;
	}


	//////////////////////////////////////////////////////////////////////////
	// Resize
	public bool Resize(uint newSize)
	{
		// Skip for invalid new size
		if (newSize == 0)
		{
			return false;
		}

		// Return true if the requested size is the currently set
		if (m_Storage.Count == newSize)
			return false;

		// Calculate the delta
		int delta = Mathf.Abs(m_Storage.Count - (int)newSize);


		// Enlarge
		if (m_Storage.Count < newSize)
		{
			for (int i = 0; i < delta; i++)
			{
				m_Storage.Add(new T());
			}
		}

		// Reduction
		if (m_Storage.Count > newSize)
		{
			for (int i = delta - 1; i >= 0; i--)
			{
				m_Storage.RemoveAt(i);
			}
		}

		m_InternalIndex = 0;
		return true;
	}


	//////////////////////////////////////////////////////////////////////////
	// Get
	public T Get()
	{
		m_InternalIndex++;
		if (m_InternalIndex >= m_Storage.Count)
			m_InternalIndex = 0;

		return m_Storage[m_InternalIndex];
	}


	//////////////////////////////////////////////////////////////////////////
	// At
	public T At(int index)
	{
		if (index >= m_Storage.Count)
			return default(T);

		return m_Storage[index];
	}


	//////////////////////////////////////////////////////////////////////////
	// Destroy
	internal void Destroy()
	{
		m_Storage.Clear();
		m_Storage = null;
	}

}



public class ClassPoolStack<T> where T : class, new()
{

	private Stack<T> m_Storage = null;
	private IEnumerator m_Enumerator = null;
	public int Count => m_Storage.Count;


	//////////////////////////////////////////////////////////////////////////
	// Costructor
	public ClassPoolStack(uint size = 0)
	{
		m_Storage = new Stack<T>((int)size);
		Resize(size);

		m_Enumerator = m_Storage.GetEnumerator();
	}


	//////////////////////////////////////////////////////////////////////////
	// Add
	public void Add(T newItem)
	{
		if (newItem.IsNotNull())
		{
			m_Storage.Push(newItem);
		}
	}

	//////////////////////////////////////////////////////////////////////////
	// Set
	public void Set(T[] array)
	{
		m_Storage = new Stack<T>(array);
		m_Enumerator = m_Storage.GetEnumerator();
	}

	//////////////////////////////////////////////////////////////////////////
	// Peek
	public T Peek()
	{
		return m_Enumerator.Current as T;
	}


	//////////////////////////////////////////////////////////////////////////
	// Resize
	public bool Resize(uint newSize)
	{
		// Skip for invalid new size
		if (newSize == 0)
		{
			return false;
		}

		// Return true if the requested size is the currently set
		if (m_Storage.Count == newSize)
			return false;

		// Calculate the delta
		int delta = Mathf.Abs(m_Storage.Count - (int)newSize);

		// Enlarge
		if (m_Storage.Count < newSize)
		{
			for (int i = 0; i < delta; i++)
			{
				m_Storage.Push(new T());
			}
		}

		// Reduction
		if (m_Storage.Count > newSize)
		{
			for (int i = delta - 1; i >= 0; i--)
			{
				m_Storage.Pop();
			}
		}

		m_Enumerator.Reset();
		return true;
	}


	//////////////////////////////////////////////////////////////////////////
	// Get
	public T Get()
	{
		bool bHasNext = m_Enumerator.MoveNext();

		T result = m_Enumerator.Current as T;

		if (bHasNext == false)
		{
			m_Enumerator.Reset();
		}

		return result;
	}


	//////////////////////////////////////////////////////////////////////////
	// Destroy
	internal void Destroy()
	{
		m_Storage.Clear();
		m_Storage = null;
	}

}