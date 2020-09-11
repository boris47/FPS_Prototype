
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;

public interface ICameraControl {

	Transform					Transform							{ get; }
	bool						Enabled								{ get; set; } 
	PostProcessingProfile		GetPP_Profile						{ get; }
	Camera						MainCamera							{ get; }
	Transform					Target								{ get; set; }
	Transform					WeaponPivot							{ get; }
	Vector3						CurrentDirection					{ get; set; }
	CameraEffectorsManager		CameraEffectorsManager				{ get; }

	void						SetViewPoint						( Transform viewPoint, Transform viewPointBody );
	void						OnCutsceneEnd						();
	void						ApplyDeviation						( float deviation, float weightX = 1f, float weightY = 1f );
	void						ApplyDispersion						( float dispersion, float weightX = 1f, float weightY = 1f );
	void						ApplyFallFeedback					( float delta, float weightX = 1f, float weightY = 1f );
	void						AddRecoil							( float recoil );
}

public sealed class CameraControl : MonoBehaviour, ICameraControl {

	private		const		float				WPN_ROTATION_CLAMP_VALUE				= 0.6f;
	private		const		float				WPN_FALL_FEEDBACK_CLAMP_VALUE			= 5f;

	public		const		float				CLAMP_MAX_X_AXIS						=  70.0f;
	public		const		float				CLAMP_MIN_X_AXIS						= -70.0f;

	public		static		ICameraControl		m_Instance								= null;
	public		static		ICameraControl		Instance
	{
		get { return m_Instance; }
	}
	

	// INTERFACE START
				Transform						ICameraControl.Transform				{ get { return this.transform; } }
				bool							ICameraControl.Enabled					{ get { return this.enabled; } set { this.enabled = value; } }
				PostProcessingProfile			ICameraControl.GetPP_Profile			{ get { return this.m_PP_Profile; } }
				Camera							ICameraControl.MainCamera				{ get { return this.m_CameraRef; } }
				Transform						ICameraControl.Target					{ get { return this.m_Target; } set { this.OnTargetSet( value ); } }
				Transform						ICameraControl.WeaponPivot				{ get { return this.m_WeaponPivot; } }
				Vector3							ICameraControl.CurrentDirection			{ get { return this.m_CurrentDirection; } set{ this.m_CurrentDirection = value; } }
				CameraEffectorsManager			ICameraControl.CameraEffectorsManager	{ get { return this.m_CameraEffectorsManager; } }
	// INTERFACE END
	
	[SerializeField, Tooltip("Camera Target"), ReadOnly]
	private		Transform						m_Target								= null;

	[SerializeField]
	private		bool							m_WeaponMoveEffectEnabled				= true;

	[SerializeField, Range( 0.2f, 20.0f )]
	private		float							m_MouseSensitivity						= 1.0f;

	[SerializeField]
	private		bool							m_SmoothedRotation						= true;

	[SerializeField, Range( 1.0f, 10.0f )]
	private		float							m_SmoothFactor							= 1.0f;
	
	[SerializeField]
	private		Transform						m_WeaponPivot							= null;

	[SerializeField]
	private		Transform						m_ViewPoint								= null;

	[SerializeField]
	private		Transform						m_ViewPointBody							= null;


	// EFFECTORS	
	[SerializeField]
	private		CameraEffectorsManager			m_CameraEffectorsManager				= new CameraEffectorsManager();

	public		CameraEffectorsManager			CameraEffectorsManager => this.m_CameraEffectorsManager;

	private		Vector3							m_CurrentDirection						= Vector3.zero;
	private		PostProcessingProfile			m_PP_Profile							= null;
	private		Camera							m_CameraRef								= null;
	private		float							m_CurrentRotation_X_Delta				= 0.0f;
	private		float							m_CurrentRotation_Y_Delta				= 0.0f;

	// WEAPON
	private		Vector3							m_WpnCurrentDeviation					= Vector3.zero;
	private		Vector3							m_WpnCurrentDispersion					= Vector3.zero;
	private		Vector3							m_WpnRotationFeedback					= Vector3.zero;
	private		Vector3							m_WpnFallFeedback						= Vector3.zero;
	private		float							m_Recoil								= 0.0f;


	[System.Serializable]
	private class CameraSectionData {
		public	float	ViewDistance					= 500f;
	}

	[SerializeField]
	private		CameraSectionData			m_CameraSectionData = new CameraSectionData();



	//////////////////////////////////////////////////////////////////////////
	// Awake
	private	void	Awake()
	{
		// Singleton
		if ( Instance != null )
		{
			print( "Camera Awake: Instance already found" );
			this.gameObject.SetActive( false );
			return;
		}
		m_Instance = this;

		this.m_WeaponPivot = this.transform.Find( "WeaponPivot" );

		// Sprites for TargetToKill, LocationToReach or ObjectToInteractWith
		ResourceManager.LoadedData<PostProcessingProfile> cameraPostProcesses = new ResourceManager.LoadedData<PostProcessingProfile>();
		bool bLoadResult = ResourceManager.LoadResourceSync
		(
			ResourcePath:			"Scriptables/CameraPostProcesses",
			loadedResource:			cameraPostProcesses
		);

		UnityEngine.Assertions.Assert.IsTrue
		(
			bLoadResult,
			"CameraControl::Awake: Failed the load of camera post processes profile"
		);

		this.m_CameraRef = this.GetComponent<Camera>();
		this.m_PP_Profile = this.gameObject.GetOrAddIfNotFound<PostProcessingBehaviour>().profile = cameraPostProcesses.Asset;

		Database.Section cameraSection = null;
		if ( !(GlobalManager.Configs.GetSection("Camera", ref cameraSection) && GlobalManager.Configs.bSectionToOuter(cameraSection, this.m_CameraSectionData )) )
		{
			Debug.Log( "UI_Indicators::Initialize:Cannot load m_CameraSectionData" );
		}
		else
		{
			EffectActiveCondition mainCondition = () => Player.Instance.IsGrounded;

			this.m_CameraEffectorsManager.Add<HeadBob>( mainCondition + delegate() { return Player.Instance.IsMoving == true; } );
			this.m_CameraEffectorsManager.Add<HeadMove>( mainCondition + delegate() { return Player.Instance.IsMoving == false; } );

			this.m_CameraRef.farClipPlane = this.m_CameraSectionData.ViewDistance;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// OnEnable
	private void OnEnable()
	{
		UnityEngine.Assertions.Assert.IsNotNull
		(
			GameManager.StreamEvents,
			"CameraControl::OnEnable : GameManager.StreamEvents is null"
		);

		GameManager.StreamEvents.OnSave += this.OnSave;
		GameManager.StreamEvents.OnLoad += this.OnLoad;

		OutlineEffectManager.SetEffectCamera(this.m_CameraRef );
	}
	

	//////////////////////////////////////////////////////////////////////////
	// OnEnable
	private void OnDisable()
	{
		if ( GameManager.StreamEvents.IsNotNull() )
		{
			GameManager.StreamEvents.OnSave -= this.OnSave;
			GameManager.StreamEvents.OnLoad -= this.OnLoad;
		}
		OutlineEffectManager.SetEffectCamera( null );
	}


	//////////////////////////////////////////////////////////////////////////
	// OnSave
	private	StreamUnit	OnSave( StreamData streamData )
	{
		StreamUnit streamUnit	= streamData.NewUnit(this.gameObject );

		// Current internal direction
		streamUnit.SetInternal( "CurrentDirection", Utils.Converters.Vector3ToString(this.m_CurrentDirection ) );

		// Can parse input
		streamUnit.SetInternal( "CanParseInput", GlobalManager.InputMgr.HasCategoryEnabled(EInputCategory.CAMERA) );
		
		// TODO load effectors

		return streamUnit;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnLoad
	private	StreamUnit	OnLoad( StreamData streamData )
	{
		StreamUnit streamUnit = null;
		if ( streamData.GetUnit(this.gameObject, ref streamUnit ) )
		{
			// Camera internals
			this.m_WpnCurrentDeviation	= Vector3.zero;
			this.m_WpnCurrentDispersion	= Vector3.zero;
			this.m_WpnRotationFeedback	= Vector3.zero;
			this.m_WpnFallFeedback		= Vector3.zero;

			// Current internal direction
			this.m_CurrentDirection		= streamUnit.GetAsVector( "CurrentDirection" );

			// Can parse input
			GlobalManager.InputMgr.SetCategory(EInputCategory.CAMERA, streamUnit.GetAsBool( "CanParseInput" ));

			// TODO Save Effectors
		}

		return streamUnit;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnTargetSet
	private	void	OnTargetSet( Transform value )
	{
		if ( value == null )
		{
			float x = this.transform.localEulerAngles.x;

			while( x > CLAMP_MAX_X_AXIS ) x -= 360f;
            while( x < CLAMP_MIN_X_AXIS ) x += 360f;

			this.m_CurrentDirection.x = x;
			this.m_CurrentDirection.y = this.m_ViewPoint ? this.m_ViewPoint.localEulerAngles.y : 0.0f;
			this.m_CurrentDirection.z = 0.0f;
		}

		if ( value && !this.m_Target && this.m_ViewPoint )
		{
			this.m_ViewPoint.up = this.m_ViewPointBody?.up ?? Vector3.up;
		}

		this.m_Target = value;
	}


	//////////////////////////////////////////////////////////////////////////
	// SetViewPoint
	void	ICameraControl.SetViewPoint( Transform viewPoint, Transform viewPointBody )
	{
		this.m_ViewPoint = viewPoint ?? this.m_ViewPoint;
		this.m_ViewPointBody = viewPointBody ?? this.m_ViewPointBody;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnCutsceneEnd
	void	ICameraControl.OnCutsceneEnd()
	{
		this.OnTargetSet( null );
		this.m_CurrentRotation_X_Delta	= 0.0f;
		this.m_CurrentRotation_Y_Delta	= 0.0f;
		this.m_WpnCurrentDeviation		= this.m_WpnCurrentDispersion = Vector3.zero;
	}


	//////////////////////////////////////////////////////////////////////////
	// ApplyDeviation
	void	ICameraControl.ApplyDeviation( float deviation, float weightX, float weightY )
	{
//		if ( Player.Instance.IsDodging )
//			return;

		this.m_WpnCurrentDeviation.x -= Random.Range( 0.001f, deviation ) * weightX;
		this.m_WpnCurrentDeviation.y += Random.Range( -deviation, deviation ) * weightY;
	}


	//////////////////////////////////////////////////////////////////////////
	// ApplyDispersion
	void	ICameraControl.ApplyDispersion( float dispersion, float weightX, float weightY )
	{
		this.m_WpnCurrentDispersion.x += Random.Range( 0.001f, dispersion ) * weightX;
		this.m_WpnCurrentDispersion.y += Random.Range( -dispersion, dispersion ) * weightY;
	}


	//////////////////////////////////////////////////////////////////////////
	// ApplyFallFeedback
	void	ICameraControl.ApplyFallFeedback( float delta, float weightX, float weightY )
	{
		this.m_WpnFallFeedback.x = delta * weightX;
		this.m_WpnFallFeedback.y = delta * weightY;
//		m_WpnFallFeedback = Vector3.ClampMagnitude( m_WpnCurrentDeviation, WPN_FALL_FEEDBACK_CLAMP_VALUE );
	}


	//////////////////////////////////////////////////////////////////////////
	// AddRecoil
	void	ICameraControl.AddRecoil( float recoil )
	{
		this.m_Recoil += recoil;
		this.m_Recoil = Mathf.Clamp(this.m_Recoil, 0.0f, 0.05f );
	}


	//////////////////////////////////////////////////////////////////////////
	// LateUpdate
	private	void	LateUpdate()
	{
		Transform viewPoint = this.m_ViewPoint ?? this.transform;
		Transform viewPointBody = this.m_ViewPointBody ?? this.transform;
		bool bHasTargetAssigned = this.m_Target;

		// ByDefault Camera follow viewPoint
		this.transform.SetPositionAndRotation( viewPoint.position, viewPoint.rotation );

		float deltaTime = GameManager.IsPaused ? 0f : Time.deltaTime;

		this.m_CameraEffectorsManager.Update( deltaTime );

		// Used for view smoothness
		this.m_SmoothFactor = Mathf.Clamp(this.m_SmoothFactor, 1.0f, 10.0f );

		// Weapon movements
		{
			this.m_WpnCurrentDispersion	= Vector3.Lerp(this.m_WpnCurrentDispersion, Vector3.zero, deltaTime * 3.7f );
			this.m_WpnRotationFeedback	= Vector3.Lerp(this.m_WpnRotationFeedback,	Vector3.zero, deltaTime );
			this.m_WpnFallFeedback		= Vector3.Lerp(this.m_WpnFallFeedback,		Vector3.zero, deltaTime * 3.7f );
			this.m_WpnCurrentDeviation	= Vector3.Lerp(this.m_WpnCurrentDeviation,  Vector3.zero, deltaTime * 3.7f );
			this.m_Recoil				= Mathf.Lerp(this.m_Recoil, 0.0f, deltaTime * 3.7f );

			if (this.m_WeaponMoveEffectEnabled )
			{
				// Weapon Local Position
				Vector3 localPosition = this.m_CameraEffectorsManager.CameraEffectorsData.WeaponPositionDelta + ( Vector3.left * this.m_Recoil );
				WeaponManager.Instance.CurrentWeapon.Transform.localPosition = localPosition;

				// Headbob effect on weapon
				Vector3 headbobEffectOnWeapon = (this.m_CameraEffectorsManager.GetEffectorData<HeadBob>()?.CameraEffectsDirection * -1f ) ?? Vector3.zero;

				// Weapon Local Rotation
				Vector3 localEulerAngles = this.m_CameraEffectorsManager.CameraEffectorsData.WeaponRotationDelta + this.m_WpnCurrentDispersion + this.m_WpnRotationFeedback + this.m_WpnFallFeedback + headbobEffectOnWeapon;
				WeaponManager.Instance.CurrentWeapon.Transform.localEulerAngles	= localEulerAngles;
			
				// Optic sight alignment
				if ( WeaponManager.Instance.IsZoomed )
				{
					Vector2 delta = Vector2.zero;
					{
						Vector3 wpnDir = WeaponManager.Instance.CurrentWeapon.Transform.right;
						Vector3 wpnPos = WeaponManager.Instance.CurrentWeapon.Transform.position;
						Vector3 distantPoint = wpnPos + ( wpnDir * 1000f );
						Vector3 projectedPoint = this.m_CameraRef.WorldToScreenPoint( distantPoint );

						delta =  projectedPoint;
					}

					float frameFeedBack =  1.0f + ( this.m_Recoil * 10.0f ); // 1.0f + Because is scale factor
					UIManager.InGame.FrameFeedBack( frameFeedBack, delta ); 
				}
			}
		}

		// Look at target if assigned ( override previous assignment )
		if ( bHasTargetAssigned )
		{
			// Body Rotation
			if (this.m_ViewPointBody )
			{
				Vector3 projectedPoint = Utils.Math.ProjectPointOnPlane( viewPointBody.up, viewPointBody.position, this.m_Target.position );
				viewPointBody.LookAt( projectedPoint, viewPointBody.up );
			}
			
//			viewPointBody.rotation = Quaternion.LookRotation( projectedPoint - viewPointBody.position, viewPointBody.up );

			// Camera Rotation
//			viewPoint.LookAt( m_Target, viewPointBody.up );

			Vector3 direction = this.m_Target.position - viewPoint.position;
			Vector3 DirectionPlusEffects = direction + this.m_CameraEffectorsManager.CameraEffectorsData.CameraEffectsDirection + this.m_WpnCurrentDeviation;
//			viewPoint.rotation = Quaternion.Euler( DirectionPlusEffects.x, 0f, 0f );
			return;
		}


		// Return if input is disabled
		if ( GlobalManager.InputMgr.HasCategoryEnabled( EInputCategory.CAMERA ) == false )
			return;


		bool	isZoomed			= WeaponManager.Instance.IsZoomed;
		float	wpnZoomSensitivity  = WeaponManager.Instance.ZoomSensitivity;
		float	Axis_X_Delta		= Input.GetAxisRaw ( "Mouse X" ) * this.m_MouseSensitivity * ( ( isZoomed ) ? wpnZoomSensitivity : 1.0f );
		float	Axis_Y_Delta		= Input.GetAxisRaw ( "Mouse Y" ) * this.m_MouseSensitivity * ( ( isZoomed ) ? wpnZoomSensitivity : 1.0f );

		// Interpolate if m_SmoothedRotation is enabled
		float interpolant = this.m_SmoothedRotation ? ( Time.unscaledDeltaTime * ( 100f / this.m_SmoothFactor ) ) : 1f;
		this.m_CurrentRotation_X_Delta = Mathf.LerpUnclamped(this.m_CurrentRotation_X_Delta, Axis_X_Delta, interpolant );
		this.m_CurrentRotation_Y_Delta = Mathf.LerpUnclamped(this.m_CurrentRotation_Y_Delta, Axis_Y_Delta, interpolant );

		this.m_CurrentDirection.x = Utils.Math.Clamp(this.m_CurrentDirection.x - this.m_CurrentRotation_X_Delta, CLAMP_MIN_X_AXIS, CLAMP_MAX_X_AXIS );
		this.m_CurrentDirection.y = this.m_CurrentDirection.y + this.m_CurrentRotation_Y_Delta;

		// Apply rotation
		{
			// Apply effects
			Vector3 DirectionPlusEffects = (this.m_CurrentDirection + this.m_CameraEffectorsManager.CameraEffectorsData.CameraEffectsDirection + this.m_WpnCurrentDeviation );

			// Horizonatal rotation
			this.m_ViewPointBody.localRotation = Quaternion.Euler( Vector3.up * DirectionPlusEffects.y );

			// Vertical rotation
			viewPoint.localRotation = Quaternion.Euler( Vector3.right * DirectionPlusEffects.x );
		}

		// rotation with effect added
		{
			Vector3 finalWpnRotationFeedback = new Vector3( Axis_Y_Delta, Axis_X_Delta );
			this.m_WpnRotationFeedback = Vector3.ClampMagnitude( Vector3.Lerp(this.m_WpnRotationFeedback, new Vector3( Axis_Y_Delta, Axis_X_Delta ), deltaTime * 8f ), WPN_ROTATION_CLAMP_VALUE );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// OnDestroy
	private void OnDestroy()
	{
		if ( (Object)m_Instance != this )
			return;

//		var settings = m_PP_Profile.vignette.settings;
//		settings.intensity = 0f;
//		m_PP_Profile.vignette.settings = settings;

		m_Instance = null;
	}

}




public	delegate	bool	EffectActiveCondition();

[System.Serializable]
public class CameraEffectorsManager {

	[System.Serializable]
	public struct CameraEffectorData {

		/// <summary> </summary>
		public Vector3		CameraEffectsDirection;

		/// <summary> </summary>
		public Vector3		WeaponPositionDelta;

		/// <summary> </summary>
		public Vector3		WeaponRotationDelta;


		public void Reset()
		{
			this.CameraEffectsDirection.Set( 0f, 0f, 0f );
			this.WeaponPositionDelta.Set( 0f, 0f, 0f );
			this.WeaponRotationDelta.Set( 0f, 0f, 0f );
		}
	}

	[SerializeField]
	protected CameraEffectorData m_CameraEffectorData = new CameraEffectorData();
	public CameraEffectorData CameraEffectorsData  => this.m_CameraEffectorData;

	[SerializeField]
	protected List<CameraEffectBase> m_Effects = new List<CameraEffectBase>();


	//////////////////////////////////////////////////////////////////////////
	public void Add<T>( EffectActiveCondition condition ) where T : CameraEffectBase, new()
	{
		if (this.m_Effects.Exists( e => e is T ) )
			return;

		T newEffect = new T();

		newEffect.Setup( condition );

		this.m_Effects.Add( newEffect );
	}


	//////////////////////////////////////////////////////////////////////////
	public CameraEffectorData? GetEffectorData<T>() where T: CameraEffectBase
	{
		CameraEffectBase effector = this.m_Effects.Find( e => e is T );
		if ( effector != null )
		{
			CameraEffectorData data = default;
			effector.SetData( ref data );
			return data;
		}

		return null;
	}


	//////////////////////////////////////////////////////////////////////////
	public void SetEffectorState<T>( bool newState ) where T: CameraEffectBase
	{
		CameraEffectBase effector = this.m_Effects.Find( e => e is T );
		if ( effector != null )
		{
			effector.IsActive= newState;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public void SetAmplitudeMultiplier<T>( float newAmplitudeMultiplier ) where T : CameraEffectBase
	{
		CameraEffectBase effector = this.m_Effects.Find( e => e is T );
		if ( effector != null )
		{
			effector.AmplitudeMult = newAmplitudeMultiplier;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public void SetSpeedMultiplier<T>( float newSpeedMultiplier ) where T : CameraEffectBase
	{
		CameraEffectBase effector = this.m_Effects.Find( e => e is T );
		if ( effector != null )
		{
			effector.SpeedMul = newSpeedMultiplier;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public void	Update( float deltaTime )
	{
		this.m_CameraEffectorData.Reset();
		this.m_Effects.ForEach( e => e.Update( deltaTime, ref this.m_CameraEffectorData ) );
	}


	//////////////////////////////////////////////////////////////////////////
	public void Remove<T>() where T : CameraEffectBase
	{
		this.m_Effects.RemoveAll( e => e is T );
	}


	//////////////////////////////////////////////////////////////////////////
	public void Reset()
	{
		this.m_Effects.Clear();

		this.m_CameraEffectorData = new CameraEffectorData();
	}
}
