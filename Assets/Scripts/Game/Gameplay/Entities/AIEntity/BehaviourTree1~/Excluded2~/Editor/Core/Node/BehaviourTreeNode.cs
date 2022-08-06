using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Entities.AI.Components.Behaviours.Editor
{
	public abstract class BehaviourTreeNode : Node {

		protected NodeBehaviour NodeBehaviour { set;  get; }

		private Type dirtyNodeBehaviourType;

		public Port Parent { private set; get; }

		private readonly VisualElement container;

		private readonly TextField description;

		private readonly FieldResolverFactory fieldResolverFactory;

		private readonly List<IFieldResolver> resolvers = new List<IFieldResolver>();

		protected BehaviourTreeNode()
		{
			fieldResolverFactory = new FieldResolverFactory();
			container = new VisualElement();
			description = new TextField();
			Initialize();
		}

		private void Initialize()
		{
			AddDescription();
			mainContainer.Add(this.container);
			AddParent();
		}

		protected virtual void AddDescription()
		{
			description.RegisterCallback<FocusInEvent>(evt => { Input.imeCompositionMode = IMECompositionMode.On; });
			description.RegisterCallback<FocusOutEvent>(evt => { Input.imeCompositionMode = IMECompositionMode.Auto; });
			mainContainer.Add(description);
		}

		public void Restore(NodeBehaviour Behaviour)
		{
			NodeBehaviour = Behaviour;
			resolvers.ForEach(e => e.Restore(NodeBehaviour));
			NodeBehaviour.NotifyEditor = MarkAsExecuted;
			description.value = NodeBehaviour.description;
			OnRestore();
		}

		protected virtual void OnRestore()
		{

		}

		public NodeBehaviour ReplaceBehaviour()
		{
			this.NodeBehaviour = Activator.CreateInstance(GetBehaviour()) as NodeBehaviour;
			return NodeBehaviour;
		}

		protected virtual void AddParent()
		{
			Parent = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(Port));
			Parent.portName = "Parent";
			inputContainer.Add(Parent);
		}

		protected Port CreateChildPort()
		{
			var port = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(Port));
			port.portName = "Child";
			return port;
		}

		private Type GetBehaviour()
		{
			return dirtyNodeBehaviourType;
		}

		public void Commit(Stack<BehaviourTreeNode> stack)
		{
			OnCommit(stack);
			resolvers.ForEach( r => r.Commit(NodeBehaviour));
			NodeBehaviour.description = this.description.value;
			NodeBehaviour.graphPosition = GetPosition();
			NodeBehaviour.NotifyEditor = MarkAsExecuted;
		}
		protected abstract void OnCommit(Stack<BehaviourTreeNode> stack);

		public bool Validate(Stack<BehaviourTreeNode> stack)
		{
			var valid = GetBehaviour() != null && OnValidate(stack);
			if (valid)
			{
				style.backgroundColor = new StyleColor(StyleKeyword.Null);
			}
			else
			{
				style.backgroundColor = Color.red;
			}
			return valid;
		}

		protected abstract bool OnValidate(Stack<BehaviourTreeNode> stack);

		public void SetBehaviour(System.Type nodeBehaviour)
		{
			if (dirtyNodeBehaviourType != null)
			{
				dirtyNodeBehaviourType = null;
				container.Clear();
				resolvers.Clear();
			}
			dirtyNodeBehaviourType = nodeBehaviour;

			nodeBehaviour
				.GetFields(BindingFlags.Public | BindingFlags.Instance)
					.Where(field => field.GetCustomAttribute<HideInEditorWindow>() == null)
				.Concat(GetAllFields(nodeBehaviour))
				.Where(field => field.IsInitOnly == false)
				.ToList().ForEach((p) =>
				{
					var fieldResolver = fieldResolverFactory.Create(p);
					var defaultValue = Activator.CreateInstance(nodeBehaviour) as NodeBehaviour;
					fieldResolver.Restore(defaultValue);
					container.Add( fieldResolver.GetEditorField());
					resolvers.Add(fieldResolver);
				});
			title = nodeBehaviour.Name;
		}

		private static IEnumerable<FieldInfo> GetAllFields(Type t)
		{
			if (t == null)
				return Enumerable.Empty<FieldInfo>();

			return t.GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
				.Where(field => field.GetCustomAttribute<SerializeField>() != null)
				.Where(field => field.GetCustomAttribute<HideInEditorWindow>() == null).Concat(GetAllFields(t.BaseType));
		}

		private void MarkAsExecuted(Status status)
		{
			switch (status)
			{
				case Status.Failure:
				{
					style.backgroundColor = Color.red;
					break;
				}
				case Status.Running:
				{
					style.backgroundColor = Color.yellow;
					break;
				}
				case Status.Success:
				{
					style.backgroundColor = Color.green;
					break;
				}
			}
		}

		public void ClearStyle()
		{
			style.backgroundColor = new StyleColor(StyleKeyword.Null);
			OnClearStyle();
		}

		protected abstract void OnClearStyle();

	}
}
