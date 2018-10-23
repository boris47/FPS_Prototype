
using UnityEngine;
using System.Collections.Generic;

public	delegate	void	OnTargetEvent( TargetInfo_t targetInfo );



public interface IFieldOfView {
	
	OnTargetEvent		OnTargetAquired		{ set; }
	OnTargetEvent		OnTargetChanged		{ set; }
	OnTargetEvent		OnTargetUpdate		{ set; }
	OnTargetEvent		OnTargetLost		{ set; }

	float				Distance			{ get; set; }
	float				Angle				{ get; set; }
	Entity.ENTITY_TYPE	TargetType			{ get; set; }

	void				Setup				( uint maxVisibleEntities );
	bool				UpdateFOV			();
	void				OnReset				();
}



[RequireComponent( typeof( SphereCollider ) )]
public class FieldOfView : MonoBehaviour, IFieldOfView {

	
	private		OnTargetEvent			m_OnTargetAquired		= null;
	private		OnTargetEvent			m_OnTargetChanged		= null;
	private		OnTargetEvent			m_OnTargetUpdate		= null;
	private		OnTargetEvent			m_OnTargetLost			= null;

	public		OnTargetEvent			OnTargetAquired			{ set { m_OnTargetAquired = value; } }
	public		OnTargetEvent			OnTargetChanged			{ set { m_OnTargetChanged = value; } }
	public		OnTargetEvent			OnTargetUpdate			{ set { m_OnTargetUpdate = value; } }
	public		OnTargetEvent			OnTargetLost			{ set { m_OnTargetLost	= value; } }


	[SerializeField]
	private		Transform				m_ViewPoint				= null;

	[SerializeField, Range( 1f, 200f )]
	private		float					m_ViewDistance			= 10f;

	[SerializeField,Range( 1f, 150f)]
	private		float					m_ViewCone				= 100f;


	[ SerializeField ]
	private		Entity.ENTITY_TYPE		m_EntityType			= Entity.ENTITY_TYPE.NONE;

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

	Entity.ENTITY_TYPE IFieldOfView.TargetType
	{
		get { return m_EntityType; }
		set { m_EntityType = value; }
	}



	private		RaycastHit				m_RaycastHit			= default( RaycastHit );
	private		SphereCollider			m_ViewTriggerCollider	= null;
	private		List<Entity>			m_AllTargets			= new List<Entity>();
	private		Entity[]				m_ValidTargets			= null;
	private		uint					m_MaxVisibleEntities	= 10;

	private		bool					m_NeedSetup				= true;

	private		TargetInfo_t			m_CurrentTargetInfo		= default( TargetInfo_t );
	private		Quaternion				m_LookRotation			= Quaternion.identity;



	//////////////////////////////////////////////////////////////////////////
	// Awake
	private void Awake()
	{
		m_ViewTriggerCollider = GetComponent<SphereCollider>();
		m_ViewTriggerCollider.isTrigger = true;
		m_ViewTriggerCollider.radius = m_ViewDistance;

		m_LayerMask = Utils.Base.LayersAllButOne( 1, LayerMask.NameToLayer ("Bullets") );
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
	void	IFieldOfView.Setup( uint maxVisibleEntities )
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
			if ( entity == null 
				|| entity.Interface.IsActive == false 
				|| entity.Interface.Health <= 0.0f 
				|| entity.transform.gameObject.activeSelf == false )
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
	// ClearLastTarget
	private	void	ClearLastTarget()
	{
		// TARGET LOST
		if ( m_CurrentTargetInfo.HasTarget == true && m_OnTargetLost != null )
		{
			m_OnTargetLost ( m_CurrentTargetInfo );
		}
		m_CurrentTargetInfo = default( TargetInfo_t );
	}


	//////////////////////////////////////////////////////////////////////////
	// UpdateFoV
	bool	IFieldOfView.UpdateFOV()
	{
		// Prepare results array
		System.Array.Clear( m_ValidTargets, 0, m_ValidTargets.Length );

		{	// SANITY CHECK
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

		int currentCount = 0;

		// Choose view point
		Transform	currentViewPoint	= ( m_ViewPoint == null ) ? transform : m_ViewPoint;

		// Sort targets by distance
		if ( m_AllTargets.Count > 1 )
			m_AllTargets.Sort( ( a, b ) => CompareTargetsDistances( currentViewPoint.position, a, b ) );

		// FIND ALL VISIBLE TARGETS
		foreach( Entity target in m_AllTargets )
		{
			Vector3 targettablePosition = target.Interface.Transform.position;
			Vector3 direction = ( targettablePosition - currentViewPoint.position );

			m_LookRotation.SetLookRotation( direction, currentViewPoint.up );

			float angle = Quaternion.Angle( m_LookRotation, currentViewPoint.rotation );

			// CHECK IF IS IN VIEW CONE
			if ( angle <= ( m_ViewCone * 0.5f ) )
			{
//				Debug.DrawLine( currentViewPoint.position, targettablePosition, Color.white, 1.0f );

				bool result = Physics.Raycast
				(
					origin:						currentViewPoint.position,
					direction:					direction,
					hitInfo:					out m_RaycastHit,
					maxDistance:				Mathf.Infinity //,
//					layerMask:					m_LayerMask,
//					queryTriggerInteraction:	QueryTriggerInteraction.Ignore
				);
				/*
				if ( result && m_RaycastHit.collider != target.Interface.PhysicCollider )
				{
					print("steange " + m_RaycastHit.collider.name + ", " + target.Interface.PhysicCollider.name );
				}
				*/
				/*
				// CHECK IF HITTED IS A TARGET
				bool result = Physics.Linecast(
					currentViewPoint.position,
					targettablePosition,
					out m_RaycastHit //,
//					Utils.Base.LayersAllButOne( 1, m_LayerMaskToSkip ),
//					QueryTriggerInteraction.Ignore
				);
				*/
				if ( result == true && m_RaycastHit.collider == target.Interface.PhysicCollider )
				{
					m_ValidTargets[ currentCount ] = target;
					currentCount ++;
					
					if ( currentCount == m_ValidTargets.Length )
					{
						break;
					}
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
		if ( m_CurrentTargetInfo.HasTarget == false && m_OnTargetAquired != null )
		{
			m_CurrentTargetInfo.HasTarget = true;			
			m_OnTargetAquired ( m_CurrentTargetInfo );
		}
		else
		// CHANGING A TARGET
		if ( m_CurrentTargetInfo.HasTarget == true && previousTarget != null && previousTarget != currentTarget && m_OnTargetChanged != null )
		{
			m_OnTargetChanged( m_CurrentTargetInfo );
		}
		else

		if( m_CurrentTargetInfo.HasTarget == true && previousTarget != null && previousTarget == currentTarget && m_OnTargetUpdate != null )
		{
			m_OnTargetUpdate( m_CurrentTargetInfo );
		}

		return true;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnReset
	void	IFieldOfView.OnReset()
	{
		m_CurrentTargetInfo	= default( TargetInfo_t );
		System.Array.Clear( m_ValidTargets, 0, ( int ) m_MaxVisibleEntities );
		m_AllTargets.Clear();
	}


	//////////////////////////////////////////////////////////////////////////
	// OnTriggerEnter
	private void OnTriggerEnter( Collider other )
	{
		Entity entity = other.GetComponent<Entity>();
		if ( entity != null && entity.Interface.IsActive == true && entity.Interface.Health > 0.0f && entity.Interface.EntityType == m_EntityType )
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
