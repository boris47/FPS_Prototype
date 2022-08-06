using System;
using System.Reflection;
using UnityEngine.UIElements;

namespace Entities.AI.Components.Behaviours.Editor
{
	sealed class Ordered : Attribute
	{
		public int Order = 100;
	}

	public interface IFieldResolver
	{
		VisualElement GetEditorField();

		void Restore(NodeBehaviour Behaviour);

		void Commit(NodeBehaviour Behaviour);
	}

	public abstract class FieldResolver<T, K> :IFieldResolver where T: BaseField<K>
	{
		private readonly FieldInfo fieldInfo;
		private T editorField;

		protected FieldResolver(FieldInfo fieldInfo)
		{
			this.fieldInfo = fieldInfo;
			SetEditorField();
		}

		private void SetEditorField()
		{
			this.editorField = this.CreateEditorField(this.fieldInfo);
		}

		protected abstract T CreateEditorField(FieldInfo fieldInfo);

		public VisualElement GetEditorField()
		{
			return this.editorField;
		}

		public void Restore(NodeBehaviour Behaviour)
		{
			editorField.value = (K)fieldInfo.GetValue(Behaviour);
		}

		public void Commit(NodeBehaviour Behaviour)
		{
		   fieldInfo.SetValue(Behaviour, editorField.value);
		}
	}
}
