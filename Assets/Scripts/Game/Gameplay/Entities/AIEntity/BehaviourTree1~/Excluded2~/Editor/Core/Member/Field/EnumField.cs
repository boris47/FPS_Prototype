using System;
using System.Collections.Generic;
using UnityEditor.UIElements;

namespace Entities.AI.Components.Behaviours.Editor
{
	public class EnumField : PopupField<Enum>
	{
		public EnumField(string label, List<Enum> choices, Enum defaultValue = null)
			: base(label, choices,  defaultValue, null, null) {
		}
	}
}
