
using UnityEngine;


public class ControlledButton : MonoBehaviour, IInteractable {

	[SerializeField]
    private GameEvent           m_OnUse                = null;

	[SerializeField]
	private	bool	m_CanInteract						= true;
	bool IInteractable.CanInteract						{ get { return m_CanInteract; } set { m_CanInteract = value; } }



	void IInteractable.OnInteraction()
	{
		if ( m_OnUse != null && m_OnUse.GetPersistentEventCount() > 0 )
        {
            m_OnUse.Invoke();
        }
	}
}
