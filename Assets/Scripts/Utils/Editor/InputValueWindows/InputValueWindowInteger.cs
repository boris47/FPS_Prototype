using UnityEngine;
using UnityEditor;

namespace EditorUtils
{
	public static partial class InputValueWindow
	{
		public static void OpenIntegerInput(System.Func<int, bool> InTryAcceptValue, System.Action InOnCancel)
		{
			InputValueWindowInteger.OpenWindow(InTryAcceptValue, InOnCancel);
		}
	}

	internal class InputValueWindowInteger : InputValueWindowBase<int>
	{
		public static void OpenWindow(System.Func<int, bool> InTryAcceptValue, System.Action InOnCancel)
		{
			InputValueWindowInteger window = OpenWindow<InputValueWindowInteger>("Integer input window");
			if (window)
			{
				window.m_TryAcceptValue = InTryAcceptValue;
				window.m_InOnCancel = InOnCancel ?? window.m_InOnCancel;
			}
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnGUIInternal()
		{
			m_Value = EditorGUILayout.IntField(m_Value);
		}
	}
}
