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
	private void OnBeforeAssemblyReload()
	{
		m_IsCompiling = true;
		for (int i = instance.m_WindowsData.Count - 1; i >= 0; i--)
		{
			WindowData windowData = instance.m_WindowsData[i];

			// Save all the window data useful at re-create
			windowData.SaveWindowData();

			if (Utils.Types.IsNotNull(System.Type.GetType($"{windowData.TypeName}, {windowData.AssemblyName}"), out System.Type windowType))
			{
				if (Utils.CustomAssertions.IsTrue(Utils.Types.IsNotNull(EditorWindow.GetWindow(windowType), out EditorWindowWithResource editorWindow)))
				{
					editorWindow.SaveChanges();

					editorWindow.Close();
				}
			}
			else
			{
				instance.m_WindowsData.RemoveAt(i);
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

		m_FrameCount = k_FrameToWait;
		EditorApplication.update += OnAfterAssemblyReload_Internal;
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnAfterAssemblyReload_Internal()
	{
		if (m_FrameCount > 0)
		{
			m_FrameCount--;
			return;
		}

		m_IsCompiling = false;

		// No need to update this method, unsubscribe from the application update
		EditorApplication.update -= OnAfterAssemblyReload_Internal;

		//////////////////////////////////////////////////////////////////////////////////////
		
		for (int i = instance.m_WindowsData.Count - 1; i >= 0; i--)
		{
			WindowData windowData = instance.m_WindowsData[i];
			if (Utils.Types.IsNotNull(System.Type.GetType($"{windowData.TypeName}, {windowData.AssemblyName}"), out System.Type windowType))
			{
				if (Utils.CustomAssertions.IsTrue(Utils.Types.IsNotNull(EditorWindow.GetWindow(windowType), out EditorWindowWithResource editorWindow)))
				{
					windowData.ToWindow(editorWindow);

					editorWindow.Show();
				}
			}
			else
			{
				instance.m_WindowsData.RemoveAt(i);
			}
		}
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