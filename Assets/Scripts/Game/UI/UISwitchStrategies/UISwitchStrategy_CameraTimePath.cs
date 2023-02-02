using System.Collections;
using UnityEngine;
using Cinemachine;

/// <summary> Usable with world canvas </summary>
public class UISwitchStrategy_CameraTimePath : UISwitchStrategy
{
	[SerializeField]
	private Camera m_Camera = null;

	[SerializeField]
	private CinemachinePathBase m_CameraPath = null;

	[SerializeField, Min(float.Epsilon)]
	private float m_TransitionTimeSeconds = 1f;


	//////////////////////////////////////////////////////////////////////////
	private void OnValidate()
	{
		 gameObject.TryGetComponent(out m_CameraPath);
	}

	//////////////////////////////////////////////////////////////////////////
	public override IEnumerator ExecuteUISwitch(UI_Base InCurrentUI, UI_Base InUIToShow)
	{
		if (m_CameraPath)
		{
			void Action(float InInterpolant)
			{
				Vector3 cameraWorldPosition = m_CameraPath.EvaluatePosition(InInterpolant);
				Quaternion cameraWorldRotation = m_CameraPath.EvaluateOrientation(InInterpolant);
				m_Camera.transform.SetPositionAndRotation(cameraWorldPosition, cameraWorldRotation);
			}

			UI_Base uiToShow = Inverted ? InCurrentUI : InUIToShow;
			UI_Base uiToHide = Inverted ? InUIToShow : InCurrentUI;

			// Enable current active menu gameObject
			uiToShow.gameObject.SetActive(true);

			yield return CoroutinesManager.NewTimeBasedCoroutine(m_TransitionTimeSeconds, Action, null, Inverted);

			// Disable current active menu gameObject
			uiToHide.gameObject.SetActive(false);
		}
		else
		{
			Debug.LogError($"{gameObject.GetFullPath()}: {nameof(m_CameraPath)} is unaassigned");
			yield return DefaultInstant(InCurrentUI, InUIToShow);
		}
	}
}