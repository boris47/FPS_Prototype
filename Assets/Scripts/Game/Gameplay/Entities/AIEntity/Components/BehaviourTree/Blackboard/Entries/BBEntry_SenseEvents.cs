
namespace Entities.AI
{
	using Components.Senses;

	[System.Serializable]
	public class BBEntry_SightEvent : BlackboardEntryKeyValue<SightEvent>
	{
#if UNITY_EDITOR
		protected override void OnGUIlayout()
		{
			UnityEditor.EditorGUILayout.ObjectField(Value, StoredType, false);
		}
#endif
	}

	//////////////////////////////////////////////////////////////////////////

	[System.Serializable]
	public sealed class BBEntry_SoundHeardEvent : BlackboardEntryKeyValue<HearingEvent>
	{
#if UNITY_EDITOR
		protected override void OnGUIlayout()
		{
			UnityEditor.EditorGUILayout.ObjectField(Value, StoredType, false);
		}
#endif
	}

	//////////////////////////////////////////////////////////////////////////

	[System.Serializable]
	public sealed class BBEntry_DamageReceivedEvent : BlackboardEntryKeyValue<DamageEvent>
	{
#if UNITY_EDITOR
		protected override void OnGUIlayout()
		{
			UnityEditor.EditorGUILayout.ObjectField(Value, StoredType, false);
		}
#endif
	}

	//////////////////////////////////////////////////////////////////////////

	public abstract class BBEntryTeamEventAbstract : BlackboardEntryKeyValue<TeamEvent>
	{
#if UNITY_EDITOR
		protected override void OnGUIlayout()
		{
			UnityEditor.EditorGUILayout.ObjectField(Value, StoredType, false);
		}
#endif
	}

	[System.Serializable]
	public sealed class BBEntry_TeamEvent_Damage_Event : BBEntryTeamEventAbstract
	{

	}

	[System.Serializable]
	public sealed class BBEntry_TeamEvent_SoundHeard_Event : BBEntryTeamEventAbstract
	{

	}

	[System.Serializable]
	public sealed class BBEntry_TeamEvent_TargetSeen_Event : BBEntryTeamEventAbstract
	{

	}

	[System.Serializable]
	public sealed class BBEntry_TeamEvent_TargetLost_Event : BBEntryTeamEventAbstract
	{

	}
}
