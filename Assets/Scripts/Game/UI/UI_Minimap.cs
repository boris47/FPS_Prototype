
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


	//////////////////////////////////////////////////////////////////////////
	// Initialize
	bool IStateDefiner.Initialize()
	{
		if ( m_bIsInitialized )
			return true;

		m_bIsInitialized = true;
		{
			m_bIsInitialized &= transform.SearchComponent( ref m_RawImage, SearchContext.CHILDREN );

			ResourceManager.LoadData<RenderTexture> data = new ResourceManager.LoadData<RenderTexture>();
			if ( m_bIsInitialized && ( m_bIsInitialized &= ResourceManager.LoadResourceSync( "Textures/MinimapRenderTexture", data ) ) )
			{
				m_MinimapRenderTexture = data.Asset;

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

				m_TopViewCamera.allowMSAA			= false;
				m_TopViewCamera.useOcclusionCulling	= false;
				m_TopViewCamera.allowHDR			= false;
				m_TopViewCamera.farClipPlane		= m_CameraContainer.transform.position.y * 2f;
				m_TopViewCamera.targetTexture		= m_RawImage.texture as RenderTexture;


				m_MiniMapRectTransform = m_RawImage.transform as RectTransform;

				m_HelperRectTransform = new GameObject("MinimapHelper").AddComponent<RectTransform>();
				m_HelperRectTransform.SetParent( m_MiniMapRectTransform, worldPositionStays: false );
				m_HelperRectTransform.anchorMin = Vector2.zero;
				m_HelperRectTransform.anchorMax = Vector2.zero;
				m_HelperRectTransform.hideFlags = HideFlags.HideAndDontSave;

				m_RatioVector = new Vector2( m_MiniMapRectTransform.rect.width / m_TopViewCamera.pixelWidth, m_MiniMapRectTransform.rect.height / m_TopViewCamera.pixelHeight );
			}

			if ( m_bIsInitialized )
			{	
				
			}
			else
			{
				Debug.LogError( "UI_Minimap: Bad initialization!!!" );
			}
		}
		return m_bIsInitialized;
	}


	//////////////////////////////////////////////////////////////////////////
	// ReInit
	bool IStateDefiner.ReInit()
	{
		return m_bIsInitialized;
	}


	//////////////////////////////////////////////////////////////////////////
	// Finalize
	bool	 IStateDefiner.Finalize()
	{
		return m_bIsInitialized;
	}


	// Ref: http://answers.unity.com/answers/1461171/view.html
	public bool GetPositionOnUI( Vector3 worldPosition, out Vector2 WorldPosition2D )
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
		
		WorldPosition2D = m_HelperRectTransform.position;

		return RectTransformUtility.RectangleContainsScreenPoint( m_MiniMapRectTransform, WorldPosition2D );
	}


	//////////////////////////////////////////////////////////////////////////
	private void FixedUpdate()
	{
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
		m_bIsVisible = true;

		const float alphaValue = 0.7333333333333333f;
		Color colorToAssign = Color.white;
		colorToAssign.a = alphaValue;
		m_RawImage.material.color = colorToAssign;
	}


	//////////////////////////////////////////////////////////////////////////
	public	void	Hide()
	{
		m_bIsVisible = false;
		m_RawImage.material.color = Color.clear;
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnDestroy()
	{
		Show();
	}

}
