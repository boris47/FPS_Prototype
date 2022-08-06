
using UnityEngine;

namespace Entities.Player.Components
{
	[System.AttributeUsage(System.AttributeTargets.Class, Inherited = false)]
	public class MotionStrategyConfigType : System.Attribute
	{
		public readonly System.Type ConfigType = null;
		public MotionStrategyConfigType(System.Type InConfigType)
		{
			ConfigType = InConfigType;
		}
	}

	[DefaultExecutionOrder(20)]
	public abstract class MotionStrategyBase : PlayerEntityComponent
	{
		public abstract bool IsMotionConditionValid { get; }
	}
}
