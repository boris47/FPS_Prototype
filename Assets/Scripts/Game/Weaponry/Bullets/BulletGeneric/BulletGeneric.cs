
using UnityEngine;

/// <summary> Base class for projectiles </summary>
public abstract class BulletGeneric : Bullet
{
	[SerializeField]
	protected		Light				m_PointLight			= null;
	[SerializeField]
	protected		LensFlare			m_LensFlare				= null;

	private			bool				m_HasLight				= false;
	private			bool				m_HasFlare				= false;


	//////////////////////////////////////////////////////////////////////////
	protected override void Awake()
	{
		base.Awake();

		m_HasLight = transform.TryGetComponent(out m_PointLight);
		m_HasFlare = transform.TryGetComponent(out m_LensFlare);
	}


	//////////////////////////////////////////////////////////////////////////
	protected override void SetupBullet()
	{
		base.SetupBullet();

		m_RigidBody.freezeRotation = true;

		if (m_HasLight)
		{
			m_PointLight.color = m_Renderer.material.GetColor("_EmissionColor");
			if (m_HasFlare)
			{
				m_LensFlare.color = m_PointLight.color;
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	protected override void OnEndTravel()
	{
		gameObject.SetActive(false);
	}
}
