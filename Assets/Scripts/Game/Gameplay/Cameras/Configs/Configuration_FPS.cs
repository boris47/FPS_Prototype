using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "FPSCameraConfig", menuName = "Cameras Config/FPS")]
public class Configuration_FPS : ConfigurationBase
{
	[SerializeField]
	public				InputActionReference	LookAction					= null;

	[SerializeField]
	public				float					LookSensitivity				= 1f;
}

