
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI.Behaviours {

	public class Drone_AI_Normal_Beaviour : Behaviour_Normal {


		protected override void OnEnable()
		{
			// Events registration
			GameManager.UpdateEvents.OnFrame		+= OnFrame;

			GameManager.StreamEvents.OnSave			+= OnSave;
			GameManager.StreamEvents.OnLoad			+= OnLoad;


			m_Brain.FieldOfView.OnTargetAquired		= OnTargetAquired;
			m_Brain.FieldOfView.OnTargetChanged		= OnTargetChanged;
			m_Brain.FieldOfView.OnTargetUpdate		= OnTargetUpdate;
			m_Brain.FieldOfView.OnTargetLost		= OnTargetLost;
		}

		protected override void OnDisable()
		{
			// Events un-registration
			GameManager.UpdateEvents.OnFrame		-= OnFrame;

			GameManager.StreamEvents.OnSave			-= OnSave;
			GameManager.StreamEvents.OnLoad			-= OnLoad;

			m_Brain.FieldOfView.OnTargetAquired		= null;
			m_Brain.FieldOfView.OnTargetChanged		= null;
			m_Brain.FieldOfView.OnTargetUpdate		= null;
			m_Brain.FieldOfView.OnTargetLost		= null;
		}

		private void OnFrame(float DeltaTime)
		{
			
		}

		public override void OnThink()
		{
			
		}

		private		StreamUnit	OnSave( StreamData streamData )
		{
			return null;
		}

		private		StreamUnit	OnLoad( StreamData streamData )
		{
			return null;
		}

		//////////////////////////////////////////////////////////////////////////
		public			void		OnTargetAquired( TargetInfo_t targetInfo )
		{

		}


		//////////////////////////////////////////////////////////////////////////
		public			void		OnTargetUpdate( TargetInfo_t targetInfo )
		{

		}
	

		//////////////////////////////////////////////////////////////////////////
		public			void		OnTargetChanged( TargetInfo_t targetInfo )
		{

		}

	
		//////////////////////////////////////////////////////////////////////////
		public			void		OnTargetLost( TargetInfo_t targetInfo )
		{

		}

	}

}
