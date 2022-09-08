using UnityEngine;
using UnityEditor;

namespace EditorUtils
{
	public static partial class InputValueWindow
	{
		public static void OpenBooleanInput(System.Func<bool, bool> InTryAcceptValue, System.Action InOnCancel)
		{
			InputValueWindowBoolean.OpenWindow(InTryAcceptValue, InOnCancel);
		}
	}

	internal class InputValueWindowBoolean : InputValueWindowBase<bool>
	{
		public static void OpenWindow(System.Func<bool, bool> InTryAcceptValue, System.Action InOnCancel)
		{
			InputValueWindowBoolean window = OpenWindow<InputValueWindowBoolean>("Bool input window");
			if (window)
			{
				window.m_TryAcceptValue = InTryAcceptValue;
				window.m_InOnCancel = InOnCancel ?? window.m_InOnCancel;
			}
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnGUIInternal()
		{
			m_Value = EditorGUILayout.Toggle(m_Value);
		}
	}
}
