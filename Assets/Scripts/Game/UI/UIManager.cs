using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class UIManager : MonoBehaviour
{
	[StructLayout(LayoutKind.Sequential)]
	public struct POINT
	{
		public int X;
		public int Y;
	}

	[DllImport("User32.Dll")]
	internal static extern long SetCursorPos(int x, int y);

	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	internal static extern bool GetCursorPos(out POINT lpPoint);


	private	static	UI_MainMenu				m_MainMenu						= null;
	private	static	UI_InGame				m_InGame						= null;
	private static	UI_SettingsMenu				m_Settings						= null;

	[SerializeField, ReadOnly]
	private			UI_Base					m_CurrentActiveUI				= null;

	[SerializeField, ReadOnly]
	private			UI_Base					m_PreviousActiveUI				= null;

	[SerializeField]
	private			Stack<UI_Base>			m_History						= new Stack<UI_Base>();


	[SerializeField]
	private			Transform				m_RayCastInterceptor			= null;


	//////////////////////////////////////////////////////////////////////////
	private void Awake()
	{
		transform.TrySearchComponentByChildName("UI_MainMenu",				out m_MainMenu);
		transform.TrySearchComponentByChildName("UI_InGameMenu",			out m_InGame);

		m_RayCastInterceptor.gameObject.SetActive(false);

		m_CurrentActiveUI = m_InGame.gameObject.activeSelf
			? m_InGame as UI_Base
			: m_MainMenu.gameObject.activeSelf
			? m_MainMenu as UI_Base : null;
	}


	//////////////////////////////////////////////////////////////////////////
	public bool IsCurrentActive(UI_Base menu) => m_CurrentActiveUI == menu;


	//////////////////////////////////////////////////////////////////////////
	private IEnumerator SwitchTo(UI_Base InUIToShow, UISwitchStrategy InUISwitchStrategy)
	{
		if (Utils.CustomAssertions.IsNotNull(InUISwitchStrategy) && Utils.CustomAssertions.IsNotNull(InUIToShow))
		{
			if (m_CurrentActiveUI != InUIToShow)
			{
				// Save the current cursor position on the screen
				bool bResult = GetCursorPos(out POINT lastCursorPosition);

				yield return InUISwitchStrategy.ExecuteUISwitch(m_CurrentActiveUI, InUIToShow);

				m_PreviousActiveUI = m_CurrentActiveUI;

				// Switch to new menu
				m_CurrentActiveUI = InUIToShow;

				// Re-set the cursor position
				if (bResult)
				{
					SetCursorPos(lastCursorPosition.X, lastCursorPosition.Y);
				}
			}
		}
	}
}