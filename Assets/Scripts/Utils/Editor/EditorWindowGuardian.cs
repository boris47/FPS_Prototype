using UnityEngine;
using UnityEditor;
using System.Collections.Generic;


public class EditorWindowGuardian : ScriptableSingleton<EditorWindowGuardian>
{
	const uint k_FrameToWait = 10u;

	[SerializeField]
	private List<WindowData> m_WindowsData = new List<WindowData>();

	private bool m_IsCompiling = false;
	private uint m_FrameCount = 0u;


	//////////////////////////////////////////////////////////////////////////
	public void PushWindow(EditorWindowWithResource window)
	{
		m_WindowsData.Add(new WindowData(window));
	}

	//////////////////////////////////////////////////////////////////////////
	public void RemoveWindow(EditorWindowWithResource window)
	{
		if (!m_IsCompiling && m_WindowsData.TryFind(out WindowData _, out int outIndex, data => data.WindowRef == window))
		{
			m_WindowsData.RemoveAt(outIndex);
		}
	}

	//////////////////////////////////////////////////////////////////////////
	private void OnEnable()
	{
		AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
		AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
	}

	//////////////////////////////////////////////////////////////////////////
	private static bool TryRetrieveWindowBeforeReload(in WindowData InWindowData, out EditorWindowWithResource OutEditorWindow)
	{
		static bool HasOpenInstances(System.Type windowType)
		{
			Object[] array = Resources.FindObjectsOfTypeAll(windowType);
			return array != null && array.Length != 0;
		}

		OutEditorWindow = null;
		System.Type windowType = System.Type.GetType($"{InWindowData.TypeName}, {InWindowData.AssemblyName}");
		if (windowType.IsNotNull() && HasOpenInstances(windowType) && EditorWindow.GetWindow(windowType) is EditorWindowWithResource editorWindow)
		{
			OutEditorWindow = editorWindow;
		}
		return OutEditorWindow.IsNotNull();
	}

	//////////////////////////////////////////////////////////////////////////
	private static bool TryRetrieveWindowAfterReload(in WindowData InWindowData, out EditorWindowWithResource OutEditorWindow)
	{
		OutEditorWindow = null;
		System.Type windowType = System.Type.GetType($"{InWindowData.TypeName}, {InWindowData.AssemblyName}");
		if (windowType.IsNotNull() && EditorWindow.GetWindow(windowType) is EditorWindowWithResource editorWindow)
		{
			OutEditorWindow = editorWindow;
		}
		return OutEditorWindow.IsNotNull();
	}

	//////////////////////////////////////////////////////////////////////////
	private void OnBeforeAssemblyReload()
	{
		if (!EditorApplication.isPlayingOrWillChangePlaymode)
		{
			m_IsCompiling = true;
			for (int i = instance.m_WindowsData.Count - 1; i >= 0; i--)
			{
				WindowData windowData = instance.m_WindowsData[i];
				if (TryRetrieveWindowBeforeReload(windowData, out EditorWindowWithResource editorWindow))
				{
					// Save all the window data useful at re-create
					windowData.SaveWindowData();

					editorWindow.SaveChanges();
					editorWindow.Close();
				}
				else
				{
					instance.m_WindowsData.RemoveAt(i);
					Debug.LogWarning($"Unable to restore window of type {windowData.TypeName}");
				}
			}
		}
	}

	//////////////////////////////////////////////////////////////////////////
	private void OnAfterAssemblyReload()
	{
		if (EditorApplication.isCompiling || EditorApplication.isUpdating)
		{
			EditorApplication.delayCall += OnAfterAssemblyReload;
			return;
		}

		if (EditorApplication.isPlayingOrWillChangePlaymode)
		{
			return;
		}

		m_FrameCount = k_FrameToWait;


		void OnAfterAssemblyReloadDelayed()
		{
			if (m_FrameCount > 0)
			{
				m_FrameCount--;
				return;
			}

			m_IsCompiling = false;

			// No need to update this method, unsubscribe from the application update
			EditorApplication.update -= OnAfterAssemblyReloadDelayed;

			//////////////////////////////////////////////////////////////////////////////////////
		
			for (int i = instance.m_WindowsData.Count - 1; i >= 0; i--)
			{
				WindowData windowData = instance.m_WindowsData[i];

				if (windowData.IsValid() && TryRetrieveWindowAfterReload(windowData, out EditorWindowWithResource editorWindow))
				{
					windowData.ToWindow(editorWindow);

					editorWindow.Show();
				}
				else
				{
					instance.m_WindowsData.RemoveAt(i);
				}
			}
		}
		EditorApplication.update += OnAfterAssemblyReloadDelayed;
	}



	[System.Serializable]
	private class WindowData
	{
		public readonly EditorWindowWithResource WindowRef = null;

		[SerializeField] public string TypeName = null;
		[SerializeField] public string AssemblyName = null;

		[SerializeField] private string WindowTitle = null;
		[SerializeField] private Vector3 MinSize = Vector3.zero;
		[SerializeField] private Vector3 MaxSize = Vector3.zero;
		[SerializeField] private string ResourcePath = null;


		public WindowData(EditorWindowWithResource InWindowRef)
		{
			WindowRef = InWindowRef;

			System.Type editorType = WindowRef.GetType();
			TypeName = editorType.FullName;
			AssemblyName = editorType.Assembly.FullName;
		}

		public bool IsValid()
		{
			bool bResult = true;
			{
				bResult &= !string.IsNullOrEmpty(TypeName);
				bResult &= !string.IsNullOrEmpty(AssemblyName);
				bResult &= !string.IsNullOrEmpty(WindowTitle);
				bResult &= !string.IsNullOrEmpty(ResourcePath);
			}
			return bResult;
		}

		public void SaveWindowData()
		{
			WindowTitle = WindowRef.titleContent.text;
			MinSize = WindowRef.minSize;
			MaxSize = WindowRef.maxSize;
			ResourcePath = WindowRef.ResourcePath;
		}

		public void ToWindow(EditorWindowWithResource window)
		{
			window.titleContent = new GUIContent(WindowTitle);
			window.minSize = MinSize;
			window.maxSize = MaxSize;
			window.ReOpen(WindowTitle, ResourcePath, MinSize, MaxSize);
		}
	}
}