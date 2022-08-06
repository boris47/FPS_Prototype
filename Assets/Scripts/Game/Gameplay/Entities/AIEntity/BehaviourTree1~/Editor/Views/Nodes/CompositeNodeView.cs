using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.Experimental.GraphView;

namespace Entities.AI.Components.Behaviours
{
	internal class CompositeNodeView : NodeViewBase
	{
		private IBTCompositeNodeEditorInterface m_Composite = null;

		private Editor m_ConditionalEditor = null;
		private Color m_PreviousBackgroundColor;


		public CompositeNodeView(in BTNode InNode, in EdgeConnectorListener InEdgeConnectorListener, in bool bIsBehaviourTreeInstance)
		: base(InNode, InEdgeConnectorListener, bIsBehaviourTreeInstance)
		{
			m_Composite = InNode as BTCompositeNode;

			style.width = 300f;

			m_Aux.style.alignContent = Align.Stretch;
			m_Aux.style.alignItems = Align.Stretch;

			if (m_Composite.GetConditional().IsNotNull())
			{
				CreateConditionalEditor(m_Composite.GetConditional());
			}
			else
			{
				DestroyConditionalEditor();
			}
		}

		//////////////////////////////////////////////////////////////////////////
		protected override Port CreateInputPort()
		{
			Port input = base.CreatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, m_BehaviourTreeNode);
			inputContainer.Add(input);
			return input;
		}

		//////////////////////////////////////////////////////////////////////////
		protected override List<Port> CreateOutputPorts()
		{
			Port output = base.CreatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Multi, m_BehaviourTreeNode);
			outputContainer.Add(output);
			return new List<Port>() { output };
		}

		//////////////////////////////////////////////////////////////////////////
		public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
		{
			base.BuildContextualMenu(evt);

			if (evt.target is CompositeNodeView)
			{
				if (m_Composite.GetConditional().IsNotNull())
				{
					evt.menu.AppendAction("Remove Conditional", action => RemoveConditional());
				}
				else
				{
					foreach (System.Type conditional in TypeCache.GetTypesDerivedFrom<BTConditional>())
					{
						evt.menu.AppendAction($"Conditionals/{conditional.Name}", action => SetConditional(conditional));
					}
				}
			}
		}

		//////////////////////////////////////////////////////////////////////////
		private void RegisterOnInspectorGUI()
		{
			if (m_ConditionalEditor?.serializedObject?.targetObject.IsNotNull() ?? false)
			{
				m_ConditionalEditor.OnInspectorGUI();
			}
		}

		//////////////////////////////////////////////////////////////////////////
		private void SetConditional(in System.Type InConditionalType)
		{
			BTConditional conditionalInstance = (BTConditional)UnityEngine.ScriptableObject.CreateInstance(InConditionalType);

			BehaviourTree behaviourTree = (m_Composite as BTCompositeNode).BehaviourTree;
			EditorUtility.SetDirty(behaviourTree);
			{
				AssetDatabase.AddObjectToAsset(conditionalInstance, behaviourTree);
			}
			AssetDatabase.SaveAssetIfDirty(behaviourTree);

			m_Composite.SetConditioanl(conditionalInstance);
			(conditionalInstance as IBTConditionalEditorInterface).NodeConditionalized = m_Composite as BTCompositeNode;
			CreateConditionalEditor(conditionalInstance);
		}

		//////////////////////////////////////////////////////////////////////////
		private void CreateConditionalEditor(in BTConditional InConditionalInstance)
		{
			m_Aux.Clear();
			UnityEngine.Object.DestroyImmediate(m_ConditionalEditor);
			m_ConditionalEditor = Editor.CreateEditor(InConditionalInstance, typeof(BTConditional.BTConditionalEditor));

			IMGUIContainer container = new IMGUIContainer(RegisterOnInspectorGUI);
			m_Aux.Add(container);

			m_PreviousBackgroundColor = m_Aux.style.backgroundColor.value;
			m_Aux.style.backgroundColor = Color.red;
			m_Aux.style.paddingBottom = m_Aux.style.paddingTop = 5f;
			m_Aux.style.paddingLeft = m_Aux.style.paddingRight = 3f;
			m_Aux.style.alignContent = Align.Stretch;
			m_Aux.style.alignItems = Align.Stretch;
		}

		//////////////////////////////////////////////////////////////////////////
		private void DestroyConditionalEditor()
		{
			m_Aux.Clear();
			if (m_ConditionalEditor)
			{
				UnityEngine.Object.DestroyImmediate(m_ConditionalEditor);
			}
			Label label = new Label("No conditional");
			{
				label.style.alignContent = Align.Center;
			}
			m_Aux.Add(label);

			m_Aux.style.backgroundColor = m_PreviousBackgroundColor;
			m_Aux.style.paddingBottom = m_Aux.style.paddingTop = 0f;
			m_Aux.style.paddingLeft = m_Aux.style.paddingRight = 0f;
			m_Aux.style.alignContent = Align.Center;
			m_Aux.style.alignItems = Align.Center;
		}

		//////////////////////////////////////////////////////////////////////////
		private void RemoveConditional()
		{
			DestroyConditionalEditor();
			AssetDatabase.RemoveObjectFromAsset(m_Composite.GetConditional());
			m_Composite.SetConditioanl(null);
		}
	}
}
