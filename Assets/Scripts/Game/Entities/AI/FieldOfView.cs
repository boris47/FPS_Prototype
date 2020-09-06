
using UnityEngine;
using System.Collections.Generic;

public	delegate	void	OnTargetEvent( TargetInfo targetInfo );
public	delegate	void	OnTargetsAcquired( Entity[] entities );


public interface IFieldOfView {
	
	OnTargetEvent		OnTargetAquired		{ set; }
	OnTargetEvent		OnTargetChanged		{ set; }
	OnTargetEvent		OnTargetLost		{ set; }

	float				Distance			{ get; set; }
	float				Angle				{ get; set; }
	EEntityType			TargetType			{ get; set; }

	void				Setup				( uint maxVisibleEntities );
	void				UpdateFOV			();
	void				OnReset				();
}



[RequireComponent( typeof( SphereCollider ) )]
public class FieldOfView : MonoBehaviour, IFieldOfView {

	public		OnTargetEvent			OnTargetAquired			{ set { this.m_OnTargetAquired = value; } }
	public		OnTargetEvent			OnTargetChanged			{ set { this.m_OnTargetChanged = value; } }
	public		OnTargetEvent			OnTargetLost			{ set { this.m_OnTargetLost	  = value; } }
	public		OnTargetsAcquired		OnTargetsAcquired		{ set { this.m_OnTargetsAcquired = value; } }


	[SerializeField]
	private		Transform				m_ViewPoint				= null;

	[SerializeField, Range( 1f, 200f )]
	private		float					m_ViewDistance			= 10f;

	[SerializeField,Range( 1f, 150f)]
	private		float					m_ViewCone				= 100f;


	[ SerializeField ]
	private		EEntityType				m_EntityType			= EEntityType.NONE;

	[SerializeField]
	private		LayerMask				m_LayerMask				 = default( LayerMask );


	float	IFieldOfView.Distance
	{
		get { return this.m_ViewDistance; }
		set { this.m_ViewDistance = value; }
	}

	float	IFieldOfView.Angle
	{
		get { return this.m_ViewCone; }
		set { this.m_ViewCone = value; }
	}

	EEntityType IFieldOfView.TargetType
	{
		get { return this.m_EntityType; }
		set { this.m_EntityType = value; }
	}

	private		OnTargetEvent			m_OnTargetAquired		= null;
	private		OnTargetEvent			m_OnTargetChanged		= null;
	private		OnTargetEvent			m_OnTargetLost			= null;
	private		OnTargetsAcquired		m_OnTargetsAcquired		= null;

	private		RaycastHit				m_RaycastHit			= default( RaycastHit );
	private		SphereCollider			m_ViewTriggerCollider	= null;
	private		List<Entity>			m_AllTargets			= new List<Entity>();
	private		Entity[]				m_ValidTargets			= null;
	private		uint					m_MaxVisibleEntities	= 10;

	private		TargetInfo				m_CurrentTargetInfo		= new TargetInfo();
	private		Quaternion				m_LookRotation			= Quaternion.identity;



	//////////////////////////////////////////////////////////////////////////
	// Awake
	private void Awake()
	{
		this.m_ViewTriggerCollider = this.GetComponent<SphereCollider>();
		this.m_ViewTriggerCollider.isTrigger = true;
		this.m_ViewTriggerCollider.radius = this.m_ViewDistance;

		this.m_LayerMask = Utils.LayersHelper.Layers_AllButOne( "Shield" );// 1 << LayerMask.NameToLayer("Entities");
	}



	//////////////////////////////////////////////////////////////////////////
	// OnValidate
	private void OnValidate()
	{
		this.m_ViewTriggerCollider = this.GetComponent<SphereCollider>();
		this.m_ViewTriggerCollider.isTrigger = true;
		this.m_ViewTriggerCollider.radius = this.m_ViewDistance;
	}



	//////////////////////////////////////////////////////////////////////////
	// Setup
	public	void	Setup( uint maxVisibleEntities )
	{
		this.m_MaxVisibleEntities = maxVisibleEntities;
		this.m_ValidTargets = new Entity[this.m_MaxVisibleEntities ];

		Collider[] colliders = null;
		if (this.transform.parent && this.transform.parent.SearchComponents( ref colliders, ESearchContext.CHILDREN ) )
		{
			System.Array.ForEach( colliders, ( c ) => Physics.IgnoreCollision(this.m_ViewTriggerCollider, c ) );
		}
	}



	//////////////////////////////////////////////////////////////////////////
	// CheckTargets
	public	void	UpdateTargets( EEntityType newType )
	{
		if ( newType == this.m_EntityType )
			return;

		this.m_EntityType = newType;

		this.m_AllTargets.Clear();

		Collider[] colliders = Physics.OverlapSphere(this.transform.position, this.m_ViewTriggerCollider.radius, 1, QueryTriggerInteraction.Ignore );

		// This avoid to the current entity being added
		List<Collider> list = new List<Collider>( colliders );
		list.Remove(this.m_ViewTriggerCollider );
		colliders = list.ToArray();

		IEntity entityComponent = null;
		void addToTargets( Collider c )
		{
			if ( Utils.Base.SearchComponent<IEntity>( c.gameObject, ref entityComponent, ESearchContext.CHILDREN, ( IEntity e ) => { return e.EntityType == newType; } ) )
			{
				this.m_AllTargets.Add( entityComponent as Entity );
			}
		}
		System.Array.ForEach( colliders, addToTargets );
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
		if (this.m_CurrentTargetInfo.HasTarget == true )
		{
			this.m_CurrentTargetInfo.Type = ETargetInfoType.LOST;
			this.m_OnTargetLost (this.m_CurrentTargetInfo );
		}
		this.m_CurrentTargetInfo.Reset();
	}



	//////////////////////////////////////////////////////////////////////////
	// UpdateFoV
	public	void	UpdateFOV()
	{
		// Prepare results array
		System.Array.Clear(this.m_ValidTargets, 0, this.m_ValidTargets.Length );

		if (this.m_AllTargets.Count == 0 )
		{
			this.ClearLastTarget();
			return;
		}


		// Choose view point
		Transform currentViewPoint = this.m_ViewPoint ?? this.transform;

		// Sort targets by distance
		if (this.m_AllTargets.Count > 1 )
			this.m_AllTargets.Sort( ( a, b ) => this.CompareTargetsDistances( currentViewPoint.position, a, b ) );

		// FIND ALL VISIBLE TARGETS
		int currentCount = 0;
		for ( int i = 0; i < this.m_AllTargets.Count && currentCount < this.m_MaxVisibleEntities; i++ )
		{
			Entity target = this.m_AllTargets[ i ];

			Vector3 targettablePosition = target.AsInterface.AsEntity.transform.position;
			Vector3 direction = ( targettablePosition - currentViewPoint.position );

			this.m_LookRotation.SetLookRotation( direction, currentViewPoint.up );

			float angle = Quaternion.Angle(this.m_LookRotation, currentViewPoint.rotation );

			// CHECK IF IS IN VIEW CONE
			if ( angle <= (this.m_ViewCone * 0.5f ) )
			{
				// CHECK IF HITTED IS A TARGET
				bool result = Physics.Raycast
				(
					origin:						currentViewPoint.position,
					direction:					direction,
					hitInfo:					out this.m_RaycastHit,
					maxDistance: this.m_ViewDistance,
					layerMask: this.m_LayerMask,
					queryTriggerInteraction:	QueryTriggerInteraction.Ignore
				);
				

				if ( result == true && this.m_RaycastHit.collider.GetInstanceID() == target.AsInterface.PhysicCollider.GetInstanceID() )
				{
					this.m_ValidTargets[ currentCount ] = target;
					currentCount ++;
				}
			}
		}

		if ( currentCount == 0 )
		{
			this.ClearLastTarget();
			return;
		}

		IEntity currentTarget  = this.m_ValidTargets[ 0 ];
		IEntity previousTarget = this.m_CurrentTargetInfo.CurrentTarget;

		this.m_CurrentTargetInfo.CurrentTarget = currentTarget;
		this.m_CurrentTargetInfo.TargetSqrDistance = (this.m_CurrentTargetInfo.CurrentTarget.AsEntity.transform.position - currentViewPoint.position ).sqrMagnitude;
		
		// SET NEW TARGET
		if (this.m_CurrentTargetInfo.HasTarget == false )
		{
			this.m_CurrentTargetInfo.HasTarget = true;
			this.m_CurrentTargetInfo.Type = ETargetInfoType.ACQUIRED;
			this.m_OnTargetAquired(this.m_CurrentTargetInfo );
		}
		else
		// CHANGING A TARGET
		{
			if ( previousTarget != null && previousTarget.ID != currentTarget.ID )
			{
				this.m_CurrentTargetInfo.Type = ETargetInfoType.LOST;
				this.m_OnTargetChanged(this.m_CurrentTargetInfo );
			}
		}
	}



	//////////////////////////////////////////////////////////////////////////
	// OnReset
	public	void	OnReset()
	{
		this.m_CurrentTargetInfo.Reset();
		System.Array.Clear(this.m_ValidTargets, 0, ( int )this.m_MaxVisibleEntities );
		this.m_AllTargets.Clear();
	}



	//////////////////////////////////////////////////////////////////////////
	// OnTriggerEnter
	private void OnTriggerEnter( Collider other )
	{
		Entity entity = other.GetComponent<Entity>();
		if ( entity.IsNotNull() && entity.IsAlive == true && entity.AsInterface.EntityType == this.m_EntityType )
		{
			// This avoid to the current entity being added
		//	if ( entity.transform.GetInstanceID() == transform.parent.GetInstanceID() )
		//		return;

			if (this.m_AllTargets.Contains( entity ) == true )
				return;

			entity.OnEvent_Killed += ( Entity entityKilled ) => {
				this.m_AllTargets.Remove( entityKilled );
			};

			this.m_AllTargets.Add( entity );
		}
	}



	//////////////////////////////////////////////////////////////////////////
	// OnTriggerExit
	private void OnTriggerExit( Collider other )
	{
		Entity entity = other.GetComponent<Entity>();
		if ( entity.IsNotNull() && this.m_AllTargets.Contains( entity ) == true )
		{
			this.m_AllTargets.Remove( entity );

			entity.OnEvent_Killed -= ( Entity entityKilled ) => {
				this.m_AllTargets.Remove( entityKilled );
			};
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// OnDrawGizmosSelected
	void OnDrawGizmosSelected()
	{
		float halfFOV = this.m_ViewCone * 0.5f;

		Transform currentViewPoint		= (this.m_ViewPoint == null ) ? this.transform : this.m_ViewPoint;
		Gizmos.matrix = Matrix4x4.TRS( currentViewPoint.position, Quaternion.Euler(this.transform.rotation * ( Vector3.one * 0.5f ) ), Vector3.one );

		for ( float i = 0; i < 180f; i += 10f )
		{
			float cos = Mathf.Cos( i * Mathf.Deg2Rad );
			float sin = Mathf.Sin( i * Mathf.Deg2Rad );

			Vector3 axisRight =  currentViewPoint.up * cos +  currentViewPoint.right * sin;
			Vector3 axisLeft  = -currentViewPoint.up * cos + -currentViewPoint.right * sin;

			// left
			Quaternion leftRayRotation		= Quaternion.AngleAxis( halfFOV, axisLeft );
			Vector3 leftRayDirection		= ( leftRayRotation  * currentViewPoint.forward ).normalized;
			Gizmos.DrawRay( Vector3.zero, leftRayDirection  * this.m_ViewDistance );

			// right
			Quaternion rightRayRotation		= Quaternion.AngleAxis(  halfFOV, axisRight );
			Vector3 rightRayDirection		= ( rightRayRotation * currentViewPoint.forward ).normalized;
			Gizmos.DrawRay( Vector3.zero, rightRayDirection * this.m_ViewDistance );
		}
		Gizmos.matrix = Matrix4x4.identity;
	}

}


public enum ETargetInfoType {

	NONE, ACQUIRED, CHANGED, LOST

}

[System.Serializable]
public class TargetInfo {
	public	bool	HasTarget;
	public	IEntity	CurrentTarget;
	public	ETargetInfoType Type;
	public	float	TargetSqrDistance;

	public	void	Update( TargetInfo Infos )
	{
		this.HasTarget			= Infos.HasTarget;
		this.CurrentTarget		= Infos.CurrentTarget;
		this.TargetSqrDistance	= Infos.TargetSqrDistance;
	}

	public	void	Reset()
	{
		this.Type				= ETargetInfoType.NONE;
		this.HasTarget			= false;
		this.CurrentTarget		= null;
		this.TargetSqrDistance	= 0.0f;
	}
}