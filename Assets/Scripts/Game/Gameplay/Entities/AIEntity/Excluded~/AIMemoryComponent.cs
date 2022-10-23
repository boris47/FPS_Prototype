using System.Collections.Generic;
using UnityEngine;

namespace Entities.AI.Components
{
	[RequireComponent(typeof(AIController))]
	public partial class AIMemoryComponent : AIEntityComponent
	{
		[SerializeReference, ReadOnly]
		private List<MemoryBase> m_Memories = new List<MemoryBase>();


		private event System.Action<MemoryBase> m_OnMemoryAdded = delegate { };
		private event System.Action<MemoryBase, MemoryBase> m_OnMemoryChanged = delegate { };
		private event System.Action<MemoryBase, bool> m_OnMemoryRemoved = delegate { };
		
		public event System.Action<MemoryBase> OnMemoryAdded
		{
			add => m_OnMemoryAdded += value; remove => m_OnMemoryAdded -= value;
		}
		public event System.Action<MemoryBase, MemoryBase> OnMemoryChanged
		{
			add => m_OnMemoryChanged += value; remove => m_OnMemoryChanged -= value;
		}
		public event System.Action<MemoryBase, bool> OnMemoryRemoved
		{
			add => m_OnMemoryRemoved += value; remove => m_OnMemoryRemoved -= value;
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnEnable()
		{
			base.OnEnable();

			if (Utils.CustomAssertions.IsNotNull(GameManager.CyclesEvents))
			{
				GameManager.CyclesEvents.OnThink += OnThink;
			}
		}

		//////////////////////////////////////////////////////////////////////////
		private void OnThink()
		{
			for (int i = m_Memories.Count - 1; i >= 0; i--)
			{
				MemoryBase memory = m_Memories[i];
				if (!memory.IsValid())
				{
					m_Memories.RemoveAt(i);
				}
			}
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnDisable()
		{
			base.OnDisable();

			if (GameManager.CyclesEvents.IsNotNull())
			{
				GameManager.CyclesEvents.OnThink -= OnThink;
			}
		}

		//////////////////////////////////////////////////////////////////////////
		public void InvalidateMemory(MemoryIdentifier InIdentifier)
		{
			if (MemoryIdentifier.IsValid(InIdentifier) && m_Memories.TryFind(out MemoryBase OutMemory, out int _, m => m.Identifier == InIdentifier))
			{
				OutMemory.Invalidate();
			}
		}

		//////////////////////////////////////////////////////////////////////////
		public bool TryGetMemory<T>(MemoryIdentifier InIdentifier, out MemoryValue<T> OutMemory)
		{
			OutMemory = null;
			if (MemoryIdentifier.IsValid(InIdentifier) && m_Memories.TryFind(out MemoryBase OutMemoryBase, out int _, m => m.Identifier == InIdentifier))
			{
				OutMemory = OutMemoryBase as MemoryValue<T>;
			}
			return OutMemory.IsNotNull();
		}

		//////////////////////////////////////////////////////////////////////////
		public bool HasMemoryOf(MemoryIdentifier InIdentifier)
		{
			return MemoryIdentifier.IsValid(InIdentifier) && m_Memories.TryFind(out MemoryBase _, out int _, m => m.Identifier == InIdentifier);
		}

		//////////////////////////////////////////////////////////////////////////
		public void AddEntityToMemory(in MemoryIdentifier InIdentifier, in Entity InEntity)
		{
			if (MemoryIdentifier.IsValid(InIdentifier) && InEntity.IsNotNull())
			{
				AddToMemory(InIdentifier, new Memory_Entity(InIdentifier, InEntity));
			}
		}

		//////////////////////////////////////////////////////////////////////////
		public void AddTrajectoryToMemory(in MemoryIdentifier InIdentifier, in Vector3 InOrigin, in Vector3 InDirection)
		{
			if (MemoryIdentifier.IsValid(InIdentifier))
			{
				AddToMemory(InIdentifier, new Memory_Trajectory(InIdentifier, new Ray(InOrigin, InDirection)));
			}
		}

		//////////////////////////////////////////////////////////////////////////
		public void AddPositionToMemory(in MemoryIdentifier InIdentifier, in Vector3 InPosition)
		{
			if (MemoryIdentifier.IsValid(InIdentifier)) // Position Zero is valid
			{
				AddToMemory(InIdentifier, new Memory_Position(InIdentifier, InPosition));
			}
		}

		//////////////////////////////////////////////////////////////////////////
		public bool RemoveMemory(MemoryIdentifier InIdentifier)
		{
			bool bOutResult = false;
			if (MemoryIdentifier.IsValid(InIdentifier) && m_Memories.TryFind(out MemoryBase OutMemory, out int OutIndex, m => m.Identifier == InIdentifier))
			{
				bOutResult = true;
				m_Memories.RemoveAt(OutIndex);
				m_OnMemoryRemoved(OutMemory, false);
			}
			return bOutResult;
		}

		//////////////////////////////////////////////////////////////////////////
		private void AddToMemory<M>(MemoryIdentifier InIdentifier, in M InMemory) where M : MemoryBase
		{
			if (MemoryIdentifier.IsValid(InIdentifier) && InMemory.IsNotNull())
			{
				if (m_Memories.TryFind(out MemoryBase OutMemory, out int OutIndex, m => m.Identifier == InIdentifier))
				{
					m_Memories[OutIndex] = InMemory;
					m_OnMemoryChanged(OutMemory, InMemory);
				}
				else
				{
					m_Memories.Add(InMemory);
					m_OnMemoryAdded(InMemory);
				}
			}
		}
	}
}
