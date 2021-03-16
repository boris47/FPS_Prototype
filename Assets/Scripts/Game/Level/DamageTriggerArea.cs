using System.Collections.Generic;
using UnityEngine;

public enum EDamageType
{
	NONE,
	BALLISTIC,
	ENERGY,
	ELECTRO,
	EXPLOSIVE,
	FLAME,
	COUNT
}

public class DamageTriggerArea : MonoBehaviour
{
	[System.Serializable]
	private class EnteredGameObjectData
	{
		public	GameObject			enteredGameObject		= null;
		public	Entity				enteredEntity			= null;
		public	bool				bIsEntity				= false;
		public	int					objectID				= -1;
	}

	private			TriggerEvents					m_TriggerEvents					= null;
	private			Collider						m_Collider						= null;

	[TypeReferences.Inherits(typeof(Entity), AllowAbstract = true, ExcludeNone = true, IncludeBaseType = true)]
	public			TypeReferences.TypeReference	m_EntityType					= typeof(Entity);

	[SerializeField, ReadOnly]
	private			List<EnteredGameObjectData>		m_EnteredGameObjects			= new List<EnteredGameObjectData>();

	[SerializeField, Range(0, 150f)]
	private			float							m_EveryFrameAppliedDamage		= 10f;

	[SerializeField]
	private			EDamageType						m_DamageType					= EDamageType.BALLISTIC;


	//////////////////////////////////////////////////////////////////////////
	private void Awake()
	{
		CustomAssertions.IsTrue(transform.TryGetComponent(out m_Collider));

		if (transform.TryGetComponent(out m_TriggerEvents))
		{
			m_TriggerEvents.OnEnterEvent += OnEnter;
			m_TriggerEvents.OnExitEvent += OnExit;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnEnable()
	{
		m_EnteredGameObjects.Clear();

		Collider[] colliders = null;
		bool bHasColliders = false;

		if (m_Collider is BoxCollider)
		{
			BoxCollider thisCollider = m_Collider as BoxCollider;
			colliders = Physics.OverlapBox(thisCollider.transform.position, thisCollider.size, thisCollider.transform.rotation);
			bHasColliders = colliders.Length > 0;
		}

		if (m_Collider is SphereCollider)
		{
			SphereCollider thisCollider = m_Collider as SphereCollider;
			colliders = Physics.OverlapSphere(thisCollider.transform.position, thisCollider.radius);
			bHasColliders = colliders.Length > 0;
		}

		if (bHasColliders)
		{
			foreach(Collider collider in colliders)
			{
				OnEnter(collider.gameObject);
			}
		}

		if (CustomAssertions.IsNotNull(GameManager.UpdateEvents))
		{
			GameManager.UpdateEvents.OnFrame += OnFrame;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnDisable()
	{
		if (GameManager.UpdateEvents.IsNotNull())
		{
			GameManager.UpdateEvents.OnFrame -= OnFrame;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnFrame(float deltaTime)
	{
		for (int i = m_EnteredGameObjects.Count - 1; i >= 0; i--)
		{
			EnteredGameObjectData data = m_EnteredGameObjects[i];
			if (data.enteredGameObject == null)
			{
				m_EnteredGameObjects.RemoveAt(i);
				continue;
			}

			if (data.bIsEntity && data.enteredEntity == null)
			{
				data.bIsEntity = false;
			}

			if (data.bIsEntity)
			{
				data.enteredEntity.OnHittedDetails(Vector3.zero, null, m_DamageType, m_EveryFrameAppliedDamage * deltaTime, false);
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnEnter(GameObject go)
	{
		if (m_EnteredGameObjects.FindIndex(o => go.transform.root.GetInstanceID() == o.objectID) > -1)
		{
			return;
		}

		if (go.TryGetComponent(m_EntityType.Type, out Component comp))
		{
			Debug.Log($"OnEnter: Enter {go.name}");

			EnteredGameObjectData newData = new EnteredGameObjectData()
			{
				bIsEntity = go.transform.TrySearchComponent(ESearchContext.LOCAL, out Entity enteredEntity),
				enteredEntity = enteredEntity,
				enteredGameObject = go,
				objectID = go.transform.root.GetInstanceID()
			};
			m_EnteredGameObjects.Add(newData);
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnExit(GameObject go)
	{
		int IDToFind = go.transform.root.GetInstanceID();
		int index = m_EnteredGameObjects.FindIndex(g => g.objectID == IDToFind);
		if (index > -1)
		{
			Debug.Log($"Exit {go.name}");
			m_EnteredGameObjects.RemoveAt(index);
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnDrawGizmos()
	{
		if (transform.TryGetComponent(out m_Collider))
		{
			Matrix4x4 mat = Gizmos.matrix;
			Gizmos.matrix = transform.localToWorldMatrix;

			if (m_Collider is BoxCollider boxCollider)
			{
				Gizmos.DrawCube(Vector3.zero, boxCollider.size);
			}

			if (m_Collider is SphereCollider sphereCollider)
			{
				Gizmos.DrawSphere(Vector3.zero, sphereCollider.radius);
			}

			Gizmos.matrix = mat;
		}
	}
}
