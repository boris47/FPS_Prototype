using UnityEngine;
using System.Collections;

public class SoundEffectManager : MonoBehaviour {

	public static	SoundEffectManager	Instance = null;

	[SerializeField]
	private		float			m_MainVolume	= 1f;

	[SerializeField]
	private		float			m_MainPitch		= 1f;

	public		float			Volume
	{
		get { return m_MainVolume; }
		set
		{
			m_MainVolume = value;
			UpdateVolume( value );
		}
	}

	public		float			Pitch
	{
		get { return m_MainPitch; }
		set
		{
			m_MainPitch = value;
			UpdatePitch( value );
		}
	}

	private		AudioSource[]	m_Sources	= null;


	private void Awake()
	{
		Instance = this;
	}

	
	private	void	Start()
	{
		m_Sources = FindObjectsOfType<AudioSource>();
		for ( int i = 0; i < m_Sources.Length; i++ )
		{
			AudioSource source = m_Sources [ i ];
			if ( source == null )
				continue;

			source.volume = Volume;
			source.pitch = Pitch;
		}
	}

	
	private	void	UpdateVolume( float value )
	{
		for ( int i = 0; i < m_Sources.Length; i++ )
		{
			AudioSource source = m_Sources [ i ];
			if ( source == null )
				continue;

			source.volume = value;
		}
	}

	public	void	UpdatePitch( float value )
	{
		for ( int i = 0; i < m_Sources.Length; i++ )
		{
			AudioSource source = m_Sources [ i ];
			if ( source == null )
				continue;

			source.pitch = value;
		}
	}

}
