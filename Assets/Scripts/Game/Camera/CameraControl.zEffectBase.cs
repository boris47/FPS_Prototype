
using UnityEngine;

[System.Serializable]
public struct CameraEffectorData
{
	/// <summary> </summary>
	public Vector3		CameraEffectsDirection;

	/// <summary> </summary>
	public Vector3		WeaponPositionDelta;

	/// <summary> </summary>
	public Vector3		WeaponRotationDelta;


	public void Reset()
	{
		CameraEffectsDirection.Set( 0f, 0f, 0f );
		WeaponPositionDelta.Set( 0f, 0f, 0f );
		WeaponRotationDelta.Set( 0f, 0f, 0f );
	}
}


[System.Serializable]
public abstract class CameraEffectBase
{
	protected	const	float					RETURN_FACTOR				= 0.1f;
	protected			EffectActiveCondition	m_EffectActiveCondition		= () => true;
	
	[SerializeField]
	protected			float					m_WpnInfluence				= 0.01f;

	[SerializeField]
	protected			float					m_AmplitudeBase				= 1.00f;

	[SerializeField]
	protected			float					m_AmplitudeHoriz			= 1.00f;

	[SerializeField]
	protected			float					m_AmplitudeVert				= 1.00f;

	[SerializeField]
	protected			float					m_SpeedBase					= 1.00f;

	[SerializeField]
	protected			float					m_Theta_Upd_Vert			= 5.00f;

	[SerializeField]
	protected			float					m_Theta_Upd_Oriz			= 2.50f;

	[SerializeField]
	protected			Vector3					m_Direction					= Vector3.zero;

	[SerializeField]
	protected			Vector3					m_WeaponRotationDelta		= Vector3.zero;

	[SerializeField]
	protected			Vector3					m_WeaponPositionDelta		= Vector3.zero;

//	[SerializeField]
	public				bool					IsActive					= true;

//	[SerializeField]
	public				float					AmplitudeMult				= 1.0f;

//	[SerializeField]
	public				float					SpeedMul					= 1.0f;


	protected			float					m_ThetaX					= 0f;
	protected			float					m_ThetaY					= 90f;

	protected			CameraEffectorData		m_Data						= new CameraEffectorData();
	public				CameraEffectorData		GetData()					=> m_Data;


	//////////////////////////////////////////////////////////////////////////
	public abstract	void Setup( in EffectActiveCondition condition );


	//////////////////////////////////////////////////////////////////////////
	public abstract	void Update( float deltaTime, ref CameraEffectorData data );

}
