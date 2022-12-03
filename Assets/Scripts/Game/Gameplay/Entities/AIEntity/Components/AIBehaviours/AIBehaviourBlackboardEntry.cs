
using UnityEngine;

namespace Entities.AI.Components.Behaviours
{
	public enum EAIBehaviourBlackboardValueOp
	{
		ADD, CHANGE, REMOVE
	}

	public abstract class AIBehaviourBlackboardEntry : ScriptableObject
	{
		private readonly string m_BlackboardEntryKey = null;

		public abstract System.Type StoredType { get; }

		public string BlackboardEntryKey => m_BlackboardEntryKey;

		//////////////////////////////////////////////////////////////////////////
		public abstract bool HasValue();
	}
}

