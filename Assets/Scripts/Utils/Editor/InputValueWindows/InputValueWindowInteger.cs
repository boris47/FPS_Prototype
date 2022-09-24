using UnityEngine;
using UnityEditor;

namespace EditorUtils
{
	public static partial class InputValueWindow
	{
		public static void OpenIntegerInput(System.Func<int, bool> InTryAcceptValue, System.Action InOnCancel, in int? InStartValue = null)
		{
			InputValueWindowInteger.OpenWindow(InTryAcceptValue, InOnCancel, InStartValue);
		}
	}

	internal class InputValueWindowInteger : InputValueWindowBase<int>
	{
		public static void OpenWindow(System.Func<int, bool> InTryAcceptValue, System.Action InOnCancel, in int? InStartValue = null)
		{
			InputValueWindowInteger window = OpenWindow<InputValueWindowInteger>("Integer input window");
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
			m_Value = EditorGUILayout.IntField(m_Value);
		}
	}
}
