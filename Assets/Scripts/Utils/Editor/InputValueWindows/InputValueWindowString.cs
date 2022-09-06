using UnityEngine;
using UnityEditor;

namespace EditorUtils
{
	public static partial class InputValueWindow
	{
		public static void OpenStringInput(System.Action<string> InOnAccepted, System.Action InOnCancel)
		{
			InputValueWindowString.OpenWindow(InOnAccepted, InOnCancel);
		}
	}

	internal class InputValueWindowString : InputValueWindowBase<string>
	{
		private				string										m_Value									= string.Empty;

		private				System.Action<string>						m_InOnAccepted							= delegate { };
		private				System.Action								m_InOnCancel							= delegate { };

		public static void OpenWindow(System.Action<string> InOnAccepted, System.Action InOnCancel)
		{
			InputValueWindowString window = OpenWindow<InputValueWindowString>("String input window");
			if (window)
			{
				window.m_InOnAccepted = InOnAccepted ?? window.m_InOnAccepted;
				window.m_InOnCancel = InOnCancel ?? window.m_InOnCancel;
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
		protected override void OnCancel()
		{
			
		}

		//////////////////////////////////////////////////////////////////////////
		protected override bool TryAcceptValue()
		{
			m_InOnAccepted(m_Value);
			return true;
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnGUIInternal()
		{
			m_Value = EditorGUILayout.TextField(m_Value);
		}
	}
}
