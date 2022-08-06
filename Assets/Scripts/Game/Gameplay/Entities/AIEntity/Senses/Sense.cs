
using UnityEngine;


namespace Entities.AI.Components.Senses
{

	[System.Serializable]
	public abstract class SenseEvent
	{
		public abstract ESenses SenseType { get; }
	}


	public abstract class Sense : MonoBehaviour
	{
		[SerializeField, ReadOnly]
		protected AIEntity				m_Owner					= null;

		[SerializeField, ReadOnly]
		protected AIPerceptionComponent	m_PerceptionComponent	= null;

		[SerializeField, ReadOnly]
		protected AIBrainComponent		m_BrainComponent		= null;

		internal static bool TryGetSenseEnumType(System.Type senseType, out ESenses result)
		{
			System.Type baseType = senseType;

			// Look up for the first Sense class derived of this sense, becomes null of invalid types
			while (baseType.BaseType != typeof(Sense))
			{
				baseType = baseType.BaseType;
			}

			bool bResult = false;
			result = default;
			switch (baseType)
			{
				case System.Type Type when Type == typeof(Damage):	{ result = ESenses.DAMAGE;		bResult = true; break; }
				case System.Type Type when Type == typeof(Hearing): { result = ESenses.HEARING;		bResult = true; break; }
				case System.Type Type when Type == typeof(Sight):	{ result = ESenses.SIGHT;		bResult = true; break; }
				case System.Type Type when Type == typeof(Team):	{ result = ESenses.TEAM;		bResult = true; break; }
			}
			return bResult;
		}

		private void Awake()
		{
			bool a = Utils.CustomAssertions.IsTrue(transform.TrySearchComponent(ESearchContext.FROM_ROOT, out m_Owner));
			bool b = Utils.CustomAssertions.IsTrue(transform.TrySearchComponent(ESearchContext.FROM_ROOT, out m_BrainComponent));
			bool c = Utils.CustomAssertions.IsTrue(transform.TrySearchComponent(ESearchContext.FROM_ROOT, out m_PerceptionComponent));
			if (a && b && c)
			{
				SetupInternal();
			}
		}

		protected abstract void SetupInternal();

		protected virtual void OnEnable()
		{
			if (Utils.CustomAssertions.IsNotNull(GameManager.CyclesEvents))
			{
				GameManager.CyclesEvents.OnThink += OnThink;
				GameManager.CyclesEvents.OnPhysicFrame += OnPhysicFrame;
				GameManager.CyclesEvents.OnFrame += OnFrame;
				GameManager.CyclesEvents.OnLateFrame += OnLateFrame;
			}

			OnEnableInternal();
		}

		protected virtual void OnDisable()
		{
			OnDisableInternal();

			if (GameManager.CyclesEvents.IsNotNull())
			{
				GameManager.CyclesEvents.OnLateFrame -= OnLateFrame;
				GameManager.CyclesEvents.OnFrame -= OnFrame;
				GameManager.CyclesEvents.OnPhysicFrame -= OnPhysicFrame;
				GameManager.CyclesEvents.OnThink -= OnThink;
			}
		}

		protected abstract void OnEnableInternal();
		protected abstract void OnDisableInternal();

		protected abstract void OnThink();
		protected abstract void OnFrame(float deltaTime);
		protected abstract void OnLateFrame(float deltaTime);
		protected abstract void OnPhysicFrame(float fixedDeltaTime);
	}
}
