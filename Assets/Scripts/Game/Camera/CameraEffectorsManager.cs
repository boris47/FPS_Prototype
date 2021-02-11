

using System.Collections.Generic;
using UnityEngine;

public	delegate	bool	EffectActiveCondition();

[System.Serializable]
public class CameraEffectorsManager
{
	[SerializeField]
	protected CameraEffectorData m_CameraEffectorData = new CameraEffectorData();
	public CameraEffectorData CameraEffectorsData  => m_CameraEffectorData;

	[SerializeField]
	protected List<CameraEffectBase> m_Effects = new List<CameraEffectBase>();


	//////////////////////////////////////////////////////////////////////////
	public void Add<T>( EffectActiveCondition condition ) where T : CameraEffectBase, new()
	{
		if (m_Effects.Exists( e => e is T ) )
			return;

		T newEffect = new T();
		newEffect.Setup( condition );
		m_Effects.Add( newEffect );
	}


	//////////////////////////////////////////////////////////////////////////
	public CameraEffectorData? GetEffectorData<T>() where T: CameraEffectBase, new()
	{
		CameraEffectBase effector = m_Effects.Find( e => e is T );
		if ( effector != null )
		{
			return effector.GetData();
		}
		return null;
	}


	//////////////////////////////////////////////////////////////////////////
	public void SetEffectorState<T>( bool newState ) where T: CameraEffectBase, new()
	{
		CameraEffectBase effector = m_Effects.Find( e => e is T );
		if ( effector != null )
		{
			effector.IsActive= newState;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public void SetAmplitudeMultiplier<T>( float newAmplitudeMultiplier ) where T : CameraEffectBase, new()
	{
		CameraEffectBase effector = m_Effects.Find( e => e is T );
		if ( effector != null )
		{
			effector.AmplitudeMult = newAmplitudeMultiplier;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public void SetSpeedMultiplier<T>( float newSpeedMultiplier ) where T : CameraEffectBase, new()
	{
		CameraEffectBase effector = m_Effects.Find( e => e is T );
		if ( effector != null )
		{
			effector.SpeedMul = newSpeedMultiplier;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public void	Update( float deltaTime )
	{
		m_CameraEffectorData.Reset();
		m_Effects.ForEach( e => e.Update( deltaTime, ref m_CameraEffectorData ) );
	}


	//////////////////////////////////////////////////////////////////////////
	public void Remove<T>() where T : CameraEffectBase, new()
	{
		m_Effects.RemoveAll( e => e is T );
	}


	//////////////////////////////////////////////////////////////////////////
	public void Reset()
	{
		m_Effects.Clear();

		m_CameraEffectorData = new CameraEffectorData();
	}
}