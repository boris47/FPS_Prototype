using System.Collections;
using UnityEngine;
using Cinemachine;

/// <summary> Usable with world canvas </summary>
public class UISwitchStrategy_CameraSpeedPath : UISwitchStrategy
{
	[SerializeField]
	private Camera m_Camera = null;

	[SerializeField]
	private CinemachinePathBase m_CameraPath = null;

	[SerializeField, Min(float.Epsilon)]
	private float m_Speed = 1f;

	[SerializeField, ReadOnly]
	private float m_CurrentDistance = 0f;


	//////////////////////////////////////////////////////////////////////////
	private void OnValidate()
	{
		 gameObject.TryGetComponent(out m_CameraPath);
	}

	//////////////////////////////////////////////////////////////////////////
	public override IEnumerator ExecuteUISwitch(UI_Base InCurrentUI, UI_Base InUIToShow)
	{
		const CinemachinePathBase.PositionUnits kPositionUnits = CinemachinePathBase.PositionUnits.Distance;
		if (m_CameraPath)
		{
			float maxDistance = m_CameraPath.PathLength;

			UI_Base uiToShow = Inverted ? InCurrentUI : InUIToShow;
			UI_Base uiToHide = Inverted ? InUIToShow : InCurrentUI;

			bool Predicate()
			{
				if (Inverted)
				{
					return m_CurrentDistance > maxDistance;
				}
				else
				{
					return m_CurrentDistance < maxDistance;
				}
			}
			void Action()
			{
				Vector3 cameraWorldPosition = m_CameraPath.EvaluatePositionAtUnit(m_CurrentDistance, kPositionUnits);
				Quaternion cameraWorldRotation = m_CameraPath.EvaluateOrientationAtUnit(m_CurrentDistance, kPositionUnits);
				m_Camera.transform.SetPositionAndRotation(cameraWorldPosition, cameraWorldRotation);
			}
			void Next()
			{
				float newDistance = m_CurrentDistance + (Utils.Math.BoolToMinusOneOrPlusOne(Inverted) *  m_Speed * Time.deltaTime);
				m_CurrentDistance = m_CameraPath.StandardizeUnit(newDistance, kPositionUnits);
			}

			// Enable current active menu gameObject
			uiToShow.gameObject.SetActive(true);

			while (Predicate())
			{
				Action();
				yield return null;
				Next();
			}

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