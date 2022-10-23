using System.Reflection;
using UnityEditor.UIElements;
using UnityEngine;

namespace Entities.AI.Components.Behaviours.Editor
{
	public class Vector2Resolver : FieldResolver<Vector2Field, Vector2>
	{
		public Vector2Resolver(FieldInfo fieldInfo) : base(fieldInfo)
		{
		}
		protected override Vector2Field CreateEditorField(FieldInfo fieldInfo)
		{
			return new Vector2Field(fieldInfo.Name);
		}
		public static bool IsAcceptable(FieldInfo info) => info.FieldType == typeof(Vector2);
	}
}
