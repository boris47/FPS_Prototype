
using UnityEngine;

[System.Serializable]
public class CameraEffectorData
{
	/// <summary> </summary>
	[SerializeField, ReadOnly]
	public Vector3 CameraEffectsDirection;

	/// <summary> </summary>
	[SerializeField, ReadOnly]
	public Vector3 WeaponPositionDelta;

	/// <summary> </summary>
	[SerializeField, ReadOnly]
	public Vector3 WeaponDirectionDelta;


	public void Reset()
	{
		CameraEffectsDirection.Set( 0f, 0f, 0f );
		WeaponPositionDelta.Set( 0f, 0f, 0f );
		WeaponDirectionDelta.Set( 0f, 0f, 0f );
	}
}


public abstract class CameraEffectBase: MonoBehaviour
{
	protected			EffectorActiveCondition	m_EffectActiveCondition		= () => true;
	
	[SerializeField]
	protected			float					m_ToWeaponConvFactor		= 0.0025f;
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
	protected			float					m_CurrentWeight				= 0f;

	[SerializeField, ReadOnly]
	protected			Vector3					m_CameraEffectsDirection	= Vector3.zero;
	[SerializeField, ReadOnly]
	protected			Vector3					m_WeaponDirectionDelta		= Vector3.zero;
	[SerializeField, ReadOnly]
	protected			Vector3					m_WeaponPositionDelta		= Vector3.zero;


	[SerializeField, ReadOnly]
	public				bool					IsActive					= true;
	[SerializeField, ReadOnly]
	public				float					AmplitudeMult				= 1.0f;
	[SerializeField, ReadOnly]
	public				float					SpeedMul					= 1.0f;

	[SerializeField, ReadOnly]
	protected			float					m_ThetaX					= 0f;
	[SerializeField, ReadOnly]
	protected			float					m_ThetaY					= 90f;

	protected			CameraEffectorData		m_Data						= new CameraEffectorData();
	public				CameraEffectorData		GetData()					=> m_Data;


	//////////////////////////////////////////////////////////////////////////
	public abstract void Setup(in EffectorActiveCondition condition);


	//////////////////////////////////////////////////////////////////////////
	public abstract void OnUpdate(float deltaTime, CameraEffectorData data);
}
