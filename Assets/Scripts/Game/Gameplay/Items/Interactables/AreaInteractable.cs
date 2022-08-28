
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


	private static Color semiTransparent = new Color(0.3f, 0.3f, 0.3f, 0.2f);
	//////////////////////////////////////////////////////////////////////////
	// Implement this OnDrawGizmos if you want to draw gizmos that are also pickable and always drawn
	private void OnDrawGizmos()
	{
		if (m_Collider.IsNotNull())
		{
			Color prevColor = Gizmos.color;
			Matrix4x4 prevMatrix = Gizmos.matrix;
			{
				Gizmos.matrix = Matrix4x4.TRS(m_Collider.bounds.center, this.transform.rotation, this.transform.lossyScale);
				Gizmos.color = semiTransparent;

				switch (m_Collider)
				{
					case BoxCollider box:
					{
						Gizmos.DrawCube(Vector3.zero, box.size);
						break;
					}
					case SphereCollider sphere:
					{
						Gizmos.DrawSphere(Vector3.zero, sphere.radius);
						break;
					}
					case CapsuleCollider capsule:
					{
						Utils.GizmosHelper.DrawWireCapsule(Vector3.zero, this.transform.rotation, capsule.radius, capsule.height, semiTransparent);
						break;
					}

				}

			}
			Gizmos.color = prevColor;
			Gizmos.matrix = prevMatrix;
		}
	}
}
