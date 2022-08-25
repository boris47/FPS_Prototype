
using UnityEngine;

namespace Entities
{
	[RequireComponent(typeof(Entity))]
	public abstract class MotionManager : EntityComponent
	{
		// Swim motion related
		public abstract bool CanSwim(SwimVolume swimVolume);

		public abstract void OnSwimVolumeEnter(SwimVolume swimVolume);

		public abstract void OnSwimVolumeExit(SwimVolume swimVolume);
	}
}
