
using UnityEngine;


[System.AttributeUsage(System.AttributeTargets.Class, Inherited = true)]
public class Configurable : System.Attribute
{
	public readonly string			FieldName				= string.Empty;
	public readonly System.Type		ConfigurationType		= null;
	public readonly string			ResourcePath			= string.Empty;
	
	// !Static compile error using in on params !!
	public Configurable(string InFieldName, System.Type InConfigurableComponentType)
	{
		FieldName = InFieldName;
		ResourcePath = $"{InConfigurableComponentType.Name}Configuration";
	}
	public Configurable(string InFieldName, string InResourcePath)
	{
		FieldName = InFieldName;
		ResourcePath = InResourcePath;
	}

	public Configurable(System.Type InConfigurationType, string InResourcePath)
	{
		ConfigurationType = InConfigurationType;
		ResourcePath = InResourcePath;
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