using UnityEngine;
using System.Collections;

public class SoundEffectManager : MonoBehaviour {

	public static	SoundEffectManager	Instance = null;

	public		float			Volume		= 1.0f;
	public		float			Pitch		= 1.0f;

	private		AudioSource[]	m_Sources	= null;


	private void Awake()
	{
		Instance = this;
	}

	
	private	void Start()
	{
		m_Sources = FindObjectsOfType<AudioSource>();
	}

	
	private	void Update()
	{
		for ( int i = 0; i < m_Sources.Length; i++ )
		{
			AudioSource source = m_Sources [ i ];
			if ( source == null )
				continue;

			source.volume = Volume;
			source.pitch = Pitch;
		}
	}

}
