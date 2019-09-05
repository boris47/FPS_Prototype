using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Loading : MonoBehaviour, IStateDefiner {

	private		Slider		m_LoadingBar			= null;
	private		float		m_CurrentProgressValue	= 0.0f;


	private		bool		m_IsInitialized = false;
	bool IStateDefiner.IsInitialized
	{
		get { return m_IsInitialized; }
	}

	string IStateDefiner.StateName
	{
		get { return name; }
	}



	//////////////////////////////////////////////////////////////////////////
	IEnumerator IStateDefiner.Initialize()
	{
		yield return null;

		m_IsInitialized = transform.SearchComponent( ref m_LoadingBar, SearchContext.CHILDREN );
	}



	//////////////////////////////////////////////////////////////////////////
	IEnumerator IStateDefiner.ReInit()
	{
		yield return null;
	}



	//////////////////////////////////////////////////////////////////////////
	bool IStateDefiner.Finalize()
	{
		return m_IsInitialized;
	}



	//////////////////////////////////////////////////////////////////////////
	public	void	Show()
	{
		gameObject.SetActive(true);
	}



	//////////////////////////////////////////////////////////////////////////
	public	void	Hide()
	{
		gameObject.SetActive( false );
	}



	//////////////////////////////////////////////////////////////////////////
	private void OnEnable()
	{
		m_CurrentProgressValue = 0.0f;
	}



	//////////////////////////////////////////////////////////////////////////
	private void OnDisable()
	{
		m_CurrentProgressValue = 0.0f;
	}



	//////////////////////////////////////////////////////////////////////////
	public void ResetBar()
	{
		m_CurrentProgressValue = 0.0f;
		m_LoadingBar.value = 0.0f;
	}



	//////////////////////////////////////////////////////////////////////////
	public	void	SetProgress( float CurrentProgress )
	{
//		m_LoadingBar.value = CurrentProgress;
		m_CurrentProgressValue = CurrentProgress;
	}



	//////////////////////////////////////////////////////////////////////////
	private void LateUpdate()
	{
		m_LoadingBar.value = Mathf.MoveTowards( m_LoadingBar.value, m_CurrentProgressValue, Time.unscaledDeltaTime );
	}
}
