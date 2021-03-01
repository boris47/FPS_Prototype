
using UnityEngine;


public class ControlledButton : Interactable {

	[SerializeField]
	private GameEvent		   m_OnUse				= null;



	public	override	 void	OnInteraction()
	{
		if (m_OnUse.IsNotNull() && m_OnUse.GetPersistentEventCount() > 0 )
		{
			m_OnUse.Invoke();
		}
	}
}
