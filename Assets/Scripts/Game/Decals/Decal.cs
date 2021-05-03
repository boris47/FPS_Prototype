using UnityEngine;


public class Decal : MonoBehaviour
{
	[SerializeField, ReadOnly]
	private			Renderer			m_Renderer			= null;


	//////////////////////////////////////////////////////////////////////////
	private void Awake()
	{
		if (!gameObject.TryGetComponent(out m_Renderer))
		{
			Debug.Log($"Cannot get renderer component");
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public void SetDecalMaterial(Material decalMaterial)
	{
		m_Renderer.material = decalMaterial;
	}


	//////////////////////////////////////////////////////////////////////////
	public void Show(Vector3 worldPosition, Quaternion worldRotation, float decalLifeTime)
	{
		transform.SetPositionAndRotation(worldPosition, worldRotation);
		gameObject.SetActive(true);
		TimersManager.Instance.AddTimerScaled(decalLifeTime, Hide);
	}


	//////////////////////////////////////////////////////////////////////////
	public void Hide()
	{
		gameObject.SetActive(false);
	}
}
