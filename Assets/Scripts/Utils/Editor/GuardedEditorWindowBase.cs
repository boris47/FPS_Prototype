using UnityEngine;
using UnityEditor;

public abstract class GuardedEditorWindowBase : EditorWindow
{
	//////////////////////////////////////////////////////////////////////////
	public static T CreateGuardedWindow<T>(in string InWindowTitle, in Vector2? InMinSize = null, in Vector2? InMaxSize = null) where T : GuardedEditorWindowBase
	{
		T windowInstance = GetWindow<T>(utility: true);
		{
			windowInstance.titleContent = new GUIContent(InWindowTitle);
			windowInstance.minSize = InMinSize ?? new Vector2(600f, 700f);
			windowInstance.maxSize = InMaxSize ?? new Vector2(600f, 900f);
		}
		return windowInstance;
	}

	//////////////////////////////////////////////////////////////////////////
	public virtual void SaveBeforeClosingForReload(in WindowData InWindowData)
	{
		
	}

	//////////////////////////////////////////////////////////////////////////
	public void ReOpen(in WindowData InWindowData)
	{
		EditorWindow window = CreateEditorWindow(InWindowData);

		BeforeWindowRestore(InWindowData);
		
		window.Show();

		AfterWindowRestore();
	}

	//////////////////////////////////////////////////////////////////////////
	protected abstract EditorWindow CreateEditorWindow(in WindowData InWindowData);

	//////////////////////////////////////////////////////////////////////////
	protected virtual void BeforeWindowRestore(in WindowData InWindowData)
	{

	}

	//////////////////////////////////////////////////////////////////////////
	protected virtual void AfterWindowRestore()
	{

	}

	//////////////////////////////////////////////////////////////////////////
	protected virtual void OnEnable()
	{
		EditorWindowGuardian.instance.PushWindow(this);
	}

	//////////////////////////////////////////////////////////////////////////
	protected virtual void OnDisable()
	{
		EditorWindowGuardian.instance.RemoveWindow(this);
	}
}