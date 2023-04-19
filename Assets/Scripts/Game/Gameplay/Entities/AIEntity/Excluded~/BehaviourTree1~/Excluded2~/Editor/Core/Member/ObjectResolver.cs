using System.Reflection;
using UnityEditor.UIElements;

namespace Entities.AI.Components.Behaviours.Editor
{
	public class ObjectResolver : FieldResolver<ObjectField,UnityEngine.Object>
	{
		public ObjectResolver(FieldInfo fieldInfo) : base(fieldInfo)
		{
		}
		protected override ObjectField CreateEditorField(FieldInfo fieldInfo)
		{
			var editorField = new ObjectField(fieldInfo.Name);
			editorField.objectType = fieldInfo.FieldType;
			return editorField;
		}

	}
}