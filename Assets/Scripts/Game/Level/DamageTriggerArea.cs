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
		
		this.m_IsActiveArea = this.transform.SearchComponent(ref this.m_TriggerEvents, ESearchContext.LOCAL);
		this.m_IsActiveArea &= this.transform.SearchComponent(ref this.m_Collider, ESearchContext.LOCAL);

		if (this.m_IsActiveArea )
		{
			this.m_TriggerEvents.OnEnterEvent += this.OnEnter;
			this.m_TriggerEvents.OnExitEvent += this.OnExit;

			GameManager.UpdateEvents.OnFrame += this.UpdateEvents_OnFrame;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnEnable()
	{
		this.m_IsActiveArea = true;

		this.m_EnteredGameObjects.Clear();

		Collider[] colliders = null;
		bool bHasColliders = false;

		if (this.m_Collider is BoxCollider )
		{
			BoxCollider thisCollider = this.m_Collider as BoxCollider;
			colliders = Physics.OverlapBox( thisCollider.transform.position, thisCollider.size, thisCollider.transform.rotation );
			bHasColliders = colliders.Length > 0;
		}

		if (this.m_Collider is SphereCollider )
		{
			SphereCollider thisCollider = this.m_Collider as SphereCollider;
			colliders = Physics.OverlapSphere( thisCollider.transform.position, thisCollider.radius );
			bHasColliders = colliders.Length > 0;
		}

		if ( bHasColliders )
		{
			for ( int i = 0; i < colliders.Length; i++ )
			{
				Collider collider = colliders[i];
				this.OnEnter( collider.gameObject );
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnDisable()
	{
		this.m_IsActiveArea = false;
	}


	//////////////////////////////////////////////////////////////////////////
	private void UpdateEvents_OnFrame( float DeltaTime )
	{
		if (this.m_IsActiveArea == false )
			return;
		
		for ( int i = this.m_EnteredGameObjects.Count - 1; i >= 0; i-- )
		{
			EnteredGameObjectData data = this.m_EnteredGameObjects[i];
			if ( data.EnteredGameObject == null )
			{
				this.m_EnteredGameObjects.RemoveAt( i ); continue;
			}

			if ( data.bIsEntity && data.EnteredEntity == null )
			{
				data.bIsEntity = false;
			}

			if ( data.bIsEntity )
			{
				data.EnteredEntity.OnHittedDetails( Vector3.zero, null, this.m_DamageType, this.m_EveryFrameAppliedDamage * DeltaTime, false );
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private	void	OnEnter( GameObject go )
	{
		if (this.m_EnteredGameObjects.FindIndex( (o) => go.transform.root.GetInstanceID() == o.ObjectID ) > -1 )
		{
			return;
		}

		if (!go.TryGetComponent(this.m_EntityType.Type, out Component comp))
		{
			return;
		}

		Debug.Log( "TixicTriggerArea::OnEnter: Enter " + go.name );

		Entity enteredEntity = null;
		EnteredGameObjectData newData = new EnteredGameObjectData()
		{
			bIsEntity = go.transform.SearchComponent(ref enteredEntity, ESearchContext.LOCAL),
			EnteredEntity = enteredEntity,
			EnteredGameObject = go,
			ObjectID = go.transform.root.GetInstanceID()
		};
		this.m_EnteredGameObjects.Add( newData );
	}


	//////////////////////////////////////////////////////////////////////////
	private	void	OnExit( GameObject go )
	{
		int IDToFind = go.transform.root.GetInstanceID();
		int index = this.m_EnteredGameObjects.FindIndex( (s) => s.ObjectID == IDToFind );
		if ( index > -1 )
		{
			Debug.Log( "TixicTriggerArea::OnExit: Exit " + go.name );
			this.m_EnteredGameObjects.RemoveAt(index);
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnDrawGizmos()
	{
		if (this.transform.SearchComponent( ref this.m_Collider, ESearchContext.LOCAL ) )
		{
			Matrix4x4 mat = Gizmos.matrix;
			Gizmos.matrix = this.transform.localToWorldMatrix;

			if (this.m_Collider is BoxCollider )
			{
				BoxCollider thisCollider = this.m_Collider as BoxCollider;
				Gizmos.DrawCube( Vector3.zero, thisCollider.size );
			}
		
			if (this.m_Collider is SphereCollider )
			{
				SphereCollider thisCollider = this.m_Collider as SphereCollider;
				Gizmos.DrawSphere( Vector3.zero, thisCollider.radius );
			}

			Gizmos.matrix = mat;
		}
	}
}
