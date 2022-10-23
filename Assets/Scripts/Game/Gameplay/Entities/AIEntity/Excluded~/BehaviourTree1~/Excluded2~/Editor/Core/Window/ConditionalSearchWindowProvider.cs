using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Entities.AI.Components.Behaviours.Editor
{
	public class ConditionalSearchWindowProvider : ScriptableObject, ISearchWindowProvider
	{
		private readonly BehaviourTreeNode node;

		public ConditionalSearchWindowProvider(BehaviourTreeNode node)
		{
			this.node = node;
		}

		List<SearchTreeEntry> ISearchWindowProvider.CreateSearchTree(SearchWindowContext context)
		{
			var entries = new List<SearchTreeEntry>();
			entries.Add(new SearchTreeGroupEntry(new GUIContent("Select Condition")));

			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				foreach (var type in assembly.GetTypes())
				{
					if (type.IsSubclassOf(typeof(Conditional)))
					{
						entries.Add(new SearchTreeEntry(new GUIContent(type.Name)) { level = 1, userData = type });
					}
				}
			}

			return entries;
		}

		bool ISearchWindowProvider.OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
		{
			var type = searchTreeEntry.userData as System.Type;
			this.node.SetBehaviour(type);
			return true;
		}

	}
}
