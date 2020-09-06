
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum EIndicatorType {

	AREA_WHERE_PLACE_OBJECT,
	AREA_TO_REACH,
	OBJECT_TO_INTERACT,
	OBJECT_TO_FOLLOW,
	TARGET_TO_KILL
}

public sealed class UI_Indicators : MonoBehaviour, IStateDefiner {

	private	const	float		MAX_DISTANCE_TO_RESIZE = 25f;
	private	const	float		MIN_DISTANCE_TO_RESIZE = 2f;
	private	const	int			MAX_ELEMENTS = 4;

	private		SpriteCollection					m_SpriteCollection			= null;

	private		GameObjectsPool<Transform>			m_Pool						= null;

	// STORE DATA
	private	struct ActiveIndicatorData {
		public	GameObject		Target;
		public	Image			MainIndicatorImage;
		public	Image			MinimapIndicatorImage;
		public	bool			bMustBeClamped;
	};
	private		List<ActiveIndicatorData>			m_CurrentlyActive			= new List<ActiveIndicatorData>(MAX_ELEMENTS);

	// REQUESTS
	[System.Serializable]
	private	struct IndicatorRequest {
		public	GameObject			target;
		public	EIndicatorType		IndicatorType;
		public	bool				bMustBeClamped;
	}
	[SerializeField]
	private	Queue<IndicatorRequest>					m_Requests					= new Queue<IndicatorRequest>();

	// SECTION DATA
	[System.Serializable]
	private class UI_IndicatorsSectionData {
		public	float	InScreenMarkerFactor		= 0.8f;

		public	float	MinimapClampFactor			= 0.72f;
	}
	[SerializeField]
	private		UI_IndicatorsSectionData			m_IndicatorsSectionData = new UI_IndicatorsSectionData();

	// INITIALIZATION
	private		bool								m_IsInitialized			= false;
	bool IStateDefiner.IsInitialized
	{
		get { return this.m_IsInitialized; }
	}

	string IStateDefiner.StateName
	{
		get { return this.name; }
	}




	//////////////////////////////////////////////////////////////////////////
	// Initialize
	IEnumerator IStateDefiner.Initialize()
	{
		if (this.m_IsInitialized )
			yield break;

		CoroutinesManager.AddCoroutineToPendingCount( 1 );

		bool resourcesLoaded = true;

		// Section Data
		UnityEngine.Assertions.Assert.IsTrue
		(
			GlobalManager.Configs.GetSection( "UI_Indicators", this.m_IndicatorsSectionData ),
			"UI_Indicators::Initialize:Cannot load UI_IndicatorsSectionData"
		);

		// Sprites for TargetToKill, LocationToReach or ObjectToInteractWith
		ResourceManager.LoadedData<SpriteCollection> indicatorsSpritesCollection = new ResourceManager.LoadedData<SpriteCollection>();
		yield return ResourceManager.LoadResourceAsyncCoroutine
		(
			ResourcePath:			"Scriptables/UI_Indicators",
			loadedResource:			indicatorsSpritesCollection,
			OnResourceLoaded :		(a) => { resourcesLoaded &= true; this.m_SpriteCollection = a; },
			OnFailure:				(p) => resourcesLoaded &= false
		);

		// A prefab where the sprites will be set
		ResourceManager.LoadedData<GameObject> indicatorPrefab = new ResourceManager.LoadedData<GameObject>();
		yield return ResourceManager.LoadResourceAsyncCoroutine
		(
			ResourcePath:			"Prefabs/UI/Task_Objective",
			loadedResource:			indicatorPrefab,
			OnResourceLoaded :		(a) => { resourcesLoaded &= true; },
			OnFailure:				(p) => resourcesLoaded &= false
		);

		if ( resourcesLoaded )
		{
			// Pool Creation
			GameObjectsPoolConstructorData<Transform> data = new GameObjectsPoolConstructorData<Transform>()
			{
				Model			= indicatorPrefab.Asset,
				Size			= MAX_ELEMENTS,
				ContainerName	= "UI_IndicatorsPool",
				ActionOnObject	= delegate( Transform t ) { t.gameObject.SetActive(false); t.SetParent(this.transform); },
				IsAsyncBuild	= true
			};
			this.m_Pool = new GameObjectsPool<Transform>( data );

			yield return data.CoroutineEnumerator;

			CoroutinesManager.RemoveCoroutineFromPendingCount( 1 );
			this.m_IsInitialized = true;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// ReInit
	IEnumerator IStateDefiner.ReInit()
	{
		yield return null;
	}


	//////////////////////////////////////////////////////////////////////////
	// Finalize
	bool IStateDefiner.Finalize()
	{
		return true;
	}


	//////////////////////////////////////////////////////////////////////////
	// ShowLabel
	private void OnEnable()
	{
		UnityEngine.Assertions.Assert.IsNotNull
		(
			GameManager.UpdateEvents,
			"UI_Indicators::OnEnable : GameManager.UpdateEvents is null"
		);

		GameManager.UpdateEvents.OnThink += this.UpdateRequestQueue;
	}


	//////////////////////////////////////////////////////////////////////////
	// ShowLabel
	private void OnDisable()
	{
		if ( GameManager.UpdateEvents.IsNotNull() )
		{
			GameManager.UpdateEvents.OnThink -= this.UpdateRequestQueue;
		}
	}

	
	//////////////////////////////////////////////////////////////////////////
	// EnableIndicator
	private	bool	EnableIndicatorInternal( IndicatorRequest request )
	{
		int index = this.m_CurrentlyActive.FindIndex( ( ActiveIndicatorData p ) => { return p.Target == request.target; } );
		if ( index > -1 )
		{
			return false;
		}

		this.InternalCheck();

		Sprite indicator = this.m_SpriteCollection.Sprites[(int)request.IndicatorType];

		Transform indicatorTransform = this.m_Pool.GetNextComponent();

		Image mainIndicatorImage = null;
		Image MinimapIndicatorImage = null;
		indicatorTransform.SearchComponentInChild( "MainIndicator",		ref mainIndicatorImage );
		indicatorTransform.SearchComponentInChild( "MinimapIndicator",	ref MinimapIndicatorImage );

		MinimapIndicatorImage.sprite = mainIndicatorImage.sprite = indicator;

		ActiveIndicatorData pair = new ActiveIndicatorData()
		{
			Target = request.target,
			MainIndicatorImage = mainIndicatorImage,
			MinimapIndicatorImage = MinimapIndicatorImage,
			bMustBeClamped = request.bMustBeClamped
		};

		this.m_CurrentlyActive.Add( pair );

		indicatorTransform.gameObject.SetActive( true );
		return true;
	}

	//////////////////////////////////////////////////////////////////////////
	// EnableIndicator
	public	void	EnableIndicator( GameObject target, EIndicatorType IndicatorType, bool bMustBeClamped )
	{
		IndicatorRequest indicatorRequest = new IndicatorRequest()
		{
			target = target,
			IndicatorType  = IndicatorType,
			bMustBeClamped = bMustBeClamped
		};
		this.m_Requests.Enqueue( indicatorRequest );
	}


	//////////////////////////////////////////////////////////////////////////
	// DisableIndicator
	public	bool	DisableIndicator( GameObject target )
	{
		int index = this.m_CurrentlyActive.FindIndex( ( ActiveIndicatorData p ) => { return p.Target == target; } );
		bool bIsFound = index > -1;
		if ( bIsFound )
		{
			this.m_CurrentlyActive[index].MainIndicatorImage.gameObject.SetActive( false );
			this.m_CurrentlyActive[index].MinimapIndicatorImage.gameObject.SetActive( false );
			this.m_CurrentlyActive.RemoveAt( index );
		}

		this.InternalCheck();
		return bIsFound;
	}


	//////////////////////////////////////////////////////////////////////////
	// InternalCheck
	/// <summary> Check whetever some data has been invalidated </summary>
	private	void InternalCheck()
	{
		for ( int i = this.m_CurrentlyActive.Count - 1; i >= 0; i-- )
		{
			ActiveIndicatorData p = this.m_CurrentlyActive[i];
			if ( p.Target == null )
			{
				this.m_CurrentlyActive[i].MainIndicatorImage.gameObject.SetActive( false );
				this.m_CurrentlyActive[i].MinimapIndicatorImage.gameObject.SetActive( false );
				this.m_CurrentlyActive.RemoveAt( i );
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// UpdateNotifications
	private	void	UpdateRequestQueue()
	{
		if (this.m_IsInitialized == false )
			return;

		if (this.m_Requests.Count > 0 )
		{
			IndicatorRequest request = this.m_Requests.Dequeue();
			this.EnableIndicatorInternal( request );
		}
	}

	//////////////////////////////////////////////////////////////////////////
	// FixedUpdate
	private void LateUpdate()
	{
		if (this.m_IsInitialized == false )
			return;

		for ( int i = this.m_CurrentlyActive.Count - 1; i >= 0; i-- )
		{
			ActiveIndicatorData indicatorData = this.m_CurrentlyActive[i];
			GameObject target			= indicatorData.Target;
			Image mainIndicatorImage	= indicatorData.MainIndicatorImage;
			Image minimapIndicatorImage = indicatorData.MinimapIndicatorImage;
			bool bMustBeClamped			= indicatorData.bMustBeClamped;

			if ( target ) // The gameobject could been destroyed in meanwhile 
			{
				this.DrawUIElementObjectivesOnScreen( target.transform, mainIndicatorImage.transform );

				if ( UIManager.Minimap.IsVisible() )
				{
					this.DrawUIElementObjectivesOnMinimap( target.transform, minimapIndicatorImage.transform, bMustBeClamped );
				}
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// DrawUIElementOnObjectives
	private	void	DrawUIElementObjectivesOnScreen( Transform targetTransform, Transform m_IconTransform )
	{
		Camera camera = CameraControl.Instance.MainCamera;

		// Icon Scale Factor
		float scaleFactor = 1.0f;
		float distance = Vector3.Distance( camera.transform.position,targetTransform.position );
		if ( distance > MIN_DISTANCE_TO_RESIZE )
		{
			float interpolant = 1.0f - Utils.Math.ScaleBetweenClamped01( distance, MIN_DISTANCE_TO_RESIZE, MAX_DISTANCE_TO_RESIZE );
			scaleFactor = Mathf.Clamp( interpolant, 0.5f, 1.0f );
			m_IconTransform.localScale = Vector3.one * scaleFactor;
		}


	//  Ref: https://www.youtube.com/watch?v=gAQpR1GN0Os
	//  Viewport space is normalized and relative to the camera. The bottom-left of the camera is (0,0); the top-right is (1,1). The z position is in world units from the camera.
	//  Screenspace is defined in pixels. The bottom-left of the screen is (0,0); the right-top is (pixelWidth,pixelHeight). The z position is in world units from the camera.
	//	Vector3 ViewportPoint = camera.WorldToViewportPoint( taretTransform.position );
		Vector3 ScreenPoint = camera.WorldToScreenPoint( targetTransform.position );	
		Vector3 DrawPosition = Vector3.zero;


		float scaledWidth  = (float)Screen.width  * this.m_IndicatorsSectionData.InScreenMarkerFactor;
		float scaledHeight = (float)Screen.height * this.m_IndicatorsSectionData.InScreenMarkerFactor;

		// Normal projection because inside screen
		if ( ScreenPoint.z > 0f 
			&& ScreenPoint.x > ( (float)Screen.width  - scaledWidth  ) && ScreenPoint.x < scaledWidth
			&& ScreenPoint.y > ( (float)Screen.height - scaledHeight ) && ScreenPoint.y < scaledHeight
		)
		{
			DrawPosition.x = ScreenPoint.x; // ScreenPoint.x - texture.width*0.5f;
			DrawPosition.y = ScreenPoint.y; // Screen.height - ( ScreenPoint.y + texture.height*0.5f );
		}
		else // Off screen
		{
			bool bIsBehind = ScreenPoint.z < 0.0f;
			if ( bIsBehind )
			{
				ScreenPoint *= -1.0f;
			}

			Vector2 ScreenPoint2D = ScreenPoint;
			Vector2 screenCenter2D = new Vector2( Screen.width, Screen.height ) * 0.5f;

			// NOTE COORDINATE TRASLATED
			// make 0, 0 the center of screen inteead of bottom left
			ScreenPoint2D -= screenCenter2D;

			// Find angle from center of screen to mouse position
			float angle = Mathf.Atan2( ScreenPoint2D.y, ScreenPoint2D.x ) - 90f * Mathf.Deg2Rad;
			float cos = Mathf.Cos( angle );
			float sin = -Mathf.Sin( angle );

			const float amplify = 150f;
			ScreenPoint2D.Set( screenCenter2D.x + sin * amplify, screenCenter2D.y + cos * amplify );

			// y = mx + b format
			float m = cos / sin;

			float rectBorderFactor = this.m_IndicatorsSectionData.InScreenMarkerFactor;
			Vector2 screenBounds = screenCenter2D * rectBorderFactor;

			// Check up and down first
			if ( cos > 0.0f )
			{
				ScreenPoint2D.Set( screenBounds.y / m, screenBounds.y );
			}
			else // down
			{
				ScreenPoint2D.Set( -screenBounds.y / m, -screenBounds.y );
			}

			// If out of bounds, get point on appropriate side
			if ( ScreenPoint2D.x > screenBounds.x )			// out of bounds right
			{
				ScreenPoint2D.Set( screenBounds.x, screenBounds.x * m );
			}
			else if ( ScreenPoint2D.x < -screenBounds.x )	// out of bounds left
			{
				ScreenPoint2D.Set( -screenBounds.x, -screenBounds.x * m );
			}

			// Remove cooridnate traslation
			ScreenPoint2D += screenCenter2D;

			DrawPosition.Set( ScreenPoint2D.x, ScreenPoint2D.y, 0.0f );

//			DrawPosition.x = ScreenPoint2D.x; // ScreenPoint2D.x - texture.width*0.5f;
//			DrawPosition.y = ScreenPoint2D.y; // Screen.height - ( ScreenPoint2D.y + texture.height*0.5f );
		}

		m_IconTransform.position = DrawPosition;
	}



	//////////////////////////////////////////////////////////////////////////
	// DrawUIElementOnObjectivesOnMinimap
	private	void	DrawUIElementObjectivesOnMinimap( Transform targetTransform, Transform m_IconTransform, bool bMustBeClamped )
	{
		RectTransform minimapRectTransform		= UIManager.Minimap.GetRawImageRect();

		if ( m_IconTransform.gameObject.activeSelf == false )
		{
			m_IconTransform.gameObject.SetActive( true );
		}

		//
		bool bIsInside = UIManager.Minimap.GetPositionOnUI(targetTransform.position, out Vector2 screenPointInWorldSpace);

		// If is no more inside minimap image rect and IS NOT required to be clamped the object will be deactivated
		if ( bIsInside == false && bMustBeClamped == false && m_IconTransform.gameObject.activeSelf == true )
		{
			m_IconTransform.gameObject.SetActive( false );
		}

		// If is no more inside minimap image rect and IS required to be clamped the object will be drawn clamped inside minimap rect
		if ( bIsInside == false && bMustBeClamped == true )
		{
			Vector2 screenPointInLocalSpace = UIManager.Minimap.transform.InverseTransformPoint( screenPointInWorldSpace );

			// Find angle from center of screen to mouse position
			float angle = Mathf.Atan2( screenPointInLocalSpace.y, screenPointInLocalSpace.x ) - 90f * Mathf.Deg2Rad;
			float cos = Mathf.Cos( angle );
			float sin = -Mathf.Sin( angle );

			float amplify = 150f;
			screenPointInLocalSpace.Set( screenPointInLocalSpace.x + sin * amplify, screenPointInLocalSpace.y + cos * amplify );

			// y = mx + b format
			float m = cos / sin;

			float rectBorderFactor = this.m_IndicatorsSectionData.MinimapClampFactor;
			Vector2 minimapBounds = minimapRectTransform.rect.size * rectBorderFactor;

			// Check up and down first
			if ( cos > 0.0f )
			{
				screenPointInLocalSpace.Set( minimapBounds.y / m, minimapBounds.y );
			}
			else // down
			{
				screenPointInLocalSpace.Set( -minimapBounds.y / m, -minimapBounds.y );
			}

			// If out of bounds, get point on appropriate side
			if ( screenPointInLocalSpace.x > minimapBounds.x ) // out of bounds, must be on right
			{
				screenPointInLocalSpace.Set( minimapBounds.x, minimapBounds.x * m );
			}
			else if ( screenPointInLocalSpace.x < -minimapBounds.x ) // out of bounds left
			{
				screenPointInLocalSpace.Set( -minimapBounds.x, -minimapBounds.x * m );
			}

			// Because of the different scale applied to rawImage rectTransform, this must be token in account fo computations
			Vector3 scaleToApply = minimapRectTransform.localScale * ( rectBorderFactor * 0.5f );
			screenPointInLocalSpace.Scale( scaleToApply );

			screenPointInWorldSpace = UIManager.Minimap.transform.TransformPoint( screenPointInLocalSpace );
		}

		m_IconTransform.position = screenPointInWorldSpace;
	}
	
}

