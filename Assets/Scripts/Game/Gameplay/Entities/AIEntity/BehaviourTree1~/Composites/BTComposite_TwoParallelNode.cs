/*
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Entities.AI.Components.Behaviours
{
	public enum EBTParallelMode
	{
		/// <summary> When main task finishes, immediately abort background tree. </summary>
		AbortBackground,
		/// <summary> When main task finishes, wait for background tree to finish. </summary>
		WaitForBackground,
	}

	/// <summary>
	/// Two Parallel composite node. <br/>
	/// Allows for running two children: one which must be a single action node, and the other of which can be a composite. <br/>
	/// </summary>
	public partial class BTComposite_TwoParallelNode : BTCompositeNode
	{
		public override string NodeName => "Two Parallel";
		public override string NodeInfo => "Allows for running two children: one which must be a single action node, and the other of which can be a composite";

		[SerializeField]
		protected EBTParallelMode m_parallelMode = EBTParallelMode.AbortBackground;

		[SerializeField, HideInInspector]
		protected BTActionNode m_ActionNode = null;

		[SerializeField, HideInInspector]
		protected BTNode m_ChildNode = null;


		//////////////////////////////////////////////////////////////////////////
		protected override void OnStartInternal()
		{
			
		}

		//////////////////////////////////////////////////////////////////////////
		protected override EBTNodeResult OnFrameInternal(in float InDeltaTime)
		{
			return EBTNodeResult.SUCCEEDED;
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnFinishedInternal()
		{
			
		}
	}
}
*/
