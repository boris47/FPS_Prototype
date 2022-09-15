
using UnityEngine;
using Entities;

public interface IInteractor
{
	bool HasInteractableAvailable();
	Interactable GetCurrentInteractable();
	bool IsCurrentlyInteracting();
	bool CanInteractWith(Interactable interactable);
	void InteractionStart();
	void InteractionEnd();
}
public enum EInteractionStages
{
	NONE,           // No interaction
	LOADING,        // The interactable need load time
	READY,			// The interactable is ready to be interacted with
	INTERACTING,	// The interactable is being interacted with right now
}

public abstract class Interactable : MonoBehaviour
{
	public const string LayerName = "Interactables";

	[SerializeField]
	protected UnityEngine.Events.UnityEvent m_OnInteractionStart = null;
	[SerializeField]
	protected UnityEngine.Events.UnityEvent m_OnInteractionRepeat = null;
	[SerializeField]
	protected UnityEngine.Events.UnityEvent m_InteractionEnd = null;

	[SerializeField, Min(0f), Tooltip("If greater than Zero equals to number of seconds of usage before trigger the real interaction")]
	private float m_LoadingTimeSeconds = 0f;

	[SerializeField, Tooltip("If true at interaction stop keep loaded time value")]
	private bool m_KeepLoadedTimeIfNotEnded = false;

	[SerializeField, Tooltip("If true repeat the reset internal interacted time after an interaction is completed enabling multiple interactions")]
	private bool m_RepeatInteraction = false;

	[SerializeField, ReadOnly]
	private float m_LoadedTimeSeconds = 0f;

	[SerializeField, ReadOnly]
	private EInteractionStages m_InteractionStage = EInteractionStages.NONE;

	// --------------------------
	public EInteractionStages InteractionStage => m_InteractionStage;
	public bool IsLoaded => m_LoadedTimeSeconds >= m_LoadingTimeSeconds;
	public bool NeedToBeLoaded => m_LoadedTimeSeconds < m_LoadingTimeSeconds;
	public bool IsLoading => Utils.Math.IsBetweenValues(m_LoadedTimeSeconds, 0f, m_LoadingTimeSeconds);
	public bool RepeatInteraction => m_RepeatInteraction;
	public bool HasLoadingTime => m_LoadingTimeSeconds > 0f;
	public float LoadingTimeSeconds => m_LoadingTimeSeconds;
	public float LoadedTimeSeconds => m_LoadedTimeSeconds;
	public float LoadedTimeNormalized
	{
		get
		{
			float outValue = 1f;
			if (m_LoadedTimeSeconds > 0f)
			{
				outValue = Mathf.Clamp01(m_LoadedTimeSeconds / m_LoadingTimeSeconds);
			}
			return outValue;
		}
	}


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
		m_LoadingTimeSeconds = Mathf.Max(m_LoadingTimeSeconds, 0f);

		m_InteractionStage = HasLoadingTime ? EInteractionStages.NONE : EInteractionStages.READY;

#if UNITY_EDITOR
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
#endif
	}

	/////////////////////////////////////////////////////////////////////////////
	public abstract bool CanInteract(Entity entity);

	/////////////////////////////////////////////////////////////////////////////
	public void EvaluateRepeat(Entity interactor, float deltaTime)
	{
		if (m_RepeatInteraction && HasLoadingTime)
		{
			if (IsLoaded)
			{
				OnInteractionRepeatedInternal(interactor);
				m_LoadedTimeSeconds = 0f;
			}
			else
			{
				m_LoadedTimeSeconds = Mathf.Clamp(m_LoadedTimeSeconds + Mathf.Max(deltaTime, 0f), 0f, m_LoadingTimeSeconds);
			}
		}
	}

	/////////////////////////////////////////////////////////////////////////////
	public void Load(float deltaTime)
	{
		if (HasLoadingTime)
		{
			if (NeedToBeLoaded)
			{
				m_LoadedTimeSeconds = Mathf.Clamp(m_LoadedTimeSeconds + Mathf.Max(deltaTime, 0f), 0f, m_LoadingTimeSeconds);
				if (IsLoaded)
				{
					m_InteractionStage = EInteractionStages.READY;
				}
				else
				{
					m_InteractionStage = EInteractionStages.LOADING;
				}
			}
		}
		else
		{
			m_InteractionStage = EInteractionStages.READY;
		}
	}

	/////////////////////////////////////////////////////////////////////////////
	public void AbortLoad()
	{
		if (!m_KeepLoadedTimeIfNotEnded)
		{
			m_LoadedTimeSeconds = 0f;
			m_InteractionStage = EInteractionStages.NONE;
		}
	}

	/////////////////////////////////////////////////////////////////////////////
	public void OnInteractionStart(Entity interactor)
	{
		if (HasLoadingTime && NeedToBeLoaded)
		{
			m_InteractionStage = EInteractionStages.LOADING;
		}
		else
		{
			OnInteractionStartInternal(interactor);
			m_InteractionStage = EInteractionStages.INTERACTING;
		}

		if (m_RepeatInteraction)
		{
			m_LoadedTimeSeconds = 0f;
		}
	}

	/////////////////////////////////////////////////////////////////////////////
	public void OnInteractionEnd(Entity interactor)
	{
		AbortLoad();

		// If value should be kept and interaction is completed, on interaction end reset loaded value
		if (m_KeepLoadedTimeIfNotEnded && HasLoadingTime && m_InteractionStage >= EInteractionStages.READY)
		{
			m_InteractionStage = EInteractionStages.NONE;
			m_LoadedTimeSeconds = 0f;
		}

		OnInteractionEndInternal(interactor);
	}

	/////////////////////////////////////////////////////////////////////////////
	protected abstract void OnInteractionStartInternal(Entity interactor);
	/////////////////////////////////////////////////////////////////////////////
	protected abstract void OnInteractionRepeatedInternal(Entity interactor);
	/////////////////////////////////////////////////////////////////////////////
	protected abstract void OnInteractionEndInternal(Entity interactor);
}
