

using System.Collections.Generic;
using UnityEngine;

public	delegate	bool	EffectorActiveCondition();

[System.Serializable]
public sealed class CameraEffectorsManager
{
	[SerializeField, ReadOnly]
	private		CameraEffectorData								m_CameraEffectorData		= new CameraEffectorData();
	[SerializeField, ReadOnly]
	private		List<CameraEffectBase>							m_Effects					= new List<CameraEffectBase>();

	private		Dictionary<System.Type, CameraEffectBase>		m_EffectDictionaty			= new Dictionary<System.Type, CameraEffectBase>();

	public		CameraEffectorData								CameraEffectorsData			=> m_CameraEffectorData;


	//////////////////////////////////////////////////////////////////////////
	public void AddCondition<T>(EffectorActiveCondition condition) where T : CameraEffectBase, new()
	{
		if (!m_EffectDictionaty.ContainsKey(typeof(T)))
		{
			var newEffector = FPSEntityCamera.Instance.gameObject.AddComponent<T>();
			newEffector.Setup(condition);
			m_EffectDictionaty.Add(typeof(T), newEffector);

			/// Debug purpose
			m_Effects.Add(newEffector);
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public bool TryGetEffectorData<T>(out CameraEffectorData effectorData) where T : CameraEffectBase, new()
	{
		effectorData = default;
		if (m_EffectDictionaty.TryGetValue(typeof(T), out CameraEffectBase effector))
		{
			effectorData = effector.GetData();
			return true;
		}
		return false;
	}


	//////////////////////////////////////////////////////////////////////////
	public bool TrySetEffectorState<T>(bool newState) where T : CameraEffectBase, new()
	{
		if (m_EffectDictionaty.TryGetValue(typeof(T), out CameraEffectBase effector))
		{
			effector.IsActive = newState;
			return true;
		}
		return false;
	}


	//////////////////////////////////////////////////////////////////////////
	public bool TrySetAmplitudeMultiplier<T>(float newAmplitudeMultiplier) where T : CameraEffectBase, new()
	{
		if (m_EffectDictionaty.TryGetValue(typeof(T), out CameraEffectBase effector))
		{
			effector.AmplitudeMult = newAmplitudeMultiplier;
			return true;
		}
		return false;
	}


	//////////////////////////////////////////////////////////////////////////
	public bool TrySetSpeedMultiplier<T>(float newSpeedMultiplier) where T : CameraEffectBase, new()
	{
		if (m_EffectDictionaty.TryGetValue(typeof(T), out CameraEffectBase effector))
		{
			effector.SpeedMul = newSpeedMultiplier;
			return true;
		}
		return false;
	}


	//////////////////////////////////////////////////////////////////////////
	public void OnFrame(float deltaTime)
	{
		m_CameraEffectorData.Reset();

		foreach (CameraEffectBase effect in m_Effects)
		{
			effect.OnUpdate(deltaTime, m_CameraEffectorData);
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public void Remove<T>() where T : CameraEffectBase, new()
	{
		for (int i = m_Effects.Count - 1; i >= 0; i--)
		{
			var effect = m_Effects[i];
			if (effect is T)
			{
				Object.Destroy(effect);
				m_Effects.RemoveAt(i);
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public void Reset()
	{
		for (int i = m_Effects.Count - 1; i >= 0; i--)
		{
			var effect = m_Effects[i];
			Object.Destroy(effect);
			m_Effects.RemoveAt(i);
		}

		m_CameraEffectorData.Reset();
	}
}