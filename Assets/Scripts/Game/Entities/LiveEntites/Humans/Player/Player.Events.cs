
using UnityEngine;

public partial class Player
{
	private	const	float			MAX_INTERACTION_DISTANCE		= 40.1f; // TODO set to 2.1

	// The ammount of hit effect to show

	[Header("Player Events")]
	[SerializeField]
	private		float				m_DamageEffect					= 0f;


	//////////////////////////////////////////////////////////////////////////
	protected	override	bool	OnSave( StreamData streamData, ref StreamUnit streamUnit )
	{
		bool bResult = base.OnSave( streamData, ref streamUnit );
		if (bResult)
		{
			// Health
			streamUnit.SetInternal( "Health", m_Health );

			// Stamina
		//	streamUnit.SetInternal( "Stamina", m_Stamina );

			// Crouch state
		//	streamUnit.SetInternal( "IsCrouched", IsCrouched );

			// Motion Type
		//	streamUnit.SetInternal( "MotionType", m_CurrentMotionType );
		}
		return bResult;
	}


	//////////////////////////////////////////////////////////////////////////
	protected	override	bool	OnLoad( StreamData streamData, ref StreamUnit streamUnit )
	{
		bool bResult = base.OnLoad(streamData, ref streamUnit);
		if (bResult)
		{
			// Cutscene Manager
			if (m_CutsceneManager.IsPlaying)
			{
				m_CutsceneManager.Terminate();
			}



			// UI effect reset
			UIManager.EffectFrame.color = Color.clear;

			m_RigidBody.constraints						= RigidbodyConstraints.FreezeRotation;
			m_RigidBody.velocity						= Vector3.zero;

			// Health
			m_Health			= streamUnit.GetAsFloat("Health");

			// Stamina
		//	m_Stamina			= streamUnit.GetAsFloat("Stamina");

			// Crouch state
		//	Motion.States.IsCrouched = streamUnit.GetAsBool("IsCrouched");

			// TODO Load motion data ?

			m_RigidBody.useGravity = false;

		}
		return bResult;
	}

	/*
	//////////////////////////////////////////////////////////////////////////
	protected	override	void		OnTargetAquired( TargetInfo targetInfo )
	{
		base.OnTargetAquired(targetInfo);
	}


	//////////////////////////////////////////////////////////////////////////
	protected	override	void		OnTargetChanged( TargetInfo targetInfo )
	{
		base.OnTargetChanged(targetInfo);
	}


	//////////////////////////////////////////////////////////////////////////
	protected	override	void		OnTargetLost( TargetInfo targetInfo )
	{
		base.OnTargetLost(targetInfo);
	}
	*/

	//////////////////////////////////////////////////////////////////////////
	public		override	void		OnHittedDetails( Vector3 startPosition, Entity whoRef, EDamageType damageType, float damage, bool canPenetrate = false )
	{
		m_DamageEffect = 0.2f; // damage / m_Health;

		base.OnHittedDetails(startPosition, whoRef, damageType, damage, canPenetrate);

		UIManager.InGame.UpdateUI();
	}

	/*
	//////////////////////////////////////////////////////////////////////////
	protected	override	void		OnPhysicFrame(float fixedDeltaTime)
	{
		base.OnPhysicFrame(fixedDeltaTime);

		if (m_IsActive)
		{
//			CheckIfUnderSomething();
		//	CheckForFallOrUserBreak();
		}
	}
	*/

	/*
	// Pick eventual collision info from camera to up
	private					void		CheckIfUnderSomething()
	{
		Vector3 position = m_HeadTransform.position;
		Vector3 upwards = m_BodyTransform.up;
		Vector3 cameraUpPosition = position + (upwards * 0.3f);
		m_IsUnderSomething = Physics.Linecast(position, cameraUpPosition, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
	}
	*/


	//////////////////////////////////////////////////////////////////////////
	private	void		OnFrame(float deltaTime)
	{
		// Damage Effect
		if (m_DamageEffect > 0.0f)
		{
			var settings = FPSEntityCamera.Instance.PP_Profile.vignette.settings;
			m_DamageEffect = Mathf.Lerp(m_DamageEffect, 0f, Time.deltaTime * 2f);
			settings.intensity = m_DamageEffect;
			FPSEntityCamera.Instance.PP_Profile.vignette.settings = settings;
		}
		/*
		// Interactions
		{
			Vector3 position  = FPSEntityCamera.Instance.transform.position;
			Vector3 direction = FPSEntityCamera.Instance.transform.forward;

			m_Interactable = default;

			if (m_HasRaycasthit = Physics.Raycast(position, direction, out m_RaycastHit, MAX_INTERACTION_DISTANCE, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
			{
				Utils.Base.TrySearchComponent(m_RaycastHit.transform.gameObject, ESearchContext.LOCAL, out m_Interactable);
			}
		}
		*/
		#region TO IMPLEMENT (Water)
		////////////////////////////////////////////////////////////////////////////////////////
		// Water
		/*		bool bIsEntityInWater, bIsCameraUnderWater, bIsCameraReallyUnderWater;
				if ( !IsClimbing() && World()->GetWorld()->GetWaterMode() ) {

					float fWaterHeight		 = World()->GetWorld()->GetWaterHeight();
					// camera is under water level
					bIsCameraUnderWater = ( vCamPos.y - 0.1f ) < fWaterHeight;
					bIsCameraReallyUnderWater = vCamPos.y < fWaterHeight;
					// entity is under water level, but camera is over water level
					bIsEntityInWater = pEntity->GetPosition().y-0.1 < fWaterHeight && !bIsCameraUnderWater;

					SetInWater( bIsEntityInWater );

					// If now camera is over water level, but prev update was under it
					if ( bIsEntityInWater ) {

						// if distance beetwen ground and parent is minus than camera height
						if ( GetAirbourneHeigth() < CamManager()->GetStdHeight() ) {
							// restore walking state
						//	if ( iMotionType != LIVE_ENTITY::Motion::Walk ) {
								SetMotionType( LIVE_ENTITY::Motion::Walk );
							//	SetCrouched( true );
						//	}
						}

					}

					// If camera go under water level enable underwater state
					if ( bIsCameraUnderWater && iMotionType != LIVE_ENTITY::Motion::Swim ) {
						SetSwimming();
					}

					// if actual motion is 'Swim' but is not entity and camera underwater restore 'walk' motion
					if ( iMotionType == LIVE_ENTITY::Motion::Swim && !bIsEntityInWater && !bIsCameraUnderWater )
						SetMotionType( LIVE_ENTITY::Motion::Walk );

					if ( bIsCameraReallyUnderWater ) {
						SetUnderWater( true );

						// Underwater stamina is consumed as oxygen
						fStamina -= fRunStamina * 2.0f;
					}

				}
		*/
		#endregion
	}


	//////////////////////////////////////////////////////////////////////////
	public override void LookAt(in Vector3 worldpoint, in float bodyRotationSpeed, in float headRotationSpeed, in Vector2? clampsHoriz, in Vector2? clampsVert, out bool isBodyAlligned, out bool isHeadAlligned)
	{
		throw new System.NotSupportedException();
	}


	//////////////////////////////////////////////////////////////////////////
	protected		override	void		OnKill()
	{
		base.OnKill();

		// disable weapon actions
		WeaponManager.Instance.CurrentWeapon.Enabled = false;
		WeaponManager.Instance.Enabled = false;
		
		// Disable camera updates
		FPSEntityCamera.Instance.enabled = false;

		// Update UI elements
		UIManager.InGame.UpdateUI();

		// Turn off player object
		gameObject.SetActive(false);

		// print a message
		print( "U r dead" );
	}
}