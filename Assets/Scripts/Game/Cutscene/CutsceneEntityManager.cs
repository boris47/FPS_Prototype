
using UnityEngine;

namespace CutScene {

	public class CutsceneEntityManager : MonoBehaviour {
	
		public		bool							IsPlaying					{ get; private set; }
		public		bool							IsOK						{ get; private set; }


		private		PointsCollectionOnline			m_PointsCollection			= null;

		private		IEntity							m_EntityRef					= null;
		private		IEntitySimulation				m_EntitySimulation			= null;
		private		int								m_CurrentIdx				= 0;

		private		Entity.SimMovementType			m_MovementType				= Entity.SimMovementType.WALK;
		private		Vector3							m_Destination				= Vector3.zero;
		private		Transform						m_Target					= null;
		private		float							m_TimeScaleTarget			= 1f;



		//////////////////////////////////////////////////////////////////////////
		// Awake
		private void	Awake()
		{
			m_EntityRef = transform.parent.GetComponent<IEntity>();
			if ( m_EntityRef == null )
			{
				Destroy( gameObject );
				return;
			}

			m_EntitySimulation = m_EntityRef as IEntitySimulation;
			m_EntityRef.CutsceneManager = this;
			IsOK = true;
		}


		//////////////////////////////////////////////////////////////////////////
		// Setup
		public	void	Play( PointsCollectionOnline pointsCollection )
		{
			m_PointsCollection				= pointsCollection;
			InternalPlay();
		}


		//////////////////////////////////////////////////////////////////////////
		// Play
		public	void	InternalPlay()
		{
			if ( IsOK == false )
				return;

			if ( m_PointsCollection == null || m_PointsCollection.Count == 0 )
			{
				m_PointsCollection = null;
				return;
			}
			IsPlaying = true;

			m_EntitySimulation.EnterSimulationState();

			// Let's start
			CutsceneWaypointData data	= m_PointsCollection[ m_CurrentIdx ];
			m_Destination				= data.point.position;
			m_Target					= data.target;
			m_MovementType				= data.movementType;
			m_TimeScaleTarget			= data.timeScaleTraget;

			if ( m_PointsCollection.OnStart != null && m_PointsCollection.OnStart.GetPersistentEventCount() > 0 )
			{
				m_PointsCollection.OnStart.Invoke();
			}

			m_EntitySimulation.SimulateMovement( m_MovementType, m_Destination, m_Target, m_TimeScaleTarget );
		}


		//////////////////////////////////////////////////////////////////////////
		// Update
		private	void	Update()
		{
			if ( IsPlaying == false )
				return;

			// Continue simulation until need updates
			bool result = m_EntitySimulation.SimulateMovement( m_MovementType, m_Destination, m_Target, m_TimeScaleTarget );
			if ( result == true ) // if true is currently simulating
				return;

			// call callback when each waypoint is reached
			GameEvent onWayPointReached = m_PointsCollection[ m_CurrentIdx ].OnWayPointReached;
			if ( onWayPointReached != null && onWayPointReached.GetPersistentEventCount() > 0 )
			{
				onWayPointReached.Invoke();
			}

			// Next waypoint index
			m_CurrentIdx ++;

			// End of simulation
			if ( m_CurrentIdx == m_PointsCollection.Count )
			{
				Termiante();
				return;
			}

			// Update store start position for distance check
			m_EntitySimulation.StarPosition = m_EntityRef.Transform.position;

			// Update to next simulation targets
			CutsceneWaypointData data	= m_PointsCollection[ m_CurrentIdx ];
			m_Destination				= data.point.position;
			m_Target					= data.target != null ? data.target : m_Target;
			m_MovementType				= data.movementType;
			m_TimeScaleTarget			= data.timeScaleTraget;
		}


		//////////////////////////////////////////////////////////////////////////
		// Termiante
		public	void	Termiante()
		{
			CameraControl.Instance.OnCutsceneEnd();
			m_EntitySimulation.ExitSimulationState();
			m_EntitySimulation.StarPosition	= Vector3.zero;
			IsPlaying						= false;
			m_CurrentIdx					= 0;
			m_MovementType					= Entity.SimMovementType.WALK;
			m_Destination					= Vector3.zero;
			m_Target						= null;
			m_TimeScaleTarget				= 1f;
			m_PointsCollection				= null;
			this.enabled = false;
		}
	
	}


}