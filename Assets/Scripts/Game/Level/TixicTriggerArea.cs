using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TixicTriggerArea : MonoBehaviour {

	private			TriggerEvents		m_TriggerEvents	 = null;

	private			bool				m_bIsActiveArea	= false;

	private void Awake()
	{
		m_bIsActiveArea = transform.SearchComponent( ref m_TriggerEvents, SearchContext.LOCAL );

		if ( m_bIsActiveArea )
		{
			m_TriggerEvents.OnEnterEvent += OnEnter;
			m_TriggerEvents.OnExitEvent += OnExit;
		}
	}

	private	void	OnEnter( GameObject go )
	{
		print( "TixicTriggerArea::OnEnter: entered " + go.name );
	}

	private	void	OnExit( GameObject go )
	{
		print( "TixicTriggerArea::OnExit: entered " + go.name );
	}
}
