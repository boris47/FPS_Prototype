
using UnityEngine;

namespace Entities.AI
{
	/// <summary> Used to get or store a value inside a blackboard </summary>
	[System.Serializable]
	public sealed class BlackboardEntryKey : System.IEquatable<BlackboardEntryKey>
	{
		public static readonly		BlackboardEntryKey		None = new BlackboardEntryKey("None", 0u);

		[SerializeField, HideInInspector]
		private						string					m_Name = "None";

		private	readonly			uint?					m_OverridenID = null;


		public						uint					UniqueId => m_OverridenID ?? Utils.Generic.GetUniqueId(m_Name);
		public						string					Name => m_Name;

		//////////////////////////////////////////////////////////////////////////
		bool System.IEquatable<BlackboardEntryKey>.Equals(BlackboardEntryKey other) => other?.UniqueId == UniqueId;

		//////////////////////////////////////////////////////////////////////////
		public override bool Equals(object obj) => obj is BlackboardEntryKey key && UniqueId == key.UniqueId;

		//////////////////////////////////////////////////////////////////////////
		public override int GetHashCode() => System.HashCode.Combine(UniqueId);

		//////////////////////////////////////////////////////////////////////////
		public static implicit operator BlackboardEntryKey(in string InString) => new BlackboardEntryKey(InString);
		public static implicit operator string(in BlackboardEntryKey InKey) => InKey?.m_Name;

		//////////////////////////////////////////////////////////////////////////
		public static bool operator ==(BlackboardEntryKey left, BlackboardEntryKey right) => left?.UniqueId == right?.UniqueId;

		//////////////////////////////////////////////////////////////////////////
		public static bool operator !=(BlackboardEntryKey left, BlackboardEntryKey right) => !(left == right);

		//////////////////////////////////////////////////////////////////////////
		private BlackboardEntryKey(in string InName, in uint? InOverrideID = null)
		{
			m_Name = InName;
			m_OverridenID = InOverrideID;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////////////

	[System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false)]
	public class BlackboardKeyTypeAttribute : System.Attribute
	{
		public readonly System.Type[] Types = new System.Type[0];

		//////////////////////////////////////////////////////////////////////////
		public BlackboardKeyTypeAttribute(params System.Type[] InTypes)
		{
			Types = InTypes ?? Types;
		}
	}

	//////////////////////////////////////////////////////////////////////////
	public static class Extension
	{
		public static bool IsValid(this BlackboardEntryKey ThisBlackboardEntryKey) => ThisBlackboardEntryKey.IsNotNull() && ThisBlackboardEntryKey != BlackboardEntryKey.None;
	}
}

