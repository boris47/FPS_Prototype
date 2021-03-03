
using UnityEngine;

public class Turret_AI_Behaviour_Evasive : AIBehaviour
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
	}

	public override void OnHit(Vector3 startPosition, Entity whoRef, float damage, bool canPenetrate = false)
	{
		base.OnHit(startPosition, whoRef, damage, canPenetrate);
	}

	public override void OnDestinationReached(Vector3 Destination)
	{
		base.OnDestinationReached(Destination);
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
		base.OnTargetChange();
	}

	public override void OnTargetLost()
	{
		base.OnTargetLost();
	}

	public override void OnKilled()
	{
		base.OnKilled();
	}
}

