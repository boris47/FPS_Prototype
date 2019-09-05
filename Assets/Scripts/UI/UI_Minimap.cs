
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Minimap : MonoBehaviour, IStateDefiner {

	private			Camera			m_TopViewCamera				= null;
	private			bool			m_bIsVisible				= true;
	private			RawImage		m_RawImage					= null;
	private			RenderTexture	m_MinimapRenderTexture		= null;

	private			GameObject		m_CameraContainer			= null;

	private			RectTransform	m_MiniMapRectTransform		= null;
	private			RectTransform	m_HelperRectTransform		= null;
	private			Vector2			m_RatioVector				= Vector2.zero;

	public			RectTransform	GetRawImageRect()
	{
		return m_MiniMapRectTransform;
	}

	public			bool			IsVisible()
	{
		return m_bIsVisible;
	}

	private			bool			m_bIsInitialized			= false;
	bool IStateDefiner.IsInitialized
	{
		get { return m_bIsInitialized; }
	}

	string IStateDefiner.StateName
	{
		get { return name; }
	}

	private			bool			m_bIsCompletedInitialization	= false;


	//////////////////////////////////////////////////////////////////////////
	// Initialize
	IEnumerator IStateDefiner.Initialize()
	{
		if ( m_bIsInitialized == true )
			yield break;

		yield return null;

		CoroutinesManager.AddCoroutineToPendingCount( 1 );

		// Trick where panel is set very on the bottom of ui and allowed to do onFrame stuf in coroutine, then gameobject is deactivated
		Vector3 previousCanvasPosition = transform.position;

		m_bIsInitialized = true;
		{
			m_bIsInitialized &= transform.SearchComponent( ref m_RawImage, SearchContext.CHILDREN );

			ResourceManager.LoadedData<RenderTexture> loadedResource = new ResourceManager.LoadedData<RenderTexture>();
			yield return ResourceManager.LoadResourceAsyncCoroutine
			(
				ResourcePath:			"Textures/MinimapRenderTexture",
				loadedData:				loadedResource,
				OnResourceLoaded :		(a) => { m_bIsInitialized &= true; m_MinimapRenderTexture = a; },
				OnFailure:				(p) => m_bIsInitialized &= false
			);

			if ( m_bIsInitialized )
			{
				m_MinimapRenderTexture = loadedResource.Asset;

				if ( m_CameraContainer != null )
				{
					Object.Destroy( m_CameraContainer );
				}

				m_CameraContainer = new GameObject("TopViewCamera");
				m_CameraContainer.transform.position = Vector3.up * 100f;

				m_TopViewCamera = m_CameraContainer.AddComponent<Camera>();
				m_TopViewCamera.orthographic		= true;
				m_TopViewCamera.orthographicSize	= 32f;
				m_TopViewCamera.clearFlags			= CameraClearFlags.Depth;
				DontDestroyOnLoad( m_CameraContainer );

				m_TopViewCamera.allowMSAA			= false;
				m_TopViewCamera.useOcclusionCulling	= false;
				m_TopViewCamera.allowHDR			= false;
				m_TopViewCamera.farClipPlane		= m_CameraContainer.transform.position.y * 2f;
				m_TopViewCamera.targetTexture		= m_RawImage.texture as RenderTexture;

				yield return null;

				m_MiniMapRectTransform = m_RawImage.transform as RectTransform;

				m_HelperRectTransform = new GameObject("MinimapHelper").AddComponent<RectTransform>();
				m_HelperRectTransform.SetParent( m_MiniMapRectTransform, worldPositionStays: false );
				m_HelperRectTransform.anchorMin = Vector2.zero;
				m_HelperRectTransform.anchorMax = Vector2.zero;
				m_HelperRectTransform.gameObject.hideFlags = HideFlags.HideAndDontSave;

				m_RatioVector = new Vector2( m_MiniMapRectTransform.rect.width / m_TopViewCamera.pixelWidth, m_MiniMapRectTransform.rect.height / m_TopViewCamera.pixelHeight );
				m_bIsCompletedInitialization = true;
			}

			if ( m_bIsInitialized )
			{	
				CoroutinesManager.RemoveCoroutineFromPendingCount( 1 );
			}
			else
			{
				Debug.LogError( "UI_Minimap: Bad initialization!!!" );
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// ReInit
	IEnumerator	IStateDefiner.ReInit()
	{
		yield return null;
	}


	//////////////////////////////////////////////////////////////////////////
	// Finalize
	bool	 IStateDefiner.Finalize()
	{
		return m_bIsCompletedInitialization;
	}


	//////////////////////////////////////////////////////////////////////////
	// Ref: http://answers.unity.com/answers/1461171/view.html
	public bool GetPositionOnUI( Vector3 worldPosition, out Vector2 screenPointInWorldSpace )
	{
		//first we get screnPoint in camera viewport space
		Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint (m_TopViewCamera, worldPosition);
		
		// if render texture has different size of map rect size then we multiply by factor the scrrenPoint
		if ( m_MinimapRenderTexture.width != m_MiniMapRectTransform.rect.width || m_MinimapRenderTexture.height != m_MiniMapRectTransform.rect.height )
		{
			// then transform it to position in worldImage using its rect
			screenPoint.x *= m_RatioVector.x;
			screenPoint.y *= m_RatioVector.y;
		}

		//after positioning helper to that spot
		m_HelperRectTransform.anchoredPosition = screenPoint;
		
		screenPointInWorldSpace = m_HelperRectTransform.position;

		return RectTransformUtility.RectangleContainsScreenPoint( m_MiniMapRectTransform, screenPointInWorldSpace );
	}


	//////////////////////////////////////////////////////////////////////////
	private void FixedUpdate()
	{
		if ( m_bIsCompletedInitialization == false || Player.Instance.IsNotNull() == false )
			return;

		Vector3 prevPosition = m_TopViewCamera.transform.position;
		prevPosition.x = Player.Instance.transform.position.x;
		prevPosition.z = Player.Instance.transform.position.z;
		m_TopViewCamera.transform.position = prevPosition;
		
		Vector3 planePoint		= CameraControl.Instance.Transform.position;
		Vector3 planeNormal		= Vector3.up;
		Vector3 point			= CameraControl.Instance.Transform.position + CameraControl.Instance.Transform.forward * 100f;
		Vector3 projectedPoint	= Utils.Math.ProjectPointOnPlane( planeNormal, planePoint, point );
		Vector3 upwards			= ( projectedPoint - planePoint ).normalized;
		
		m_TopViewCamera.transform.rotation = Quaternion.LookRotation( Vector3.down, upwards );
	}


	//////////////////////////////////////////////////////////////////////////
	public	void	Show()
	{
		if ( m_bIsCompletedInitialization == false )
			return;

		m_bIsVisible = true;

		const float alphaValue = 0.7333333333333333f;
		Color colorToAssign = Color.white;
		colorToAssign.a = alphaValue;
		m_RawImage.material.color = colorToAssign;
	}


	//////////////////////////////////////////////////////////////////////////
	public	void	Hide()
	{
		if ( m_bIsCompletedInitialization == false )
			return;

		m_bIsVisible = false;
		m_RawImage.material.color = Color.clear;
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnDestroy()
	{
		Show();
	}

}
