
using System.Linq;
using UnityEngine;

namespace Entities.AI.Components.Behaviours
{
	/// <summary>
	/// Selector composite node.<br/>
	/// Selector Nodes execute their children from left to right, and will stop executing its children when one of their children succeeds.<br/>
	/// If a Selector's child succeeds, the Selector succeeds. If all the Selector's children fail, the Selector fails.<br/>
	/// </summary>
	public partial class BTComposite_SelectorNode : BTCompositeNode
	{
		public override		string			NodeName			=> "Selector";
		public override		string			NodeInfo			=> "Executes its children from left to right, and will stop executing its children when one of their children succeeds";


		//////////////////////////////////////////////////////////////////////////
		public override void OnChildFinished(in BTNode InNode, in EBTNodeState InChildState)
		{
			if (CustomAssertions.IsTrue(m_CurrentIndex == Children.IndexOf(InNode)))
			{
				if (InChildState == EBTNodeState.SUCCEEDED)
				{
					m_NodeState = EBTNodeState.SUCCEEDED;
				}

				if (InChildState == EBTNodeState.FAILED)
				{
					// Start thinking the selector has failed
					m_NodeState = EBTNodeState.FAILED;

					// If next child index is not out of range
					if (Children.IsValidIndex(m_CurrentIndex + 1))
					{
						// Search or the next child which activation don't fail
						if (Children.TryFind(out BTNode node, out int OutIndex, child => child.TryActivation(), (int)(m_CurrentIndex + 1)))
						{
							// If a child is found, its index will be the used on
							m_CurrentIndex = (uint)OutIndex;
							m_NodeState = EBTNodeState.RUNNING;
						}
					}
				}
			}
		}
	}
}
