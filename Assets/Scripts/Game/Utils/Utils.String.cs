
namespace Utils
{
	public static class String
	{
		public static bool IsAssetsPath(in string InPath) => InPath.StartsWith("Assets/");

		public static bool IsResourcesPath(in string InPath) => !IsAssetsPath(InPath);

		public static bool IsAbsolutePath(in string path)
		{
			try
			{
				return !string.IsNullOrWhiteSpace(path)
				&& path.IndexOfAny(System.IO.Path.GetInvalidPathChars()) == -1
				&& System.IO.Path.IsPathRooted(path) //  whether the specified path string contains a root.
				&& !System.IO.Path.GetPathRoot(path).Equals(System.IO.Path.DirectorySeparatorChar.ToString(), System.StringComparison.Ordinal);
			}
			catch (System.Exception)
			{
				return false;
			}
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary> </summary>
		public static bool TryConvertFromAssetPathToResourcePath(in string InAssetPath, out string OutResourcePath)
		{
			const string AssetPathPrefix = "Assets/Resources/";
			const int AssetPathPrefixLength = 17;

			if (!string.IsNullOrEmpty(InAssetPath))
			{
				if (IsResourcesPath(InAssetPath))
				{
					OutResourcePath = InAssetPath;
					return true;
				}

				if (InAssetPath.StartsWith(AssetPathPrefix))
				{
					OutResourcePath =
					// Assets/Resources/PATH_TO_FILE.png
					global::System.IO.Path.ChangeExtension(InAssetPath, null)
					// Assets/Resources/PATH_TO_FILE
					.Remove(0, AssetPathPrefixLength);
					// resourcePath -> // PATH_TO_FILE
					return true;
				}
			}
			OutResourcePath = string.Empty;
			return false;
		}


		//////////////////////////////////////////////////////////////////////////
		public static bool TryConvertFromResourcePathToAssetPath(in string InResourcePath, out string OutAssetPath)
		{
			const string AssetPathPrefix = "Assets";

			if (IsAssetsPath(InResourcePath))
			{
				OutAssetPath = InResourcePath;
				return true;
			}

			if (!string.IsNullOrEmpty(InResourcePath))
			{
				OutAssetPath = $"{AssetPathPrefix}/Resources/{InResourcePath}.asset";
				return true;
			}

			OutAssetPath = string.Empty;
			return false;
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary> </summary>
		public static bool TryConvertFromAbsolutePathToResourcePath(in string InAbsoluteAssetPath, out string OutResourcePath)
		{
			if (!string.IsNullOrEmpty(InAbsoluteAssetPath))
			{
				if (IsAbsolutePath(InAbsoluteAssetPath))
				{
					OutResourcePath = InAbsoluteAssetPath;
					return true;
				}

				int index = InAbsoluteAssetPath.IndexOf("Resources");
				if (index > -1)
				{
					// ABSOLUTE_PATH_TO_RESOURCE_FOLDER/Resources/PATH_TO_RESOURCE.png
					string result = InAbsoluteAssetPath;

					// Remove extension
					if (System.IO.Path.HasExtension(InAbsoluteAssetPath))
					{
						result = System.IO.Path.ChangeExtension(InAbsoluteAssetPath, null);
					}

					// ABSOLUTE_PATH_TO_RESOURCE_FOLDER/Resources/PATH_TO_RESOURCE
					OutResourcePath = result.Remove(0, index + 9 /*'Resource'*/ + 1 /*'/'*/ );
					// PATH_TO_RESOURCE
					return true;
				}
			}
			OutResourcePath = string.Empty;
			return false;
		}
	}
}

