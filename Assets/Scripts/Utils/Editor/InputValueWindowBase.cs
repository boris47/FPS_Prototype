using UnityEngine;
using UnityEditor;

namespace EditorUtils
{
	public static partial class InputValueWindow
	{
		
	}

	internal abstract class InputValueWindowBase<T> : EditorWindow
	{
		protected			T											m_Value									= default;

		protected			System.Func<T, bool>						m_TryAcceptValue						= null;
		protected			System.Action								m_InOnCancel							= delegate { };

		//////////////////////////////////////////////////////////////////////////
		protected static WindowType OpenWindow<WindowType>(in string InTitle) where WindowType : InputValueWindowBase<T>, new()
		{
			WindowType window = EditorWindow.CreateWindow<WindowType>(InTitle);
			window.minSize = new Vector2(600f, 700f);
			window.maxSize = new Vector2(600f, 900f);
			return window;
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
							Close();
						}
					}
					if (GUILayout.Button("Cancel"))
					{
						OnCancel();
						Close();
					}
				}
				GUILayout.EndHorizontal();
			}
			GUILayout.EndVertical();
		}
	}
}
