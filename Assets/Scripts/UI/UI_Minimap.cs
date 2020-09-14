
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class UI_Minimap : MonoBehaviour, IStateDefiner {

	private			Camera			m_TopViewCamera				= null;
	private			bool			m_IsVisible					= true;
	private			RawImage		m_RawImage					= null;
	private			RenderTexture	m_MinimapRenderTexture		= null;

	private			GameObject		m_CameraContainer			= null;

	private			RectTransform	m_MiniMapRectTransform		= null;
	private			RectTransform	m_HelperRectTransform		= null;
	private			Vector2			m_RatioVector				= Vector2.zero;

	public			RectTransform	GetRawImageRect()
	{
		return this.m_MiniMapRectTransform;
	}

	public			bool			IsVisible()
	{
		return this.m_IsVisible;
	}

	private			bool			m_IsInitialized			= false;
	bool IStateDefiner.IsInitialized
	{
		get { return this.m_IsInitialized; }
	}

	string IStateDefiner.StateName
	{
		get { return this.name; }
	}

	private			bool			m_IsCompletedInitialization	= false;


	//////////////////////////////////////////////////////////////////////////
	// Initialize
	IEnumerator IStateDefiner.Initialize()
	{
		if (this.m_IsInitialized == true )
			yield break;

		yield return null;

		CoroutinesManager.AddCoroutineToPendingCount( 1 );

		// Trick where panel is set very on the bottom of ui and allowed to do onFrame stuf in coroutine, then gameobject is deactivated
		Vector3 previousCanvasPosition = this.transform.position;

		this.m_IsInitialized = true;
		{
			this.m_IsInitialized &= this.transform.SearchComponent( ref this.m_RawImage, ESearchContext.CHILDREN );

			ResourceManager.LoadedData<RenderTexture> loadedResource = new ResourceManager.LoadedData<RenderTexture>();
			yield return ResourceManager.LoadResourceAsyncCoroutine
			(
				ResourcePath:			"Textures/MinimapRenderTexture",
				loadedResource:			loadedResource,
				OnResourceLoaded :		(a) => { this.m_IsInitialized &= true; this.m_MinimapRenderTexture = a; },
				OnFailure:				(p) => this.m_IsInitialized &= false
			);

			if (this.m_IsInitialized )
			{
				this.m_MinimapRenderTexture = loadedResource.Asset;

				if (this.m_CameraContainer != null )
				{
					Object.Destroy(this.m_CameraContainer );
				}

				this.m_CameraContainer = new GameObject("TopViewCamera");
				this.m_CameraContainer.transform.position = Vector3.up * 100f;

				this.m_TopViewCamera = this.m_CameraContainer.AddComponent<Camera>();
				this.m_TopViewCamera.orthographic		= true;
				this.m_TopViewCamera.orthographicSize	= 32f;
				this.m_TopViewCamera.clearFlags			= CameraClearFlags.Depth;
				DontDestroyOnLoad(this.m_CameraContainer );

				this.m_TopViewCamera.allowMSAA			= false;
				this.m_TopViewCamera.useOcclusionCulling	= false;
				this.m_TopViewCamera.allowHDR			= false;
				this.m_TopViewCamera.farClipPlane		= this.m_CameraContainer.transform.position.y * 2f;
				this.m_TopViewCamera.targetTexture		= this.m_RawImage.texture as RenderTexture;

				yield return null;

				this.m_MiniMapRectTransform = this.m_RawImage.transform as RectTransform;

				this.m_HelperRectTransform = new GameObject("MinimapHelper").AddComponent<RectTransform>();
				this.m_HelperRectTransform.SetParent(this.m_MiniMapRectTransform, worldPositionStays: false );
				this.m_HelperRectTransform.anchorMin = Vector2.zero;
				this.m_HelperRectTransform.anchorMax = Vector2.zero;
				this.m_HelperRectTransform.gameObject.hideFlags = HideFlags.HideAndDontSave;

				this.m_RatioVector = new Vector2(this.m_MiniMapRectTransform.rect.width / this.m_TopViewCamera.pixelWidth, this.m_MiniMapRectTransform.rect.height / this.m_TopViewCamera.pixelHeight );
				this.m_IsCompletedInitialization = true;
			}

			if (this.m_IsInitialized )
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
		return this.m_IsCompletedInitialization;
	}


	//////////////////////////////////////////////////////////////////////////
	// Ref: http://answers.unity.com/answers/1461171/view.html
	public bool GetPositionOnUI( Vector3 worldPosition, out Vector2 screenPointInWorldSpace )
	{
		//first we get screnPoint in camera viewport space
		Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint (this.m_TopViewCamera, worldPosition);
		
		// if render texture has different size of map rect size then we multiply by factor the scrrenPoint
		if (this.m_MinimapRenderTexture.width != this.m_MiniMapRectTransform.rect.width || this.m_MinimapRenderTexture.height != this.m_MiniMapRectTransform.rect.height )
		{
			// then transform it to position in worldImage using its rect
			screenPoint.x *= this.m_RatioVector.x;
			screenPoint.y *= this.m_RatioVector.y;
		}

		//after positioning helper to that spot
		this.m_HelperRectTransform.anchoredPosition = screenPoint;
		
		screenPointInWorldSpace = this.m_HelperRectTransform.position;

		return RectTransformUtility.RectangleContainsScreenPoint(this.m_MiniMapRectTransform, screenPointInWorldSpace );
	}


	//////////////////////////////////////////////////////////////////////////
	private void FixedUpdate()
	{
		if (this.m_IsCompletedInitialization == false || Player.Instance.IsNotNull() == false )
			return;

		Vector3 prevPosition = this.m_TopViewCamera.transform.position;
		prevPosition.x = Player.Instance.transform.position.x;
		prevPosition.z = Player.Instance.transform.position.z;
		this.m_TopViewCamera.transform.position = prevPosition;
		
		Vector3 planePoint		= CameraControl.Instance.Transform.position;
		Vector3 planeNormal		= Vector3.up;
		Vector3 point			= CameraControl.Instance.Transform.position + CameraControl.Instance.Transform.forward * 100f;
		Vector3 projectedPoint	= Utils.Math.ProjectPointOnPlane( planeNormal, planePoint, point );
		Vector3 upwards			= ( projectedPoint - planePoint ).normalized;

		this.m_TopViewCamera.transform.rotation = Quaternion.LookRotation( Vector3.down, upwards );
	}


	//////////////////////////////////////////////////////////////////////////
	public	void	Show()
	{
		if (this.m_IsCompletedInitialization == false )
			return;

		this.m_IsVisible = true;

		const float alphaValue = 0.7333333333333333f;
		Color colorToAssign = Color.white;
		colorToAssign.a = alphaValue;
		this.m_RawImage.material.color = colorToAssign;
	}


	//////////////////////////////////////////////////////////////////////////
	public	void	Hide()
	{
		if (this.m_IsCompletedInitialization == false )
			return;

		this.m_IsVisible = false;
		this.m_RawImage.material.color = Color.clear;
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnDestroy()
	{
		Resources.UnloadAsset( this.m_MinimapRenderTexture );
		this.Show();
	}

}
