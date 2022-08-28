
using UnityEngine;
using Entities;

public interface IInteractor
{
	bool HasInteractableAvailable();
	Interactable GetCurrentInteractable();
	bool IsCurrentlyInteracting();
	bool CanInteractWith(Interactable interactable);
	void Interact(Interactable interactable);
	void StopInteraction();
}

public abstract class Interactable : MonoBehaviour
{
	public const string LayerName = "Interactables";

	[SerializeField]
	protected UnityEngine.Events.UnityEvent m_OnInteraction = null;

	/////////////////////////////////////////////////////////////////////////////
	protected virtual void Awake()
	{
		if (LayerMask.LayerToName(gameObject.layer) != LayerName)
		{
			Debug.Log($"{nameof(Interactable)}: Object {name} has {GetType().Name} component but layer is not {LayerName}, setting {LayerName} as layer!");
			gameObject.layer = LayerMask.NameToLayer(LayerName);
		}
	}

	/////////////////////////////////////////////////////////////////////////////
	protected virtual void OnValidate()
	{
		// using this approach because on layer assignatiion use SendMessage to broadcast the event and it appears that
		// "SendMessage cannot be called during Awake, CheckConsistency, or OnValidate" 
		// Ref: https://forum.unity.com/threads/sendmessage-cannot-be-called-during-awake-checkconsistency-or-onvalidate-can-we-suppress.537265/
		void _OnValidate()
		{
			UnityEditor.EditorApplication.update -= _OnValidate;
			if (this.IsNotNull())
			{
				gameObject.layer = LayerMask.NameToLayer(LayerName);
			}
		}
		UnityEditor.EditorApplication.update += _OnValidate;
	}

	public abstract bool CanInteract(Entity entity);
	public abstract void OnInteraction(Entity entity);
}
