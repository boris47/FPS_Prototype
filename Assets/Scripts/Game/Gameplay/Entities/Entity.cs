
using UnityEngine;

namespace Entities
{
	using Relations;
	using UnityEngine.Rendering;

	public class Shield : MonoBehaviour
	{
		private		Collider		m_Collider				= null;
		public		Collider		Collider				=> m_Collider;
	}

	[DefaultExecutionOrder(-6)]
	public abstract partial class Entity : MonoBehaviour
	{
		private	static		uint							m_CurrentID							= 1;
		protected 			uint							m_Id								= 0;

		[SerializeField, ReadOnly]
		private				Transform						m_Head								= null;
		[SerializeField, ReadOnly]
		private				Transform						m_Body								= null;
		[SerializeField, ReadOnly]
		private				Transform						m_Targetable						= null;
		[SerializeField, ReadOnly]
		private				Collider						m_PrimaryCollider					= null;

		[SerializeField, ReadOnly]
		protected			float							m_Health							= 100f;
		
		[SerializeField]
		protected			EntityFaction					m_Faction							= null;

		[SerializeField, ReadOnly]
		protected			Shield							m_Shield							= null;


		//--------------------
		protected			EFactionRelationType?			m_GlobalRelationOverride			= null;


		public				uint							Id									=> m_Id;
		
		public				float							Health								=> m_Health;

		public				bool							IsAlive								=> m_Health > 0f;

		public				EntityFaction					Faction								=> m_Faction;

		public				EFactionRelationType?			GlobalRelationOverride				=> m_GlobalRelationOverride;

		public				Shield							EntityShield						=> m_Shield;


		public Transform Head
		{
			get
			{
				if (m_Head == null)
				{
					m_Head = GetHead();
				}
				return m_Head;
			}
		}
		public Transform Body
		{
			get
			{
				if (m_Body == null)
				{
					m_Body = GetBody();
				}
				return m_Body;
			}
		}
		public Collider PrimaryCollider
		{
			get
			{
				if (m_PrimaryCollider == null)
				{
					m_PrimaryCollider = GetPrimaryCollider();
				}
				return m_PrimaryCollider;
			}
		}
		public Transform Targetable
		{
			get
			{
				if (m_Targetable == null)
				{
					m_Targetable = GetTargetable();
				}
				return m_Targetable;
			}
		}



		//////////////////////////////////////////////////////////////////////////
		protected virtual void Awake()
		{
			m_Id			= m_CurrentID++;
		}

		//////////////////////////////////////////////////////////////////////////
		public virtual void BeforeSceneChange()
		{
			
		}

		//////////////////////////////////////////////////////////////////////////
		public virtual void OnSceneLoaded()
		{
			
		}

		//////////////////////////////////////////////////////////////////////////
		public virtual bool IsInterestedAt(in Entity source)
		{
			return false;
		}

		//////////////////////////////////////////////////////////////////////////
		public void SetEntityFaction(in EntityFaction InEntityFaction)
		{
			m_Faction = InEntityFaction;
		}


		//////////////////////////////////////////////////////////////////////////
		protected abstract Transform GetHead();
		//////////////////////////////////////////////////////////////////////////
		protected abstract Transform GetBody();
		//////////////////////////////////////////////////////////////////////////
		protected abstract Transform GetTargetable();
		//////////////////////////////////////////////////////////////////////////
		protected abstract Collider GetPrimaryCollider();
		//////////////////////////////////////////////////////////////////////////
		public abstract Vector3 GetVelocity();
	}
}
