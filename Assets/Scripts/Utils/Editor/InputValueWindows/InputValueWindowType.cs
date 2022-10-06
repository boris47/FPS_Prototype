using UnityEngine;
using UnityEditor;

namespace EditorUtils
{
	using System.Linq;

	public static partial class InputValueWindow
	{
		public static void OpenSystemTypeType(System.Func<System.Type, bool> InTryAcceptValue, System.Action InOnCancel, in System.Type[] InOptions, in System.Type InStartValue = null)
		{
			InputValueWindowType.OpenWindow(InTryAcceptValue, InOnCancel, InOptions, InStartValue);
		}
	}

	internal class InputValueWindowType : InputValueWindowBase<System.Type>
	{
		private System.Type[] m_Options = null;
		private string[] m_Names = null;
		private int m_SelectedIndex = 0;

		public static void OpenWindow(System.Func<System.Type, bool> InTryAcceptValue, System.Action InOnCancel, in System.Type[] InOptions, in System.Type InStartValue = null)
		{
			if (InOptions == null || InOptions.Length == 0)
			{
				Debug.LogError($"Invlid Options");
				return;
			}

			InputValueWindowType window = OpenWindow<InputValueWindowType>("Type input window");
			if (window)
			{
				window.m_TryAcceptValue = InTryAcceptValue;
				window.m_InOnCancel = InOnCancel ?? window.m_InOnCancel;
				window.m_Options = InOptions;
				window.m_Names = InOptions.Select(t => t.Name).ToArray();
				if (InStartValue.IsNotNull() && InOptions.Contains(InStartValue))
				{
					window.m_Value = InStartValue;
					window.m_SelectedIndex = System.Array.IndexOf(InOptions, InStartValue);
				}
			}
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnEnable()
		{
			base.OnEnable();
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnDisable()
		{
			base.OnDisable();
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnGUIInternal()
		{
			int tempIndex = EditorGUILayout.Popup(m_SelectedIndex, m_Names);
			if (tempIndex != m_SelectedIndex)
			{
				m_Value = m_Options[tempIndex];
			}
			m_SelectedIndex = tempIndex;
		}
	}
}
