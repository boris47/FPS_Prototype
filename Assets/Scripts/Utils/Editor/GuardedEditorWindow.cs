using UnityEngine;
using UnityEditor;

public abstract class EditorWindowWithResource : EditorWindow
{
	protected string m_ResourcePath = null;

	public string ResourcePath => m_ResourcePath;

	public abstract void ReOpen(in string InWindowTitle, in string InResourcePath, in Vector2 InMinSize, in Vector2 InMaxSize);
}


public abstract class GuardedEditorWindow<T, D> : EditorWindowWithResource where T : EditorWindowWithResource where D : ScriptableObject
{
	private static		T								m_WindowInstance		= null;
	private static		GuardedEditorWindow<T, D>		m_WindowInstanceGuarded	= null;
	
	protected			D								m_Data					= null;


	//////////////////////////////////////////////////////////////////////////
	public static void OpenWindow(in string InWindowTitle, in string InResourcePath, in Vector2? InMinSize = null, in Vector2? InMaxSize = null)
	{
		D OutData = null;
		if (Utils.CustomAssertions.IsTrue(Utils.Paths.TryConvertFromResourcePathToAssetPath(InResourcePath, out string AssetPath)))
		{
			AssetDatabase.CreateAsset(OutData = ScriptableObject.CreateInstance<D>(), AssetPath);
		}
		OpenWindow(InWindowTitle, InResourcePath, OutData, InMinSize, InMaxSize);
	}


	//////////////////////////////////////////////////////////////////////////
	public static void OpenWindow(string InWindowTitle, string InResourcePath, D InData, Vector2? InMinSize = null, Vector2? InMaxSize = null)
	{
		if (Utils.CustomAssertions.IsNotNull(InData))
		{
			m_WindowInstance = GetWindow<T>(utility: true);
			m_WindowInstance.titleContent = new GUIContent(InWindowTitle);
			m_WindowInstance.minSize = InMinSize ?? new Vector2(600f, 700f);
			m_WindowInstance.maxSize = InMaxSize ?? new Vector2(600f, 900f);

			m_WindowInstanceGuarded = m_WindowInstance as GuardedEditorWindow<T, D>;
			m_WindowInstanceGuarded.m_ResourcePath = InResourcePath;
			m_WindowInstanceGuarded.m_Data = InData;
			
			m_WindowInstance.Show();
		}
	}

	//////////////////////////////////////////////////////////////////////////
	public sealed override void ReOpen(in string InWindowTitle, in string InResourcePath, in Vector2 InMinSize, in Vector2 InMaxSize)
	{
		OpenWindow(InWindowTitle, InResourcePath, InMinSize, InMaxSize);
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

		if (m_Data.IsNotNull())
		{
			AssetDatabase.SaveAssetIfDirty(m_Data);
			EditorUtility.ClearDirty(m_Data);
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnDestroy()
	{
		m_WindowInstance		= null;
		m_WindowInstanceGuarded	= null;

		m_Data					= null;
	}
}
