using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Entities.AI.Components.Behaviours.Editor
{
	public class GraphEditorWindow : EditorWindow
	{
		// GraphView window per GameObject
		private static readonly Dictionary<GameObject,GraphEditorWindow> cache = new Dictionary<GameObject, GraphEditorWindow>();

		private GameObject key { get; set; }

		public static void Show(BehaviourTree bt)
		{
			var window = Create(bt);
			window.Show();
			window.Focus();
		}

		private static GraphEditorWindow Create(BehaviourTree bt)
		{
			var key = bt.gameObject;
			if (cache.ContainsKey(key))
			{
				return cache[key];
			}

			var window = CreateInstance<GraphEditorWindow>();
			StructGraphView(window, bt);
			window.titleContent = new GUIContent($"BehaviourTree Editor({bt.gameObject.name})");
			window.key = key;
			cache[key] = window;
			return window;
		}

		private static void StructGraphView(GraphEditorWindow window, BehaviourTree behaviourTree)
		{
			window.rootVisualElement.Clear();
			var graphView = new BehaviourTreeView(behaviourTree, window);
			graphView.Restore();
			window.rootVisualElement.Add(window.CreateToolBar(graphView));
			window.rootVisualElement.Add(graphView);
		}

		private void OnDestroy()
		{
			if (key != null && cache.ContainsKey(key))
			{
				cache.Remove(key);
			}
		}

		private void OnPlayModeStateChanged(PlayModeStateChange playModeStateChange)
		{
			switch (playModeStateChange)
			{
				case PlayModeStateChange.EnteredEditMode:
					Reload();
					break;
				case PlayModeStateChange.ExitingEditMode:
					break;
				case PlayModeStateChange.EnteredPlayMode:
					Reload();
					break;
				case PlayModeStateChange.ExitingPlayMode:
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(playModeStateChange), playModeStateChange, null);
			}
		}

		private void OnEnable()
		{
			EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
			Reload();
		}

		private void OnDisable()
		{
			EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
		}

		private void Reload()
		{
			if (key != null)
			{
				StructGraphView(this, key.GetComponent<BehaviourTree>());
				Repaint();
			}
		}

		private VisualElement CreateToolBar(BehaviourTreeView graphView)
		{
			return new IMGUIContainer(
				() =>
				{
					GUILayout.BeginHorizontal(EditorStyles.toolbar);

					if (!Application.isPlaying)
					{
						if (GUILayout.Button("Save", EditorStyles.toolbarButton))
						{
							var guiContent = new GUIContent();
							if (graphView.Save())
							{
								guiContent.text = "Successfully updated.";
								this.ShowNotification(guiContent);
							}
							else
							{
								guiContent.text = "Invalid tree. one or more nodes have error.";
								this.ShowNotification(guiContent);
							}
						}
					}
					GUILayout.FlexibleSpace();
					GUILayout.EndHorizontal();
				}
			);

		}


	}
}
