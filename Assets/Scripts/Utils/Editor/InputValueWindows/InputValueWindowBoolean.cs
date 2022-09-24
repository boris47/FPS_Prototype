using UnityEngine;
using UnityEditor;

namespace EditorUtils
{
	public static partial class InputValueWindow
	{
		public static void OpenBooleanInput(System.Func<bool, bool> InTryAcceptValue, System.Action InOnCancel, in bool? InStartValue = null)
		{
			InputValueWindowBoolean.OpenWindow(InTryAcceptValue, InOnCancel, InStartValue);
		}
	}

	internal class InputValueWindowBoolean : InputValueWindowBase<bool>
	{
		public static void OpenWindow(System.Func<bool, bool> InTryAcceptValue, System.Action InOnCancel, in bool? InStartValue = null)
		{
			InputValueWindowBoolean window = OpenWindow<InputValueWindowBoolean>("Bool input window");
			if (window)
			{
				window.m_TryAcceptValue = InTryAcceptValue;
				window.m_InOnCancel = InOnCancel ?? window.m_InOnCancel;
				if (InStartValue.HasValue)
				{
					window.m_Value = InStartValue.Value;
				}
			}
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnGUIInternal()
		{
			m_Value = EditorGUILayout.Toggle(m_Value);
		}
	}
}
