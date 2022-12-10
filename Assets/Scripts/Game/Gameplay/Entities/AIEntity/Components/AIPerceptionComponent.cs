
using UnityEngine;

namespace Entities.AI.Components
{
	using Senses;
	using System.Collections.Generic;

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
		public event OnNewSenseEventDel OnNewSenseEvent = delegate { };

		[System.Serializable]
		private class SenseData
		{
			[SerializeField, ReadOnly]
			private		Sense			m_Sense					= null;
			[SerializeField, ReadOnly]
			private		ESenses			m_SenseType				= ESenses.NONE;
			[SerializeField, ReadOnly]
			private		Transform		m_TargetTransform		= null;

			public		Sense			Sense					=> m_Sense;
			public		ESenses			SenseType				=> m_SenseType;
			public		Transform		TargetTransform			=> m_TargetTransform;

			public SenseData(Sense InSense, ESenses InSenseType, Transform InTargetTransform)
			{
				m_Sense = InSense;
				m_SenseType = InSenseType;
				m_TargetTransform = InTargetTransform;
			}
		}

		[SerializeField]
		private List<SenseData> m_Senses = new List<SenseData>();


		//////////////////////////////////////////////////////////////////////////
		protected override void Awake()
		{
			base.Awake();

			CollectCurrentSenses();
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnValidate()
		{
			base.OnValidate();

			CollectCurrentSenses();
		}

		//////////////////////////////////////////////////////////////////////////
		private void CollectCurrentSenses()
		{
			m_Senses.Clear();

			Sense[] allSense = gameObject.GetComponentsInChildren<Sense>(includeInactive: true);
			m_Senses.Capacity = allSense.Length;

			foreach (Sense sense in allSense)
			{
				if (Utils.CustomAssertions.IsTrue(Sense.TryGetSenseEnumType(sense.GetType(), out ESenses senseEnum)))
				{
					m_Senses.Add(new SenseData(sense, senseEnum, sense.transform));
				}
			}
		}

		//////////////////////////////////////////////////////////////////////////
		/// <summary> Stimulus received from a sense </summary>
		public void SendSenseEvent(in SenseEvent senseEvent) => OnNewSenseEvent(senseEvent);

		//////////////////////////////////////////////////////////////////////////
		public void AddSense<T>(in Transform InTargetTransform = null) => AddSense(typeof(T), InTargetTransform);

		//////////////////////////////////////////////////////////////////////////
		public bool TryGetSenseByType<T>(out T sense, in ESenses senseType, in Transform InTargetTransform = null) where T : Sense, new()
		{
			sense = null;
			bool bResult = false;
			if (TryGetSenseByType(out Sense result, senseType, InTargetTransform) && result is T)
			{
				sense = result as T;
				bResult = true;
			}
			return bResult;
		}

		//////////////////////////////////////////////////////////////////////////
		public bool TryGetSense<T>(out T sense, in Transform InTargetTransform = null) where T : Sense, new()
		{
			bool bResult = TryGetSense(out Sense result, typeof(T), InTargetTransform);
			{
				sense = result as T;
			}
			return bResult;
		}

		//////////////////////////////////////////////////////////////////////////
		public T GetSense<T>() where T : Sense, new() => GetSense(typeof(T)) as T;


		//////////////////////////////////////////////////////////////////////////
		public Sense AddSense(in System.Type senseType, in Transform InTargetTransform = null)
		{
			Sense outValue = null;
			if (Sense.TryGetSenseEnumType(senseType, out ESenses senseEnum))
			{
				Transform targetTransform = InTargetTransform.IsNotNull() ? InTargetTransform : transform;
				outValue = targetTransform.gameObject.AddComponent(senseType) as Sense;
				m_Senses.Add(new SenseData(outValue, senseEnum, targetTransform));
			}
			return outValue;
		}

		//////////////////////////////////////////////////////////////////////////
		public bool TryGetSenseByType(out Sense sense, in ESenses InSenseType, in Transform InTargetTransform = null)
		{
			sense = null;
			bool bResult = false;
			switch (InSenseType)
			{
				case ESenses.DAMAGE:
				{
					bResult = TryGetSense(out sense, typeof(Damage), InTargetTransform);
					break;
				}
				case ESenses.HEARING:
				{
					bResult = TryGetSense(out sense, typeof(Hearing), InTargetTransform);
					break;
				}
				case ESenses.SIGHT:
				{
					bResult = TryGetSense(out sense, typeof(Sight), InTargetTransform);
					break;
				}
				case ESenses.TEAM:
				{
					bResult = TryGetSense(out sense, typeof(Team), InTargetTransform);
					break;
				}
			}
			return bResult;
		}

		//////////////////////////////////////////////////////////////////////////
		public bool TryGetSense(out Sense OutSense, in System.Type InSenseType, in Transform InTargetTransform = null)
		{
			OutSense = null;
			bool bResult = false;
			if (Sense.TryGetSenseEnumType(InSenseType, out ESenses senseEnum))
			{
				if (InTargetTransform.IsNotNull())
				{
					bResult = InTargetTransform.TryGetComponent(out OutSense);
				}
				else
				{
					if (bResult = m_Senses.TryFind(out SenseData OutSenseData, out int _, s => s.SenseType == senseEnum))
					{
						OutSense = OutSenseData.Sense;
					}
				}
			}
			return bResult;
		}

		//////////////////////////////////////////////////////////////////////////
		public Sense GetSense(in System.Type senseType, in Transform InTargetTransform = null) => TryGetSense(out Sense result, senseType, InTargetTransform) ? result : null;

		//////////////////////////////////////////////////////////////////////////
		public void SetSenseEnabled(in ESenses senseType, in bool bSenseEnabled, in Transform InTargetTransform = null)
		{
			if (TryGetSenseByType(out Sense sense, senseType, InTargetTransform))
			{
				sense.enabled = bSenseEnabled;
			}
		}

		//////////////////////////////////////////////////////////////////////////
		public void RemoveSense(ESenses senseType, in Transform InTargetTransform = null)
		{
			if (TryGetSenseByType(out Sense sense, senseType, InTargetTransform))
			{
				if (m_Senses.TryFind(out SenseData _, out int index, s => s.Sense == sense))
				{
					m_Senses.RemoveAt(index);
				}
				sense.Destroy();
			}
		}
	}
}

#if UNITY_EDITOR
namespace Entities.AI.Components
{
	using UnityEditor;

	public partial class AIPerceptionComponent
	{
		[CustomEditor(typeof(AIPerceptionComponent))]
		public class AIPerceptionComponentEditor : Editor
		{
			private static readonly System.Type[] m_CachedTypes = default;

			private AIPerceptionComponent perceptionComponent = null;

			/*
			//////////////////////////////////////////////////////////////////////////
			static AIPerceptionComponentEditor()
			{
				m_CachedTypes = TypeCache.GetTypesDerivedFrom<Sense>().Where(t => !t.IsAbstract).ToArray();
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
			*/
		}
	}
}
#endif
