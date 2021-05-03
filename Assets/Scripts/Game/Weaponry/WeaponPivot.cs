using UnityEngine;

public class WeaponPivot : MonoBehaviour
{
	private		static	WeaponPivot					m_Instance						= null;
	public		static	WeaponPivot					Instance						=> m_Instance;

	[SerializeField, ReadOnly]
	private				Vector3						m_Deviation						= Vector3.zero;
	[SerializeField, ReadOnly]
	private				Vector3						m_Dispersion					= Vector3.zero;
	[SerializeField, ReadOnly]
	private				Vector3						m_FallFeedback					= Vector3.zero;
	[SerializeField, ReadOnly]
	private				Vector3						m_RotationFeedback				= Vector3.zero;
	[SerializeField, ReadOnly]
	private				float						m_Recoil						= 0f;

	[System.Serializable]
	private class WeaponPivotSectionData
	{
		public float	RecoverySpeedMult			= 0.0f;
		public float	MaxRecoil					= 0.0f;
		public float	MaxDeviation				= 0.0f;
		public float	MaxDispersion				= 0.0f;
		public float	WeaponRotationFeedBackClamp	= 0.0f;
	}
	[SerializeField]
	private				WeaponPivotSectionData		m_WeaponPivotSectionData		= new WeaponPivotSectionData();

	[SerializeField]
	private				bool						m_ApplyDeviation				= true;
	[SerializeField]
	private				bool						m_ApplyDispersion				= true;
	[SerializeField]
	private				bool						m_ApplyRecoil					= true;
	[SerializeField]
	private				bool						m_ApplyRotationFeedback			= true;

	public				Vector3						Deviation						=> m_Deviation;
	public				Vector3						Dispersion						=> m_Dispersion;
	public				Vector3						FallFeedback					=> m_FallFeedback;
	public				Vector3						RotationFeedback				=> m_RotationFeedback;
	public				float						Recoil							=> m_Recoil;

	//////////////////////////////////////////////////////////////////////////
	private void Awake()
	{
		// Singleton
		if (m_Instance.IsNotNull())
		{
			gameObject.SetActive(false);
			return;
		}
		m_Instance = this;

		if (CustomAssertions.IsNotNull(GameManager.UpdateEvents))
		{
			GameManager.UpdateEvents.OnFrame += OnFrame;
		}

		CustomAssertions.IsTrue(GlobalManager.Configs.TryGetSection(GetType().Name, out var section) && GlobalManager.Configs.TrySectionToOuter(section, m_WeaponPivotSectionData));
	}

	//////////////////////////////////////////////////////////////////////////
	private void OnDestroy()
	{
		if ((Object)m_Instance != this)
			return;

		if (GameManager.UpdateEvents.IsNotNull())
		{
			GameManager.UpdateEvents.OnFrame -= OnFrame;
		}

		m_Instance = null;
	}

	//////////////////////////////////////////////////////////////////////////
	private void OnFrame(float deltaTime)
	{
		float interpolant = deltaTime * m_WeaponPivotSectionData.RecoverySpeedMult;

		m_Dispersion.LerpTo(Vector3.zero, interpolant);
		m_Deviation.LerpTo(Vector3.zero, interpolant);
		m_FallFeedback.LerpTo(Vector3.zero, interpolant);
		m_RotationFeedback.LerpTo(Vector3.zero, interpolant);

		m_Recoil = Mathf.Lerp(m_Recoil, 0.0f, interpolant);

		transform.localEulerAngles = m_Deviation + new Vector3(0f, -90f, 0f);
	}

	//////////////////////////////////////////////////////////////////////////
	/// <summary> The deviation to be applied to the weapon </summary>
	/// <param name="deviation">Deviation value</param>
	/// <param name="weightX">Weight for vertical dispersion</param>
	/// <param name="weightY">Weight for horizontal dispersion</param>
	public void ApplyDeviation(float deviation, float weightX = 1.0f, float weightY = 1.0f, float balanceX = 1.0f, float balanceY = 0.5f)
	{
		if (m_ApplyDeviation)
		{
			float deviationX = (deviation * Mathf.Sign(Mathf.Clamp01(balanceX) - 0.5f) * weightX);
			m_Deviation.z = Mathf.Clamp(m_Deviation.z + deviationX, -m_WeaponPivotSectionData.MaxDeviation, m_WeaponPivotSectionData.MaxDeviation);
			m_Deviation.y += Random.Range(-deviation, deviation) * weightY;
		}
	}

	//////////////////////////////////////////////////////////////////////////
	/// <summary> The dispersion applied to camera </summary>
	/// <param name="dispersion">Dispersion value</param>
	/// <param name="weightX">Weight for vertical dispersion</param>
	/// <param name="weightY">Weight for horizontal dispersion</param>
	public void ApplyDispersion(float dispersion, float weightX = 1.0f, float weightY = 1.0f)
	{
		if (m_ApplyDispersion)
		{
			m_Dispersion.x = Mathf.Min(m_Dispersion.x - (dispersion * weightX), m_WeaponPivotSectionData.MaxDispersion);
			m_Dispersion.y += Random.Range(-dispersion, dispersion) * weightY; 
		}
	}

	//////////////////////////////////////////////////////////////////////////
	public void AddRecoil(float recoil)
	{
		if (m_ApplyRecoil)
		{
			m_Recoil = Mathf.Min(m_Recoil + recoil, m_WeaponPivotSectionData.MaxRecoil);
		}
	}

	//////////////////////////////////////////////////////////////////////////
	public void ApplyFallFeedback(float delta, float weightX = 1.0f, float weightY = 1.0f)
	{
		m_FallFeedback.x = delta * weightX;
		m_FallFeedback.y = delta * weightY;
	}

	//////////////////////////////////////////////////////////////////////////
	public void AddRotationFeedBack(Vector3 rotation)
	{
		if (m_ApplyRotationFeedback)
		{
			m_RotationFeedback = Vector3.ClampMagnitude(m_RotationFeedback + rotation, m_WeaponPivotSectionData.WeaponRotationFeedBackClamp);
		}
	}
}
