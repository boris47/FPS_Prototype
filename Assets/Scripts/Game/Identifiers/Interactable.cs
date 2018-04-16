
using UnityEngine;


public interface IInteractable {

	bool	CanInteract { get; set; }

	void	OnInteraction();

}