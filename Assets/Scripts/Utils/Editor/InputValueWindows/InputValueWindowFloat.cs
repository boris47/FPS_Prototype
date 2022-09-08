using UnityEngine;
using UnityEditor;

namespace EditorUtils
{
	public static partial class InputValueWindow
	{
		public static void OpenFloatInput(System.Func<float, bool> InTryAcceptValue, System.Action InOnCancel)
		{
			InputValueWindowFloat.OpenWindow(InTryAcceptValue, InOnCancel);
		}
	}

	internal class InputValueWindowFloat : InputValueWindowBase<float>
	{
		public static void OpenWindow(System.Func<float, bool> InTryAcceptValue, System.Action InOnCancel)
		{
			InputValueWindowFloat window = OpenWindow<InputValueWindowFloat>("Float input window");
			if (window)
			{
				window.m_TryAcceptValue = InTryAcceptValue;
				window.m_InOnCancel = InOnCancel ?? window.m_InOnCancel;
			}
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnGUIInternal()
		{
			m_Value = EditorGUILayout.FloatField(m_Value);
		}
	}
}
