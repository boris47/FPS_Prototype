
using UnityEngine;

namespace CutScene {
	[System.Serializable]
	public	class EntityCutsceneController {


		private		Entity							m_EntityParent				= null;
		private		IEntitySimulation				m_EntitySimulation			= null;
		private		PointsCollectionOnline			m_PointsCollection			= null;
		private		int								m_CurrentIdx				= 0;

		private		ESimMovementType					m_MovementType				= ESimMovementType.WALK;
		private		Vector3							m_Destination				= Vector3.zero;
		private		Transform						m_Target					= null;
		private		float							m_TimeScaleTarget			= 1f;
		private		Waiter_Base						m_Waiter					= null;


		public	void	Setup( Entity entityParent, PointsCollectionOnline pointCollection )
		{
			this.m_EntityParent		= entityParent;
			this.m_EntitySimulation	= entityParent;
			this.m_PointsCollection	= pointCollection;
			this.m_CurrentIdx		= 0;

			this.m_EntitySimulation.EnterSimulationState();

			CutsceneWaypointData data = this.m_PointsCollection[this.m_CurrentIdx ];
			this.SetupForNextWaypoint( data );
		}


		//////////////////////////////////////////////////////////////////////////
		private	void	SetupForNextWaypoint( CutsceneWaypointData data )
		{
			this.m_Target					= data.target;                                  // target to look at
			this.m_MovementType				= data.movementType;                            // movement type
			this.m_TimeScaleTarget			= Mathf.Clamp01( data.timeScaleTraget );		// time scale for this trip

			// WEAPON ZOOM
			if (this.m_EntityParent is Player )
			{
				if ( WeaponManager.Instance.CurrentWeapon.WeaponState == EWeaponState.DRAWED )
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

			this.m_Waiter = data.waiter;

			if ( !this.m_Waiter  )
			{
				// MOVEMENT
				Vector3 destination = data.point.position;	// destination to reach
				if ( Physics.Raycast( destination, -this.m_EntityParent.transform.up, out RaycastHit hit) )
				{
					this.m_Destination = Utils.Math.ProjectPointOnPlane(this.m_EntityParent.transform.up, this.m_EntityParent.transform.position, hit.point );
				}
				else
				{
					this.Terminate();
					return;
				}
			}

			// BEFORE A SIMULATION STAGE
			this.m_EntitySimulation.BeforeSimulationStage(this.m_MovementType, this.m_Destination, this.m_Target, this.m_TimeScaleTarget );
		}


		/// <summary>
		/// return true if completed, otherwise false
		/// </summary>
		/// <returns></returns>
		public	bool	Update()
		{
			// If a waiter is defined, we have to wait for its completion
			if (this.m_Waiter && this.m_Waiter.HasToWait == true )
			{
				Vector3 tempDestination = Utils.Math.ProjectPointOnPlane(this.m_EntityParent.transform.up, this.m_Destination, this.m_EntityParent.transform.position );
				this.m_EntitySimulation.SimulateMovement( ESimMovementType.STATIONARY, tempDestination, this.m_Target, this.m_TimeScaleTarget );
				this.m_Waiter.Wait();
				return false;
			}

			this.m_Waiter = null;

			// Continue simulation until need updates
			bool isBusy = this.m_EntitySimulation.SimulateMovement(this.m_MovementType, this.m_Destination, this.m_Target, this.m_TimeScaleTarget );
			if ( isBusy == true ) // if true is currently simulating and here we have to wait simulation to be completed
			{
				return false;
			}

			// call callback when each waypoint is reached
			this.m_PointsCollection[this.m_CurrentIdx ].OnWayPointReached?.Invoke();

			// AFTER A SIMULATION STAGE
			this.m_EntitySimulation.AfterSimulationStage(this.m_MovementType, this.m_Destination, this.m_Target, this.m_TimeScaleTarget );

			// Next waypoint index
			this.m_CurrentIdx ++;

			// End of simulation
			if (this.m_CurrentIdx != this.m_PointsCollection.Count )
			{
				// Update store start position for distance check
				this.m_EntitySimulation.StartPosition = this.m_EntityParent.transform.position;

				// Update to next simulation targets
				CutsceneWaypointData data	= this.m_PointsCollection[this.m_CurrentIdx ];

				this.SetupForNextWaypoint( data );

				return false;
			}

			this.Terminate();
			return true;
		}


		public	void Terminate()
		{
			// Reset some internal variables
			CameraControl.Instance.OnCutsceneEnd();

			// Restore zoom
			if ( WeaponManager.Instance.IsZoomed == true )
				WeaponManager.Instance.ZoomOut();

			// Called on entity in order to reset vars or everything else
			this.m_EntitySimulation.ExitSimulationState();

			// Resetting internals
			this.m_CurrentIdx						= 0;
			this.m_MovementType						= ESimMovementType.WALK;
			this.m_Destination						= Vector3.zero;
			this.m_Target							= null;
			this.m_TimeScaleTarget					= 1f;
			this.m_PointsCollection					= null;
		}

	}

}