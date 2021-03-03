
using System.Collections;
using UnityEngine;

public enum EInvestigationDirection : uint
{
	RIGHT, LEFT, BACK, FRONT, END
}

public class Drone_AI_Behaviour_Seeker : AIBehaviour
{
	private EInvestigationDirection m_CurrentInvestigationDirection = EInvestigationDirection.RIGHT;

	private Coroutine m_InvestigationCO = null;

	public override void OnEnable()
	{
		base.OnEnable();
	}

	public override void OnDisable()
	{
		base.OnDisable();

		if (m_InvestigationCO.IsNotNull())
		{
			EntityData.EntityRef.StopCoroutine(m_InvestigationCO);
		}

		m_CurrentInvestigationDirection = EInvestigationDirection.RIGHT;
	}

	public override void OnSave(StreamUnit streamUnit)
	{
		base.OnSave(streamUnit);

		streamUnit.SetInternal($"{EBrainState.SEEKER.ToString()}_CurrentInvestigationDirection", m_CurrentInvestigationDirection);
	}

	public override void OnLoad(StreamUnit streamUnit)
	{
		base.OnLoad(streamUnit);

		m_CurrentInvestigationDirection = streamUnit.GetAsEnum<EInvestigationDirection>($"{EBrainState.SEEKER.ToString()}_CurrentInvestigationDirection");
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

		EntityData.EntityRef.ChangeState(EBrainState.ALARMED);
	}

	public override void OnLookRotationReached(Vector3 Direction)
	{
		base.OnLookRotationReached(Direction);
	}

	public override void OnDestinationReached(Vector3 Destination)
	{
		base.OnDestinationReached(Destination);

		EntityData.EntityRef.NavReset();

		// TODO
		// before returning to normal state should investigate around current position

		if (m_InvestigationCO == null)
		{
			IEnumerator InvestigateAroundCO()
			{
				while (m_CurrentInvestigationDirection < EInvestigationDirection.END)
				{
					yield return new WaitForSeconds(UnityEngine.Random.Range(0.7f, 1.3f));
					Vector3 newDirection = Vector3.zero;
					switch (m_CurrentInvestigationDirection)
					{
						case EInvestigationDirection.RIGHT: newDirection = EntityData.Head_Right   *  1f; print("Right"); break;
						case EInvestigationDirection.LEFT:  newDirection = EntityData.Head_Forward * -1f; print("left");  break;
						case EInvestigationDirection.BACK:  newDirection = EntityData.Head_Right   * -1f; print("back");  break;
						case EInvestigationDirection.FRONT: newDirection = EntityData.Head_Forward * -1f; print("Front"); break;
					}

					m_CurrentInvestigationDirection++;
					EntityData.EntityRef.SetPointToLookAt(EntityData.Head_Position + newDirection, ELookTargetMode.HEAD_ONLY);
					yield return new WaitUntil(() => Vector3.Angle(EntityData.Head_Forward, newDirection) < 4.5f);
				}

				EntityData.EntityRef.RequestMovement(EntityData.SpawnBodyLocation);
				EntityData.EntityRef.SetPointToLookAt(EntityData.SpawnHeadLocation + EntityData.SpawnHeadRotation.GetVector(Vector3.forward));
				EntityData.EntityRef.ChangeState(EBrainState.NORMAL);
				m_CurrentInvestigationDirection = EInvestigationDirection.RIGHT;
				m_InvestigationCO = null;
			}

			m_CurrentInvestigationDirection = EInvestigationDirection.RIGHT;
			m_InvestigationCO = CoroutinesManager.Start(InvestigateAroundCO(), "Drone-Seeker::On DestinationReached: Start of search");
		}
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

		// Update movement speed along path
		if (EntityData.EntityRef.HasDestination)
		{
			if (EntityData.EntityRef.IsAllignedHeadToPoint)
			{
				EntityData.AgentSpeed = EntityData.EntityRef.MaxAgentSpeed;
			}

			if (EntityData.EntityRef.IsDisallignedHeadWithPoint)
			{
				EntityData.AgentSpeed = 0.0f;
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

		// Destination
		{
			Vector3 projectedPoint = Utils.Math.ProjectPointOnPlane(
				planeNormal: EntityData.Body_Up,
				planePoint: EntityData.Body_Position,
				point: EntityData.TargetInfo.CurrentTarget.AsEntity.transform.position
			);


			EntityData.EntityRef.RequestMovement(projectedPoint);
		}

		// Switch brain State
		EntityData.EntityRef.ChangeState(EBrainState.ATTACKER);
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

