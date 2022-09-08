using UnityEngine;
using UnityEditor;

namespace EditorUtils
{
	public static partial class InputValueWindow
	{
		
	}

	internal abstract class InputValueWindowBase<T> : EditorWindow
	{
		public static		InputValueWindowBase<T>						s_Window								= null;

		protected			T											m_Value									= default;

		protected			System.Func<T, bool>						m_TryAcceptValue						= null;
		protected			System.Action								m_InOnCancel							= delegate { };

		//////////////////////////////////////////////////////////////////////////
		protected static WindowType OpenWindow<WindowType>(in string InTitle) where WindowType : InputValueWindowBase<T>, new()
		{
			if (s_Window == null)
			{
				s_Window = EditorWindow.CreateWindow<WindowType>(InTitle);
				s_Window.minSize = new Vector2(600f, 700f);
				s_Window.maxSize = new Vector2(600f, 900f);
			}
			return s_Window as WindowType;
		}

		//////////////////////////////////////////////////////////////////////////
		protected virtual void OnEnable()
		{
			
		}

		//////////////////////////////////////////////////////////////////////////
		protected virtual void OnDisable()
		{
			
		}

		//////////////////////////////////////////////////////////////////////////
		protected abstract void OnGUIInternal();

		//////////////////////////////////////////////////////////////////////////
		/// <summary> Validate value and accept if valid </summary>
		private bool TryAcceptValue()
		{
			return m_TryAcceptValue?.Invoke(m_Value) ?? true;
		}

		//////////////////////////////////////////////////////////////////////////
		private void OnCancel()
		{
			m_InOnCancel();
		}


		//////////////////////////////////////////////////////////////////////////
		private void OnGUI()
		{
			GUILayout.BeginVertical();
			{
				GUILayout.Label("Value");

				OnGUIInternal();

				GUILayout.BeginHorizontal();
				{
					if (GUILayout.Button("OK"))
					{
						if (TryAcceptValue())
						{
							s_Window.Close();
						}
					}
					if (GUILayout.Button("Cancel"))
					{
						OnCancel();
						s_Window.Close();
					}
				}
				GUILayout.EndHorizontal();
			}
			GUILayout.EndVertical();
		}
	}
}
