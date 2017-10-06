using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Foots : MonoBehaviour {

	private		LiveEntity		m_Parent		= null;
	public		LiveEntity	Parent {
		set { m_Parent = value; }
	}

	private void OnTriggerEnter( Collider other ) {
		
		if ( m_Parent != null ) {

		if ( other.tag == "Terrain" )
			m_Parent.Grounded = true;

		}

	}

	private void OnTriggerExit( Collider other ) {
		
		if ( other.tag == "Terrain" && m_Parent != null )
			m_Parent.Grounded = false;

	}


}
