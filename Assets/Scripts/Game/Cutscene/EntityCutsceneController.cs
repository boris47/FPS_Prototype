
using UnityEngine;

namespace CutScene {
	[System.Serializable]
	public	class EntityCutsceneController
	{
		[SerializeField, ReadOnly]
		private		Entity							m_EntityParent				= null;
		[SerializeField, ReadOnly]
		private		PointsCollectionOnline			m_PointsCollection			= null;
		[SerializeField, ReadOnly]
		private		int								m_CurrentIdx				= 0;

		[SerializeField, ReadOnly]
		private		ESimMovementType				m_MovementType				= ESimMovementType.WALK;
		[SerializeField, ReadOnly]
		private		Vector3							m_Destination				= Vector3.zero;
		[SerializeField, ReadOnly]
		private		Transform						m_Target					= null;
		[SerializeField, ReadOnly]
		private		float							m_TimeScaleTarget			= 1f;
		[SerializeField, ReadOnly]
		private		Waiter_Base						m_Waiter					= null;

		private		IEntitySimulation				m_EntitySimulation			= null;


		//////////////////////////////////////////////////////////////////////////
		public	void	Setup(Entity entityParent, PointsCollectionOnline pointCollection)
		{
			m_EntityParent		= entityParent;
			m_EntitySimulation	= entityParent;
			m_PointsCollection	= pointCollection;
			m_CurrentIdx		= 0;

			m_EntitySimulation.EnterSimulationState();

			CutsceneWaypointData data = m_PointsCollection[m_CurrentIdx];
			SetupForNextWaypoint(data);
		}


		//////////////////////////////////////////////////////////////////////////
		private	void	SetupForNextWaypoint(CutsceneWaypointData data)
		{
			m_Target					= data.target;                                  // target to look at
			m_MovementType				= data.movementType;                            // movement type
			m_TimeScaleTarget			= Mathf.Clamp01(data.timeScaleTraget);		// time scale for this trip

			// WEAPON ZOOM
			if (m_EntityParent is Player)
			{
				if (WeaponManager.Instance.CurrentWeapon.WeaponState == EWeaponState.DRAWED)
				{
					if (data.zoomEnabled == true)
					{
						if (WeaponManager.Instance.IsZoomed == false) WeaponManager.Instance.ZoomIn();
					}
					else
					{
						if (WeaponManager.Instance.IsZoomed == true) WeaponManager.Instance.ZoomOut();
					}
				}
			}

			m_Waiter = data.waiter;

		//	if (!m_Waiter)
			{
				// MOVEMENT
				Vector3 destination = data.point.position;  // destination to reach
				if (Physics.Raycast(destination, -m_EntityParent.transform.up, out RaycastHit hit))
				{
					m_Destination = Utils.Math.ProjectPointOnPlane(m_EntityParent.transform.up, m_EntityParent.transform.position, hit.point);
				}
				else
				{
					Terminate();
					return;
				}
			}

			// BEFORE A SIMULATION STAGE
			m_EntitySimulation.BeforeSimulationStage(m_MovementType, m_Destination, m_Target, m_TimeScaleTarget );
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary> Return true if completed, otherwise false </summary>
		public	bool	Update()
		{
			// Continue simulation until need updates
			bool isBusy = m_EntitySimulation.SimulateMovement(m_MovementType, m_Destination, m_Target, m_TimeScaleTarget);
			if (isBusy) // if true is currently simulating and here we have to wait simulation to be completed
			{
				return false;
			}

			// If a waiter is defined, we have to wait for its completion
			if (m_Waiter && m_Waiter.HasToWait)
			{
				Vector3 tempDestination = Utils.Math.ProjectPointOnPlane(m_EntityParent.transform.up, m_Destination, m_EntityParent.transform.position);
			//	m_EntitySimulation.SimulateMovement(ESimMovementType.STATIONARY, tempDestination, m_Target, m_TimeScaleTarget); // Why?? TODO
				m_Waiter.Wait();
				return false;
			}

			// Ensuse no waiter
			m_Waiter = null;

			// call callback when each waypoint is reached
			m_PointsCollection[m_CurrentIdx].OnWayPointReached?.Invoke();

			// AFTER A SIMULATION STAGE
			m_EntitySimulation.AfterSimulationStage(m_MovementType, m_Destination, m_Target, m_TimeScaleTarget);

			// Next waypoint index
			++m_CurrentIdx;

			// End of simulation
			if (m_CurrentIdx < m_PointsCollection.Count)
			{
				// Update to next simulation targets
				CutsceneWaypointData data = m_PointsCollection[m_CurrentIdx];
				SetupForNextWaypoint(data);
				return false;
			}

			return true;
		}


		public	void	Terminate()
		{
			// Reset some internal variables
			FPSEntityCamera.Instance.OnCutsceneEnd();

			// Restore zoom
	//		if ( WeaponManager.Instance.IsZoomed == true )
	//			WeaponManager.Instance.ZoomOut();

			// Called on entity in order to reset vars or everything else
			m_EntitySimulation.ExitSimulationState();

			// Resetting internals
			m_CurrentIdx						= 0;
			m_MovementType						= ESimMovementType.WALK;
			m_Destination						= Vector3.zero;
			m_Target							= null;
			m_TimeScaleTarget					= 1f;
			m_PointsCollection					= null;
		}

	}

}