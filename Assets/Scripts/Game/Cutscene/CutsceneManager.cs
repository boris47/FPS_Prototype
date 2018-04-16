
using UnityEngine;


public class CutsceneManager : MonoBehaviour {
	
	public	static	CutsceneManager Instance			= null;

	private	CutsceneSequence		m_CurrentSequence	= null;
	public	CutsceneSequence		CurrentSequence
	{
		get { return m_CurrentSequence; }
		set
		{
//			if ( value == null && m_CurrentSequence != null && m_CurrentSequence.EndSequenceCallback != null )
			{
//				m_CurrentSequence.EndSequenceCallback();
			}
			m_CurrentSequence = value;
		}
	}

	private	bool					m_IsOK				= false;
	public	bool					IsOK
	{
		get { return m_IsOK; }
	}


	//////////////////////////////////////////////////////////////////////////
	// Awake
	private void Awake()
	{
		Instance = this;

		// Insert check here

		m_IsOK = true;
	}

	//////////////////////////////////////////////////////////////////////////
	// StopCutscene
	public	void	ShowInterface()
	{
		if ( m_IsOK == false )
			return;

//		m_CutsceneInterface.SetActive( true );
	}


	//////////////////////////////////////////////////////////////////////////
	// StopCutscene
	public	void	HideInterface()
	{
		if ( m_IsOK == false )
			return;

//		m_CutsceneInterface.SetActive( false );
	}

	//////////////////////////////////////////////////////////////////////////
	// InterruptCurrentSequence
	public	void	InterruptSequence()
	{
		if( m_IsOK && m_CurrentSequence != null )
		{
			m_CurrentSequence.InterruptSequence();
		}
	}


}
