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
			UnityEngine.Assertions.Assert.IsTrue
			(
				ResourceManager.LoadResourceSync(PostProcessResourcePath, out m_PP_Profile),
				"Failed the load of camera post processes profile"
			);

			UserSettings.VideoSettings.SetPPProfile(m_PP_Profile);
		}
		
		UnityEngine.Assertions.Assert.IsTrue
		(
			transform.TrySearchComponent(ESearchContext.LOCAL_AND_CHILDREN, out m_CameraRef),
		//	TryGetComponent(out m_CameraRef),
			"Cannot find unity camera component"
		);

		m_PostProcessingBehaviour = m_CameraRef.gameObject.GetOrAddIfNotFound<PostProcessingBehaviour>();
		m_PostProcessingBehaviour.profile = m_PP_Profile;
	}

	protected virtual void OnEnable()
	{
		UnityEngine.Assertions.Assert.IsNotNull(GameManager.UpdateEvents);

		GameManager.UpdateEvents.OnLateFrame += m_CameraEffectorsManager.Update;
	}

	protected virtual void OnDisable()
	{
		if (GameManager.UpdateEvents.IsNotNull())
		{
			GameManager.UpdateEvents.OnLateFrame -= m_CameraEffectorsManager.Update;
		}
	}

	protected virtual void OnDestroy()
	{

	}
}
