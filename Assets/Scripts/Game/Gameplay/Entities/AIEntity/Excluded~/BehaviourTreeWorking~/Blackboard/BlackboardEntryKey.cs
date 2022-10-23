using System.Collections;
using UnityEngine;

namespace Entities.AI
{
	/// <summary> Used to get or store a value inside a blackboard </summary>
	[System.Serializable]
	[CreateAssetMenu(fileName = "", menuName = "Blackboard/Entry Key")]
	
	public sealed class BlackboardEntryKey: ScriptableObject
	{
#if UNITY_EDITOR
		[SerializeField]
		private string m_DebugName = string.Empty;
		public string DebugName => m_DebugName;
#endif
		private string m_TypeName => GetType().Name;


		//////////////////////////////////////////////////////////////////////////
		public bool IsEqualTo(in BlackboardEntryKey InOtherKey) => InOtherKey.name.Equals(name);
		
	}
}

