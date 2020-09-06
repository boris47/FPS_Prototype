using System.Collections;
using System.Collections.Generic;
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
		if (this.m_Initialized )
			return;

		this.m_RectTransform = this.transform as RectTransform;
		this.m_Initialized = true;
	}

	private	void	UpdateInfo()
	{
		this.Init();

		this.anchoredPosition		= this.m_RectTransform.anchoredPosition;
		this.anchoredPosition3D		= this.m_RectTransform.anchoredPosition3D;

		this.anchorMin				= this.m_RectTransform.anchorMin;
		this.anchorMax				= this.m_RectTransform.anchorMax;

		this.rect					= this.m_RectTransform.rect;

		this.sizeDelta				= this.m_RectTransform.sizeDelta;
	}





	private void OnTransformChildrenChanged()
	{
		this.UpdateInfo();	
	}

	private void OnTransformParentChanged()
	{
		this.UpdateInfo();
	}

	private void OnBeforeTransformParentChanged()
	{
		this.UpdateInfo();
	}

	private void OnRectTransformDimensionsChange()
	{
		this.UpdateInfo();		
	}

	private void OnValidate()
	{
		this.UpdateInfo();
	}
}
