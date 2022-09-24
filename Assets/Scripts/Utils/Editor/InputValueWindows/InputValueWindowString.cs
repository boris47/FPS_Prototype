using UnityEngine;
using UnityEditor;

namespace EditorUtils
{
	public static partial class InputValueWindow
	{
		public static void OpenStringInput(System.Func<string, bool> InTryAcceptValue, System.Action InOnCancel, in string InStartValue = null)
		{
			InputValueWindowString.OpenWindow(InTryAcceptValue, InOnCancel, InStartValue);
		}
	}

	internal class InputValueWindowString : InputValueWindowBase<string>
	{
		public static void OpenWindow(System.Func<string, bool> InTryAcceptValue, System.Action InOnCancel, in string InStartValue = null)
		{
			InputValueWindowString window = OpenWindow<InputValueWindowString>("String input window");
			if (window)
			{
				window.m_TryAcceptValue = InTryAcceptValue;
				window.m_InOnCancel = InOnCancel ?? window.m_InOnCancel;
				if (!string.IsNullOrEmpty(InStartValue))
				{
					window.m_Value = InStartValue;
				}
			}
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnEnable()
		{
			base.OnEnable();

			EditorGUIUtility.editingTextField = true;
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnDisable()
		{
			base.OnDisable();

			EditorGUIUtility.editingTextField = false;
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnGUIInternal()
		{
			m_Value = EditorGUILayout.TextField(m_Value);
		}
	}
}
