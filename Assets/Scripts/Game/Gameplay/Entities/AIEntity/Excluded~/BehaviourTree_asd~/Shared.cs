﻿using UnityEngine;
using System.Collections.Generic;

namespace Entities.AI.Components.Behaviours
{
	[System.Serializable]
	internal enum ESpace
	{
		[InspectorName("2D")]
		BiDimensional,
		[InspectorName("3D")]
		ThreeDimensional,
	};

	[System.Serializable]
	public enum EBehaviourTreeState
	{
		STOPPED,
		RUNNING,
		PAUSED,
		INVALID
	}

	public interface IBTNodeTickable
	{
		void UpdateTickable(in float InDeltaTime);
	}

	public interface IParentNode
	{
		List<BTNode> Children { get; }
	}

	[System.Serializable]
	public enum EBTNodeState
	{
		INACTIVE,       // not has not state defined
		RUNNING,        // not finished yet
	//	ABORTING,       // start aborting
		FAILED,         // finished as failure
		SUCCEEDED,      // finished as success
	//	ABORTED,        // finished aborting
	}

	public class BTNodeDetailsAttribute : System.Attribute
	{
		public readonly string Name = string.Empty;

		public readonly string Description = string.Empty;

		public BTNodeDetailsAttribute(string InName)
		{
			Name = InName;
		}

		public BTNodeDetailsAttribute(string InName, string InDescription)
		{
			Name = InName;
			Description = InDescription;
		}
	}
}
