
using UnityEngine;

namespace CutScene {

	public class CutsceneEntityManager : MonoBehaviour {
	
		public		bool							IsPlaying					{ get; private set; }
		public		bool							IsOK						{ get; private set; }

		private		Entity							m_EntityParent				= null;

		private		PointsCollectionOnline			m_PointsCollection			= null;

		private		IEntitySimulation				m_EntitySimulation			= null;
		private		int								m_CurrentIdx				= 0;

		private		SimMovementType					m_MovementType				= SimMovementType.WALK;
		private		Vector3							m_Destination				= Vector3.zero;
		private		Transform						m_Target					= null;
		private		float							m_TimeScaleTarget			= 1f;



		//////////////////////////////////////////////////////////////////////////
		private void	Awake()
		{
			m_EntityParent = GetComponentInParent<Entity>();
			if ( m_EntityParent == null )
			{
				Destroy( gameObject );
				return;
			}

			m_EntitySimulation = m_EntityParent as IEntitySimulation;
			IsOK = true;

			this.enabled						= false;
		}


		//////////////////////////////////////////////////////////////////////////
		public	void	Play( PointsCollectionOnline pointsCollection )
		{
			this.enabled						= true;

			m_PointsCollection				= pointsCollection;
			InternalPlay();
		}


		//////////////////////////////////////////////////////////////////////////
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

			// On start event called
			if ( m_PointsCollection.OnStart != null && m_PointsCollection.OnStart.GetPersistentEventCount() > 0 )
			{
				m_PointsCollection.OnStart.Invoke();
			}

			// emit for movement silìmulation
			m_EntitySimulation.SimulateMovement( m_MovementType, m_Destination, m_Target, m_TimeScaleTarget );

			// Zoom is controlled by waypoint setting
			if ( WeaponManager.Instance.CurrentWeapon.WeaponState == WeaponState.DRAWED )
			{
				if ( data.zoomEnabled == true )
				{
					if ( WeaponManager.Instance.IsZoomed == false ) WeaponManager.Instance.ZoomIn();
				}
				else
				{
					if ( WeaponManager.Instance.IsZoomed == true ) WeaponManager.Instance.ZoomOut();
				}
			}
		}


		//////////////////////////////////////////////////////////////////////////
		private	void	Update()
		{
			if ( GameManager.IsPaused == true )
				return;

			if ( IsPlaying == false )
				return;

			// Continue simulation until need updates
			bool result = m_EntitySimulation.SimulateMovement( m_MovementType, m_Destination, m_Target, m_TimeScaleTarget );
			if ( result == true ) // if true is currently simulating and here we have to wait simulation to be completed
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
			if ( m_CurrentIdx != m_PointsCollection.Count )
			{
				// Update store start position for distance check
				m_EntitySimulation.StartPosition = m_EntityParent.transform.position;

				// Update to next simulation targets
				CutsceneWaypointData data	= m_PointsCollection[ m_CurrentIdx ];
				m_Destination				= data.point.position;							// destination to reach
				m_Target					= data.target != null ? data.target : m_Target;	// target to look at
				m_MovementType				= data.movementType;							// movement type
				m_TimeScaleTarget			= data.timeScaleTraget;						 // time scale for this trip
			}
			else
			{
				Termiante();
			}
		}


		//////////////////////////////////////////////////////////////////////////
		public	void	Termiante()
		{
			// Reset some internal variables
			CameraControl.Instance.OnCutsceneEnd();

			// Called on entity in order to reset vars or evething else
			m_EntitySimulation.ExitSimulationState();

			// Resetting internals
			m_EntitySimulation.StartPosition	= Vector3.zero;
			IsPlaying							= false;
			m_CurrentIdx						= 0;
			m_MovementType						= SimMovementType.WALK;
			m_Destination						= Vector3.zero;
			m_Target							= null;
			m_TimeScaleTarget					= 1f;
			m_PointsCollection					= null;

			// to save performance disable this script
			this.enabled						= false;
		}
	
	}


}