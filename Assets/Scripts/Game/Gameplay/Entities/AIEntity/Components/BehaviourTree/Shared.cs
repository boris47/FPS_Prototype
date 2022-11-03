using UnityEngine;
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
		BTNode[] Children { get; }
	}

	[System.Serializable]
	public enum EBTNodeInitializationResult
	{
		/// <summary> Produce the call on OnNodeUpdate </summary>
		RUNNING = EBTNodeState.RUNNING,
		/// <summary> Terminate the node with failure </summary>
		FAILED = EBTNodeState.FAILED,
		/// <summary> Terminate the node with success </summary>
		SUCCEEDED = EBTNodeState.SUCCEEDED
	}

	[System.Serializable]
	public enum EBTNodeState
	{
		/// <summary> No State </summary>
		INACTIVE,
		/// <summary> Not finished yet </summary>
		RUNNING,
		/// <summary> finished as failure </summary>
		FAILED,
		/// <summary> finished as success </summary>
		SUCCEEDED,
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

