
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public interface IFieldsOfViewManager {

	bool	RegisterAgent( FieldOfView agent, System.Action update );

	bool	IsRegistered( FieldOfView agent );

	bool	UnregisterAgent( FieldOfView agent );

};


public sealed partial class GameManager : IFieldsOfViewManager {

	private		static	IFieldsOfViewManager	m_FieldsOfViewManager = null;
	public		static	IFieldsOfViewManager	FieldsOfViewManager
	{
		get { return m_FieldsOfViewManager; }
	}

	private	List<KeyValuePair<FieldOfView, System.Action> > m_FieldsOfViewList = new List<KeyValuePair<FieldOfView, System.Action>>();

	private	int	m_CurrentFieldOfViewIndex = 0;


	bool IFieldsOfViewManager.RegisterAgent( FieldOfView agent, System.Action update )
	{
		if ( m_FieldsOfViewManager.IsRegistered( agent ) == true )
			return false;

		if ( update == null )
			return false;

		this.m_FieldsOfViewList.Add( 
			new KeyValuePair<FieldOfView, System.Action>( agent, update )
		);;
		return true;
	}



	bool IFieldsOfViewManager.IsRegistered( FieldOfView agent )
	{
		bool result = this.m_FieldsOfViewList.FindIndex((p) => p.Key== agent ) > -1;

		return result;
	}



	bool IFieldsOfViewManager.UnregisterAgent( FieldOfView agent )
	{
		int index = 0;
		if ( ( index = this.m_FieldsOfViewList.FindIndex((p) => p.Key== agent ) ) == -1 )
			return false;

		if ( index == this.m_CurrentFieldOfViewIndex )
			this.m_CurrentFieldOfViewIndex--;

		this.m_FieldsOfViewList.RemoveAt( index );
		return true;
	}



	private	void	UpdateCurrentFieldOfView()
	{
		if (this.m_FieldsOfViewList.Count == 0 )
			return;

		this.m_CurrentFieldOfViewIndex++;
		if (this.m_CurrentFieldOfViewIndex > this.m_FieldsOfViewList.Count - 1 )
		{
			this.m_CurrentFieldOfViewIndex = 0;
		}

		this.m_FieldsOfViewList[this.m_CurrentFieldOfViewIndex].Value();

		
	}

}

