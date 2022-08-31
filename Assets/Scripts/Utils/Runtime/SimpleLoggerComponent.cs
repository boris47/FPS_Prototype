﻿using UnityEngine;

public class SimpleLoggerComponent : MonoBehaviour
{
	public void Log(string msg) => Debug.Log(msg);

	public void LogWarning(string msg) => Debug.LogWarning(msg);

	public void LogError(string msg) => Debug.LogError(msg);
}
