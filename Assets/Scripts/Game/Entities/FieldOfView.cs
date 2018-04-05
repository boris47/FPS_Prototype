
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AI_Behaviours;

public interface IFieldOfView {

	void	Setup( uint maxVisibleEntities );
	bool	UpdateFOV( out TargetInfo_t targetInfo );
}


[RequireComponent( typeof( SphereCollider ) )]
public class FieldOfView : MonoBehaviour, IFieldOfView {

	[SerializeField]
	private		Transform				m_ViewPoint				= null;

	[SerializeField, Range( 1f, 200f )]
	private		float					m_ViewDistance			= 10f;

	[SerializeField,Range( 20f, 150f)]
	private		float					m_ViewCone				= 100f;


	private		RaycastHit				m_RaycastHit			= default( RaycastHit );
	private		SphereCollider			m_ViewTriggerCollider	= null;
	private		List<Entity>			m_AllTargets			= new List<Entity>();
	private		Entity[]				m_ValidTargets			= null;
	private		uint					m_MaxVisibleEntities	= 10;

	private		bool					m_NeedSetup				= true;



	//////////////////////////////////////////////////////////////////////////
	// Awake
	private void Awake()
	{
		m_ViewTriggerCollider = GetComponent<SphereCollider>();
		m_ViewTriggerCollider.isTrigger = true;
		m_ViewTriggerCollider.radius = m_ViewDistance;
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
		m_ValidTargets = new Entity[ maxVisibleEntities ];

		m_NeedSetup = false;
	}


	//////////////////////////////////////////////////////////////////////////
	// CheckTargets
	private	void	CheckTargets()
	{
		for ( int i = m_AllTargets.Count - 1; i > 0; i-- )
		{
			Entity entity = m_AllTargets[ i ];
			if ( entity == null )
			{
				m_AllTargets.RemoveAt( i );
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// CompareTargetsDistances
	private	int  CompareTargetsDistances( Vector3 currentPosition, Entity a, Entity b )
	{
		float distanceA = ( a.transform.position - currentPosition ).sqrMagnitude;
		float distanceB = ( b.transform.position - currentPosition ).sqrMagnitude;

		if ( distanceA > distanceB )
			return 1;

		if ( distanceA < distanceB )
			return -1;

		return 0;
	}


	//////////////////////////////////////////////////////////////////////////
	// UpdateFoV
	public	bool	UpdateFOV( out TargetInfo_t targetInfo )
	{
		targetInfo = default( TargetInfo_t );

		// Prepare results array
		System.Array.Clear( m_ValidTargets, 0, m_ValidTargets.Length );

		{	// SANITY CHECK
			if ( m_NeedSetup == true )
			{
				Setup( maxVisibleEntities : 10 );
				print( transform.parent.name + " need Field of view setup, default settings applied" );
				m_NeedSetup = false;
			}

			if ( m_AllTargets.Count == 0 )
				return false;

			CheckTargets();

			if ( m_AllTargets.Count == 0 )
				return false;
		}

		int	currentCount = 0;

		// Choose view point
		Transform	currentViewPoint	= ( m_ViewPoint == null ) ? transform : m_ViewPoint;

		// Sort targets by distance
		m_AllTargets.Sort( ( a, b ) => CompareTargetsDistances( currentViewPoint.position, a, b ) );

		// FIND ALL VISIBLE TARGETS
		foreach( Entity entity in m_AllTargets )
		{
			Vector3 direction = ( entity.transform.position - currentViewPoint.position );

			// CHECK IF THERE IS NOT OBSTACLES
			if ( Physics.Raycast( currentViewPoint.position, direction, out m_RaycastHit ) )
			{
				if ( m_RaycastHit.transform != entity.transform )
					continue;

				// CHECK IF IS IN VIEW CONE
				if ( Vector3.Angle( currentViewPoint.forward, direction.normalized ) < ( m_ViewCone * 0.5f ) )
				{
					m_ValidTargets[ currentCount ] = entity;
					currentCount ++;

					// Debug stuff
					Debug.DrawLine( currentViewPoint.position, entity.transform.position, Color.red );

					if ( currentCount == m_ValidTargets.Length )
					{
						break;
					}
				}
			}
		}

		if ( currentCount == 0 )
			return false;

		targetInfo.CurrentTarget = m_ValidTargets[ 0 ];
		targetInfo.TargetSqrDistance = ( targetInfo.CurrentTarget.transform.position - currentViewPoint.position ).sqrMagnitude;
		targetInfo.HasTarget = true;
		return true;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnTriggerEnter
	private void OnTriggerEnter( Collider other )
	{
		Entity entity = other.GetComponent<Entity>();
		if ( entity is LiveEntity )
		{
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
		if ( entity is LiveEntity )
		{
			m_AllTargets.Remove( entity );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// OnDrawGizmosSelected
	void OnDrawGizmosSelected()
	{
		float halfFOV = m_ViewCone * 0.5f;
		Quaternion leftRayRotation		= Quaternion.AngleAxis( -halfFOV, Vector3.up );
		Quaternion rightRayRotation		= Quaternion.AngleAxis(  halfFOV, Vector3.up );
		Vector3 leftRayDirection		= leftRayRotation  * transform.forward;
		Vector3 rightRayDirection		= rightRayRotation * transform.forward;

		Gizmos.DrawRay( transform.position, leftRayDirection  * m_ViewDistance );
		Gizmos.DrawRay( transform.position, rightRayDirection * m_ViewDistance );
	}

}
