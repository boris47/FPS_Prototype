
using UnityEngine;

namespace Entities
{
	public abstract class MotionTransitionSnapshot
	{
		public Vector3 CurrentVelocity = Vector3.zero;
	}

	public interface IMotionTransition<T> where T : MotionTransitionSnapshot, new()
	{
		T CreateSnapshot();

		void PorcessSnapshot(T InSnapShot);
	}

	[RequireComponent(typeof(Entity))]
	public abstract class MotionManager : EntityComponent
	{
		// Swim motion related
		public abstract bool CanSwim(SwimVolume swimVolume);

		public abstract void OnSwimVolumeEnter(SwimVolume swimVolume);

		public abstract void OnSwimVolumeExit(SwimVolume swimVolume);
	}
}
