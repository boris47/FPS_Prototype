using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TixicTriggerArea : MonoBehaviour {

	[System.Serializable]
	private class EnteredGameObjectData {

		public	GameObject			EnteredGameObject		= null;
		public	Entity				EnteredEntity			= null;
		public	bool				bIsEntity				= false;
		public	int					ObjectID				= -1;
	}

	private			TriggerEvents		m_TriggerEvents	 = null;

	[SerializeField, ReadOnly]
	private			bool				m_bIsActiveArea	= false;

	[SerializeField, ReadOnly]
	private			bool				m_bHasOmogeneousDmg = false;

	[SerializeField, ReadOnly]
	private			List<EnteredGameObjectData> m_EnteredGameObjects = new List<EnteredGameObjectData>();

	[SerializeField, Range( 0, 150f )]
	private			float				m_AppliedDamage = 10f;


	//////////////////////////////////////////////////////////////////////////
	private void Awake()
	{
		m_bIsActiveArea = transform.SearchComponent( ref m_TriggerEvents, SearchContext.LOCAL );

		if ( m_bIsActiveArea )
		{
			m_TriggerEvents.OnEnterEvent += OnEnter;
			m_TriggerEvents.OnExitEvent += OnExit;

			GameManager.UpdateEvents.OnFrame += UpdateEvents_OnFrame;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private void UpdateEvents_OnFrame( float DeltaTime )
	{
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
				data.EnteredEntity.OnHittedDetails( Vector3.zero, null, m_AppliedDamage * DeltaTime, false );
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private	void	OnEnter( GameObject go )
	{
		print( "TixicTriggerArea::OnEnter: entered " + go.name );

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
}
