
using UnityEngine;

public class Turret_AI_Behaviour_Alarmed : AIBehaviour
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
		
	}

	public override void OnLateFrame(float DeltaTime)
	{

	}

	public override void OnPauseSet(bool isPaused)
	{
		
	}

	public override void OnTargetAcquired(TargetInfo targetInfo)
	{
		// Switch brain State
		EntityData.EntityRef.Behaviours.ChangeState(EBrainState.ATTACKER);
	}

	public override void OnTargetChange(TargetInfo targetInfo)
	{
		
	}

	public override void OnTargetLost(TargetInfo targetInfo)
	{
		
	}

	public override void OnKilled(Entity entityKilled)
	{

	}
}

