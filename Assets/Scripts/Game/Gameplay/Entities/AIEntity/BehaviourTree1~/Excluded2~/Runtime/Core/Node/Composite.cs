using System.Collections.Generic;
using UnityEngine;

namespace Entities.AI.Components.Behaviours
{
	public abstract class Composite : NodeBehaviour
	{
		[SerializeReference]
		private List<NodeBehaviour> children = new List<NodeBehaviour>();

		public List<NodeBehaviour> Children => children;

		protected sealed override void OnRun()
		{
			children.ForEach( e => e.Run(gameObject));
		}

		public sealed override void Awake()
		{
			OnAwake();
			children.ForEach( e => e.Awake());
		}

		protected virtual void OnAwake()
		{
		}

		public sealed override void Start()
		{
			OnStart();
			children.ForEach(c => c.Start());
		}

		protected virtual void OnStart()
		{
		}

		public sealed override void PreUpdate()
		{
			children.ForEach(c => c.PreUpdate());
		}

		public sealed override void PostUpdate()
		{
			children.ForEach(c => c.PostUpdate());
		}

#if UNITY_EDITOR
		public void AddChild(NodeBehaviour child)
		{
			children.Add(child);
		}
#endif

	}
}
