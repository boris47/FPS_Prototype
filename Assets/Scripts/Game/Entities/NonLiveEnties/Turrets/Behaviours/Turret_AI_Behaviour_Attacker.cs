
using UnityEngine;

public class Turret_AI_Behaviour_Attacker : AIBehaviour
{
	public override void OnEnable()
	{
		
	}

	public override void OnDisable()
	{
		
	}

	public override void OnSave(StreamUnit streamUnit)
	{
		
	}

	public override void OnLoad(StreamUnit streamUnit)
	{
		
	}

	public override void OnHit(IBullet bullet)
	{
		OnHit(bullet.StartPosition, bullet.WhoRef, bullet.Damage, bullet.CanPenetrate);
	}

	public override void OnHit(Vector3 startPosition, Entity whoRef, float damage, bool canPenetrate = false)
	{
		EntityData.EntityRef.Behaviours.SetPointToLookAt(startPosition);
	}

	public override void OnDestinationReached(Vector3 Destination)
	{
		EntityData.EntityRef.Navigation.NavReset();
	}

	public override void OnLookRotationReached(Vector3 Direction)
	{
		
	}

	public override void OnThink()
	{
		
	}

	public override void OnPhysicFrame(float FixedDeltaTime)
	{
		
	}

	public override void OnFrame(float DeltaTime)
	{
		// Update targeting
		if (EntityData.TargetInfo.HasTarget)
		{
			EntityData.EntityRef.Behaviours.SetPointToLookAt(EntityData.TargetInfo.CurrentTarget.transform.position);

		// with a target, if gun alligned, fire TODO
		//	if (EntityData.EntityRef.CanFire() == true )
		//	{
		//		EntityData.EntityRef.FireWeapon();
		//	}
		}
	}

	public override void OnLateFrame(float DeltaTime)
	{
		
	}

	public override void OnPauseSet(bool isPaused)
	{
		
	}

	public override void OnTargetAcquired(TargetInfo targetInfo)
	{
		
	}

	public override void OnTargetChange(TargetInfo targetInfo)
	{
		
	}

	public override void OnTargetLost(TargetInfo targetInfo)
	{
		// Orientation
		{
			Vector3 newPointToLookAt = EntityData.TargetInfo.CurrentTarget.transform.position + EntityData.TargetInfo.CurrentTarget.EntityRigidBody.velocity.normalized;
			Vector3 projectedPoint = Utils.Math.ProjectPointOnPlane( 
				planeNormal: EntityData.Body_Up,
				planePoint: EntityData.Head_Position,
				point:			newPointToLookAt
			);

			EntityData.EntityRef.Behaviours.SetPointToLookAt( projectedPoint );
		}

		EntityData.EntityRef.Behaviours.ChangeState(EBrainState.SEEKER);
	}

	public override void OnKilled(Entity entityKilled)
	{
		
	}
}

