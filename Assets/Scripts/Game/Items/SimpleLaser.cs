using UnityEngine;

[System.Serializable]
public class SimpleLaser : MonoBehaviour
{
	[SerializeField]
	protected		float				m_SizeFactor		= 0.03f;

	[SerializeField]
	protected		Color				m_Color				= Color.red;

	[SerializeField]
	protected		float				m_LaserLength		= 100f;

	protected		Transform			m_MeshTransform		= null;
	protected		Renderer			m_Renderer			= null;
	protected		Vector3				m_LocalScale		= new Vector3();


	//////////////////////////////////////////////////////////////////////////
	private void Awake()
	{
		m_MeshTransform = transform.GetChild(0);

		if (enabled = transform.TrySearchComponent(ESearchContext.LOCAL_AND_CHILDREN, out m_Renderer))
		{
			m_Renderer.material.color = m_Color;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnEnable()
	{
		if(transform.TrySearchComponent(ESearchContext.FROM_ROOT, out FieldOfView fieldOfView))
		{
			m_LaserLength = fieldOfView.Distance;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnValidate()
	{
		m_MeshTransform = transform.GetChild(0);
		CustomAssertions.IsNotNull(m_MeshTransform);

		FixedUpdate();
	}


	//////////////////////////////////////////////////////////////////////////
	private void FixedUpdate()
	{
		bool bHasHit = Physics.Raycast(transform.position, transform.forward, out RaycastHit rayCastHit, m_LaserLength, Utils.LayersHelper.Layers_AllButOne("Bullets"));

		float currentLength = bHasHit ? rayCastHit.distance : m_LaserLength;

		//if the additional decimal isn't added then the beam position glitches
		float beamPosition = currentLength * (0.5f + 0.0001f);

		m_LocalScale.Set(m_SizeFactor, m_SizeFactor, currentLength);

		m_MeshTransform.localScale = m_LocalScale;
		m_MeshTransform.localPosition = Vector3.forward * beamPosition;
	}


	//////////////////////////////////////////////////////////////////////////
	public void SetLaserLength(float newLength)
	{
		m_LaserLength = Mathf.Max(0f, newLength);
	}
}
