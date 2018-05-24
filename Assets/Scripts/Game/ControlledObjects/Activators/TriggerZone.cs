
using UnityEngine;


[RequireComponent( typeof( Collider ) )]
public class TriggerZone : MonoBehaviour {
	
	
	private	ObjectActivator		m_ObjectActivator = null;
	public	ObjectActivator		ObjectActivator
	{
		set { m_ObjectActivator = value; }
	}

	public	OnTriggerCall		m_OnTriggerCallBack = null;


	//////////////////////////////////////////////////////////////////////////
	// OnTriggerEnter
	private void OnTriggerEnter( Collider other )
	{
		print( other.name );
		if ( other == m_ObjectActivator.Collider )
		{
			if ( m_OnTriggerCallBack != null )
			{
				m_OnTriggerCallBack( other );
			}
		}
	}

}
