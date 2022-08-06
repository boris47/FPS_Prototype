using UnityEngine;

namespace Entities.AI.Components.Behaviours
{
	public enum UpdateType
	{
	   Auto,
	   Manual
	}

	/// <summary>
	/// <see href="https://github.com/yoshidan/UniBT"/>
	/// </summary>
	public class BehaviourTree : MonoBehaviour
	{

		[HideInInspector]
		[SerializeReference]
		private Root root = new Root();

		[SerializeField]
		private UpdateType updateType;

		public Root Root
		{
			get => root;
#if UNITY_EDITOR
			set => root = value;
#endif
		}

		private void Awake() {
			root.Run(gameObject);
			root.Awake();
		}

		private void Start()
		{
			root.Start();
		}

		private void Update()
		{
			if (updateType == UpdateType.Auto) Tick();
		}

		public void Tick()
		{
			root.PreUpdate();
			root.Update();
			root.PostUpdate();
		}

	}
}
