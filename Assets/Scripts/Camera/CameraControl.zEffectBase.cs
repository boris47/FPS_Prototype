using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CameraEffectBase {
	
	protected	float	m_SpeedMul			= 1.0f;
	protected	float	m_AmplitudeMult		= 1.0f;
	protected	float	m_InternalWeight	= 0f;
	protected	bool	m_IsActive			= true;
	public		bool	IsActive
	{
		get { return m_IsActive; }
		set { m_IsActive = value; }
	}

}
