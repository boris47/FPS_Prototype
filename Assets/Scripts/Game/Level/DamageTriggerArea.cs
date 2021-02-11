using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EDamageType {
	NONE,
	BALLISTIC,
	ENERGY,
	ELECTRO,
	EXPLOSIVE,
	FLAME,
	COUNT
}



public class DamageTriggerArea : MonoBehaviour {

	[System.Serializable]
	private class EnteredGameObjectData {

		public	GameObject			EnteredGameObject		= null;
		public	Entity				EnteredEntity			= null;
		public	bool				bIsEntity				= false;
		public	int					ObjectID				= -1;
	}

	private			TriggerEvents		m_TriggerEvents					= null;

	private			Collider			m_Collider						= null;

	//	[SerializeField, ClassExtends(baseType: typeof(Entity), AllowAbstract = true)]
	//	public ClassTypeReference m_EntityType = typeof(Entity);
	[TypeReferences.Inherits(typeof(Entity), AllowAbstract = true, ExcludeNone = true, IncludeBaseType = true)]
	public TypeReferences.TypeReference m_EntityType = typeof(Entity);

	[SerializeField, ReadOnly]
	private			bool				m_IsActiveArea					= false;

	[SerializeField, ReadOnly]
	private			List<EnteredGameObjectData> m_EnteredGameObjects	= new List<EnteredGameObjectData>();

	[SerializeField, Range( 0, 150f )]
	private			float				m_EveryFrameAppliedDamage		= 10f;

	[SerializeField]
	private			EDamageType			m_DamageType					= EDamageType.BALLISTIC;


	//////////////////////////////////////////////////////////////////////////
	private void Awake()
	{
		
		m_IsActiveArea = transform.TrySearchComponent(ESearchContext.LOCAL, out m_TriggerEvents);
		m_IsActiveArea &= transform.TrySearchComponent(ESearchContext.LOCAL, out m_Collider);

		if (m_IsActiveArea )
		{
			m_TriggerEvents.OnEnterEvent += OnEnter;
			m_TriggerEvents.OnExitEvent += OnExit;

			GameManager.UpdateEvents.OnFrame += UpdateEvents_OnFrame;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnEnable()
	{
		m_IsActiveArea = true;

		m_EnteredGameObjects.Clear();

		Collider[] colliders = null;
		bool bHasColliders = false;

		if (m_Collider is BoxCollider )
		{
			BoxCollider thisCollider = m_Collider as BoxCollider;
			colliders = Physics.OverlapBox( thisCollider.transform.position, thisCollider.size, thisCollider.transform.rotation );
			bHasColliders = colliders.Length > 0;
		}

		if (m_Collider is SphereCollider )
		{
			SphereCollider thisCollider = m_Collider as SphereCollider;
			colliders = Physics.OverlapSphere( thisCollider.transform.position, thisCollider.radius );
			bHasColliders = colliders.Length > 0;
		}

		if ( bHasColliders )
		{
			for ( int i = 0; i < colliders.Length; i++ )
			{
				Collider collider = colliders[i];
				OnEnter( collider.gameObject );
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnDisable()
	{
		m_IsActiveArea = false;
	}


	//////////////////////////////////////////////////////////////////////////
	private void UpdateEvents_OnFrame( float DeltaTime )
	{
		if (m_IsActiveArea == false )
			return;
		
		for ( int i = m_EnteredGameObjects.Count - 1; i >= 0; i-- )
		{
			EnteredGameObjectData data = m_EnteredGameObjects[i];
			if ( data.EnteredGameObject == null )
			{
				m_EnteredGameObjects.RemoveAt( i ); continue;
			}

			if ( data.bIsEntity && data.EnteredEntity == null )
			{
				data.bIsEntity = false;
			}

			if ( data.bIsEntity )
			{
				data.EnteredEntity.OnHittedDetails( Vector3.zero, null, m_DamageType, m_EveryFrameAppliedDamage * DeltaTime, false );
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private	void	OnEnter( GameObject go )
	{
		if (m_EnteredGameObjects.FindIndex( (o) => go.transform.root.GetInstanceID() == o.ObjectID ) > -1 )
		{
			return;
		}

		if (!go.TryGetComponent(m_EntityType.Type, out Component comp))
		{
			return;
		}

		Debug.Log( "TixicTriggerArea::OnEnter: Enter " + go.name );

		Entity enteredEntity = null;
		EnteredGameObjectData newData = new EnteredGameObjectData()
		{
			bIsEntity = go.transform.TrySearchComponent(ESearchContext.LOCAL, out enteredEntity),
			EnteredEntity = enteredEntity,
			EnteredGameObject = go,
			ObjectID = go.transform.root.GetInstanceID()
		};
		m_EnteredGameObjects.Add( newData );
	}


	//////////////////////////////////////////////////////////////////////////
	private	void	OnExit( GameObject go )
	{
		int IDToFind = go.transform.root.GetInstanceID();
		int index = m_EnteredGameObjects.FindIndex( (s) => s.ObjectID == IDToFind );
		if ( index > -1 )
		{
			Debug.Log( "TixicTriggerArea::OnExit: Exit " + go.name );
			m_EnteredGameObjects.RemoveAt(index);
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnDrawGizmos()
	{
		if (transform.TrySearchComponent(ESearchContext.LOCAL, out m_Collider) )
		{
			Matrix4x4 mat = Gizmos.matrix;
			Gizmos.matrix = transform.localToWorldMatrix;

			if (m_Collider is BoxCollider )
			{
				BoxCollider thisCollider = m_Collider as BoxCollider;
				Gizmos.DrawCube( Vector3.zero, thisCollider.size );
			}
		
			if (m_Collider is SphereCollider )
			{
				SphereCollider thisCollider = m_Collider as SphereCollider;
				Gizmos.DrawSphere( Vector3.zero, thisCollider.radius );
			}

			Gizmos.matrix = mat;
		}
	}
}
