using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Entities.AI.Components
{
	[System.Serializable]
	public sealed class Memory_Trajectory : MemoryValue<Ray>
	{
		//////////////////////////////////////////////////////////////////////////
		public Memory_Trajectory(in MemoryIdentifier InIdentifier, in Ray InRay) : base(InIdentifier, InRay)
		{

		}

		//////////////////////////////////////////////////////////////////////////
		public override bool IsValid() => !IsInvalidated;
	}
}
