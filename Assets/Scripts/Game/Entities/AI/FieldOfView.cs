
using UnityEngine;
using System.Collections.Generic;

public	delegate	void	OnTargetEvent( TargetInfo targetInfo );



public interface IFieldOfView {
	
	OnTargetEvent		OnTargetAquired		{ set; }
	OnTargetEvent		OnTargetChanged		{ set; }
	OnTargetEvent		OnTargetLost		{ set; }

	float				Distance			{ get; set; }
	float				Angle				{ get; set; }
	ENTITY_TYPE			TargetType			{ get; set; }

	void				Setup				( uint maxVisibleEntities );
	bool				UpdateFOV			();
	void				OnReset				();
}



[RequireComponent( typeof( SphereCollider ) )]
public class FieldOfView : MonoBehaviour, IFieldOfView {

	public		OnTargetEvent			OnTargetAquired			{ set { m_OnTargetAquired = value; } }
	public		OnTargetEvent			OnTargetChanged			{ set { m_OnTargetChanged = value; } }
	public		OnTargetEvent			OnTargetLost			{ set { m_OnTargetLost	= value; } }


	[SerializeField]
	private		Transform				m_ViewPoint				= null;

	[SerializeField, Range( 1f, 200f )]
	private		float					m_ViewDistance			= 10f;

	[SerializeField,Range( 1f, 150f)]
	private		float					m_ViewCone				= 100f;


	[ SerializeField ]
	private		ENTITY_TYPE				m_EntityType			= ENTITY_TYPE.NONE;

	[SerializeField]
	private		LayerMask				m_LayerMask				 = default( LayerMask );


	float	IFieldOfView.Distance
	{
		get { return m_ViewDistance; }
		set { m_ViewDistance = value; }
	}

	float	IFieldOfView.Angle
	{
		get { return m_ViewCone; }
		set { m_ViewCone = value; }
	}

	ENTITY_TYPE IFieldOfView.TargetType
	{
		get { return m_EntityType; }
		set { m_EntityType = value; }
	}

	private		OnTargetEvent			m_OnTargetAquired		= null;
	private		OnTargetEvent			m_OnTargetChanged		= null;
	private		OnTargetEvent			m_OnTargetLost			= null;

	private		RaycastHit				m_RaycastHit			= default( RaycastHit );
	private		SphereCollider			m_ViewTriggerCollider	= null;
	private		List<Entity>			m_AllTargets			= new List<Entity>();
	private		Entity[]				m_ValidTargets			= null;
	private		uint					m_MaxVisibleEntities	= 10;

	private		bool					m_NeedSetup				= true;

	private		TargetInfo				m_CurrentTargetInfo		= new TargetInfo();
	private		Quaternion				m_LookRotation			= Quaternion.identity;



	//////////////////////////////////////////////////////////////////////////
	// Awake
	private void Awake()
	{
		m_ViewTriggerCollider = GetComponent<SphereCollider>();
		m_ViewTriggerCollider.isTrigger = true;
		m_ViewTriggerCollider.radius = m_ViewDistance;

		m_LayerMask = 1 << LayerMask.NameToLayer("Entities");
	}


	//////////////////////////////////////////////////////////////////////////
	// OnValidate
	private void OnValidate()
	{
		m_ViewTriggerCollider = GetComponent<SphereCollider>();
		m_ViewTriggerCollider.isTrigger = true;
		m_ViewTriggerCollider.radius = m_ViewDistance;
	}


	//////////////////////////////////////////////////////////////////////////
	// Setup
	public	void	Setup( uint maxVisibleEntities )
	{
		m_MaxVisibleEntities = maxVisibleEntities;
		m_ValidTargets = new Entity[ m_MaxVisibleEntities ];

		m_NeedSetup = false;
	}


	//////////////////////////////////////////////////////////////////////////
	// CheckTargets
	private	void	CheckTargets()
	{
		for ( int i = m_AllTargets.Count - 1; i > 0; i-- )
		{
			Entity entity = m_AllTargets[ i ];
			if ( entity == null 
				|| entity.IsAlive == false
				|| entity.transform.gameObject.activeSelf == false )
			{
				m_AllTargets.RemoveAt( i );
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// CheckTargets
	public	void	UpdateTargets( ENTITY_TYPE newType )
	{
		if ( newType == m_EntityType )
			return;

		m_EntityType = newType;

		m_AllTargets.Clear();

		Collider[] colliders = Physics.OverlapSphere( transform.position, m_ViewTriggerCollider.radius );

		IEntity entity = null;
		System.Array.ForEach( colliders, ( Collider c ) => {

			if ( Utils.Base.SearchComponent( c.gameObject, ref entity, SearchContext.ALL, ( IEntity e ) => { return e.EntityType == newType; } ) )
			{
				// This avoid to the current entity being added
				if ( transform.parent.GetInstanceID() != entity.Transform.GetInstanceID() )
				{
					m_AllTargets.Add( entity as Entity );
				}
			}

		} );
	}


	//////////////////////////////////////////////////////////////////////////
	// CompareTargetsDistances
	private	int  CompareTargetsDistances( Vector3 currentPosition, Entity a, Entity b )
	{
		float distanceA = ( a.transform.position - currentPosition ).sqrMagnitude;
		float distanceB = ( b.transform.position - currentPosition ).sqrMagnitude;

		return ( distanceA > distanceB ) ? 1 : ( distanceA < distanceB ) ? -1 : 0;
	}


	//////////////////////////////////////////////////////////////////////////
	// ClearLastTarget
	private	void	ClearLastTarget()
	{
		// TARGET LOST
		if ( m_CurrentTargetInfo.HasTarget == true )
		{
			m_OnTargetLost ( m_CurrentTargetInfo );
		}
		m_CurrentTargetInfo.Reset();
	}


	//////////////////////////////////////////////////////////////////////////
	// UpdateFoV
	public	bool	UpdateFOV()
	{
		// Prepare results array
		System.Array.Clear( m_ValidTargets, 0, m_ValidTargets.Length );

#region Sanity Check
		{   // SANITY CHECK
			if ( m_NeedSetup == true )
			{
				( this as IFieldOfView ).Setup( maxVisibleEntities : 10 );
				print( transform.parent.name + " need Field of view setup, default settings applyed" );
				m_NeedSetup = false;
			}

			if ( m_AllTargets.Count == 0 )
			{
				ClearLastTarget();
				return false;
			}

			CheckTargets();

			if ( m_AllTargets.Count == 0 )
			{
				ClearLastTarget();
				return false;
			}
		}
#endregion

		// Choose view point
		Transform currentViewPoint	= ( m_ViewPoint == null ) ? transform : m_ViewPoint;

		// Sort targets by distance
		if ( m_AllTargets.Count > 1 )
			m_AllTargets.Sort( ( a, b ) => CompareTargetsDistances( currentViewPoint.position, a, b ) );

		// FIND ALL VISIBLE TARGETS
		int currentCount = 0;
		for ( int i = 0; i < m_AllTargets.Count && currentCount < m_MaxVisibleEntities; i++ )
		{
			Entity target = m_AllTargets[ i ];

			Vector3 targettablePosition = target.Interface.Transform.position;
			Vector3 direction = ( targettablePosition - currentViewPoint.position );

			m_LookRotation.SetLookRotation( direction, currentViewPoint.up );

			float angle = Quaternion.Angle( m_LookRotation, currentViewPoint.rotation );

			// CHECK IF IS IN VIEW CONE
			if ( angle <= ( m_ViewCone * 0.5f ) )
			{
				// CHECK IF HITTED IS A TARGET
				bool result = Physics.Raycast
				(
					origin:						currentViewPoint.position,
					direction:					direction,
					hitInfo:					out m_RaycastHit,
					maxDistance:				m_ViewDistance,
					layerMask:					m_LayerMask,
					queryTriggerInteraction:	QueryTriggerInteraction.Ignore
				);

				if ( result == true && m_RaycastHit.collider.GetInstanceID() == target.Interface.PhysicCollider.GetInstanceID() )
				{
					m_ValidTargets[ currentCount ] = target;
					currentCount ++;
				}
			}
		}

		if ( currentCount == 0 )
		{
			ClearLastTarget();
			return false;
		}

		IEntity currentTarget  = m_ValidTargets[ 0 ];
		IEntity previousTarget = m_CurrentTargetInfo.CurrentTarget;

		m_CurrentTargetInfo.CurrentTarget = currentTarget;
		m_CurrentTargetInfo.TargetSqrDistance = ( m_CurrentTargetInfo.CurrentTarget.Transform.position - currentViewPoint.position ).sqrMagnitude;
		
		// SET NEW TARGET
		if ( m_CurrentTargetInfo.HasTarget == false )
		{
			m_CurrentTargetInfo.HasTarget = true;			
			m_OnTargetAquired( m_CurrentTargetInfo );
		}
		else
		// CHANGING A TARGET
//		if ( m_CurrentTargetInfo.HasTarget == true )
		{
			if ( previousTarget != null && previousTarget.ID != currentTarget.ID )
			{
				m_OnTargetChanged( m_CurrentTargetInfo );
			}
		}

		return true;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnReset
	public	void	OnReset()
	{
		m_CurrentTargetInfo.Reset();
		System.Array.Clear( m_ValidTargets, 0, ( int ) m_MaxVisibleEntities );
		m_AllTargets.Clear();
	}


	//////////////////////////////////////////////////////////////////////////
	// OnTriggerEnter
	private void OnTriggerEnter( Collider other )
	{
		Entity entity = other.GetComponent<Entity>();
		if ( entity != null && entity.IsAlive == true && entity.Interface.EntityType == m_EntityType )
		{
			// This avoid to the current entity being added
			if ( entity.transform.GetInstanceID() == transform.parent.GetInstanceID() )
				return;

			if ( m_AllTargets.Contains( entity ) == true )
				return;

			m_AllTargets.Add( entity );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// OnTriggerExit
	private void OnTriggerExit( Collider other )
	{
		Entity entity = other.GetComponent<Entity>();
		if ( entity != null && m_AllTargets.Contains( entity ) == true )
		{
			m_AllTargets.Remove( entity );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// OnDrawGizmosSelected
	void OnDrawGizmosSelected()
	{
		float halfFOV = m_ViewCone * 0.5f;

		Transform currentViewPoint		= ( m_ViewPoint == null ) ? transform : m_ViewPoint;
		Gizmos.matrix = Matrix4x4.TRS( currentViewPoint.position, Quaternion.Euler( transform.rotation * ( Vector3.one * 0.5f ) ), Vector3.one );

		for ( float i = 0; i < 180f; i += 10f )
		{
			float cos = Mathf.Cos( i * Mathf.Deg2Rad );
			float sin = Mathf.Sin( i * Mathf.Deg2Rad );

			Vector3 axisRight =  currentViewPoint.up * cos +  currentViewPoint.right * sin;
			Vector3 axisLeft  = -currentViewPoint.up * cos + -currentViewPoint.right * sin;

			// left
			Quaternion leftRayRotation		= Quaternion.AngleAxis( halfFOV, axisLeft );
			Vector3 leftRayDirection		= ( leftRayRotation  * currentViewPoint.forward ).normalized;
			Gizmos.DrawRay( Vector3.zero, leftRayDirection  * m_ViewDistance );

			// right
			Quaternion rightRayRotation		= Quaternion.AngleAxis(  halfFOV, axisRight );
			Vector3 rightRayDirection		= ( rightRayRotation * currentViewPoint.forward ).normalized;
			Gizmos.DrawRay( Vector3.zero, rightRayDirection * m_ViewDistance );
		}
		Gizmos.matrix = Matrix4x4.identity;
	}

}
