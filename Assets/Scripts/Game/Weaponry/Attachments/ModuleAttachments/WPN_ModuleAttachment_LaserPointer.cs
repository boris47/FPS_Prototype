using UnityEngine;

public class WPN_ModuleAttachment_LaserPointer : WPN_ModuleAttachmentBase
{
	[SerializeField]
	protected		float				m_ScaleFactor		= 0.03f;

	[SerializeField]
	protected		Color				m_Color				= Color.red;

	[SerializeField]
	protected		float				m_LaserLength		= 100f;

	[SerializeField, ReadOnly]
	protected		bool				m_HasHit			= false;

	protected		RaycastHit			m_RayCastHit		= default;
	protected		Transform			m_LaserTransform	= null;
	protected		Renderer			m_Renderer			= null;

	public			bool				HasHit				=> m_HasHit;
	public			RaycastHit			RayCastHit			=> m_RayCastHit;
	public			float				LaserLength
	{
		get => m_LaserLength;
		set => m_LaserLength = value;
	}


	//////////////////////////////////////////////////////////////////////////
	protected override void Awake()
	{
		base.Awake();

		if (CustomAssertions.IsNotNull(m_LaserTransform = transform.GetChild(0)))
		{
			m_LaserTransform.gameObject.SetActive(false);
		}

		if (CustomAssertions.IsTrue(transform.TrySearchComponent(ESearchContext.LOCAL_AND_CHILDREN, out m_Renderer)))
		{
			m_Renderer.material.color = m_Color;
		}

		if (CustomAssertions.IsNotNull(GameManager.UpdateEvents))
		{
			GameManager.UpdateEvents.OnFrame += OnFrame;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	protected override void OnDestroy()
	{
		if (GameManager.UpdateEvents.IsNotNull())
		{
			GameManager.UpdateEvents.OnFrame -= OnFrame;
		}

		base.OnDestroy();
	}


	//////////////////////////////////////////////////////////////////////////
	protected override void OnAttachInternal()
	{
		// TODO Apply Silencer Effects
		// maybe increased accurancy
	}


	//////////////////////////////////////////////////////////////////////////
	protected override void OnDetachInternal()
	{
		// TODO Remove Silencer Effects
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnFrame(float DeltaTime)
	{
		// Save cpu
		if (gameObject.EveryFrames(15))
		{
			m_RayCastHit = default;

			m_HasHit = Physics.Raycast(transform.position, transform.forward, out m_RayCastHit, m_LaserLength, Utils.LayersHelper.Layers_AllButOne("Bullets"));

			float currentLength = HasHit ? m_RayCastHit.distance : m_LaserLength;

			//if the additional decimal isn't added then the beam position glitches
			float beamPosition = currentLength * (0.5f + 0.0001f);

			m_LaserTransform.localScale = new Vector3(m_ScaleFactor, m_ScaleFactor, currentLength);
			m_LaserTransform.localPosition = Vector3.forward * beamPosition;
		}
	}
}
