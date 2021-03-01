
using UnityEngine;

[System.Serializable]
public class CameraEffectorData
{
	/// <summary> </summary>
	public Vector3 CameraEffectsDirection;

	/// <summary> </summary>
	public Vector3 WeaponPositionDelta;

	/// <summary> </summary>
	public Vector3 WeaponDirectionDelta;


	public void Reset()
	{
		CameraEffectsDirection.Set( 0f, 0f, 0f );
		WeaponPositionDelta.Set( 0f, 0f, 0f );
		WeaponDirectionDelta.Set( 0f, 0f, 0f );
	}
}


[System.Serializable]
public abstract class CameraEffectBase
{
	protected			EffectorActiveCondition	m_EffectActiveCondition		= () => true;
	
	protected			float					m_WpnInfluence				= 0.01f;
	protected			float					m_AmplitudeBase				= 1.00f;
	protected			float					m_AmplitudeHoriz			= 1.00f;
	protected			float					m_AmplitudeVert				= 1.00f;
	protected			float					m_SpeedBase					= 1.00f;

	protected			float					m_Theta_Upd_Vert			= 5.00f;
	protected			float					m_Theta_Upd_Oriz			= 2.50f;

	protected			Vector3					m_Direction					= Vector3.zero;
	protected			Vector3					m_WeaponDirectionDelta		= Vector3.zero;
	protected			Vector3					m_WeaponPositionDelta		= Vector3.zero;

	public				bool					IsActive					= true;
	public				float					AmplitudeMult				= 1.0f;
	public				float					SpeedMul					= 1.0f;

	protected			float					m_ThetaX					= 0f;
	protected			float					m_ThetaY					= 90f;

	protected			CameraEffectorData		m_Data						= new CameraEffectorData();
	public				CameraEffectorData		GetData()					=> m_Data;


	//////////////////////////////////////////////////////////////////////////
	public abstract	void Setup( in EffectorActiveCondition condition );


	//////////////////////////////////////////////////////////////////////////
	public abstract	void Update( float deltaTime, CameraEffectorData data );

}
