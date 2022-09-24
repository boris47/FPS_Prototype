using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Entities.AI.Components
{
	[System.Serializable]
	public abstract class MemoryBase
	{
		[SerializeField, ReadOnly]
		private MemoryIdentifier m_Identifier = null;

		[SerializeField, ReadOnly]
		private bool m_IsInvalidated = false;


		public MemoryIdentifier Identifier => m_Identifier;
		protected bool IsInvalidated => m_IsInvalidated;


		//////////////////////////////////////////////////////////////////////////
		public MemoryBase(in MemoryIdentifier InIdentifier)
		{
			m_Identifier = InIdentifier;
		}


		//////////////////////////////////////////////////////////////////////////
		public abstract bool IsValid();

		//////////////////////////////////////////////////////////////////////////
		public void Invalidate() => m_IsInvalidated = true;
	}

	[System.Serializable]
	public abstract class MemoryValue<T> : MemoryBase
	{
		[SerializeField]
		private			T 						m_Value			= default;

		private			T						m_DefaultValue	= default;
		public			T						Value			=> m_Value;

		//////////////////////////////////////////////////////////////////////////
		protected MemoryValue(in MemoryIdentifier InIdentifier, in T InValue) : base(InIdentifier)
		{
			m_Value = InValue;
		}
	}
}
