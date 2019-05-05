
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

	private		GameObjectsPool<Image>				m_Pool						= null;

	private	struct CurrentActivePair {
		public	GameObject	GO;
		public	Image		IMG;
	};

	private		List<CurrentActivePair>				m_CurrentlyActive			= new List<CurrentActivePair>(MAX_ELEMENTS);

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
			GameObject model	= indicatorPrefab.Asset;
			m_SpriteCollection	= indicatorsSpritesCollection.Asset;

			// Pool Creation
			m_Pool = new GameObjectsPool<Image>
			(
				model:				model,
				size:				MAX_ELEMENTS,
				containerName:		"UI_IndicatorsPool",
				parent:				transform,
				actionOnObject:		delegate( Image i ) { i.gameObject.SetActive(false); }
			);

			foreach( Image i in m_Pool )
			{

			}
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
		int index = m_CurrentlyActive.FindIndex( ( CurrentActivePair p ) => { return p.GO == target; } );
		if ( index > -0 )
		{
			return false;
		}

		InternalCheck();

		Sprite indicator = m_SpriteCollection.Sprites[(int)IndicatorType];

		Image availableImage = m_Pool.GetComponent();
		availableImage.sprite = indicator;

		CurrentActivePair pair = new CurrentActivePair()
		{
			GO = target,
			IMG = availableImage
		};

		m_CurrentlyActive.Add( pair );

		availableImage.gameObject.SetActive( true );
		return true;
	}


	//////////////////////////////////////////////////////////////////////////
	// DisableIndicator
	public	bool	DisableIndicator( GameObject target )
	{
		int index = m_CurrentlyActive.FindIndex( ( CurrentActivePair p ) => { return p.GO == target; } );
		bool bIsFound = index > -1;
		if ( bIsFound )
		{
			m_CurrentlyActive[index].IMG.gameObject.SetActive( false );
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
			CurrentActivePair p = m_CurrentlyActive[i];
			if ( p.GO == null )
			{
				m_CurrentlyActive[i].IMG.gameObject.SetActive( false );
				m_CurrentlyActive.RemoveAt( i );
			}
		}
	}

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



	//////////////////////////////////////////////////////////////////////////
	// FixedUpdate
	private void FixedUpdate()
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


	//////////////////////////////////////////////////////////////////////////
	// DrawUIElementOnObjectives
	protected	static void	DrawUIElementOnObjectives( Transform targetTransform, Transform m_IconTransform )
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
				ScreenPoint2D.Set( screenBounds.y/m, screenBounds.y );
			}
			else // down
			{
				ScreenPoint2D.Set( -screenBounds.y/m, -screenBounds.y );
			}

			// If out of bounds, get point on appropriate side
			if ( ScreenPoint2D.x > screenBounds.x ) // out of bounds, must be on right
			{
				ScreenPoint2D.Set( screenBounds.x, screenBounds.x*m );
			}
			else if ( ScreenPoint2D.x < -screenBounds.x ) // out of bounds left
			{
				ScreenPoint2D.Set( -screenBounds.x, -screenBounds.x*m );
			}

			// Remove cooridnate traslation
			ScreenPoint2D += screenCenter2D;

			DrawPosition.x = ScreenPoint2D.x; // ScreenPoint2D.x - texture.width*0.5f;
			DrawPosition.y = ScreenPoint2D.y; // Screen.height - ( ScreenPoint2D.y + texture.height*0.5f );
		}

		m_IconTransform.position = DrawPosition;
	}
	
}
