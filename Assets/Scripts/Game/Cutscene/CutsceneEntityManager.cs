
using UnityEngine;


public class CutsceneEntityManager : MonoBehaviour {
	
	public		bool					IsPlaying					{ get; private set; }
	public		bool					IsOK						{ get; private set; }

	public		PointsCollectionOnline	m_PointsCollection			= null;


	private		float					m_InternalTimeNormalized	= 0f;
	private		delegate void			func( float dt );
	private		func					m_InterpolationFunction		= null;
	private		IEntity					m_EntityRef					= null;
	private		IEntitySimulation		m_EntitySimulation			= null;
	private		bool					m_IsExecuted				= false;
	private		int						m_CurrentIdx				= 0;



	//////////////////////////////////////////////////////////////////////////
	// Awake
	private void	Awake()
	{
		if ( m_PointsCollection == null ||
			m_PointsCollection.Positions == null ||
			m_PointsCollection.Positions.Count == 0 ||
			m_PointsCollection.TargetPositions == null ||
			m_PointsCollection.TargetPositions.Count == 0 ||
			m_PointsCollection.Positions.Count != m_PointsCollection.TargetPositions.Count )
		{
			enabled = false;
			return;
		}

		m_EntityRef = transform.parent.GetComponent<Entity>();
		if ( m_EntityRef == null )
		{
			Destroy( gameObject );
			return;
		}

		m_EntitySimulation = m_EntityRef as IEntitySimulation;
		m_EntityRef.CutsceneManager = this;

		if ( m_PointsCollection.UseNormalizedTime == true )
		{
			switch( m_PointsCollection.Positions.Count )
			{
				case 1:		m_InterpolationFunction = LinearInterpolation;		break;
				case 2:		m_InterpolationFunction = CubicInterpolation;		break;
				case 3:		m_InterpolationFunction = QuarticInterpolation;		break;
				case 4:		m_InterpolationFunction = QuinticInterpoolation;	break;
				default :	m_InterpolationFunction = SexticInterpolant;		break;
			}
		}

		IsOK = true;
	}


	//////////////////////////////////////////////////////////////////////////
	// Setup
	public	void	Setup( PointsCollectionOnline pointsCollection )
	{
		m_PointsCollection				= pointsCollection;
		m_InternalTimeNormalized		= 0f;
		m_IsExecuted					= false;
		IsOK							= false;

		Awake();
	}


	//////////////////////////////////////////////////////////////////////////
	// Play
	public	void	Play()
	{
		if ( m_IsExecuted == true || IsOK == false )
			return;

		IsPlaying = true;

		m_EntitySimulation.EnterSimulationState();
	}

	private	Vector3	m_Destination		= Vector3.zero;
	private	Vector3	m_TargetPosition	= Vector3.zero;
	//////////////////////////////////////////////////////////////////////////
	// Update
	private	void	Update()
	{
		if ( IsPlaying == false )
			return;

		if ( m_PointsCollection.UseNormalizedTime == true )
		{
			m_InternalTimeNormalized += Time.deltaTime;
			if ( m_InternalTimeNormalized > 1f )
			{
				m_EntitySimulation.ExitSimulationState();
				IsPlaying = false;
				this.enabled = false;
			}
			m_InterpolationFunction( Time.deltaTime );
		}
		else
		{
			if ( m_CurrentIdx < m_PointsCollection.Positions.Count )			m_Destination		= m_PointsCollection.Positions[ m_CurrentIdx ];
			if ( m_CurrentIdx < m_PointsCollection.TargetPositions.Count )		m_TargetPosition	= m_PointsCollection.TargetPositions[ m_CurrentIdx ];

			transform.position	= m_TargetPosition;

			bool result = m_EntitySimulation.SimulateMovement( m_PointsCollection.EntityState, m_Destination, transform, Time.deltaTime );
			if ( result == false )
			{
				m_CurrentIdx ++;
				if ( m_CurrentIdx >= m_PointsCollection.Positions.Count )
				{
					m_EntitySimulation.ExitSimulationState();
					IsPlaying = false;
				}
			}
		}
		
	}


	//////////////////////////////////////////////////////////////////////////
	// PlayLinearInterpolation
	private	void	LinearInterpolation( float dt )
	{
		Vector3 position		= Vector3.Lerp( m_EntityRef.Transform.position, m_PointsCollection.Positions[0],		m_InternalTimeNormalized );
		Vector3 targetPosition	= Vector3.Lerp( m_EntityRef.Transform.forward,  m_PointsCollection.TargetPositions[0],	m_InternalTimeNormalized );

		transform.LookAt( targetPosition, Vector3.up );
		m_EntitySimulation.SimulateMovement( m_PointsCollection.EntityState, position, transform, dt, m_InternalTimeNormalized );
	}


	//////////////////////////////////////////////////////////////////////////
	// PlayCubicInterpolation
	private	void	CubicInterpolation( float dt )
	{
		Vector3 position;
		{
			Vector3 a = m_PointsCollection.Positions[0];
			Vector3 b = m_PointsCollection.Positions[1];
			position = Utils.Math.GetPoint( m_EntityRef.Transform.position, a, b, m_InternalTimeNormalized );
		}

		Vector3 targetPosition;
		{
			Vector3 a = m_PointsCollection.TargetPositions[0];
			Vector3 b = m_PointsCollection.TargetPositions[1];
			targetPosition = Utils.Math.GetPoint( m_EntityRef.Transform.position, a, b, m_InternalTimeNormalized );
		}
		transform.LookAt( targetPosition, Vector3.up );
		m_EntitySimulation.SimulateMovement( m_PointsCollection.EntityState, position, transform, dt, m_InternalTimeNormalized );
	}


	//////////////////////////////////////////////////////////////////////////
	// QuarticInterpolation
	public	void	QuarticInterpolation( float dt )
	{
		Vector3 position;
		{
			Vector3 a = m_PointsCollection.Positions[0];
			Vector3 b = m_PointsCollection.Positions[1];
			Vector3 c = m_PointsCollection.Positions[2];
			position = Utils.Math.GetPoint( m_EntityRef.Transform.position, a, b, c, m_InternalTimeNormalized );
		}

		Vector3 targetPosition;
		{
			Vector3 a = m_PointsCollection.TargetPositions[0];
			Vector3 b = m_PointsCollection.TargetPositions[1];
			Vector3 c = m_PointsCollection.TargetPositions[2];
			targetPosition = Utils.Math.GetPoint( m_EntityRef.Transform.position, a, b, c, m_InternalTimeNormalized );
		}
		transform.LookAt( targetPosition, Vector3.up );
		m_EntitySimulation.SimulateMovement( m_PointsCollection.EntityState, position, transform, dt, m_InternalTimeNormalized );
	}


	//////////////////////////////////////////////////////////////////////////
	// QuinticInterpoolation
	public	void	QuinticInterpoolation( float dt )
	{
		Vector3 position;
		{
			Vector3 a = m_PointsCollection.Positions[0];
			Vector3 b = m_PointsCollection.Positions[1];
			Vector3 c = m_PointsCollection.Positions[2];
			Vector3 d = m_PointsCollection.Positions[3];
			position = Utils.Math.GetPoint( m_EntityRef.Transform.position, a, b, c, d, m_InternalTimeNormalized );
		}

		Vector3 targetPosition;
		{
			Vector3 a = m_PointsCollection.TargetPositions[0];
			Vector3 b = m_PointsCollection.TargetPositions[1];
			Vector3 c = m_PointsCollection.TargetPositions[2];
			Vector3 d = m_PointsCollection.TargetPositions[3];
			targetPosition = Utils.Math.GetPoint( m_EntityRef.Transform.position, a, b, c, d, m_InternalTimeNormalized );
		}
		transform.LookAt( targetPosition, Vector3.up );
		m_EntitySimulation.SimulateMovement( m_PointsCollection.EntityState, position, transform, dt, m_InternalTimeNormalized );
	}


	//////////////////////////////////////////////////////////////////////////
	// SexticInterpolant
	private	void	SexticInterpolant( float dt )
	{
		var positions = m_PointsCollection.Positions.ToArray();
		Vector3 position = Utils.Math.GetPoint( ref positions, m_InternalTimeNormalized );

		var forwards = m_PointsCollection.TargetPositions.ToArray();
		Vector3 targetPosition = Utils.Math.GetPoint( ref forwards, m_InternalTimeNormalized );

		transform.LookAt( targetPosition, Vector3.up );
		m_EntitySimulation.SimulateMovement( m_PointsCollection.EntityState, position, transform, dt, m_InternalTimeNormalized );
	}

}
