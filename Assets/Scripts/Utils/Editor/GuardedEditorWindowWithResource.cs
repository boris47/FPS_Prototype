using UnityEngine;
using UnityEditor;

public abstract class GuardedEditorWindowWithResource<WindowType, ResourceType> : GuardedEditorWindowSimple<WindowType>
	where WindowType : GuardedEditorWindowWithResource<WindowType, ResourceType>, new()
	where ResourceType : ScriptableObject
{
	private				ResourceType								m_Data					= null;
	protected			ResourceType								Data					=> m_Data;

	protected			string										m_ResourcePath			= null;

	public				string										ResourcePath			=> m_ResourcePath;

	
	//////////////////////////////////////////////////////////////////////////
	public static void LoadAndOpenGuardedWindow(in string InWindowTitle, in string InResourcePath, in Vector2? InMinSize = null, in Vector2? InMaxSize = null)
	{
		ResourceType OutData = null;
		if (Utils.CustomAssertions.IsTrue(Utils.Paths.TryConvertFromResourcePathToAssetPath(InResourcePath, out string AssetPath)))
		{
			OutData = AssetDatabase.LoadAssetAtPath<ResourceType>(AssetPath);
			if (OutData == null)
			{
				System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(AssetPath));

				AssetDatabase.CreateAsset(OutData = ScriptableObject.CreateInstance<ResourceType>(), AssetPath);
			}
		}
		OpenGuardedWindowWithResources(InWindowTitle, InResourcePath, OutData, InMinSize, InMaxSize);
	}

	//////////////////////////////////////////////////////////////////////////
	public static void OpenGuardedWindowWithResources(string InWindowTitle, string InResourcePath, ResourceType InData, Vector2? InMinSize = null, Vector2? InMaxSize = null)
	{
		if (Utils.CustomAssertions.IsNotNull(InData))
		{
			WindowType windowInstance = CreateGuardedWindow<WindowType>(InWindowTitle, InMinSize, InMaxSize);

			windowInstance.m_ResourcePath = InResourcePath;
			windowInstance.m_Data = InData;
			windowInstance.OnBeforeShow();
			windowInstance.Show();
		}
	}

	//////////////////////////////////////////////////////////////////////////
	public sealed override void SaveBeforeClosingForReload(in WindowData InWindowData)
	{
		base.SaveBeforeClosingForReload(InWindowData);

		InWindowData.SetData("ResourcePath", m_ResourcePath);
	}

	//////////////////////////////////////////////////////////////////////////
	protected sealed override void BeforeWindowRestore(in WindowData InWindowData)
	{
		base.BeforeWindowRestore(InWindowData);

		m_ResourcePath = InWindowData.GetValue("ResourcePath");

		ResourceType OutData = null;
		if (Utils.CustomAssertions.IsTrue(Utils.Paths.TryConvertFromResourcePathToAssetPath(m_ResourcePath, out string AssetPath)))
		{
			OutData = AssetDatabase.LoadAssetAtPath<ResourceType>(AssetPath);
			if (OutData == null)
			{
				System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(AssetPath));

				AssetDatabase.CreateAsset(OutData = ScriptableObject.CreateInstance<ResourceType>(), AssetPath);
			}
		}
		m_Data = OutData;

		OnBeforeShow();
	}

	//////////////////////////////////////////////////////////////////////////
	/// <summary> Called after data load </summary>
	protected virtual void OnBeforeShow()
	{

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

		if (m_Data.IsNotNull())
		{
			AssetDatabase.SaveAssetIfDirty(m_Data);
			EditorUtility.ClearDirty(m_Data);
		}
	}

	//////////////////////////////////////////////////////////////////////////
	protected virtual void OnDestroy()
	{
		m_Data = null;
	}
}