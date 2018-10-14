
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract partial class Drone {

	protected partial class Drone_AI_Beaviour_Evasive {

		public		override	StreamUnit	OnSave( StreamData streamData )
		{
			return null;
		}

		public		override	StreamUnit	OnLoad( StreamData streamData )
		{
			return null;
		}

		public		override	void		OnHit( IBullet bullet )
		{
			
		}

		public		override	void		OnHit( Vector3 startPosition, Entity whoRef, float damage, bool canPenetrate = false )
		{
			
		}

		public		override	void		OnThink()
		{
			
		}

		public		override	void		OnPhysicFrame( float FixedDeltaTime )
		{
			
		}

		public		override	void		OnFrame( float DeltaTime )
		{
			
		}

		public		override	void		OnPauseSet( bool isPaused )
		{
			
		}

		public		override	void		OnTargetAcquired( TargetInfo_t targetInfo )
		{
			
		}

		public		override	void		OnTargetUpdate( TargetInfo_t targetInfo )
		{
			
		}
		
		public		override	void		OnTargetChange( TargetInfo_t targetInfo )
		{
			
		}
		
		public		override	void		OnTargetLost( TargetInfo_t targetInfo )
		{
			
		}

		public		override	void		OnDestinationReached( Vector3 Destination )
		{
			
		}

		public		override	void		OnKilled()
		{
			
		}
	}

}
