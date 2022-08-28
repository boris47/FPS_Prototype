
using UnityEngine;
using Entities;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class GrabInteractable : Interactable
{
	[SerializeField, ReadOnly]
	private				Rigidbody						m_Rigidbody							= null;


	public				Rigidbody						Rigidbody							=> m_Rigidbody;


	//////////////////////////////////////////////////////////////////
	protected override void Awake()
	{
		base.Awake();

		if (m_Rigidbody.IsNotNull() || Utils.CustomAssertions.IsTrue(gameObject.TryGetComponent(out m_Rigidbody)))
		{

		}
	}

	//////////////////////////////////////////////////////////////////
	protected override void OnValidate()
	{
		base.OnValidate();

		gameObject.TryGetComponent(out m_Rigidbody);
	}

	//////////////////////////////////////////////////////////////////
	public override bool CanInteract(Entity entity)
	{
		return true;
	}

	//////////////////////////////////////////////////////////////////
	public override void OnInteraction(Entity entity)
	{
		m_OnInteraction.Invoke();
	}
}
