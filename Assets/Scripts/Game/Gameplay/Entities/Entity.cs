
using UnityEngine;

namespace Entities
{
	public class Shield : MonoBehaviour
	{
		private		Collider		m_Collider				= null;
		public		Collider		Collider				=> m_Collider;
	}

//	[RequireComponent(typeof(Rigidbody))]
	public abstract partial class Entity : MonoBehaviour
	{
		private	static uint									m_CurrentID					= 1;
		protected 	uint									m_Id						= 0;
		public		uint									Id							=> m_Id;

		[SerializeField]
		protected float m_Health = 100f;

		public float Health => m_Health;

		public bool IsAlive => m_Health > 0f;

		[SerializeField]
		protected EFactions m_Faction = default;
		public EFactions Faction => m_Faction;

		protected EFactionRelationType? m_GlobalRelationOverride = null;
		public EFactionRelationType? GlobalRelationOverride => m_GlobalRelationOverride;

		public Transform Targettable => transform;
		public Transform Head => transform;
		public Transform Body => transform;

		[SerializeField]
		protected	Collider								m_PhysicCollider			= null;

		[SerializeField]
		protected Rigidbody									m_Rigidbody					= null;

		[SerializeField, ReadOnly]
		protected	Shield									m_Shield					= null;

		public		Collider								PhysicCollider				=> m_PhysicCollider;
		public		Rigidbody								Rigidbody					=> m_Rigidbody;
		public		Shield									EntityShield				=> m_Shield;

		protected virtual void Awake()
		{
			m_Id			= m_CurrentID++;
			m_PhysicCollider = GetComponent<Collider>();
		}

		public virtual bool IsInterestedAt(in Entity source)
		{
			return false;
		}
	}
}
