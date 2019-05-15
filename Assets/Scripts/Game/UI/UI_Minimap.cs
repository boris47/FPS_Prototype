
using UnityEngine;
using UnityEngine.UI;

public class UI_Minimap : MonoBehaviour, IStateDefiner {

	private			Camera			m_TopViewCamera				= null;
	private			bool			m_bIsVisible				= true;
	private			RawImage		m_RawImage					= null;

	private			GameObject		m_CameraContainer			= null;


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

				m_CameraContainer = new GameObject();

				m_CameraContainer.transform.SetParent( transform );
				m_CameraContainer.transform.position = Vector3.zero;
				m_CameraContainer.transform.rotation = Quaternion.identity;

				m_TopViewCamera = m_CameraContainer.AddComponent<Camera>();
				m_TopViewCamera.orthographic = true;
				m_TopViewCamera.orthographicSize = 32;

				m_TopViewCamera.targetTexture = m_RawImage.texture as RenderTexture;
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
		m_TopViewCamera.transform.position = Player.Instance.transform.position + ( Vector3.up * 10f );
		
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
		const float alphaValue = 0.7333333333333333f;
		Color colorToAssign = Color.white;
		colorToAssign.a = alphaValue;
		m_RawImage.material.color = colorToAssign;
	}

}
