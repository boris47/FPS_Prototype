
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class LevelSwitcher : MonoBehaviour
{
	[SerializeField]
	private		ESceneEnumeration				m_NextSceneIdx				= ESceneEnumeration.NONE;


	//////////////////////////////////////////////////////////////////////////
	private void OnTriggerEnter(Collider other)
	{
		if (GlobalManager.bIsChangingScene)
			return;

		if (other.transform.HasComponent<Player>())
		{
			CustomSceneManager.LoadSceneData loadSceneData = new CustomSceneManager.LoadSceneData()
			{
				eScene = m_NextSceneIdx
			};
			CustomSceneManager.LoadSceneAsync(loadSceneData);
		}
	}

}
