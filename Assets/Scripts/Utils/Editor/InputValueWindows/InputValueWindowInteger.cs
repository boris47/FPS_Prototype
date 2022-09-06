using UnityEngine;
using UnityEditor;

namespace EditorUtils
{
	public static partial class InputValueWindow
	{
		public static void OpenIntegerInput(System.Action<int> InOnAccepted, System.Action InOnCancel)
		{
			InputValueWindowInteger.OpenWindow(InOnAccepted, InOnCancel);
		}
	}

	internal class InputValueWindowInteger : InputValueWindowBase<int>
	{
		private				int											m_Value									= 0;

		private				System.Action<int>							m_InOnAccepted							= delegate { };
		private				System.Action								m_InOnCancel							= delegate { };

		public static void OpenWindow(System.Action<int> InOnAccepted, System.Action InOnCancel)
		{
			InputValueWindowInteger window = OpenWindow<InputValueWindowInteger>("Integer input window");
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
			m_Value = EditorGUILayout.IntField(m_Value);
		}
	}
}
