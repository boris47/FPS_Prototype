using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DamageType {
	BALLISTIC,
	ENERGY,
	ELECTRO,
	EXPLOSIVE,
	FLAME
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

	[SerializeField, ReadOnly]
	private			bool				m_bIsActiveArea					= false;

	[SerializeField, ReadOnly]
	private			bool				m_bHasOmogeneousDmg				= false;

	[SerializeField, ReadOnly]
	private			List<EnteredGameObjectData> m_EnteredGameObjects	= new List<EnteredGameObjectData>();

	[SerializeField, Range( 0, 150f )]
	private			float				m_EveryFrameAppliedDamage		= 10f;

	[SerializeField]
	private			DamageType			m_DamageType					= DamageType.BALLISTIC;


	//////////////////////////////////////////////////////////////////////////
	private void Awake()
	{
		m_bIsActiveArea = transform.SearchComponent( ref m_TriggerEvents, SearchContext.LOCAL ) && 
			transform.SearchComponent( ref m_Collider, SearchContext.LOCAL );

		if ( m_bIsActiveArea )
		{
			m_TriggerEvents.OnEnterEvent += OnEnter;
			m_TriggerEvents.OnExitEvent += OnExit;

			GameManager.UpdateEvents.OnFrame += UpdateEvents_OnFrame;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnEnable()
	{
		m_bIsActiveArea = true;

		m_EnteredGameObjects.Clear();

		Collider[] colliders = null;
		bool bHasColliders = false;

		if ( m_Collider is BoxCollider )
		{
			BoxCollider thisCollider = m_Collider as BoxCollider;
			colliders = Physics.OverlapBox( thisCollider.transform.position, thisCollider.size, thisCollider.transform.rotation );
			bHasColliders = colliders.Length > 0;
		}

		if ( m_Collider is SphereCollider )
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
		m_bIsActiveArea = false;
	}


	//////////////////////////////////////////////////////////////////////////
	private void UpdateEvents_OnFrame( float DeltaTime )
	{
		if ( m_bIsActiveArea == false )
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
				data.EnteredEntity.OnHittedDetails( Vector3.zero, null, m_EveryFrameAppliedDamage * DeltaTime, false );
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private	void	OnEnter( GameObject go )
	{
		if ( m_EnteredGameObjects.FindIndex( (o) => go.GetInstanceID() == o.ObjectID ) > -1 )
			return;

		print( "TixicTriggerArea::OnEnter: Enter " + go.name );

		Entity enteredEntity = null;
		bool bIsEntity = go.transform.SearchComponent( ref enteredEntity, SearchContext.LOCAL );

		EnteredGameObjectData newData = new EnteredGameObjectData()
		{
			bIsEntity = bIsEntity,
			EnteredEntity = enteredEntity,
			EnteredGameObject = go,
			ObjectID = go.GetInstanceID()
		};
		m_EnteredGameObjects.Add( newData );

	}


	//////////////////////////////////////////////////////////////////////////
	private	void	OnExit( GameObject go )
	{
		print( "TixicTriggerArea::OnExit: Exit " + go.name );

		int IDToFind = go.GetInstanceID();
		int index = m_EnteredGameObjects.FindIndex( (s) => s.ObjectID == IDToFind );
		if ( index > -1 )
		{
			m_EnteredGameObjects.RemoveAt(index);
		}
		else
		{
			Debug.Log( "Strange, object "+ go.name + "Leavaing, but never entered!!!" );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnDrawGizmos()
	{
		if ( transform.SearchComponent( ref m_Collider, SearchContext.LOCAL ) )
		{
			Matrix4x4 mat = Gizmos.matrix;
			Gizmos.matrix = transform.localToWorldMatrix;

			if ( m_Collider is BoxCollider )
			{
				BoxCollider thisCollider = m_Collider as BoxCollider;
				Gizmos.DrawCube( Vector3.zero, thisCollider.size );
			}
		
			if ( m_Collider is SphereCollider )
			{
				SphereCollider thisCollider = m_Collider as SphereCollider;
				Gizmos.DrawSphere( Vector3.zero, thisCollider.radius );
			}

			Gizmos.matrix = mat;
		}
	}
}
