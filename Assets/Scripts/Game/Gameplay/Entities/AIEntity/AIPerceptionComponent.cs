
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Entities.AI.Components
{
	using Senses;

	public delegate void OnNewSenseEventDel(in SenseEvent newSenseEvent);

	[System.Serializable]
	public enum ESenses: sbyte
	{
		DAMAGE,
		HEARING,
		SIGHT,
		TEAM,
		COUNT
	}

	public interface IPerceptions
	{
		event OnNewSenseEventDel OnNewSenseEvent;

		void AddSense<T>(in bool bForce = false) where T : Sense, new();
		void AddSense(in System.Type senseType, in bool bForce);

		bool TryGetSenseByType<T>(in ESenses senseType, out T sense) where T : Sense, new();
		bool TryGetSenseByType(in ESenses senseType, out Sense sense);
		bool TryGetSense<T>(out T sense) where T : Sense, new();
		bool TryGetSense(in System.Type senseType, out Sense sense);
		T GetSense<T>() where T : Sense, new();
		Sense GetSense(in System.Type senseType);

		void SetSenseEnabled(in ESenses senseType, in bool bSenseEnabled);
		void RemoveSense(in ESenses senseType);

		void OnSenseEvent(in SenseEvent senseEvent);
	}

	public class AIPerceptionComponent : AIEntityComponent, IPerceptions
	{
		private const string SenseGameObjectName = "Senses";

		event				OnNewSenseEventDel								IPerceptions.OnNewSenseEvent
		{
			add		=>	m_OnNewSenseEvent += value;
			remove	=>	m_OnNewSenseEvent -= value;
		}

		[SerializeField, UDictionary.ReadOnly]
		protected			UDictionary<ESenses, Sense>						m_MappedSenses							= new UDictionary<ESenses, Sense>();


		private event		OnNewSenseEventDel								m_OnNewSenseEvent						= delegate { };
		internal			IPerceptions									Senses									=> this;

		private				GameObject										m_SensesContainerGO						= null;


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
			//	if (Utils.CustomAssertions.IsTrue(!m_MappedSenses.ContainsKey(senseEnum), $"Cannot have multiple sense of type {senseEnum}", gameObject))
				if (!m_MappedSenses.ContainsKey(senseEnum))
				{
					m_MappedSenses[senseEnum] = sense;
				}
			}
		}

		//////////////////////////////////////////////////////////////////////////
		private GameObject EnsureSenseRoot()
		{
			m_SensesContainerGO = transform.Find(SenseGameObjectName)?.gameObject ?? new GameObject(SenseGameObjectName);
			m_SensesContainerGO.transform.SetParent(transform);

			m_SensesContainerGO.transform.localPosition = Vector3.zero;
			m_SensesContainerGO.transform.localRotation = Quaternion.identity;
			return m_SensesContainerGO;
		}

		//////////////////////////////////////////////////////////////////////////
		void IPerceptions.AddSense<T>(in bool bForce) => Senses.AddSense(typeof(T), bForce);
		void IPerceptions.AddSense(in System.Type senseType, in bool bForce)
		{
			if (Utils.CustomAssertions.IsTrue(Sense.TryGetSenseEnumType(senseType, out ESenses senseEnum)))
			{
				if ((m_MappedSenses.ContainsKey(senseEnum) && bForce) || true)
				{
					EnsureSenseRoot();
					Sense sense = m_SensesContainerGO.AddChildWithComponent(senseEnum.ToString(), senseType) as Sense;
					m_MappedSenses[senseEnum] = sense;
				}
			}
		}

		//////////////////////////////////////////////////////////////////////////
		bool IPerceptions.TryGetSenseByType<T>(in ESenses senseType, out T sense)
		{
			sense = null;
			bool bResult = Senses.TryGetSenseByType(senseType, out Sense result);
			if (bResult)
			{
				sense = result as T;
			}
			return bResult;
		}
		bool IPerceptions.TryGetSenseByType(in ESenses senseType, out Sense sense)
		{
			sense = null;
			switch (senseType)
			{
				case ESenses.DAMAGE:	return Senses.TryGetSense(typeof(Damage), out sense);
				case ESenses.HEARING:	return Senses.TryGetSense(typeof(Hearing), out sense);
				case ESenses.SIGHT:		return Senses.TryGetSense(typeof(Sight), out sense);
				case ESenses.TEAM:		return Senses.TryGetSense(typeof(Team), out sense);
			}
			return false;
		}
		bool IPerceptions.TryGetSense<T>(out T sense)
		{
			bool bResult = Senses.TryGetSense(typeof(T), out Sense result);
			{
				sense = result as T;
			}
			return bResult;
		}
		bool IPerceptions.TryGetSense(in System.Type senseType, out Sense sense)
		{
			Utils.CustomAssertions.IsTrue(Sense.TryGetSenseEnumType(senseType, out ESenses senseEnum));
			return m_MappedSenses.TryGetValue(senseEnum, out sense);
		}

		T IPerceptions.GetSense<T>() => Senses.GetSense(typeof(T)) as T;
		
		Sense IPerceptions.GetSense(in System.Type senseType) => Senses.TryGetSense(senseType, out Sense result) ? result : null;
		
		//////////////////////////////////////////////////////////////////////////
		void IPerceptions.SetSenseEnabled(in ESenses senseType, in bool bSenseEnabled)
		{
			if (Senses.TryGetSenseByType(senseType, out Sense sense))
			{
				sense.enabled = bSenseEnabled;
			}
		}

		//////////////////////////////////////////////////////////////////////////
		void IPerceptions.RemoveSense(in ESenses senseType)
		{
			if (Senses.TryGetSenseByType(senseType, out Sense sense))
			{
				m_MappedSenses.Remove(senseType);
				sense.gameObject.Destroy();
			}
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary> Stimulus received from a sense </summary>
		void IPerceptions.OnSenseEvent(in SenseEvent senseEvent) => m_OnNewSenseEvent(senseEvent);


#if UNITY_EDITOR
		[CustomEditor(typeof(AIPerceptionComponent))]
		// ^ This is the script we are making a custom editor for.
		private class AIPerceptionComponentEditor : Editor
		{
			static private System.Type[] m_CachedTypes = default;

			private AIPerceptionComponent instance = null;

			//////////////////////////////////////////////////////////////////////////
			static AIPerceptionComponentEditor()
			{
				m_CachedTypes = TypeCache.GetTypesDerivedFrom<Sense>().ToArray();
			}


			//////////////////////////////////////////////////////////////////////////
			private void OnEnable()
			{
				instance = target as AIPerceptionComponent;
			}


			//////////////////////////////////////////////////////////////////////////
			private void OnItemSelected_Add(object arg)
			{
				System.Type senseType = arg as System.Type;
				if (Sense.TryGetSenseEnumType(senseType, out ESenses senseEnum))
				{
					GameObject container = instance.EnsureSenseRoot();

					GameObject go = new GameObject(senseEnum.ToString());
					go.transform.SetParent(container.transform);
					go.transform.localPosition = Vector3.zero;
					go.transform.localRotation = Quaternion.identity;

					Sense sense = go.AddComponent(senseType) as Sense;
					instance.m_MappedSenses[senseEnum] = sense;
				}
			}

			//////////////////////////////////////////////////////////////////////////
			private void OnItemSelected_Rem(object arg)
			{
				System.Type senseType = arg as System.Type;
				if (Sense.TryGetSenseEnumType(senseType, out ESenses senseEnum))
				{
					instance.Senses.RemoveSense(senseEnum);
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
					foreach (System.Type item in m_CachedTypes.Except(instance.GetComponentsInChildren<Sense>(includeInactive: true).Select(s => s.GetType())).AsEnumerable())
					{
						bShow = true;
						menu.AddItem(new GUIContent(item.Name), false, OnItemSelected_Add, item);
					}

					if (bShow)
					{
						menu.ShowAsContext();
					}
				}

				if (instance.m_MappedSenses.Any() && GUILayout.Button("Remove Sense"))
				{
					GenericMenu menu = new GenericMenu();

					bool bShow = false;
					foreach (System.Type item in instance.m_MappedSenses.Values.Select(s => s.GetType()))
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
#endif
	}
}
