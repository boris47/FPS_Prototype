using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[System.Serializable]
public sealed class WindowData
{
	[System.Serializable]
	private class SerializableKeyValue
	{
		public string Key = string.Empty;
		public string Value = string.Empty;

		public SerializableKeyValue(in string InKey, in string InValue)
		{
			Key = InKey;
			Value = InValue;
		}
	}

	public readonly GuardedEditorWindowBase WindowRef = null;

	[SerializeField] private string m_TypeName = null;
	[SerializeField] private string m_AssemblyName = null;
	[SerializeField] private string m_WindowTitle = null;
	[SerializeField] private Vector3 m_MinSize = Vector3.zero;
	[SerializeField] private Vector3 m_MaxSize = Vector3.zero;

	[SerializeField] private List<SerializableKeyValue> m_OthersData = new List<SerializableKeyValue>();

	public string TypeName => m_TypeName;
	public string AssemblyName => m_AssemblyName;
	public string WindowTitle => m_WindowTitle;
	public Vector3 MinSize => m_MinSize;
	public Vector3 MaxSize => m_MaxSize;

	//////////////////////////////////////////////////////////////////////////
	public WindowData(GuardedEditorWindowBase InWindowRef)
	{
		WindowRef = InWindowRef;
		System.Type editorType = WindowRef.GetType();
		m_TypeName = editorType.FullName;
		m_AssemblyName = editorType.Assembly.FullName;
	}

	//////////////////////////////////////////////////////////////////////////
	public bool IsValid()
	{
		bool bResult = true;
		{
			bResult &= !string.IsNullOrEmpty(m_TypeName);
			bResult &= !string.IsNullOrEmpty(m_AssemblyName);
			bResult &= !string.IsNullOrEmpty(m_WindowTitle);
		}
		return bResult;
	}

	//////////////////////////////////////////////////////////////////////////
	public void SetData(string InKey, string InValue)
	{
		if (m_OthersData.TryFind(out SerializableKeyValue keyValuePair, out int index, pair => pair.Key == InKey))
		{
			keyValuePair.Value = InValue;
		}
		else
		{
			m_OthersData.Add(new SerializableKeyValue(InKey, InValue));
		}
	}

	//////////////////////////////////////////////////////////////////////////
	public bool TryGetValue(string InKey, out string OutValue)
	{
		OutValue = null;
		return m_OthersData.TryFind(out SerializableKeyValue keyValuePair, out int index, pair => pair.Key == InKey) && (OutValue = keyValuePair.Value).IsNotNull();
	}

	//////////////////////////////////////////////////////////////////////////
	public string GetValue(string InKey)
	{
		string OutValue = null;
		if (m_OthersData.TryFind(out SerializableKeyValue keyValuePair, out int index, pair => pair.Key == InKey))
		{
			OutValue = keyValuePair.Value;
		}
		return OutValue;
	}

	//////////////////////////////////////////////////////////////////////////
	public void SaveWindowData()
	{
		m_WindowTitle = WindowRef.titleContent.text;
		m_MinSize = WindowRef.minSize;
		m_MaxSize = WindowRef.maxSize;
		WindowRef.SaveBeforeClosingForReload(this);
	}

	//////////////////////////////////////////////////////////////////////////
	public void ToWindow(GuardedEditorWindowBase window)
	{
		window.titleContent = new GUIContent(m_WindowTitle);
		window.minSize = m_MinSize;
		window.maxSize = m_MaxSize;
		window.ReOpen(this);
	}
}


public class EditorWindowGuardian : ScriptableSingleton<EditorWindowGuardian>
{
	const uint k_FrameToWait = 10u;

	[SerializeField]
	private List<WindowData> m_WindowsData = new List<WindowData>();

	private bool m_IsCompiling = false;
	private uint m_FrameCount = 0u;


	//////////////////////////////////////////////////////////////////////////
	public void PushWindow(GuardedEditorWindowBase window)
	{
		m_WindowsData.Add(new WindowData(window));
	}

	//////////////////////////////////////////////////////////////////////////
	public void RemoveWindow(GuardedEditorWindowBase window)
	{
		if (!m_IsCompiling && m_WindowsData.TryFind(out WindowData _, out int outIndex, data => data.WindowRef == window))
		{
			m_WindowsData.RemoveAt(outIndex);
		}
	}

	//////////////////////////////////////////////////////////////////////////
	private void OnEnable()
	{
		AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
		AssemblyReloadEvents.afterAssemblyReload -= OnAfterAssemblyReload;

		AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
		AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
	}
	
	//////////////////////////////////////////////////////////////////////////
	private static bool TryRetrieveWindowAfterReload(in WindowData InWindowData, out GuardedEditorWindowBase OutEditorWindow)
	{
		OutEditorWindow = null;
		System.Type windowType = System.Type.GetType($"{InWindowData.TypeName}, {InWindowData.AssemblyName}");
		if (windowType.IsNotNull() && EditorWindow.GetWindow(windowType) is GuardedEditorWindowBase editorWindow)
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
			for (int i = m_WindowsData.Count - 1; i >= 0; i--)
			{
				WindowData windowData = m_WindowsData[i];
				GuardedEditorWindowBase editorWindow = windowData.WindowRef;
				if (editorWindow.IsNotNull())
				{
					// Save all the window data useful at re-create
					windowData.SaveWindowData();

					editorWindow.SaveChanges();
					editorWindow.Close();
				}
				else
				{
					m_WindowsData.RemoveAt(i);
					Debug.LogWarning($"Unable to restore window of type {windowData.TypeName}");
				}
			}
		}
	}

	//////////////////////////////////////////////////////////////////////////
	private void OnAfterAssemblyReload()
	{
		if (EditorApplication.isPlayingOrWillChangePlaymode)
		{
			return;
		}

		if (EditorApplication.isCompiling || EditorApplication.isUpdating)
		{
			EditorApplication.delayCall += OnAfterAssemblyReload;
			return;
		}

		m_FrameCount = k_FrameToWait;

		EditorApplication.update += OnAfterAssemblyReloadDelayed;
	}

	//////////////////////////////////////////////////////////////////////////
	private void OnAfterAssemblyReloadDelayed()
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

		for (int i = m_WindowsData.Count - 1; i >= 0; i--)
		{
			WindowData windowData = m_WindowsData[i];

			if (windowData.IsValid() && TryRetrieveWindowAfterReload(windowData, out GuardedEditorWindowBase editorWindow))
			{
				windowData.ToWindow(editorWindow);
			}
			else
			{
				m_WindowsData.RemoveAt(i);
			}
		}
	}
}

