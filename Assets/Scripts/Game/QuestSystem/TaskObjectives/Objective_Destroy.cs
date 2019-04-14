
using UnityEngine;


namespace QuestSystem {

	[RequireComponent(typeof(Collider), typeof(Entity))]
	public class Objective_Destroy : Objective_Base {

		[SerializeField]
		private GameEvent			m_OnDestroy						= null;

		private	Entity				m_Target						= null;

		private	Transform			m_Signal						= null;

		//////////////////////////////////////////////////////////////////////////
		// AWAKE
		private void Awake()
		{
			m_Target	= GetComponent<Entity>();
			m_Target.OnKilled += OnKill;

			GameObject a = Resources.Load("Prefabs/UI/Task_Objectives/Task_KillTarget") as GameObject;
			m_Signal = Instantiate( a ).transform;

			m_Signal.SetParent( UI.Instance.InGame.transform );
		}


		//////////////////////////////////////////////////////////////////////////
		// Enable ( Override )
		public override void Enable()
		{
			
		}


		//////////////////////////////////////////////////////////////////////////
		// Update
		private void Update()
		{
			DrawUIElementOnObjectives( m_Target.transform, m_Signal );
		}


		//////////////////////////////////////////////////////////////////////////
		// OnDestroy
		private void OnKill()
		{
			if ( m_OnDestroy != null && m_OnDestroy.GetPersistentEventCount() > 0 )
				m_OnDestroy.Invoke();

			m_Signal.SetParent( null );
			Destroy( m_Signal.gameObject );

			Completed = true;

			if ( RelatedTask != null )
			{
				RelatedTask.UpdateStatus();
			}
			else
			{
				print( "Entity " + name + "  has Objective_Destroy attached but not belongs any task" );
			}
		}

	}

}
