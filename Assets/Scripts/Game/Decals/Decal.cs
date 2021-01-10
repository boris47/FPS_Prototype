using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Decal : MonoBehaviour
{
	private Renderer m_Renderer = null;

	private void Awake()
	{
		if (!gameObject.TryGetComponent(out m_Renderer))
		{

		}

	}

	public void SetDecal(Material decalMaterial)
	{
		m_Renderer.material = decalMaterial;
	}


	public void Show(Vector3 worldPosition, Quaternion worldRotation, float decalLifeTime)
	{
		transform.SetPositionAndRotation(worldPosition, worldRotation);
		m_Renderer.enabled = true;
		TimersManager.Instance.AddTimerScaled(decalLifeTime, () =>
		{
			m_Renderer.enabled = false;
		});
	}

	public void Hide()
	{
		m_Renderer.enabled = false;
	}
}
