
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI.Behaviours {

	public class Drone_AI_Beaviour_Attacker : Behaviour_Normal {

		public Drone_AI_Beaviour_Attacker()
		{
			
		}

		public override void Enable()
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


		public override void Disable()
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


		public override void OnThink()
		{
			
		}


		public override void OnFrame( float DeltaTime )
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
