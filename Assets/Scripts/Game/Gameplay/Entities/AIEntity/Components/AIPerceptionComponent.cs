
using UnityEngine;

namespace Entities.AI.Components
{
	using Senses;

	public delegate void OnNewSenseEventDel(in SenseEvent newSenseEvent);

	[System.Serializable]
	public enum ESenses : sbyte
	{
		NONE,
		DAMAGE,
		HEARING,
		SIGHT,
		TEAM,
		COUNT
	}

	public partial class AIPerceptionComponent : AIEntityComponent
	{
		private const string kSenseGameObjectName = "Senses";
	
		[SerializeField, UDictionary.ReadOnly]
		private				UDictionary<ESenses, Sense>						m_MappedSenses									= new UDictionary<ESenses, Sense>();

		private				GameObject										m_SensesContainerGO								= null;

		public event		OnNewSenseEventDel								OnNewSenseEvent									= delegate { };


		//////////////////////////////////////////////////////////////////////////
		protected override void Awake()
		{
			base.Awake();

			EnsureSenseRoot();

			foreach (Sense sense in gameObject.GetComponentsInChildren<Sense>(includeInactive: true))
			{
				// Senses must be children of the brain
				Utils.CustomAssertions.IsTrue(sense.transform.IsChildOf(transform));
				Utils.CustomAssertions.IsTrue(Sense.TryGetSenseEnumType(sense.GetType(), out ESenses senseEnum));

				// Slot available
				if (!m_MappedSenses.ContainsKey(senseEnum))
				{
					m_MappedSenses[senseEnum] = sense;
				}
			}
		}

		//////////////////////////////////////////////////////////////////////////
		private GameObject EnsureSenseRoot()
		{
			if (transform.TrySearchComponentByChildName(kSenseGameObjectName, out Transform child))
			{
				m_SensesContainerGO = child.gameObject;
			}
			else
			{
				m_SensesContainerGO = new GameObject(kSenseGameObjectName);
				m_SensesContainerGO.transform.SetParent(transform);
			}

			m_SensesContainerGO.transform.localPosition = Vector3.zero;
			m_SensesContainerGO.transform.localRotation = Quaternion.identity;
			return m_SensesContainerGO;
		}

		//////////////////////////////////////////////////////////////////////////
		/// <summary> Stimulus received from a sense </summary>
		public void SendSenseEvent(in SenseEvent senseEvent) => OnNewSenseEvent(senseEvent);

		//////////////////////////////////////////////////////////////////////////
		public void AddSense<T>() => AddSense(typeof(T));

		//////////////////////////////////////////////////////////////////////////
		public bool TryGetSenseByType<T>(in ESenses senseType, out T sense) where T : Sense, new()
		{
			sense = null;
			bool bResult = TryGetSenseByType(senseType, out Sense result);
			if (bResult)
			{
				sense = result as T;
			}
			return bResult;
		}

		//////////////////////////////////////////////////////////////////////////
		public bool TryGetSense<T>(out T sense) where T : Sense, new()
		{
			bool bResult = TryGetSense(typeof(T), out Sense result);
			{
				sense = result as T;
			}
			return bResult;
		}

		//////////////////////////////////////////////////////////////////////////
		public T GetSense<T>() where T : Sense, new() => GetSense(typeof(T)) as T;


		//////////////////////////////////////////////////////////////////////////
		public Sense AddSense(in System.Type senseType)
		{
			Sense outValue = null;
			if (Utils.CustomAssertions.IsTrue(Sense.TryGetSenseEnumType(senseType, out ESenses senseEnum)))
			{
				EnsureSenseRoot();
				if (!m_MappedSenses.TryGetValue(senseEnum, out outValue))
				{
					Sense sense = m_SensesContainerGO.AddChildWithComponent(senseEnum.ToString(), senseType) as Sense;
					m_MappedSenses[senseEnum] = outValue = sense;
				}
			}
			return outValue;
		}

		//////////////////////////////////////////////////////////////////////////
		public bool TryGetSenseByType(in ESenses senseType, out Sense sense)
		{
			sense = null;
			switch (senseType)
			{
				case ESenses.DAMAGE: return TryGetSense(typeof(Damage), out sense);
				case ESenses.HEARING: return TryGetSense(typeof(Hearing), out sense);
				case ESenses.SIGHT: return TryGetSense(typeof(Sight), out sense);
				case ESenses.TEAM: return TryGetSense(typeof(Team), out sense);
			}
			return false;
		}

		//////////////////////////////////////////////////////////////////////////
		public bool TryGetSense(in System.Type senseType, out Sense sense)
		{
			Utils.CustomAssertions.IsTrue(Sense.TryGetSenseEnumType(senseType, out ESenses senseEnum));
			return m_MappedSenses.TryGetValue(senseEnum, out sense);
		}

		//////////////////////////////////////////////////////////////////////////
		public Sense GetSense(in System.Type senseType) => TryGetSense(senseType, out Sense result) ? result : null;

		//////////////////////////////////////////////////////////////////////////
		public void SetSenseEnabled(in ESenses senseType, in bool bSenseEnabled)
		{
			if (TryGetSenseByType(senseType, out Sense sense))
			{
				sense.enabled = bSenseEnabled;
			}
		}

		//////////////////////////////////////////////////////////////////////////
		public void RemoveSense(in ESenses senseType)
		{
			if (TryGetSenseByType(senseType, out Sense sense))
			{
				m_MappedSenses.Remove(senseType);
				sense.gameObject.Destroy();
			}
		}
	}
}

#if UNITY_EDITOR
namespace Entities.AI.Components
{
	using System.Linq;
	using UnityEditor;
	using Senses;

	public partial class AIPerceptionComponent
	{
		[CustomEditor(typeof(AIPerceptionComponent))]
		public class AIPerceptionComponentEditor : Editor
	{
		private static readonly System.Type[] m_CachedTypes = default;

		private AIPerceptionComponent perceptionComponent = null;

		//////////////////////////////////////////////////////////////////////////
		static AIPerceptionComponentEditor()
		{
			m_CachedTypes = TypeCache.GetTypesDerivedFrom<Sense>().ToArray();
		}

		//////////////////////////////////////////////////////////////////////////
		private void OnEnable()
		{
			perceptionComponent = target as AIPerceptionComponent;
		}

		//////////////////////////////////////////////////////////////////////////
		private void OnItemSelected_Add(object arg)
		{
			System.Type senseType = arg as System.Type;
			if (Sense.TryGetSenseEnumType(senseType, out ESenses senseEnum))
			{
				GameObject container = perceptionComponent.EnsureSenseRoot();

				GameObject go = new GameObject(senseEnum.ToString());
				go.transform.SetParent(container.transform);
				go.transform.localPosition = Vector3.zero;
				go.transform.localRotation = Quaternion.identity;

				Sense sense = go.AddComponent(senseType) as Sense;
				perceptionComponent.m_MappedSenses[senseEnum] = sense;
			}
		}

		//////////////////////////////////////////////////////////////////////////
		private void OnItemSelected_Rem(object arg)
		{
			System.Type senseType = arg as System.Type;
			if (Sense.TryGetSenseEnumType(senseType, out ESenses senseEnum))
			{
				perceptionComponent.RemoveSense(senseEnum);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			if (GUILayout.Button("Add Sense"))
			{
				GenericMenu menu = new GenericMenu();

				bool bShow = false;
				foreach (System.Type item in m_CachedTypes.Except(perceptionComponent.GetComponentsInChildren<Sense>(includeInactive: true).Select(s => s.GetType())).AsEnumerable())
				{
					bShow = true;
					menu.AddItem(new GUIContent(item.Name), false, OnItemSelected_Add, item);
				}

				if (bShow)
				{
					menu.ShowAsContext();
				}
			}

			if (perceptionComponent.m_MappedSenses.Any() && GUILayout.Button("Remove Sense"))
			{
				GenericMenu menu = new GenericMenu();

				bool bShow = false;
				foreach (System.Type item in perceptionComponent.m_MappedSenses.Values.Select(s => s.GetType()))
				{
					bShow = true;
					menu.AddItem(new GUIContent(item.Name), true, OnItemSelected_Rem, item);
				}

				if (bShow)
				{
					menu.ShowAsContext();
				}
			}
		}
	}
	}
}
#endif
