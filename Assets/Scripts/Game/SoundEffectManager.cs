
using UnityEngine;
using System.Collections.Generic;

public interface ISoundEffectManager {

	float		Volume				{ get; set; }
	float		Pitch				{ get; set; }

	void		RegisterSource		( ref AudioSource audioSource );
	void		UnRegisterSource	( ref AudioSource audioSource );

	void		UpdateVolume		( float value );
	void		UpdatePitch			( float value );
}

[ExecuteInEditMode]
public class SoundEffectManager : MonoBehaviour, ISoundEffectManager {

	public static		ISoundEffectManager		Instance		= null;

	[SerializeField]
	private				float					m_MainVolume	= 1f;

	[SerializeField]
	private				float					m_MainPitch		= 1f;

	public				float					Volume
	{
		get { return m_MainVolume; }
		set
		{
			m_MainVolume = value;
			UpdateVolume( value );
		}
	}

	public				float					Pitch
	{
		get { return m_MainPitch; }
		set
		{
			m_MainPitch = value;
			UpdatePitch( value );
		}
	}

	private		List<AudioSource>				m_Sources		= new List<AudioSource>();



	//////////////////////////////////////////////////////////////////////////
	// Awake
	private void Awake()
	{
		Instance = this as ISoundEffectManager;
	}


	//////////////////////////////////////////////////////////////////////////
	// RegisterSource
	public	void	RegisterSource( ref AudioSource audioSource )
	{
		if ( audioSource == null )
			return;

		if ( m_Sources.Contains( audioSource ) == true )
			return;

		m_Sources.Add( audioSource );
	}


	//////////////////////////////////////////////////////////////////////////
	// UnRegisterSource
	public	void	UnRegisterSource( ref AudioSource audioSource )
	{
		if ( audioSource == null )
		{
			Debug.LogError( "SoundEffectManager::UnRegisterSource: Trying to unregister a non registered AudioSource !!" );
			return;
		}

		if ( m_Sources.Contains( audioSource ) == false )
			return;

		m_Sources.Remove( audioSource );
	}
	

	//////////////////////////////////////////////////////////////////////////
	// UpdateVolume
	public	void	UpdateVolume( float value )
	{
		for ( int i = m_Sources.Count - 1; i > 0; i-- )
		{
			AudioSource source = m_Sources [ i ];
			if ( source == null )
			{
				m_Sources.RemoveAt( i );
				continue;
			}

			source.volume = value;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// UpdatePitch
	public	void	UpdatePitch( float value )
	{
		for ( int i = m_Sources.Count - 1; i > 0; i-- )
		{
			AudioSource source = m_Sources [ i ];
			if ( source == null )
			{
				m_Sources.RemoveAt( i );
				continue;
			}

			source.pitch = value;
		}
	}

}
