
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum EIndicatorType
{
	AREA_WHERE_PLACE_OBJECT,
	AREA_TO_REACH,
	OBJECT_TO_INTERACT,
	OBJECT_TO_FOLLOW,
	TARGET_TO_KILL
}

public sealed class UI_Indicators : UI_Base, IStateDefiner
{
	private	const		float								MAX_DISTANCE_TO_RESIZE			= 25f;
	private	const		float								MIN_DISTANCE_TO_RESIZE			= 2f;

	private	class ActiveIndicatorData
	{
		public	GameObject		Target							= null;
		public  Transform		IndicatorTransform				= null;
		public	Transform		MainIndicatorImageTransform		= null;
		public	Transform		MinimapIndicatorImageTransform	= null;
		public	bool			bMustBeClamped					= false;
	};

	[System.Serializable]
	private class UI_IndicatorsSectionData
	{
		public	float	InScreenMarkerFactor		= 0.8f;
		public	float	MinimapClampFactor			= 0.72f;
	}
	
	[SerializeField, ReadOnly]
	private				UI_IndicatorsSectionData			m_IndicatorsSectionData			= new UI_IndicatorsSectionData();

	private				SpriteCollection					m_SpriteCollection				= null;
	private				List<ActiveIndicatorData>			m_CurrentlyActive				= new List<ActiveIndicatorData>();
	private				GameObject							m_IndicatorPrefab				= null;
	private				bool								m_IsInitialized					= false;
						bool								IStateDefiner.IsInitialized		=> m_IsInitialized;



	//////////////////////////////////////////////////////////////////////////
	void IStateDefiner.PreInit()
	{

	}

	//////////////////////////////////////////////////////////////////////////
	void IStateDefiner.Initialize()
	{
		if (!m_IsInitialized)
		{
			// Section Data
			CustomAssertions.IsTrue
			(
				GlobalManager.Configs.TryGetSection("UI_Indicators", out Database.Section indicatorsSection) && GlobalManager.Configs.TrySectionToOuter(indicatorsSection, m_IndicatorsSectionData),
				"Cannot load UI_IndicatorsSectionData"
			);

			// 
			CustomAssertions.IsTrue(ResourceManager.LoadResourceSync("Scriptables/UI_Indicators", out m_SpriteCollection));

			// 
			CustomAssertions.IsTrue(ResourceManager.LoadResourceSync("Prefabs/UI/Task_Objective", out m_IndicatorPrefab));

			CustomAssertions.IsTrue(m_IndicatorPrefab.transform.TrySearchComponentByChildName("MainIndicator", out Image mainIndicatorImage));
			CustomAssertions.IsTrue(m_IndicatorPrefab.transform.TrySearchComponentByChildName("MinimapIndicator", out Image minimapIndicatorImage));

			m_IsInitialized = true;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	void IStateDefiner.ReInit()
	{
		
	}


	//////////////////////////////////////////////////////////////////////////
	bool IStateDefiner.Finalize()
	{
		return true;
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnEnable()
	{
		CustomAssertions.IsTrue(m_IsInitialized);

		if (CustomAssertions.IsNotNull(GameManager.UpdateEvents))
		{
			GameManager.UpdateEvents.OnLateFrame += OnLateFrame;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnDisable()
	{
		if (GameManager.UpdateEvents.IsNotNull())
		{
			GameManager.UpdateEvents.OnLateFrame -= OnLateFrame;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public void AddIndicator(GameObject target, EIndicatorType IndicatorType, bool bMustBeClamped)
	{
		Sprite indicatorImage = m_SpriteCollection.Sprites[(uint)IndicatorType];

		// Setup indicator
		Transform indicator = Instantiate(m_IndicatorPrefab, transform).transform;
		
		Image mainIndicatorImage = indicator.Find("MainIndicator").GetComponent<Image>();
		mainIndicatorImage.sprite = indicatorImage;

		Image minimapIndicatorImage = indicator.Find("MinimapIndicator").GetComponent<Image>();
		minimapIndicatorImage.sprite = indicatorImage;

		indicator.gameObject.SetActive(true);

		ActiveIndicatorData indicatorData = new ActiveIndicatorData()
		{
			Target							= target,
			IndicatorTransform				= indicator,
			MainIndicatorImageTransform		= mainIndicatorImage.transform,
			MinimapIndicatorImageTransform	= minimapIndicatorImage.transform,
			bMustBeClamped					= bMustBeClamped
		};
		m_CurrentlyActive.Add(indicatorData);
	}


	//////////////////////////////////////////////////////////////////////////
	public bool RemoveIndicator(GameObject target)
	{
		int count = m_CurrentlyActive.Count;
		for (int i = count - 1; i >= 0; i--)
		{
			var indicator = m_CurrentlyActive[i];
			if (target == indicator.Target)
			{
				Object.Destroy(indicator.IndicatorTransform.gameObject);
				m_CurrentlyActive.RemoveAt(i);
			}
		}
		return m_CurrentlyActive.Count != count;
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Check whetever some data has been invalidated </summary>
	private	void InternalCheck()
	{
		for (int index = m_CurrentlyActive.Count - 1; index >= 0; index--)
		{
			ActiveIndicatorData p = m_CurrentlyActive[index];
			if (p.Target == null)
			{
				Object.Destroy(m_CurrentlyActive[index].IndicatorTransform.gameObject);
				m_CurrentlyActive.RemoveAt(index);
			}
		}
	}

	/////////////////////////////////////////////////////////////////////////
	private void OnLateFrame(float deltaTime)
	{
		float inScreenMarkerFactor = m_IndicatorsSectionData.InScreenMarkerFactor;
		float minimapClampFactor = m_IndicatorsSectionData.MinimapClampFactor;

		InternalCheck();

		for ( int i = m_CurrentlyActive.Count - 1; i >= 0; i-- )
		{
			ActiveIndicatorData indicator = m_CurrentlyActive[i];
			if (indicator.Target.IsNotNull())
			{
				GameObject target							= indicator.Target;
				Transform mainIndicatorImageTransform		= indicator.MainIndicatorImageTransform;
				Transform minimapIndicatorImageTransform	= indicator.MinimapIndicatorImageTransform;
				bool bMustBeClamped							= indicator.bMustBeClamped;

				DrawUIElementObjectivesOnScreen(target.transform, mainIndicatorImageTransform, inScreenMarkerFactor);
				if (UIManager.Minimap.IsVisible)
				{
					DrawUIElementObjectivesOnMinimap(target.transform, minimapIndicatorImageTransform, minimapClampFactor, bMustBeClamped);
				}
			}
			else
			{
				Object.Destroy(indicator.IndicatorTransform.gameObject);
				m_CurrentlyActive.RemoveAt(i);
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private static void DrawUIElementObjectivesOnScreen(in Transform targetTransform, in Transform m_IconTransform, in float inScreenMarkerFactor)
	{
		Camera camera = FPSEntityCamera.Instance.MainCamera;

		// Icon Scale Factor
		float scaleFactor = 1.0f;
		float distance = Vector3.Distance(camera.transform.position, targetTransform.position);
		if (distance > MIN_DISTANCE_TO_RESIZE)
		{
			float interpolant = 1.0f - Utils.Math.ScaleBetweenClamped01(distance, MIN_DISTANCE_TO_RESIZE, MAX_DISTANCE_TO_RESIZE);
			scaleFactor = Mathf.Clamp(interpolant, 0.5f, 1.0f);
			m_IconTransform.localScale = Vector3.one * scaleFactor;
		}


		//  Ref: https://www.youtube.com/watch?v=gAQpR1GN0Os
		//  Viewport space is normalized and relative to the camera. The bottom-left of the camera is (0,0); the top-right is (1,1). The z position is in world units from the camera.
		//  Screenspace is defined in pixels. The bottom-left of the screen is (0,0); the right-top is (pixelWidth,pixelHeight). The z position is in world units from the camera.
		//	Vector3 ViewportPoint = camera.WorldToViewportPoint( taretTransform.position );
		Vector3 ScreenPoint = camera.WorldToScreenPoint(targetTransform.position);
		Vector3 DrawPosition = Vector3.zero;


		float scaledWidth  = (float)Screen.width  * inScreenMarkerFactor;
		float scaledHeight = (float)Screen.height * inScreenMarkerFactor;

		// Normal projection because inside screen
		if (ScreenPoint.z > 0f 
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
			if (bIsBehind)
			{
				ScreenPoint *= -1.0f;
			}

			Vector2 ScreenPoint2D = ScreenPoint;
			Vector2 screenCenter2D = new Vector2(Screen.width, Screen.height) * 0.5f;

			// NOTE COORDINATE TRASLATED
			// make 0, 0 the center of screen inteead of bottom left
			ScreenPoint2D -= screenCenter2D;

			// Find angle from center of screen to mouse position
			float angle = Mathf.Atan2(ScreenPoint2D.y, ScreenPoint2D.x) - (90f * Mathf.Deg2Rad);
			float cos = Mathf.Cos(angle);
			float sin = -Mathf.Sin(angle);

			const float amplify = 150f;
			ScreenPoint2D.Set(screenCenter2D.x + (sin * amplify), screenCenter2D.y + (cos * amplify));

			// y = mx + b format
			float m = cos / sin;

			Vector2 screenBounds = screenCenter2D * inScreenMarkerFactor;

			// Check up and down first
			if (cos > 0.0f)
			{
				ScreenPoint2D.Set(screenBounds.y / m, screenBounds.y);
			}
			else // down
			{
				ScreenPoint2D.Set(-screenBounds.y / m, -screenBounds.y);
			}

			// If out of bounds, get point on appropriate side
			if (ScreenPoint2D.x > screenBounds.x)           // out of bounds right
			{
				ScreenPoint2D.Set(screenBounds.x, screenBounds.x * m);
			}
			else if (ScreenPoint2D.x < -screenBounds.x) // out of bounds left
			{
				ScreenPoint2D.Set(-screenBounds.x, -screenBounds.x * m);
			}

			// Remove cooridnate traslation
			ScreenPoint2D += screenCenter2D;

			DrawPosition.Set(ScreenPoint2D.x, ScreenPoint2D.y, 0.0f);

			//DrawPosition.x = ScreenPoint2D.x; // ScreenPoint2D.x - texture.width*0.5f;
			//DrawPosition.y = ScreenPoint2D.y; // Screen.height - (ScreenPoint2D.y + (texture.height * 0.5f));
		}

		m_IconTransform.position = DrawPosition;
	}



	//////////////////////////////////////////////////////////////////////////
	private static void DrawUIElementObjectivesOnMinimap(in Transform targetTransform, in Transform m_IconTransform, in float minimapClampFactor, in bool bMustBeClamped)
	{
		RectTransform minimapRectTransform = UIManager.Minimap.RawImageRect;

		if (!m_IconTransform.gameObject.activeSelf)
		{
			m_IconTransform.gameObject.SetActive(true);
		}

		//
		bool bIsInside = UIManager.Minimap.GetPositionInMinimapLocalSpace(targetTransform.position, out Vector2 screenPointInWorldSpace);

		// If is no more inside minimap image rect and IS NOT required to be clamped the object will be deactivated
		if ( !bIsInside && !bMustBeClamped && m_IconTransform.gameObject.activeSelf)
		{
			m_IconTransform.gameObject.SetActive(false);
		}

		// If is no more inside minimap image rect and IS required to be clamped the object will be drawn clamped inside minimap rect
		if (!bIsInside && bMustBeClamped)
		{
			Vector2 screenPointInLocalSpace = UIManager.Minimap.transform.InverseTransformPoint(screenPointInWorldSpace);

			// Find angle from center of screen to mouse position
			float angle = Mathf.Atan2(screenPointInLocalSpace.y, screenPointInLocalSpace.x) - (90f * Mathf.Deg2Rad);
			float cos = Mathf.Cos(angle);
			float sin = -Mathf.Sin(angle);

			float amplify = 150f;
			screenPointInLocalSpace.Set(screenPointInLocalSpace.x + (sin * amplify), screenPointInLocalSpace.y + (cos * amplify));

			// y = mx + b format
			float m = cos / sin;

			Vector2 minimapBounds = minimapRectTransform.rect.size * minimapClampFactor;

			// Check up and down first
			if (cos > 0.0f)
			{
				screenPointInLocalSpace.Set(minimapBounds.y / m, minimapBounds.y);
			}
			else // down
			{
				screenPointInLocalSpace.Set(-minimapBounds.y / m, -minimapBounds.y);
			}

			// If out of bounds, get point on appropriate side
			if (screenPointInLocalSpace.x > minimapBounds.x) // out of bounds, must be on right
			{
				screenPointInLocalSpace.Set(minimapBounds.x, minimapBounds.x * m);
			}
			else if (screenPointInLocalSpace.x < -minimapBounds.x) // out of bounds left
			{
				screenPointInLocalSpace.Set(-minimapBounds.x, -minimapBounds.x * m);
			}

			// Because of the different scale applied to rawImage rectTransform, this must be token in account fo computations
			Vector3 scaleToApply = minimapRectTransform.localScale * (minimapClampFactor * 0.5f);
			screenPointInLocalSpace.Scale(scaleToApply);

			screenPointInWorldSpace = UIManager.Minimap.transform.TransformPoint(screenPointInLocalSpace);
		}

		m_IconTransform.position = screenPointInWorldSpace;
	}
	
}

