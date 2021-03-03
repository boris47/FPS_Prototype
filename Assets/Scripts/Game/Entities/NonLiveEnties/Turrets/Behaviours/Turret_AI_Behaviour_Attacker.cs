
using UnityEngine;

public class Turret_AI_Behaviour_Attacker : AIBehaviour
{
	public override void OnEnable()
	{
		base.OnEnable();
	}

	public override void OnDisable()
	{
		base.OnDisable();
	}

	public override void OnSave(StreamUnit streamUnit)
	{
		base.OnSave(streamUnit);
	}

	public override void OnLoad(StreamUnit streamUnit)
	{
		base.OnLoad(streamUnit);
	}

	public override void OnHit(IBullet bullet)
	{
		base.OnHit(bullet);

		OnHit(bullet.StartPosition, bullet.WhoRef, bullet.Damage, bullet.CanPenetrate);
	}

	public override void OnHit(Vector3 startPosition, Entity whoRef, float damage, bool canPenetrate = false)
	{
		base.OnHit(startPosition, whoRef, damage, canPenetrate);

		EntityData.EntityRef.SetPointToLookAt(startPosition);
	}

	public override void OnDestinationReached(Vector3 Destination)
	{
		base.OnDestinationReached(Destination);

		EntityData.EntityRef.NavReset();
	}

	public override void OnThink()
	{
		base.OnThink();
	}

	public override void OnPhysicFrame(float FixedDeltaTime)
	{
		base.OnPhysicFrame(FixedDeltaTime);
	}

	public override void OnFrame(float DeltaTime)
	{
		base.OnFrame(DeltaTime);

		// Update targeting
		if (EntityData.TargetInfo.HasTarget)
		{
			EntityData.EntityRef.SetPointToLookAt(EntityData.TargetInfo.CurrentTarget.AsEntity.transform.position);

			// with a target, if gun alligned, fire
			if (EntityData.EntityRef.CanFire() == true )
			{
				EntityData.EntityRef.FireWeapon();
			}
		}
	}

	public override void OnPauseSet(bool isPaused)
	{
		base.OnPauseSet(isPaused);
	}

	public override void OnTargetAcquired()
	{
		base.OnTargetAcquired();
	}

	public override void OnTargetChange()
	{
		
	}

	public override void OnTargetLost()
	{
		base.OnTargetChange();

		// Orientation
		{
			Vector3 newPointToLookAt = EntityData.TargetInfo.CurrentTarget.AsEntity.transform.position + EntityData.TargetInfo.CurrentTarget.RigidBody.velocity.normalized;
			Vector3 projectedPoint = Utils.Math.ProjectPointOnPlane( 
				planeNormal: EntityData.Body_Up,
				planePoint: EntityData.Head_Position,
				point:			newPointToLookAt
			);

			EntityData.EntityRef.SetPointToLookAt( projectedPoint );
		}

		EntityData.EntityRef.ChangeState( EBrainState.SEEKER );
	}

	public override void OnKilled()
	{
		base.OnKilled();
	}

}

