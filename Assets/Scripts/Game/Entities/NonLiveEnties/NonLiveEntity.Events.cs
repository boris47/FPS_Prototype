
using UnityEngine;
using UnityEngine.AI;


public abstract partial class NonLiveEntity : Entity {

	


	//////////////////////////////////////////////////////////////////////////

	protected	override	bool	OnLoad( StreamData streamData, ref StreamUnit streamUnit )
	{
		bool bResult = base.OnLoad( streamData, ref streamUnit );
		if (bResult)
		{
			// Health
			m_Health = streamUnit.GetAsFloat( "Health" );

			// Shield TODO
		//	if (m_Shield.IsNotNull())
		//	{
		//		m_Shield.OnLoad( streamData, streamUnit );
		//	}

			// Internals
	//		m_NavHasDestination					= streamUnit.GetAsBool( "HasDestination" );
	//		m_HasFaceTarget						= streamUnit.GetAsBool( "HasFaceTarget" );
	//		m_Destination						= streamUnit.GetAsVector( "Destination" );
	//		m_PointToFace						= streamUnit.GetAsVector( "PointToFace" );
		//	m_NavCanMoveAlongPath				= streamUnit.GetAsBool( "IsMoving" );
		//	m_IsAllignedBodyToPoint				= streamUnit.GetAsBool( "IsAllignedBodyToDestination" );
		//	m_IsAllignedHeadToPoint				= streamUnit.GetAsBool( "IsAllignedGunToPoint" );
	//		m_DistanceToTravel					= streamUnit.GetAsFloat( "DistanceToTravel" );

			// Body and Gun
			{
		//		m_BodyTransform.localRotation	= streamUnit.GetAsQuaternion( "BodyRotation" );;
	//			m_GunTransform.localRotation	= streamUnit.GetAsQuaternion( "GunRotation" );
			}

			// Brain state
	//		m_Brain.ChangeState ( streamUnit.GetAsEnum<BrainState>( "BrainState" ) );
		}
		return bResult;
	}
}
