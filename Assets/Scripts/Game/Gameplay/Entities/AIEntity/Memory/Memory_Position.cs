using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Entities.AI.Components
{
	[System.Serializable]
	public sealed class Memory_Position : MemoryValue<Vector3>
	{
		//////////////////////////////////////////////////////////////////////////
		public Memory_Position(in MemoryIdentifier InIdentifier, in Vector3 InPosition) : base(InIdentifier, InPosition)
		{

		}

		//////////////////////////////////////////////////////////////////////////
		public override bool IsValid() => !IsInvalidated;
	}
}
