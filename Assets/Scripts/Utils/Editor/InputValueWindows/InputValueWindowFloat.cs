using UnityEngine;
using UnityEditor;

namespace EditorUtils
{
	public static partial class InputValueWindow
	{
		public static void OpenFloatInput(System.Action<float> InOnAccepted, System.Action InOnCancel)
		{
			InputValueWindowFloat.OpenWindow(InOnAccepted, InOnCancel);
		}
	}

	internal class InputValueWindowFloat : InputValueWindowBase<float>
	{
		private				float										m_Value									= 0f;

		private				System.Action<float>						m_InOnAccepted							= delegate { };
		private				System.Action								m_InOnCancel							= delegate { };

		public static void OpenWindow(System.Action<float> InOnAccepted, System.Action InOnCancel)
		{
			InputValueWindowFloat window = OpenWindow<InputValueWindowFloat>("Float input window");
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
			m_Value = EditorGUILayout.FloatField(m_Value);
		}
	}
}
