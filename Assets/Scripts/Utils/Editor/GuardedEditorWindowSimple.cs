using UnityEditor;
using UnityEngine;

public abstract class GuardedEditorWindowSimple<WindowType> : GuardedEditorWindowBase where WindowType : GuardedEditorWindowSimple<WindowType>, new()
{
	//////////////////////////////////////////////////////////////////////////
	protected sealed override EditorWindow CreateEditorWindow(in WindowData InWindowData)
	{
		return CreateGuardedWindow<WindowType>(InWindowData.WindowTitle, InWindowData.MinSize, InWindowData.MaxSize);
	}
}
