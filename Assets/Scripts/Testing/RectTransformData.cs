using UnityEngine;

public class RectTransformData : MonoBehaviour {

	public Vector2 anchoredPosition;
	public Vector3 anchoredPosition3D;
	public Vector2 anchorMin;
	public Vector2 anchorMax;
	public Rect rect;
	public Vector2 sizeDelta;

	private	RectTransform	m_RectTransform = null;
	private bool m_Initialized = false;

	private void Init()
	{
		if (m_Initialized )
			return;

		m_RectTransform = transform as RectTransform;
		m_Initialized = true;
	}

	private	void	UpdateInfo()
	{
		Init();

		anchoredPosition		= m_RectTransform.anchoredPosition;
		anchoredPosition3D		= m_RectTransform.anchoredPosition3D;

		anchorMin				= m_RectTransform.anchorMin;
		anchorMax				= m_RectTransform.anchorMax;

		rect					= m_RectTransform.rect;

		sizeDelta				= m_RectTransform.sizeDelta;
	}





	private void OnTransformChildrenChanged()
	{
		UpdateInfo();	
	}

	private void OnTransformParentChanged()
	{
		UpdateInfo();
	}

	private void OnBeforeTransformParentChanged()
	{
		UpdateInfo();
	}

	private void OnRectTransformDimensionsChange()
	{
		UpdateInfo();		
	}

	private void OnValidate()
	{
		UpdateInfo();
	}
}
