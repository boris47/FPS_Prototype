using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Foots : MonoBehaviour {

	public		Entity		m_Parent		= null;

	private void OnTriggerEnter( Collider other ) {
		
		if ( other.tag == "Terrain" && m_Parent.IsLiveEntity() )
			m_Parent.GetAsLiveEntity().Grounded = true;

	}

	private void OnTriggerExit( Collider other ) {
		
		if ( other.tag == "Terrain" && m_Parent.IsLiveEntity() )
			m_Parent.GetAsLiveEntity().Grounded = false;

	}


}
