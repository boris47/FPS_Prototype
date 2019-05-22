
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum IndicatorType {

	AREA_WHERE_PLACE_OBJECT,
	AREA_TO_REACH,
	OBJECT_TO_INTERACT,
	OBJECT_TO_FOLLOW,
	TARGET_TO_KILL
}

public class UI_Indicators : MonoBehaviour, IStateDefiner {

	private	const	float		MAX_DISTANCE_TO_RESIZE = 25f;
	private	const	float		MIN_DISTANCE_TO_RESIZE = 2f;
	private	const	int			MAX_ELEMENTS = 30;

	private		SpriteCollection					m_SpriteCollection			= null;

	private		GameObjectsPool<Transform>			m_Pool						= null;

	private	struct ActiveIndicatorData {
		public	GameObject	Target;
		public	Image		MainIndicatorImage;
		public	Image		MinimapIndicatorImage;
	};

	private		List<ActiveIndicatorData>				m_CurrentlyActive			= new List<ActiveIndicatorData>(MAX_ELEMENTS);

	private		bool								m_bIsInitialized			= false;
	bool IStateDefiner.IsInitialized
	{
		get { return m_bIsInitialized; }
	}


	//////////////////////////////////////////////////////////////////////////
	// Initialize
	bool IStateDefiner.Initialize()
	{
		// Sprites for TargetToKill, LocationToReach or ObjectToInteractWith
		ResourceManager.LoadData<SpriteCollection> indicatorsSpritesCollection = new ResourceManager.LoadData<SpriteCollection>();

		// A prefab where the sprites will be set
		ResourceManager.LoadData<GameObject> indicatorPrefab = new ResourceManager.LoadData<GameObject>();
		
		if ( ResourceManager.LoadResourceSync( "Scriptables/UI_Indicators", indicatorsSpritesCollection ) &&
			 ResourceManager.LoadResourceSync( "Prefabs/UI/Task_Objective", indicatorPrefab ) )
		{
			m_SpriteCollection	= indicatorsSpritesCollection.Asset;

			// Pool Creation
			GameObject model	= indicatorPrefab.Asset;
			m_Pool = new GameObjectsPool<Transform>
			(
				model:				model,
				size:				MAX_ELEMENTS,
				containerName:		"UI_IndicatorsPool",
				parent:				transform,
				actionOnObject:		delegate( Transform t ) { t.gameObject.SetActive(false); }
			);
			
		}
		
		return m_Pool != null ? m_Pool.IsValid : false;
	}


	//////////////////////////////////////////////////////////////////////////
	// ReInit
	bool IStateDefiner.ReInit()
	{
		return true;
	}


	//////////////////////////////////////////////////////////////////////////
	// Finalize
	bool IStateDefiner.Finalize()
	{
		return true;
	}


	//////////////////////////////////////////////////////////////////////////
	// EnableIndicator
	public	bool	EnableIndicator( GameObject target, IndicatorType IndicatorType )
	{
		int index = m_CurrentlyActive.FindIndex( ( ActiveIndicatorData p ) => { return p.Target == target; } );
		if ( index > -1 )
		{
			return false;
		}

		InternalCheck();

		Sprite indicator = m_SpriteCollection.Sprites[(int)IndicatorType];

		Transform indicatorTransform = m_Pool.GetComponent();

		Image mainIndicatorImage = null;
		Image MinimapIndicatorImage = null;
		indicatorTransform.SearchComponentInChild( "MainIndicator",		ref mainIndicatorImage );
		indicatorTransform.SearchComponentInChild( "MinimapIndicator",	ref MinimapIndicatorImage );

		MinimapIndicatorImage.sprite = mainIndicatorImage.sprite = indicator;

//		MinimapIndicatorImage.transform.SetParent( UI.Instance.InGame.UI_Minimap.transform.GetChild(0) );
//		MinimapIndicatorImage.transform.localPosition = Vector3.zero;
//		MinimapIndicatorImage.transform.localRotation = Quaternion.identity;

		ActiveIndicatorData pair = new ActiveIndicatorData()
		{
			Target = target,
			MainIndicatorImage = mainIndicatorImage,
			MinimapIndicatorImage = MinimapIndicatorImage
		};

		m_CurrentlyActive.Add( pair );

		indicatorTransform.gameObject.SetActive( true );
		return true;
	}


	//////////////////////////////////////////////////////////////////////////
	// DisableIndicator
	public	bool	DisableIndicator( GameObject target )
	{
		int index = m_CurrentlyActive.FindIndex( ( ActiveIndicatorData p ) => { return p.Target == target; } );
		bool bIsFound = index > -1;
		if ( bIsFound )
		{
			m_CurrentlyActive[index].MainIndicatorImage.gameObject.SetActive( false );
			m_CurrentlyActive[index].MinimapIndicatorImage.gameObject.SetActive( false );
			m_CurrentlyActive.RemoveAt( index );
		}

		InternalCheck();
		return bIsFound;
	}


	//////////////////////////////////////////////////////////////////////////
	// DisableIndicator
	/// <summary> Check whetever some data has been invalidated </summary>
	private	void InternalCheck()
	{
		for ( int i = m_CurrentlyActive.Count - 1; i >= 0; i-- )
		{
			ActiveIndicatorData p = m_CurrentlyActive[i];
			if ( p.Target == null )
			{
				m_CurrentlyActive[i].MainIndicatorImage.gameObject.SetActive( false );
				m_CurrentlyActive[i].MinimapIndicatorImage.gameObject.SetActive( false );
				m_CurrentlyActive.RemoveAt( i );
			}
		}
	}
	/*
	private void OnGUI2()
	{
		for ( int i = m_CurrentlyActive.Count - 1; i >= 0; i-- )
		{
			CurrentActivePair pair = m_CurrentlyActive[i];
			GameObject go = pair.GO;
			Image img = pair.IMG;
			if ( go ) // The gameobject could been destroyed in meanwhile 
			{
				DrawUIElementOnObjectives( go.transform, img.transform );
			}
		}
	}
	*/


	//////////////////////////////////////////////////////////////////////////
	// FixedUpdate
	private void FixedUpdate()
	{
		for ( int i = m_CurrentlyActive.Count - 1; i >= 0; i-- )
		{
			ActiveIndicatorData pair = m_CurrentlyActive[i];
			GameObject target = pair.Target;
			Image mainIndicatorImage = pair.MainIndicatorImage;
			Image minimapIndicatorImage = pair.MinimapIndicatorImage;

			if ( target ) // The gameobject could been destroyed in meanwhile 
			{
				DrawUIElementObjectivesOnScreen( target.transform, mainIndicatorImage.transform );
				DrawUIElementObjectivesOnMinimap( target.transform, minimapIndicatorImage.transform );
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// DrawUIElementOnObjectives
	protected	static void	DrawUIElementObjectivesOnScreen( Transform targetTransform, Transform m_IconTransform )
	{
		Camera camera = CameraControl.Instance.MainCamera;

		// Icon Scale Factor
		float scaleFactor = 1.0f;
		float distance = Vector3.Distance( camera.transform.position,targetTransform.position );
		if ( distance > MIN_DISTANCE_TO_RESIZE )
		{
			float interpolant = 1.0f - Utils.Math.ScaleBetween( distance, MAX_DISTANCE_TO_RESIZE, MIN_DISTANCE_TO_RESIZE );
			scaleFactor = Mathf.Clamp( interpolant, 0.5f, 1.0f );
			m_IconTransform.localScale = Vector3.one * scaleFactor;
		}


	//  Ref: https://www.youtube.com/watch?v=gAQpR1GN0Os
	//  Viewport space is normalized and relative to the camera. The bottom-left of the camera is (0,0); the top-right is (1,1). The z position is in world units from the camera.
	//  Screenspace is defined in pixels. The bottom-left of the screen is (0,0); the right-top is (pixelWidth,pixelHeight). The z position is in world units from the camera.
	//	Vector3 ViewportPoint = camera.WorldToViewportPoint( taretTransform.position );
		Vector3 ScreenPoint = camera.WorldToScreenPoint( targetTransform.position );	
		Vector3 DrawPosition = Vector3.zero;

		// Normal projection because inside screen
		if ( ScreenPoint.z > 0f && ScreenPoint.x > 0f && ScreenPoint.x < Screen.width && ScreenPoint.y > 0f && ScreenPoint.y < Screen.height )
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

			Vector2 screenBounds = screenCenter2D * 0.9f;

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
			if ( ScreenPoint2D.x > screenBounds.x ) // out of bounds, must be on right
			{
				ScreenPoint2D.Set( screenBounds.x, screenBounds.x * m );
			}
			else if ( ScreenPoint2D.x < -screenBounds.x ) // out of bounds left
			{
				ScreenPoint2D.Set( -screenBounds.x, -screenBounds.x * m );
			}

			// Remove cooridnate traslation
			ScreenPoint2D += screenCenter2D;

			DrawPosition.x = ScreenPoint2D.x; // ScreenPoint2D.x - texture.width*0.5f;
			DrawPosition.y = ScreenPoint2D.y; // Screen.height - ( ScreenPoint2D.y + texture.height*0.5f );
		}

		m_IconTransform.position = DrawPosition;
	}



	//////////////////////////////////////////////////////////////////////////
	// DrawUIElementOnObjectivesOnMinimap
	protected	static void	DrawUIElementObjectivesOnMinimap( Transform targetTransform, Transform m_IconTransform )
	{
		Camera camera					= UI.Instance.InGame.UI_Minimap.GetTopViewCamera();
		Vector2 minimapImagePosition	= UI.Instance.InGame.UI_Minimap.GetRawImagePosition();
		Rect minimapRect				= UI.Instance.InGame.UI_Minimap.GetRawImageRect();
		RectTransform minimapRectTrans	= UI.Instance.InGame.UI_Minimap.GetComponent<RectTransform>();
		
		float distance = Utils.Math.PlanarDistance( camera.transform.position, targetTransform.position, Vector3.up );

		float orthoWidth = camera.orthographicSize;
		
		float scaleFactor = UI.Instance.InGame.ScaleFactor;

		// Project to target point on the same plane of camera
		Vector3 ScreenPoint = camera.WorldToScreenPoint( targetTransform.position );

		ScreenPoint *= scaleFactor;

		Vector2 screenPointScaled = UI.Instance.InGame.UI_Minimap.transform.InverseTransformPoint( ScreenPoint );

		screenPointScaled.x -= minimapRect.width * 0.5f;
		screenPointScaled.y -= minimapRect.height * 0.5f;

		// Find angle from center of screen to mouse position
		float angle = Mathf.Atan2( screenPointScaled.y, screenPointScaled.x ) - 90f * Mathf.Deg2Rad;
		float cos = Mathf.Cos( angle );
		float sin = -Mathf.Sin( angle );

		float amplify = 150f;
		screenPointScaled.Set( screenPointScaled.x + sin * amplify, screenPointScaled.y + cos * amplify );

		// y = mx + b format
		float m = cos / sin;

		Vector2 minimapBounds = minimapRect.size * 0.5f;

		// Check up and down first
		if ( cos > 0.0f )
		{
			screenPointScaled.Set( minimapBounds.y / m, minimapBounds.y );
		}
		else // down
		{
			screenPointScaled.Set( -minimapBounds.y / m, -minimapBounds.y );
		}

		// If out of bounds, get point on appropriate side
		if ( screenPointScaled.x > minimapBounds.x ) // out of bounds, must be on right
		{
			screenPointScaled.Set( minimapBounds.x, minimapBounds.x * m );
		}
		else if ( screenPointScaled.x < -minimapBounds.x ) // out of bounds left
		{
			screenPointScaled.Set( -minimapBounds.x, -minimapBounds.x * m );
		}


		ScreenPoint = UI.Instance.InGame.UI_Minimap.transform.TransformPoint( screenPointScaled );



//		ScreenPoint = new Vector3( ScreenPoint.x / scaleFactor, ScreenPoint.y / scaleFactor );

//		screenPointScaled -= minimapRect.size * 0.5f;

//		screenPointScaled = UI.Instance.InGame.UI_Minimap.transform.InverseTransformPoint( screenPointScaled );
		{
//			screenPointScaled = Vector2.ClampMagnitude( screenPointScaled, orthoWidth * scaleFactor );
			
//			screenPointScaled = screenPointScaled.ClampComponents( minimapRect.size * 0.5f );

//			screenPointScaled.x = Mathf.Clamp( screenPointScaled.x, -minimapRect.width, minimapRect.width );
//			screenPointScaled.y = Mathf.Clamp( screenPointScaled.y, -minimapRect.height, minimapRect.height );

		}
//		screenPointScaled = UI.Instance.InGame.UI_Minimap.transform.TransformPoint( screenPointScaled );
		
		m_IconTransform.position = ScreenPoint;
	
	}
	
}
