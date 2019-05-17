
using UnityEngine;
using UnityEngine.UI;

public class UI_Minimap : MonoBehaviour, IStateDefiner {

	private			Camera			m_TopViewCamera				= null;
	private			bool			m_bIsVisible				= true;
	private			RawImage		m_RawImage					= null;

	private			GameObject		m_CameraContainer			= null;

	
	public	Camera	GetTopViewCamera()
	{
		return m_TopViewCamera;
	}

	public	Rect	GetRawImageRect()
	{
		RectTransform rectTransform = m_RawImage.transform.transform as RectTransform;
		return rectTransform.rect;
	}

	public	Vector2 GetRawImagePosition()
	{
		return m_RawImage.transform.position;
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
				RenderTexture minimapRenderTexture = data.Asset;

				if ( m_CameraContainer != null )
				{
					Object.Destroy( m_CameraContainer );
				}

				m_CameraContainer = new GameObject("TopViewCamera");
				m_CameraContainer.transform.position = Vector3.up * 100f;

				m_TopViewCamera = m_CameraContainer.AddComponent<Camera>();
				m_TopViewCamera.orthographic		= true;
				m_TopViewCamera.orthographicSize	= 32f;

				m_TopViewCamera.allowMSAA			= false;
				m_TopViewCamera.useOcclusionCulling	= false;
				m_TopViewCamera.allowHDR			= false;
				m_TopViewCamera.farClipPlane		= m_CameraContainer.transform.position.y * 2f;
				m_TopViewCamera.targetTexture		= m_RawImage.texture as RenderTexture;
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
