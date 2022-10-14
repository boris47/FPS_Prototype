using UnityEngine;
using UnityEditor;

namespace EditorUtils
{
	using System.Linq;

	public static partial class InputValueWindow
	{
		public static void OpenDropdown(System.Func<string, bool> InTryAcceptValue, System.Action InOnCancel, in string[] InOptions, in string InStartValue = null)
		{
			InputValueWindowDropdown.OpenWindow(InTryAcceptValue, InOnCancel, InOptions, InStartValue);
		}
	}

	internal class InputValueWindowDropdown : InputValueWindowBase<string>
	{
		private string[] m_Options = null;
		private int m_SelectedIndex = 0;

		public static void OpenWindow(System.Func<string, bool> InTryAcceptValue, System.Action InOnCancel, in string[] InOptions, in string InStartValue = null)
		{
			if (InOptions == null || InOptions.Length == 0)
			{
				Debug.LogError($"Invlid Options");
				return;
			}

			InputValueWindowDropdown window = OpenWindow<InputValueWindowDropdown>("Dropdown window");
			if (window)
			{
				window.m_TryAcceptValue = InTryAcceptValue;
				window.m_InOnCancel = InOnCancel ?? window.m_InOnCancel;
				window.m_Options = InOptions;
				if (InStartValue != null && InOptions.Contains(InStartValue))
				{
					window.m_Value = InStartValue;
					window.m_SelectedIndex = System.Array.IndexOf(InOptions, InStartValue);
				}
				else
				{
					window.m_Value = InOptions[0];
					window.m_SelectedIndex = 0;
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
			int tempIndex = EditorGUILayout.Popup(m_SelectedIndex, m_Options);
			if (tempIndex != m_SelectedIndex)
			{
				m_Value = m_Options[tempIndex];
			}
			m_SelectedIndex = tempIndex;
		}
	}
}
