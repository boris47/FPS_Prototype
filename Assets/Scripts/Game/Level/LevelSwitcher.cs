
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

		if (enabled && other.transform.TrySearchComponent(ESearchContext.LOCAL_AND_PARENTS, out Player player))
		{
			CustomSceneManager.LoadSceneData loadSceneData = new CustomSceneManager.LoadSceneData()
			{
				eScene = m_NextSceneIdx
			};
			CustomSceneManager.LoadSceneAsync(loadSceneData);
			enabled = false;
		}
	}

}
