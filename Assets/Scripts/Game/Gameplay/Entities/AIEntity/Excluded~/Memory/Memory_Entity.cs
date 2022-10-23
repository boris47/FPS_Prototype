using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Entities.AI.Components
{
	[System.Serializable]
	public sealed class Memory_Entity : MemoryValue<Entity>
	{
		//////////////////////////////////////////////////////////////////////////
		public Memory_Entity(in MemoryIdentifier InIdentifier, in Entity InEntity) : base(InIdentifier, InEntity)
		{

		}

		//////////////////////////////////////////////////////////////////////////
		public override bool IsValid()
		{
			return !(IsInvalidated || !Value.IsNotNull());
		}
	}
}
