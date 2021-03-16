
using UnityEngine;
using UnityEngine.UI;

public sealed class UI_Minimap : UI_Base, IStateDefiner
{
	private				Camera				m_TopViewCamera						= null;
	private				bool				m_IsVisible							= true;
	private				RawImage			m_RawImage							= null;
	private				RenderTexture		m_MinimapRenderTexture				= null;

	private				GameObject			m_CameraContainer					= null;

	private				RectTransform		m_MiniMapRectTransform				= null;
	private				RectTransform		m_HelperRectTransform				= null;
	private				Vector2				m_RatioVector						= Vector2.zero;

	private				bool				m_IsInitialized						= false;
						bool				IStateDefiner.IsInitialized			=> m_IsInitialized;

	[SerializeField, ReadOnly]
	private				Transform			m_Target							= null;

	[SerializeField]
	private				bool				m_FollowTargetRotation				= true;

	public				RectTransform		RawImageRect						=> m_MiniMapRectTransform;
	public				bool				IsVisible							=> m_IsVisible;
	public				bool				FollowTargetRotation				{ set => m_FollowTargetRotation = value; }



	//////////////////////////////////////////////////////////////////////////
	void IStateDefiner.PreInit()
	{

	}


	//////////////////////////////////////////////////////////////////////////
	void IStateDefiner.Initialize()
	{
		if (!m_IsInitialized)
		{
			if (m_CameraContainer.IsNotNull())
			{
				Object.Destroy(m_CameraContainer);
			}

			m_CameraContainer = new GameObject("TopViewCamera");
			m_CameraContainer.transform.position = Vector3.up * 100f;

			m_TopViewCamera = m_CameraContainer.AddComponent<Camera>();
			m_TopViewCamera.orthographic		= true;
			m_TopViewCamera.orthographicSize	= 32f;
			m_TopViewCamera.clearFlags			= CameraClearFlags.Depth;
			DontDestroyOnLoad(m_CameraContainer);

			m_TopViewCamera.allowMSAA			= false;
			m_TopViewCamera.useOcclusionCulling	= false;
			m_TopViewCamera.allowHDR			= false;
			m_TopViewCamera.farClipPlane		= m_CameraContainer.transform.position.y * 2f;

			if (CustomAssertions.IsTrue(transform.TrySearchComponent(ESearchContext.LOCAL_AND_CHILDREN, out m_RawImage)))
			{
				CustomAssertions.IsTrue(ResourceManager.LoadResourceSync("Textures/MinimapRenderTexture", out m_MinimapRenderTexture));

				m_TopViewCamera.targetTexture = m_RawImage.texture as RenderTexture;

				m_MiniMapRectTransform = m_RawImage.transform as RectTransform;

				m_HelperRectTransform = new GameObject("MinimapHelper").AddComponent<RectTransform>();
				m_HelperRectTransform.SetParent(m_MiniMapRectTransform, worldPositionStays: false );
				m_HelperRectTransform.anchorMin = Vector2.zero;
				m_HelperRectTransform.anchorMax = Vector2.zero;
				m_HelperRectTransform.gameObject.hideFlags = HideFlags.HideAndDontSave;

				m_RatioVector = new Vector2(m_MiniMapRectTransform.rect.width / m_TopViewCamera.pixelWidth, m_MiniMapRectTransform.rect.height / m_TopViewCamera.pixelHeight);
			}

			m_IsInitialized = true;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	void	IStateDefiner.ReInit()
	{
		
	}


	//////////////////////////////////////////////////////////////////////////
	bool	 IStateDefiner.Finalize()
	{
		return m_IsInitialized;
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnEnable()
	{
		CustomAssertions.IsNotNull(GameManager.UpdateEvents);

		GameManager.UpdateEvents.OnFrame += OnFrame;
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnDisable()
	{
		if (GameManager.UpdateEvents.IsNotNull())
		{
			GameManager.UpdateEvents.OnFrame -= OnFrame;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// Ref: http://answers.unity.com/answers/1461171/view.html
	public bool GetPositionInMinimapLocalSpace(in Vector3 worldPosition, out Vector2 screenPointInWorldSpace)
	{
		//first we get screnPoint in camera viewport space
		Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(m_TopViewCamera, worldPosition);

		// if render texture has different size of map rect size then we multiply by factor the scrrenPoint
		if (m_MinimapRenderTexture.width != m_MiniMapRectTransform.rect.width || m_MinimapRenderTexture.height != m_MiniMapRectTransform.rect.height)
		{
			// then transform it to position in worldImage using its rect
			screenPoint.x *= m_RatioVector.x;
			screenPoint.y *= m_RatioVector.y;
		}

		//after positioning helper to that spot
		m_HelperRectTransform.anchoredPosition = screenPoint;

		screenPointInWorldSpace = m_HelperRectTransform.position;

		return RectTransformUtility.RectangleContainsScreenPoint(m_MiniMapRectTransform, screenPointInWorldSpace);
	}


	//////////////////////////////////////////////////////////////////////////
	public	void	Show()
	{
		CustomAssertions.IsTrue(m_IsInitialized);

		m_IsVisible = true;

		const float alphaValue = 0.7333333333333333f;
		Color colorToAssign = Color.white;
		colorToAssign.a = alphaValue;
		m_RawImage.material.color = colorToAssign;
	}


	//////////////////////////////////////////////////////////////////////////
	public	void	Hide()
	{
		CustomAssertions.IsTrue(m_IsInitialized);

		m_IsVisible = false;
		m_RawImage.material.color = Color.clear;
	}


	//////////////////////////////////////////////////////////////////////////
	public void SetTarget(Transform target)
	{
		m_Target = target;
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnFrame(float deltaTime)
	{
		Vector3 upwards = Vector3.zero;
		Vector3 newCameraPosition = m_TopViewCamera.transform.position;

		if (m_Target.IsNotNull())
		{
			newCameraPosition.x = m_Target.position.x;
			newCameraPosition.z = m_Target.position.z;

			// Point in front of the target on the target plane
			Vector3 projectedPoint = Utils.Math.ProjectPointOnPlane(Vector3.up, m_Target.position, m_Target.position + m_Target.forward);

			if (m_FollowTargetRotation)
			{
				// Actually upwards is the current 'rotation' of the top view camera
				upwards = projectedPoint - m_Target.position;

			}
		}

		// Adding some smooth to camera rotation
		upwards = Vector3.MoveTowards(m_TopViewCamera.transform.up, upwards, Time.deltaTime);

		m_TopViewCamera.transform.SetPositionAndRotation(newCameraPosition, Quaternion.LookRotation(Vector3.down, upwards));
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnDestroy()
	{
		Resources.UnloadAsset(m_MinimapRenderTexture);

		//Show(); // Why ??
	}
}
