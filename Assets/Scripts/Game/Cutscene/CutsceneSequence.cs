
using UnityEngine;


public class CutsceneSequence : MonoBehaviour {
	
	public	GameEvent					m_OnSequnceFinished		= null;
	private	global::System.Action		m_EndSequenceCallback	= null;
	public	global::System.Action		EndSequenceCallback
	{
		get { return m_EndSequenceCallback; }
	}

	private	bool						m_IsCompleted			= false;
	public	bool						IsCompleted
	{
		get { return m_IsCompleted; }
	}

	[SerializeField]
	private	SequencePair[]				m_SequenceFrames		= null;

	private	SequencePair				m_CurrentFramePair		= null;
	private	int							m_CurrentItemIdx		= 0;


	//////////////////////////////////////////////////////////////////////////
	// Build
	public CutsceneSequence Build( SequencePair[] sequenceFrames, global::System.Action endSequenceCallback = null )
	{
		m_SequenceFrames = sequenceFrames;
		m_EndSequenceCallback = endSequenceCallback;
		return this;
	}


	//////////////////////////////////////////////////////////////////////////
	// SetEndCallback
	public CutsceneSequence	SetEndCallback( global::System.Action action )
	{
		m_EndSequenceCallback = action;
		return this;
	}


	//////////////////////////////////////////////////////////////////////////
	// Play
	public	void	Play()
	{

		// Sanity checks
		if ( ( m_SequenceFrames == null || m_SequenceFrames.Length == 0 )	// no items
		||	CutsceneManager.Instance.IsOK == false							// CutsceneManager is not avaiable
//			||	m_IsCompleted == true											// already executed
		)
		{
			if ( m_EndSequenceCallback != null )
				m_EndSequenceCallback.Invoke();
			return;
		}

		// If another sequence is playing, stop it
		if ( CutsceneManager.Instance.CurrentSequence != null )
			CutsceneManager.Instance.CurrentSequence.InterruptSequence();

		// set first frame as current in play
		m_CurrentItemIdx = 0;

		SequencePair sequncePair = m_SequenceFrames[ m_CurrentItemIdx ];

//		GameManager.instance.ChangeState( GAMESTATE.CUTSCENE );

		CutsceneManager.Instance.CurrentSequence = this;

		CutsceneManager.Instance.ShowInterface();
//		CutsceneManager.Instance.InterfaceSpeaker.text	= m_CurrentFramePair.SequenceFrame.SpeakerName;
/*
		// set current audioRef for play
		if ( m_CurrentFramePair.SequenceFrame.AudioRef != null && m_CurrentFramePair.SequenceFrame.AudioRef.Length > 0 )
		{
			// set audioclip for audiosource
			m_CurrentAudioSource	= CutsceneManager.Instance.GetEventEmitter( Source );
            //m_CurrentAudioSource.Stop();
            //m_CurrentAudioSource.Event = m_CurrentFramePair.SequenceFrame.AudioRef;
            //m_CurrentAudioSource.Play();

            if (m_RuntimeAudioSource)
            { 
                m_RuntimeAudioSource.Stop();
                Destroy(m_RuntimeAudioSource);
            }
            m_RuntimeAudioSource = m_CurrentAudioSource.gameObject.AddComponent<StudioEventEmitter>();
            m_RuntimeAudioSource.Event = m_CurrentFramePair.SequenceFrame.AudioRef;
            m_RuntimeAudioSource.Play();
		}
		*/
	}

	private	void	NextFrame()
	{

		// stop all coroutines
		StopAllCoroutines();

//		if (  m_RuntimeAudioSource.IsPlaying() )
//				m_RuntimeAudioSource.Stop();
			
		// set current frame as executed
		if ( m_SequenceFrames[ m_CurrentItemIdx ].OnFrameEnd != null )
			m_SequenceFrames[ m_CurrentItemIdx ].OnFrameEnd.Invoke();

		m_SequenceFrames[ m_CurrentItemIdx ].SequenceFrame.IsExecuted = true;

		// increase index
		m_CurrentItemIdx ++;

		// sequence is finished
		if ( m_CurrentItemIdx == m_SequenceFrames.Length )
		{
			// hide interface
			CutsceneManager.Instance.HideInterface();

			// set this sequence as finished
			m_IsCompleted = true;

			// Set current game state to INGAME state
//			GameManager.instance.ChangeState( GAMESTATE.INGAME );

			// remove reference to this sequence
			CutsceneManager.Instance.CurrentSequence = null;

			// On Sequance Finished callback
			if ( m_OnSequnceFinished != null )
				m_OnSequnceFinished.Invoke();
			return;
		}

		// set next frame as current in play
		m_CurrentItemIdx ++;
		SequencePair sequncePair = m_SequenceFrames[ m_CurrentItemIdx ];
		

		// set current frame speaker and text
//		CutsceneManager.Instance.InterfaceSpeaker.text	= m_CurrentFramePair.SequenceFrame.SpeakerName;
//		StartCoroutine( WriteTextCO( CutsceneManager.Instance.InterfaceMessage, m_CurrentFramePair.SequenceFrame.Text ) );

		// set current audioRef for play
		if ( m_CurrentFramePair.SequenceFrame.AudioRef != null && m_CurrentFramePair.SequenceFrame.AudioRef.Length > 0 )
		{
			// set audioclip for audiosource
///			m_CurrentAudioSource	= CutsceneManager.Instance.GetEventEmitter( Source );
            //m_CurrentAudioSource.Stop();
            //m_CurrentAudioSource.Event = m_CurrentFramePair.SequenceFrame.AudioRef;
            //m_CurrentAudioSource.Play();
/*
            if (m_RuntimeAudioSource)
            {
                m_RuntimeAudioSource.Stop();
                Destroy(m_RuntimeAudioSource);
            }
            m_RuntimeAudioSource = m_CurrentAudioSource.gameObject.AddComponent<StudioEventEmitter>();
            m_RuntimeAudioSource.Event = m_CurrentFramePair.SequenceFrame.AudioRef;
            m_RuntimeAudioSource.Play();
*/        }

	}


	//////////////////////////////////////////////////////////////////////////
	// InterruptSequence
	public	void	InterruptSequence()
	{
		if ( ( m_SequenceFrames == null || m_SequenceFrames.Length == 0 ) )
			return;

		// stop all coroutines
		StopAllCoroutines();

		// hide interface
		CutsceneManager.Instance.HideInterface();

/*        //m_CurrentAudioSource.Stop();
        m_RuntimeAudioSource.Stop();
        Destroy(m_RuntimeAudioSource);
*/
		// set this sequence as finished
		m_IsCompleted = true;
			
		// execute all remaining actions
		for ( int i = m_CurrentItemIdx+1; i < m_SequenceFrames.Length; i++ )
		{
			if ( m_SequenceFrames[ i ].OnFrameEnd != null )
				m_SequenceFrames[ i ].OnFrameEnd.Invoke();
		}
			
		// On Seqquence Interruption callback
//			if ( m_OnSequnceInterrupted != null )
//				m_OnSequnceInterrupted.Invoke( m_CurrentAudioSource.transform.parent.gameObject );
			
		// Set current game state to INGAME state
//		GameManager.instance.ChangeState( GAMESTATE.INGAME );

		// remove reference to this sequence
		CutsceneManager.Instance.CurrentSequence = null;
	}

}
