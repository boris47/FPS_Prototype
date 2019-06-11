
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
		public	GameObject		Target;
		public	Image			MainIndicatorImage;
		public	Image			MinimapIndicatorImage;
		public	bool			bMustBeClamped;
	};
	private		List<ActiveIndicatorData>			m_CurrentlyActive			= new List<ActiveIndicatorData>(MAX_ELEMENTS);

	private	struct IndicatorRequest {
		public	GameObject			target;
		public	IndicatorType		IndicatorType;
		public	bool				bMustBeClamped;
	}
	private	List<IndicatorRequest>					m_Requests					= new List<IndicatorRequest>();

	private		bool								m_bIsInitialized			= false;
	bool IStateDefiner.IsInitialized
	{
		get { return m_bIsInitialized; }
	}


	//////////////////////////////////////////////////////////////////////////
	// Initialize
	IEnumerator IStateDefiner.Initialize()
	{
		if ( m_bIsInitialized )
			yield break;

		m_bIsInitialized = false;

		// Sprites for TargetToKill, LocationToReach or ObjectToInteractWith
		ResourceManager.LoadData<SpriteCollection> indicatorsSpritesCollection = new ResourceManager.LoadData<SpriteCollection>();

		// A prefab where the sprites will be set
		ResourceManager.LoadData<GameObject> indicatorPrefab = new ResourceManager.LoadData<GameObject>();
		
		yield return ResourceManager.LoadResourceAsyncCoroutine( "Scriptables/UI_Indicators", indicatorsSpritesCollection, null );
		yield return ResourceManager.LoadResourceAsyncCoroutine( "Prefabs/UI/Task_Objective", indicatorPrefab, null );
		m_bIsInitialized |= indicatorsSpritesCollection.Asset != null && indicatorPrefab.Asset != null;

		if ( m_bIsInitialized )
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
	// EnableIndicator
	private	bool	EnableIndicatorInternal( IndicatorRequest request )
	{
		int index = m_CurrentlyActive.FindIndex( ( ActiveIndicatorData p ) => { return p.Target == request.target; } );
		if ( index > -1 )
		{
			return false;
		}

		InternalCheck();

		Sprite indicator = m_SpriteCollection.Sprites[(int)request.IndicatorType];

		Transform indicatorTransform = m_Pool.GetComponent();

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

		m_CurrentlyActive.Add( pair );

		indicatorTransform.gameObject.SetActive( true );
		return true;
	}

	//////////////////////////////////////////////////////////////////////////
	// EnableIndicator
	public	void	EnableIndicator( GameObject target, IndicatorType IndicatorType, bool bMustBeClamped )
	{
		IndicatorRequest indicatorRequest = new IndicatorRequest()
		{
			target = target,
			IndicatorType  = IndicatorType,
			bMustBeClamped = bMustBeClamped
		};
		m_Requests.Add( indicatorRequest );
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


	//////////////////////////////////////////////////////////////////////////
	// FixedUpdate
	private void Update()
	{
		if ( m_bIsInitialized == false )
			return;

		for ( int i = 0; i < m_Requests.Count; i++ )
		{
			IndicatorRequest request = m_Requests[i];
			EnableIndicatorInternal( request );
		}
		m_Requests.Clear();

		for ( int i = m_CurrentlyActive.Count - 1; i >= 0; i-- )
		{
			ActiveIndicatorData pair = m_CurrentlyActive[i];
			GameObject target			= pair.Target;
			Image mainIndicatorImage	= pair.MainIndicatorImage;
			Image minimapIndicatorImage = pair.MinimapIndicatorImage;
			bool bMustBeClamped			= pair.bMustBeClamped;

			if ( target ) // The gameobject could been destroyed in meanwhile 
			{
				DrawUIElementObjectivesOnScreen( target.transform, mainIndicatorImage.transform );

				if ( UI.Instance.Minimap.IsVisible() )
				{
					DrawUIElementObjectivesOnMinimap( target.transform, minimapIndicatorImage.transform, bMustBeClamped );
				}
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
	protected	static void	DrawUIElementObjectivesOnMinimap( Transform targetTransform, Transform m_IconTransform, bool bMustBeClamped )
	{
		RectTransform minimapRect		= UI.Instance.Minimap.GetRawImageRect();

		if ( m_IconTransform.gameObject.activeSelf == false )
		{
			m_IconTransform.gameObject.SetActive( true );
		}

		//
		Vector2 WorldPosition2D;
		bool bIsInside = UI.Instance.Minimap.GetPositionOnUI( targetTransform.position, out WorldPosition2D );
		if ( bIsInside == false && bMustBeClamped == false )
		{
			m_IconTransform.gameObject.SetActive( false );
		}
		if ( bIsInside == false && bMustBeClamped == true )
		{
			Vector2 screenPointScaled = UI.Instance.Minimap.transform.InverseTransformPoint( WorldPosition2D );
			// Find angle from center of screen to mouse position
			float angle = Mathf.Atan2( screenPointScaled.y, screenPointScaled.x ) - 90f * Mathf.Deg2Rad;
			float cos = Mathf.Cos( angle );
			float sin = -Mathf.Sin( angle );

			float amplify = 150f;
			screenPointScaled.Set( screenPointScaled.x + sin * amplify, screenPointScaled.y + cos * amplify );

			// y = mx + b format
			float m = cos / sin;

			Vector2 minimapBounds = minimapRect.rect.size * 0.5f;

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

			// Because of the different scale applied to rawImage rectTransform, this must be token in account fo computations
			screenPointScaled.x *= minimapRect.localScale.x;
			screenPointScaled.y *= minimapRect.localScale.y;

			WorldPosition2D = UI.Instance.Minimap.transform.TransformPoint( screenPointScaled );
		}

		m_IconTransform.position = WorldPosition2D;
	}
	
}

