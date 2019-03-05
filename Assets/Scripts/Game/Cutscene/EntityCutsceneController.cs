
using UnityEngine;

namespace CutScene {

	public	class EntityCutsceneController {


		private		Entity							m_EntityParent				= null;
		private		IEntitySimulation				m_EntitySimulation			= null;
		private		PointsCollectionOnline			m_PointsCollection			= null;
		private		int								m_CurrentIdx				= 0;

		private		SimMovementType					m_MovementType				= SimMovementType.WALK;
		private		Vector3							m_Destination				= Vector3.zero;
		private		Transform						m_Target					= null;
		private		float							m_TimeScaleTarget			= 1f;
		private		Waiter_Base						m_Waiter					= null;


		public	void	Setup( Entity entityParent, PointsCollectionOnline pointCollection )
		{
			m_EntityParent = entityParent;
			m_EntitySimulation = m_EntityParent as IEntitySimulation;
			m_PointsCollection = pointCollection;
			m_CurrentIdx = 0;

			CutsceneWaypointData data = m_PointsCollection[ m_CurrentIdx ];
			SetupForNextWaypoint( data );
		}


		//////////////////////////////////////////////////////////////////////////
		private	void	SetupForNextWaypoint( CutsceneWaypointData data )
		{
			m_Target					= data.target;									// target to look at
			m_MovementType				= data.movementType;							// movement type
			m_TimeScaleTarget			= Mathf.Clamp01( data.timeScaleTraget );		// time scale for this trip

			// ORIENTATION
			CameraControl.Instance.Target = m_Target;

			// WEAPON ZOOM
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

			if ( data.waiter != null && data.movementType == SimMovementType.STATIONARY )
			{
				m_Waiter = data.waiter;
			}
			else
			{
				m_Waiter = null;

				// MOVEMENT
				Vector3 destination = data.point.position;	// destination to reach
				RaycastHit hit;
				if ( Physics.Raycast( destination, -m_EntityParent.transform.up, out hit ) )
				{
					m_Destination = Utils.Math.ProjectPointOnPlane( m_EntityParent.transform.up, m_EntityParent.transform.position, hit.point );
				}
				else
				{
					Terminate(); 
				}
			}
		}


		/// <summary>
		/// return true if completed, otherwise false
		/// </summary>
		/// <returns></returns>
		public	bool	Update()
		{
			if ( m_Waiter != null && m_Waiter.HasToWait == true )
			{
				Vector3 tempDestination = Utils.Math.ProjectPointOnPlane( m_EntityParent.transform.up, m_Destination, m_EntityParent.transform.position );
				m_EntitySimulation.SimulateMovement( SimMovementType.STATIONARY, tempDestination, m_Target, m_TimeScaleTarget );
				m_Waiter.Wait();
				return false;
			}

				// Continue simulation until need updates
			bool isBusy = m_EntitySimulation.SimulateMovement( m_MovementType, m_Destination, m_Target, m_TimeScaleTarget );
			if ( isBusy == true ) // if true is currently simulating and here we have to wait simulation to be completed
				return false;

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

				SetupForNextWaypoint( data );
				return false;
			}

			Terminate();
			return true;
		}


		public	void Terminate()
		{
			// Reset some internal variables
			CameraControl.Instance.OnCutsceneEnd();

			// Restore zoom
			if ( WeaponManager.Instance.IsZoomed == true )
				WeaponManager.Instance.ZoomOut();

			// Called on entity in order to reset vars or evething else
			m_EntitySimulation.ExitSimulationState();

			// Resetting internals
			m_CurrentIdx						= 0;
			m_MovementType						= SimMovementType.WALK;
			m_Destination						= Vector3.zero;
			m_Target							= null;
			m_TimeScaleTarget					= 1f;
			m_PointsCollection					= null;
		}

	}

}