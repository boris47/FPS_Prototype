using System.Reflection;
using UnityEditor.UIElements;
using UnityEngine;

namespace Entities.AI.Components.Behaviours.Editor
{
	public class BoundsIntResolver : FieldResolver<BoundsIntField,BoundsInt>
	{
		public BoundsIntResolver(FieldInfo fieldInfo) : base(fieldInfo)
		{
		}
		protected override BoundsIntField CreateEditorField(FieldInfo fieldInfo)
		{
			return new BoundsIntField(fieldInfo.Name);
		}
		public static bool IsAcceptable(FieldInfo info) => info.FieldType == typeof(BoundsInt);
	}
}
