using System.Reflection;
using UnityEditor.UIElements;

namespace Entities.AI.Components.Behaviours.Editor
{
	public class LongResolver : FieldResolver<LongField,long>
	{
		public LongResolver(FieldInfo fieldInfo) : base(fieldInfo)
		{
		}
		protected override LongField CreateEditorField(FieldInfo fieldInfo)
		{
			return new LongField(fieldInfo.Name);
		}
		public static bool IsAcceptable(FieldInfo info) => info.FieldType == typeof(long);
	}
}
