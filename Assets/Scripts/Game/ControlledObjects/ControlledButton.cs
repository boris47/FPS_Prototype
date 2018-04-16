
using UnityEngine;


public class ControlledButton : ControlledObject, IInteractable {

	 [SerializeField]
    private GameEvent           m_OnUse                = null;


	bool IInteractable.CanInteract						{ get; set; }



	public override void OnActivation()
	{
		if ( m_OnUse != null && m_OnUse.GetPersistentEventCount() > 0 )
        {
            m_OnUse.Invoke();
        }
	}

	void IInteractable.OnInteraction()
	{
		throw new System.NotImplementedException();
	}
}
