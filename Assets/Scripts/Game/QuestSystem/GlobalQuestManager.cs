
using UnityEngine;


namespace QuestSystem {

	using System.Collections.Generic;

	public interface IQuestManager {

		EQuestStatus		GetQuestStatus	( uint questIndex );

		bool			AddQuest		( IQuest newQuest, bool activateNow );

		int				GetQuestCount	();

		EQuestScope		GetQuestSope	( uint questIndex );

	}

	public sealed class GlobalQuestManager : MonoBehaviour, IQuestManager {

		private		static	bool				m_ShowDebugInfo					= false;
		public		static	bool				ShowDebugInfo
		{
			get { return m_ShowDebugInfo; }
		}
		
		private		static IQuestManager		m_Instance						= null;
		public		static	IQuestManager		Instance
		{
			get { return m_Instance; }
		}
		
		private	List<Quest>					m_GlobalQuests					= new List<Quest>();



		//////////////////////////////////////////////////////////////////////////
		// Awake
		private void Awake()
		{
			m_Instance = this;
			Database.Section debugInfosSection = null;
			if ( GlobalManager.Configs.GetSection( "DebugInfos", ref debugInfosSection ) )
			{
				m_ShowDebugInfo = debugInfosSection.AsBool( "Quests", false );
			}

			// Already assigned
			foreach( IQuest q in this.m_GlobalQuests )
			{
				q.Initialize(this, this.OnQuestCompleted );
			}

			if (this.m_GlobalQuests.Count > 0 )
			{
				this.m_GlobalQuests[0].Activate();
			}
		}


		//////////////////////////////////////////////////////////////////////////
		// OnQuestCompleted
		private	void	OnQuestCompleted( IQuest completedQuest )
		{
			if ( completedQuest.Scope != EQuestScope.LOCAL )
				return;

			bool bAreQuestsCompleted = true;
			foreach( IQuest q in this.m_GlobalQuests )
			{
				bAreQuestsCompleted &= q.IsCompleted;
			}

			if ( bAreQuestsCompleted )
			{
				if ( ShowDebugInfo )
					Debug.Log( "Completed All quests" );
			}
		}

		//////////////////////////////////////////////////////////////////////////
		// GetQuestStatus ( Interface )
		EQuestStatus IQuestManager.GetQuestStatus( uint questIndex )
		{
			if (this.m_GlobalQuests.Count > questIndex )
				return EQuestStatus.NONE;

			IQuest quest = this.m_GlobalQuests[ (int)questIndex ];
			return quest.Status;
		}


		//////////////////////////////////////////////////////////////////////////
		// AddQuest ( Interface )
		bool IQuestManager.AddQuest( IQuest newQuest, bool activateNow )
		{
			if ( newQuest == null )
				return false;

			if ( newQuest.Status == EQuestStatus.NONE )
				return false;

			if (this.m_GlobalQuests.Contains( newQuest as Quest ) == true )
				return false;

			this.m_GlobalQuests.Add( newQuest as Quest );
			newQuest.Initialize( this, this.OnQuestCompleted );
			if ( activateNow )
			{
				newQuest.Activate();
			}
			return true;
		}


		//////////////////////////////////////////////////////////////////////////
		// GetQuestCount ( Interface )
		int IQuestManager.GetQuestCount()
		{
			return this.m_GlobalQuests.Count;
		}


		//////////////////////////////////////////////////////////////////////////
		// GetQuestSope ( Interface )
		EQuestScope IQuestManager.GetQuestSope( uint questIndex )
		{
			if (this.m_GlobalQuests.Count > questIndex )
				return EQuestScope.NONE;

			IQuest quest = this.m_GlobalQuests[ (int)questIndex ];
			return quest.Scope;
		}
	}

}