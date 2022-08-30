﻿
using UnityEngine;
using Entities;

[RequireComponent(typeof(Collider))]
public class AreaInteractable : Interactable
{
	[SerializeField]
	private				Transform						m_ValidLocationRef				= null;

	[SerializeField, ReadOnly]
	private				Collider						m_Collider						= null;


	public				Transform						ValidLocationRef				=> m_ValidLocationRef;
	public				Bounds							Bounds							=> m_Collider.bounds;


	/////////////////////////////////////////////////////////////////////////////
	protected override void Awake()
	{
		base.Awake();

		if (m_Collider.IsNotNull() || Utils.CustomAssertions.IsTrue(gameObject.TryGetComponent(out m_Collider)))
		{
			m_Collider.isTrigger = true;
		}
	}

	/////////////////////////////////////////////////////////////////////////////
	protected override void OnValidate()
	{
		base.OnValidate();

		if (gameObject.TryGetComponent(out m_Collider))
		{
			m_Collider.isTrigger = true;
		}
	}

	/////////////////////////////////////////////////////////////////////////////
	public  override bool CanInteract(Entity entity)
	{
		return true;
	}

	/////////////////////////////////////////////////////////////////////////////
	public override void OnInteraction(Entity entity)
	{
		m_OnInteraction.Invoke();
	}


	//////////////////////////////////////////////////////////////////////////
	// Implement this OnDrawGizmos if you want to draw gizmos that are also pickable and always drawn
	private void OnDrawGizmos()
	{
		if (m_Collider.IsNotNull())
		{
			Utils.GizmosHelper.DrawCollider(m_Collider, Color.white * 0.2f);
		}
	}
}