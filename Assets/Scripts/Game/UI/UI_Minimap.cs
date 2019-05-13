
using UnityEngine;
using UnityEngine.UI;

public class UI_Minimap : MonoBehaviour, IStateDefiner {

	private			Camera			m_TopViewCamera				= null;
	private			Canvas			m_Canvas					= null;
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
			m_bIsInitialized &= transform.SearchComponentInChild( "RawImage", ref m_RawImage );


			ResourceManager.LoadData<RenderTexture> data = new ResourceManager.LoadData<RenderTexture>();
			if ( ResourceManager.LoadResourceSync( "Textures/MinimapRenderTexture", data ) )
			{
				RenderTexture minimapRenderTexture = data.Asset;

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
	private void Update()
	{
		m_TopViewCamera.transform.position = Player.Instance.transform.position + ( Vector3.up * 10f );
		m_TopViewCamera.transform.rotation = Quaternion.LookRotation( Vector3.down, Vector3.forward );
	}


	//////////////////////////////////////////////////////////////////////////
	public	void	Show()
	{

	}


	//////////////////////////////////////////////////////////////////////////
	public	void	Hide()
	{

	}

	
}
