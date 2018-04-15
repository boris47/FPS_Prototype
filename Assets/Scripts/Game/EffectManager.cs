using UnityEngine;
using System.Collections;

public class EffectManager : MonoBehaviour {

	public	static	EffectManager	Instance					= null;

	[ SerializeField ]
	private		ParticleSystem		m_ParticleSystemOnHit		= null;




	//////////////////////////////////////////////////////////////////////////
	// Awake
	private void	Awake()
	{
		if ( m_ParticleSystemOnHit == null )
			return;

		if ( Instance != null )
		{
			Destroy( gameObject );
			return;
		}
		Instance = this;
		DontDestroyOnLoad( this );
	}


	//////////////////////////////////////////////////////////////////////////
	// PlayOnHit
	public	void	PlayOnHit( Vector3 position, Vector3 direction )
	{
		m_ParticleSystemOnHit.transform.position = position;
		m_ParticleSystemOnHit.transform.forward = direction;
		m_ParticleSystemOnHit.Emit( 3 );
	}

}
