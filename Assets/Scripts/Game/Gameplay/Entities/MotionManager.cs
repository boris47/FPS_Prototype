
using UnityEngine;

namespace Entities
{
	public abstract class MotionTransitionSnapshot
	{
		public Vector3 CurrentVelocity = Vector3.zero;
		public Transform Destination = null;
	}

	public interface IMotionTransition<T> where T : MotionTransitionSnapshot, new()
	{
		T CreateSnapshot();

		void PorcessSnapshot(T InSnapShot);
	}

	public interface IMotionManager
	{
		Vector3						Position					{ get; }
		Vector3						Destination					{ get; }
		Vector3						Velocity					{ get; }

		//////////////////////////////////////////////////////////////////////////
		bool CanSwim(SwimVolume InVolume) => false;


		void OnMotionVolumeEnter(MotionVolume InVolume);
		void OnMotionVolumeExit(MotionVolume InVolume);
		/*
		//////////////////////////////////////////////////////////////////////////
		void OnMotionVolumeEnter(MotionVolume InVolume)
		{

		}

		//////////////////////////////////////////////////////////////////////////
		void OnMotionVolumeExit(MotionVolume InVolume)
		{

		}
		*/
	}
}
