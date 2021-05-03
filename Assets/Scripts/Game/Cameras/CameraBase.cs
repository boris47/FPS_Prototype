using UnityEngine;
using UnityEngine.PostProcessing;

public abstract class CameraBase : MonoBehaviour
{
	protected	static PostProcessingProfile	m_PP_Profile							= null;

	[Header("Camera Base")]
	
	[SerializeField]
	protected	CameraEffectorsManager			m_CameraEffectorsManager				= new CameraEffectorsManager();

	public		PostProcessingProfile			PP_Profile								=> m_PP_Profile;
	public		Camera							MainCamera								=> m_CameraRef;
	public		CameraEffectorsManager			CameraEffectorsManager					=> m_CameraEffectorsManager;

	protected	PostProcessingBehaviour			m_PostProcessingBehaviour				= null;
	protected	Camera							m_CameraRef								= null;

	protected abstract string					PostProcessResourcePath					{ get; }


	protected virtual void Awake()
	{
		if (!m_PP_Profile)
		{
			CustomAssertions.IsTrue
			(
				ResourceManager.LoadResourceSync(PostProcessResourcePath, out m_PP_Profile),
				"Failed the load of camera post processes profile"
			);

			UserSettings.VideoSettings.SetPPProfile(m_PP_Profile);
		}
		
		CustomAssertions.IsTrue
		(
			transform.TrySearchComponent(ESearchContext.LOCAL_AND_CHILDREN, out m_CameraRef),
		//	TryGetComponent(out m_CameraRef),
			"Cannot find unity camera component"
		);

		m_PostProcessingBehaviour = m_CameraRef.gameObject.GetOrAddIfNotFound<PostProcessingBehaviour>();
		m_PostProcessingBehaviour.profile = m_PP_Profile;

		/*
		CustomAssertions.IsNotNull(GameManager.StreamEvents);

		GameManager.StreamEvents.OnSave += OnSave;
		GameManager.StreamEvents.OnLoad += OnLoad;
		*/
	}

	protected virtual void OnEnable()
	{
		CustomAssertions.IsNotNull(GameManager.UpdateEvents);

		GameManager.UpdateEvents.OnFrame += m_CameraEffectorsManager.OnFrame;
	}

	protected virtual void OnDisable()
	{
		if (GameManager.UpdateEvents.IsNotNull())
		{
			GameManager.UpdateEvents.OnFrame -= m_CameraEffectorsManager.OnFrame;
		}
	}

	protected virtual void OnDestroy()
	{
		/*	if (GameManager.StreamEvents.IsNotNull())
		{
			GameManager.StreamEvents.OnSave -= OnSave;
			GameManager.StreamEvents.OnLoad -= OnLoad;
		}
		*/
	}
}
