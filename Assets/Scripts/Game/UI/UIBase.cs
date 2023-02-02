using UnityEngine;

public abstract class UI_Base : MonoBehaviour
{
	[SerializeField, ReadOnly]
	private Canvas m_Canvas = null;

	[SerializeField, ReadOnly]
	private CanvasGroup m_CanvasGroup = null;


	//--------------
	private Coroutine m_CurrentCoroutine = null;


	//////////////////////////////////////////////////////////////////////////
	private void Awake()
	{
		if (m_CanvasGroup.IsNull())
		{
			Utils.CustomAssertions.IsNotNull(m_CanvasGroup);
		}
	}

	//////////////////////////////////////////////////////////////////////////
	protected virtual void OnValidate()
	{
		if (transform is not RectTransform)
		{
			Utils.CustomAssertions.IsTrue(false, "Cannot add UI classes to non ui gameobject");
			Utils.Editor.Helpers.ScheduleEditorAction(() => this.Destroy());
		}
		else
		{
			if (gameObject.TryGetComponent(out m_Canvas))
			{
				if (!gameObject.TryGetComponent(out m_CanvasGroup))
				{
					m_CanvasGroup = gameObject.AddComponent<CanvasGroup>();
				}
			}
			else
			{
				Utils.CustomAssertions.IsTrue(false, $"{GetType().Name}: {gameObject.GetFullPath()} need a canvas component");
				Utils.Editor.Helpers.ScheduleEditorAction(() => this.Destroy());
			}
		}
	}

	//////////////////////////////////////////////////////////////////////////
	public void FadeIn(in float InTime)
	{
		Fade(this, InTime, bFadeIn: true);
	}

	//////////////////////////////////////////////////////////////////////////
	public void FadeOut(in float InTime)
	{
		Fade(this, InTime, bFadeIn: false);
	}

	//////////////////////////////////////////////////////////////////////////
	private static void Fade(UI_Base InUIBase, in float InTime, bool bFadeIn)
	{
		if (InUIBase.m_CanvasGroup.IsNotNull())
		{
			CoroutinesManager.Stop(InUIBase.m_CurrentCoroutine);
		}
		void Action(float interpolant)
		{
			InUIBase.m_CanvasGroup.alpha = bFadeIn ? interpolant : 1f - interpolant;
		}
		void OnEndAction()
		{
			InUIBase.m_CanvasGroup.alpha = bFadeIn ? 1f : 0f;
			InUIBase.m_CanvasGroup.interactable = bFadeIn;
		}
		InUIBase.m_CurrentCoroutine = CoroutinesManager.NewTimeBasedCoroutine(InTime, Action, OnEndAction);
	}
}