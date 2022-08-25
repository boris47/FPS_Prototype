using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "FPSCameraConfig", menuName = "Cameras Config/FPS")]
public class Configuration_FPS : ConfigurationBase
{
	private const		float					k_MaxCameraBound = 89f;

	[SerializeField]
	public				InputActionReference	LookAction					= null;

	[SerializeField]
	public				float					LookSensitivity				= 1f;

	[SerializeField][Range(10f, k_MaxCameraBound)]
	public				float					UpCameraRotationBound		= 75f;

	[SerializeField][Range(-10f, -k_MaxCameraBound)]
	public				float					DownCameraRotationBound		= -75f;
}

