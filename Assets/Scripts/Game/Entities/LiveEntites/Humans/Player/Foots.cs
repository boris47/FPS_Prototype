using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IFoots {

	void PlayStep();

}


public class Foots : MonoBehaviour, IFoots {

	private		LiveEntity		m_Parent		= null;
	public		LiveEntity	Parent {
		set { m_Parent = value; }
	}

	private	AudioSource pAudioSource = null;

	private	Collider CurrentCollider = null;
	private	Vector3	CurrentCollPoint = Vector3.zero;


	private void Start() {
		
		pAudioSource = GetComponent<AudioSource>();

	}

	public	void	PlayStep() {

		if ( CurrentCollider == null ) return;

		AudioClip randomFootstep = SurfaceManager.Instance.GetFootstep( CurrentCollider, CurrentCollPoint );
		if(randomFootstep) {
			if ( pAudioSource.isPlaying ) pAudioSource.Stop();
			pAudioSource.clip = randomFootstep;
			pAudioSource.Play();

		}

	}


	private void OnTriggerEnter( Collider other ) {
		
		CurrentCollider = other;
		CurrentCollPoint = transform.position;

		if ( m_Parent != null ) {

//		if ( other.tag == "Terrain" )
			m_Parent.Grounded = true;

		}

	}

	private void OnTriggerExit( Collider other ) {
		
		CurrentCollider = null;

		if ( m_Parent != null ) {

//			if ( other.tag == "Terrain" )
				m_Parent.Grounded = false;

		}

	}


}
