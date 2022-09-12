
using UnityEngine;


[System.AttributeUsage(System.AttributeTargets.Class, Inherited = false)]
public class Configurable : System.Attribute
{
	public const string MainFolderName = "Configs";

	public readonly string FieldName = null;
	public readonly System.Type ConfigurationType = null;
	public readonly string ResourcePath = null;

	private Configurable(string InResourcePath)
	{
		ResourcePath = $"{MainFolderName}/{InResourcePath}";
	}

	public Configurable(System.Type InConfigurationType, string InResourcePath) : this(InResourcePath)
	{
		ConfigurationType = InConfigurationType;
	}

	public Configurable(string InFieldName, string InResourcePath) : this(InResourcePath)
	{
		FieldName = InFieldName;
	}
}


public static class ConfigurableComponent_Extension
{
	//////////////////////////////////////////////////////////////////////////
	public static bool TryGetConfiguration<T>(this Component component, out T OutConfiguration) where T : ConfigurationBase
	{
		OutConfiguration = null;
		if (ReflectionHelper.TryGetAttributeValue(component.GetType(), (Configurable configurable) => configurable.ResourcePath, out string ResourcePath))
		{
			OutConfiguration = Resources.Load<T>(ResourcePath);
		}
		return OutConfiguration.IsNotNull();
	}

	//////////////////////////////////////////////////////////////////////////
	public static void TryGetConfigurationAsync<T>(this Component component, in System.Action<T> InOnResourceLoaded, in System.Action<string> InOnLoadFailed) where T : ConfigurationBase
	{
		if (ReflectionHelper.TryGetAttributeValue(component.GetType(), (Configurable configurable) => configurable.ResourcePath, out string ResourcePath))
		{
			ResourceManager.LoadResourceAsync(ResourcePath, InOnResourceLoaded, InOnLoadFailed);
		}
	}
}