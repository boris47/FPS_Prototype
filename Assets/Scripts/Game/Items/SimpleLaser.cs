using UnityEngine;
using System.Collections;

[System.Serializable]
public class SimpleLaser : MonoBehaviour
{
	[SerializeField]
	protected		float				m_ScaleFactor		= 0.03f;

	[SerializeField]
	protected		Color				m_Color				= Color.red;

	[SerializeField]
	protected		float				m_LaserLength		= 100f;

	protected		Transform			m_MeshTransform		= null;
	protected		Renderer			m_Renderer			= null;
	protected		Vector3				m_LocalScale		= new Vector3();

	private void Awake()
	{
		m_MeshTransform = transform.GetChild(0);

		if (enabled = transform.TrySearchComponent(ESearchContext.LOCAL_AND_CHILDREN, out m_Renderer))
		{
			m_Renderer.material.color = m_Color;
		}
	}

	private void OnValidate()
	{
		m_MeshTransform = transform.GetChild(0);
		UnityEngine.Assertions.Assert.IsNotNull(m_MeshTransform);

		Update();
	}


	private void Update()
	{
		// Save cpu
		if (Time.frameCount % 15 == 0)
			return;

		bool bHasHit = Physics.Raycast(transform.position, transform.forward, out RaycastHit rayCastHit, m_LaserLength, Utils.LayersHelper.Layers_AllButOne("Bullets"));

		float currentLength = bHasHit ? rayCastHit.distance : m_LaserLength;

		//if the additional decimal isn't added then the beam position glitches
		float beamPosition = currentLength * (0.5f + 0.0001f);

		m_LocalScale.Set(m_ScaleFactor, m_ScaleFactor, currentLength);

		m_MeshTransform.localScale = m_LocalScale;
		m_MeshTransform.localPosition = Vector3.forward * beamPosition;
	}
}
