using UnityEngine;
using UnityEditor;

namespace EditorUtils
{
	public static partial class InputValueWindow
	{
		public static void OpenBooleanInput(System.Action<bool> InOnAccepted, System.Action InOnCancel)
		{
			InputValueWindowBoolean.OpenWindow(InOnAccepted, InOnCancel);
		}
	}

	internal class InputValueWindowBoolean : InputValueWindowBase<bool>
	{
		private				bool										m_Value									= false;

		private				System.Action<bool>							m_InOnAccepted							= delegate { };
		private				System.Action								m_InOnCancel							= delegate { };

		public static void OpenWindow(System.Action<bool> InOnAccepted, System.Action InOnCancel)
		{
			InputValueWindowBoolean window = OpenWindow<InputValueWindowBoolean>("Bool input window");
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
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnDisable()
		{
			base.OnDisable();
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
			m_Value = EditorGUILayout.Toggle(m_Value);
		}
	}
}
