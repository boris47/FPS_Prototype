
using UnityEngine;


public class ControlledButton : Interactable {

	[SerializeField]
	private GameEvent		   m_OnUse				= null;



	public	override	 void	OnInteraction()
	{
		if (this.m_OnUse != null && this.m_OnUse.GetPersistentEventCount() > 0 )
		{
			this.m_OnUse.Invoke();
		}
	}
}
