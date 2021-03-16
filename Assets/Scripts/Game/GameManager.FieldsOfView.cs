
using System.Collections.Generic;


public interface IFieldsOfViewManager
{
	bool RegisterAgent(FieldOfView agent, System.Action update);

	bool IsRegistered(FieldOfView agent);

	bool UnregisterAgent(FieldOfView agent);
};


public sealed partial class GameManager : IFieldsOfViewManager
{
	public		static	IFieldsOfViewManager	FieldsOfViewManager => Instance;

	private	List<KeyValuePair<FieldOfView, System.Action> > m_FieldsOfViewList = new List<KeyValuePair<FieldOfView, System.Action>>();

	private	int	m_CurrentFieldOfViewIndex = 0;


	bool IFieldsOfViewManager.RegisterAgent( FieldOfView agent, System.Action update )
	{
		if (FieldsOfViewManager.IsRegistered( agent ) == true )
			return false;

		if ( update == null )
			return false;

		m_FieldsOfViewList.Add( 
			new KeyValuePair<FieldOfView, System.Action>( agent, update )
		);;
		return true;
	}



	bool IFieldsOfViewManager.IsRegistered( FieldOfView agent )
	{
		bool result = m_FieldsOfViewList.FindIndex((p) => p.Key== agent ) > -1;

		return result;
	}



	bool IFieldsOfViewManager.UnregisterAgent( FieldOfView agent )
	{
		print("removing" + agent.transform.parent.name);
		int index = 0;
		if ( ( index = m_FieldsOfViewList.FindIndex((p) => p.Key== agent ) ) > -1 )
			return false;

		if ( index == m_CurrentFieldOfViewIndex )
			m_CurrentFieldOfViewIndex--;

		m_FieldsOfViewList.RemoveAt( index );
		return true;
	}



	private	void	UpdateCurrentFieldOfView()
	{
		if (m_FieldsOfViewList.Count > 0)
		{
			m_CurrentFieldOfViewIndex++;
			if (m_CurrentFieldOfViewIndex > m_FieldsOfViewList.Count - 1 )
			{
				m_CurrentFieldOfViewIndex = 0;
			}

			m_FieldsOfViewList[m_CurrentFieldOfViewIndex].Value();
		}
	}
}

