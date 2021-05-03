using UnityEngine;


public sealed class CinematicCamera : CameraBase
{
	[Header("Cinematic Camera")]

	[SerializeField, ReadOnly]
	private		Transform						m_Target								= null;

	[SerializeField, ReadOnly]
	private		Vector3?						m_TargetPoint							= null;

	private		bool							m_bIsTargetGenerated					= false;

	protected override	string					PostProcessResourcePath					=> "Scriptables/CameraPostProcesses";


	//////////////////////////////////////////////////////////////////////////
	protected override void Awake()
	{
		base.Awake();
	}


	//////////////////////////////////////////////////////////////////////////
	protected override void OnEnable()
	{
		base.OnEnable();

		m_Target = null;
		m_TargetPoint = null;

		OutlineEffectManager.SetEffectCamera(m_CameraRef);
	}


	//////////////////////////////////////////////////////////////////////////
	protected override void OnDisable()
	{
		m_Target = null;
		m_TargetPoint = null;

		OutlineEffectManager.SetEffectCamera(null);

		base.OnDisable();
	}


	//////////////////////////////////////////////////////////////////////////
/*	private bool OnSave(StreamData streamData, ref StreamUnit streamUnit)
	{
		streamUnit = streamData.NewUnit(gameObject);



		return true;
	}


	//////////////////////////////////////////////////////////////////////////
	private bool OnLoad(StreamData streamData, ref StreamUnit streamUnit)
	{
		bool bResult = streamData.TryGetUnit(gameObject, out streamUnit);
		if (bResult)
		{

		}
		return bResult;
	}*/
	

	//////////////////////////////////////////////////////////////////////////
	/// <summary> Set the target to look at. With first two arguments null camera will stop to look at current target</summary>
	/// <param name="value">The transform to look at or null</param>
	/// <param name="position">The point to look at or null</param>
	/// <param name="Up">The upward vector or null</param>
	public void SetTarget(Transform value, Vector3? position, Vector3? Up)
	{
		DestroyGenerated();

		m_TargetPoint = position;

		m_Target = null;

		if (value)
		{
			m_Target = value;
		}
		else if (position.HasValue)
		{
			m_bIsTargetGenerated = true;
			m_Target = new GameObject($"TempTarget_{name}").transform;
			m_Target.position = position.Value;
		}

		transform.LookAt(m_Target, Up.GetValueOrDefault());
	}


	//////////////////////////////////////////////////////////////////////////
	public void OnCutsceneEnd()
	{
		SetTarget(null, null, null);

	//	(WeaponManager.Instance?.CurrentWeapon as Weapon).OnCutsceneEnd();
	}

	private void DestroyGenerated()
	{
		if (m_bIsTargetGenerated)
		{
			Destroy(m_Target.gameObject);

			m_bIsTargetGenerated = false;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private void LateUpdate()
	{
		float deltaTime = GameManager.IsPaused ? 0f : Time.deltaTime;

		// Update effectors
		m_CameraEffectorsManager.OnFrame(deltaTime);
	}


	//////////////////////////////////////////////////////////////////////////
	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

}

